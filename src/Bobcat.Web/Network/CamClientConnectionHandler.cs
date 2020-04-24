using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Logging;
using ProtoBuf;
using Bobcat.Common.Network;

namespace Bobcat.Web.Network
{
    //public class CamClientConnectionHandler : ConnectionHandler
    //{
    //    public static ConcurrentDictionary<Guid, CamClient> Clients { get; } = new ConcurrentDictionary<Guid, CamClient>();

    //    private readonly ILogger<CamClientConnectionHandler> _logger;

    //    public CamClientConnectionHandler(ILogger<CamClientConnectionHandler> logger)
    //    {
    //        _logger = logger;
    //    }

    //    public override async Task OnConnectedAsync(ConnectionContext connection)
    //    {
    //        _logger.LogInformation(connection.ConnectionId + " connected");

    //        Guid currentClient = default;

    //        while (!connection.ConnectionClosed.IsCancellationRequested)
    //        {
    //            var result = await connection.Transport.Input.ReadAsync();
    //            var buffer = result.Buffer;
    //            var arr = buffer.ToArray();
    //            var ms = new MemoryStream(arr);
    //            var request = Serializer.Deserialize<CamClientRequest>(ms);

    //            if (request == null)
    //            {
    //                Console.WriteLine("Received bad request.");
    //                continue;
    //            }

    //            var connectResponse = new CamClientConnectResponse();

    //            currentClient = request.ClientData.Id;

    //            if (!Clients.ContainsKey(request.ClientData.Id))
    //            {
    //                var addResult = Clients.TryAdd(request.ClientData.Id, request.ClientData);

    //                if (!addResult)
    //                {
    //                    connectResponse.StatusCode = CamClientConnectStatusCode.BadRequest;
    //                }
    //            }
    //            else
    //            {
    //                connectResponse.StatusCode = CamClientConnectStatusCode.OkExistingClient;
    //            }
                
    //            try
    //            {
    //                // Send response to the client.
    //                Serializer.Serialize(connection.Transport.Output.AsStream(), connectResponse);
    //            }
    //            catch (Exception e)
    //            {
    //                Console.WriteLine(e.ToString());
    //            }

    //            //await connection.Transport.Output.WriteAsync(segment);

    //            /*if (result.IsCompleted)
    //            {
    //                break;
    //            }*/

    //            connection.Transport.Input.AdvanceTo(buffer.End);
    //        }
            
    //        Clients.TryRemove(currentClient, out CamClient client);

    //        _logger.LogInformation(connection.ConnectionId + " disconnected");
    //    }
    //}
}
