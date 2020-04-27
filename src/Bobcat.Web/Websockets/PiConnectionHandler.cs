using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using Bobcat.Common;
using Microsoft.Extensions.Logging;
using ProtoBuf;
using Bobcat.Common.Network;
using Bobcat.Web.Models;
using Newtonsoft.Json;

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
                ClientType = c.ClientType,
                ClientConfig = c.ClientConfig
            }).ToList();
        }

        public override async Task<string> OnConnected(WebSocket socket)
        {
            var connectionId = await base.OnConnected(socket);
            
            return connectionId;
        }

        public override async Task ReceiveTextAsync(WebSocket socket, string connectionId, string requestedProviderId, WebSocketReceiveResult result, string text)
        {
            if (!string.IsNullOrEmpty(requestedProviderId))
            {
                var requestedProviderSocket = this.GetProviderActiveSocket(requestedProviderId);

                if (requestedProviderSocket != null)
                {
                    // Check whether we have received a ping from the Internet browser Websocket and if so send a pong message back. 
                    if (text.StartsWith("__ping__"))
                    {
                        await SendMessageAsync(socket, "__pong__");
                    }

                    if (text.StartsWith("__config__"))
                    {
                        // Try and parse as JSON message.
                        try
                        {
                            var filteredText = text.Replace("__config__", "");
                            var configChange = JsonConvert.DeserializeObject<List<CameraConfig>>(filteredText);

                            if (configChange != null && configChange.Count > 0)
                            {
                                // We have received a config change request, construct a CamClientRequest.
                                var ms = new MemoryStream();
                                var camClientRequest = this.GenerateRequest(new CamClientHeader()
                                {
                                    HeaderType = CamClientHeaderType.Config
                                }, Encoding.Unicode.GetBytes(filteredText));

                                Serializer.Serialize(ms, camClientRequest);

                                var buffer = Utility.ApplyFrameToMessage(ms);

                                await this.SendBufferAsync(requestedProviderSocket, buffer);
                            }
                        }
                        catch (Exception e)
                        {
                            // Just swallow and ignore if unable to parse JSON.
                        }
                    }
                }
                else
                {
                    await this.OnDisconnected(connectionId);
                    await this.OnDisconnected(requestedProviderId);
                }
            }
        }

        public override async Task ReceiveBufferAsync(WebSocket socket, string connectionId, WebSocketReceiveResult result, byte[] buffer)
        {
            try
            {
                var message = Utility.ExtractMessage(buffer);
                var request = Serializer.Deserialize<CamClientRequest>(message);

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
                            await SendBufferAsync(subscriber, request.Data);
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

        private WebSocket GetProviderActiveSocket(string requestedProviderId)
        {
            // Extract the requested connectionId from the ping message.
            if (!string.IsNullOrEmpty(requestedProviderId) && Guid.TryParse(requestedProviderId, out Guid parsedGuid))
            {
                // Check if the connectionId is still available.
                var checkSocket = this.WebSocketConnectionManager.GetSocketById(requestedProviderId);

                if (checkSocket != null)
                {
                    return checkSocket;
                }
            }

            return null;
        }

        private CamClientRequest GenerateRequest(CamClientHeader header, byte[] buffer)
        {
            return new CamClientRequest()
            {
                Header = header,
                Data = buffer
            };
        }
    }
}
