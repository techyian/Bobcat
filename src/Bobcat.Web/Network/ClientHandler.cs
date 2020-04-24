using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ProtoBuf;
using Bobcat.Common.Network;

namespace Bobcat.Web.Network
{
    //public class ClientHandler
    //{
    //    private int _listenUDPPort;
    //    private int _listenTCPPort;
    //    private bool _active;

    //    public ConcurrentDictionary<Guid, CamClient> CamClients { get; }
        
    //    public ClientHandler(int listenUDPPort, int listenTCPPort)
    //    {
    //        _listenUDPPort = listenUDPPort;
    //        _listenTCPPort = listenTCPPort;

    //        this.CamClients = new ConcurrentDictionary<Guid, CamClient>();
    //    }

    //    public async Task ListenConnect()
    //    {
    //        var done = false;

    //        var listener = new TcpListener(IPAddress.Any, _listenTCPPort);
    //        var receiveBuffer = new byte[4096];

    //        listener.Start();

    //        try
    //        {
    //            while (!done)
    //            {
    //                Console.Write("Waiting for TCP connection...");

    //                var client = listener.AcceptTcpClient();
                    
    //                Console.WriteLine("Connection accepted.");

    //                Task.Run(() =>
    //                {

    //                });

    //                var ns = client.GetStream();
    //                var bytesRead = ns.Read(receiveBuffer, 0, client.ReceiveBufferSize);
    //                if (bytesRead == 0)
    //                {
    //                    // Read returns 0 if the client closes the connection
    //                    break;
    //                }

    //                // Parse protobuf stream.
    //                var ms = new MemoryStream(bytesRead);
    //                var request = Serializer.Deserialize<CamClientRequest>(ms);

    //                if (request == null)
    //                {
    //                    Console.WriteLine("Received bad request.");
    //                    continue;
    //                }

    //                var connectResponse = new CamClientConnectResponse();

    //                if (!this.CamClients.ContainsKey(request.ClientData.Id))
    //                {
    //                    this.CamClients.TryAdd(request.ClientData.Id, request.ClientData);
    //                }
    //                else
    //                {
    //                    connectResponse.StatusCode = CamClientConnectStatusCode.OkExistingClient;
    //                }

    //                try
    //                {
    //                    // Send response to the client.
    //                    Serializer.Serialize(ns, connectResponse);

    //                    ns.Close();
    //                    client.Close();
    //                }
    //                catch (Exception e)
    //                {
    //                    Console.WriteLine(e.ToString());
    //                }
                    
    //            }
    //        }
    //        catch (Exception e)
    //        {

    //        }
    //        finally
    //        {
    //            listener.Stop();
    //        }
    //    }

    //    public void ListenTransmit()
    //    {
    //        var done = false;
    //        var listener = new UdpClient(_listenUDPPort);
    //        var groupEP = new IPEndPoint(IPAddress.Any, _listenUDPPort);

    //        try
    //        {
    //            while (!done)
    //            {
    //                _active = true; 

    //                Console.WriteLine("Waiting for broadcast");
    //                byte[] bytes = listener.Receive(ref groupEP);

    //                Console.WriteLine("Received broadcast from {0} :\n {1}\n", groupEP.ToString(), Encoding.ASCII.GetString(bytes, 0, bytes.Length));
                    
    //                var ms = new MemoryStream(bytes);
    //                var request = Serializer.Deserialize<CamClientRequest>(ms);

    //                if (request == null || !this.CamClients.ContainsKey(request.ClientData.Id))
    //                {
    //                    Console.WriteLine("Received bad request.");
    //                    continue;
    //                }

    //                // Validate the request.


    //                // Relay the data.

    //            }
    //        }
    //        catch (Exception e)
    //        {
    //            Console.WriteLine(e.ToString());
    //        }
    //        finally
    //        {
    //            _active = false;
    //            listener.Close();
    //        }
    //    }

    //    private CamClientHeaderType ReadHeader(CamClientHeader header)
    //    {
            
    //    }

    //    private void SendAckResponse(IPAddress ipAddress, int port, CamClientConnectResponse response)
    //    {
    //        var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
    //        var broadcast = ipAddress;
    //        var sendBuffer = Encoding.BigEndianUnicode.GetBytes(JsonConvert.SerializeObject(response));
    //        var endpoint = new IPEndPoint(broadcast, port);

    //        socket.SendTo(sendBuffer, endpoint);
    //    }

    //    private Task ConnectionHandler(NetworkStream ns, CancellationToken token)
    //    {
    //        return Task.Run(() =>
    //        {


    //        }, token);
    //    }

    //}
}
