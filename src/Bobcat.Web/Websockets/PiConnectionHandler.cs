using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ProtoBuf;
using Bobcat.Common.Network;
using Bobcat.Web.Models;

namespace Bobcat.Web.Websockets
{
    public class PiConnectionHandler : WebSocketHandler
    {
        private readonly ILogger<PiConnectionHandler> _logger;

        public PiConnectionHandler(ConnectionManager webSocketConnectionManager, ILogger<PiConnectionHandler> logger) 
            : base(webSocketConnectionManager)
        {
            _logger = logger;
        }

        public List<CamClientDto> GetProviders()
        {
            var clients = this.WebSocketConnectionManager.GetAll();
            var providers = clients.Values.Where(c => c.ClientType == CamClientType.Provider).ToList();

            return providers.Select(c => new CamClientDto()
            {
                Id = c.Id,
                ConnectionId = c.ConnectionId,
                Hostname = c.Hostname,
                ClientType = c.ClientType
            }).ToList();
        }

        public override async Task<string> OnConnected(WebSocket socket)
        {
            var connectionId = await base.OnConnected(socket);
            
            return connectionId;
        }

        public override async Task ReceiveTextAsync(WebSocket socket, string connectionId, string requestedProviderId, WebSocketReceiveResult result, string text)
        {
            // Check whether we have received a ping from the Internet browser Websocket and if so send a pong message back. 
            if (text.StartsWith("__ping__"))
            {
                // Extract the requested connectionId from the ping message.
                if (!string.IsNullOrEmpty(requestedProviderId) && Guid.TryParse(requestedProviderId, out Guid parsedGuid))
                {
                    // Check if the connectionId is still available.
                    var checkSocket = this.WebSocketConnectionManager.GetSocketById(requestedProviderId);

                    if (checkSocket != null)
                    {
                        await SendMessageAsync(socket, "__pong__");
                    }
                    else
                    {
                        socket.Abort();

                        await this.OnDisconnected(requestedProviderId);
                    }
                }
            }
        }

        public override async Task ReceiveBufferAsync(WebSocket socket, string connectionId, WebSocketReceiveResult result, byte[] buffer)
        {
            try
            {
                var request = this.ExtractCamClientRequestMessage(buffer);

                if (request?.ClientData == null)
                {
                    _logger.LogError("Received bad request.");
                    return;
                }

                var status = WebSocketConnectionManager.GetAll().TryGetValue(connectionId, out CamClient currentClient);

                if (status && currentClient != null)
                {
                    request.ClientData.ConnectionId = connectionId;
                    request.ClientData.ActiveSocket = currentClient.ActiveSocket;

                    status = WebSocketConnectionManager.GetAll().TryUpdate(connectionId, request.ClientData, currentClient);

                    if (!status)
                    {
                        _logger.LogError("Unable to update current client object.");
                        return;
                    }

                    if (request.ClientData.ClientType == CamClientType.Provider)
                    {
                        var subscribers = WebSocketConnectionManager.GetSubscribersForProvider(connectionId);

                        // Added in case original collection is modified in another thread once we've retrieved it.
                        var tempSubs = new List<string>(subscribers);

                        foreach (var subscriber in tempSubs)
                        {
                            await SendDataToSocket(subscriber, request.Data);
                        }
                    }
                }
                else
                {
                    _logger.LogError("Unable to retrieve current client object.");
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Could not process received buffer.");
            }
        }

        private CamClientRequest ExtractCamClientRequestMessage(byte[] buffer)
        {
            // Data length will be in the first 4 bytes of the buffer.
            var frame = new byte[4];
            Array.Copy(buffer, 0, frame, 0, 4);

            if (BitConverter.IsLittleEndian)
                Array.Reverse(frame);

            var dataLength = BitConverter.ToInt32(frame, 0);

            var ms = new MemoryStream(buffer, 4, dataLength);

            var request = Serializer.Deserialize<CamClientRequest>(ms);

            return request;
        }
    }
}
