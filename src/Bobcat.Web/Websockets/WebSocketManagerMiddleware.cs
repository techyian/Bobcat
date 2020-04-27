using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bobcat.Common.Network;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Bobcat.Web.Websockets
{
    // Credit to Radu Matei: https://radu-matei.com/blog/aspnet-core-websockets-middleware/
    public class WebSocketManagerMiddleware
    {
        public const string WebsocketUrlPrefix = "bobcat";

        private readonly RequestDelegate _next;
        private WebSocketHandler _webSocketHandler;
        private ILogger<WebSocketManagerMiddleware> _logger;

        public WebSocketManagerMiddleware(
            RequestDelegate next,
            WebSocketHandler webSocketHandler,
            ILogger<WebSocketManagerMiddleware> logger)
        {
            _next = next;
            _webSocketHandler = webSocketHandler;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            if (!context.WebSockets.IsWebSocketRequest)
            {
                _logger.LogWarning("Not websocket request.");
                await _next.Invoke(context);
                return;
            }

            _logger.LogInformation("Attempt to accept websocket request.");

            var socket = await context.WebSockets.AcceptWebSocketAsync();

            var connectionId = await _webSocketHandler.OnConnected(socket);

            if (string.IsNullOrEmpty(connectionId))
            {
                _logger.LogError("Unable to add socket.");
                return;
            }

            var cts = new CancellationTokenSource();
            var socketClosing = false;
            var bufferPool = ArrayPool<byte>.Shared;
            var path = context.Request.Path.Value;

            var requestedProvider = await this.HandleProviderRequest(path, connectionId);

            this.SpawnPingWorker(socket, connectionId, cts);
            
            while (!socketClosing)
            {
                try
                {
                    await Receive(socket, bufferPool.Rent(1024 * 64), async (result, buffer) =>
                    {
                        if (result.MessageType == WebSocketMessageType.Text)
                        {
                            var zeroIndex = Array.IndexOf(buffer, (byte) 0);

                            await _webSocketHandler.ReceiveTextAsync(socket, connectionId, requestedProvider, result, Encoding.UTF8.GetString(buffer, 0, zeroIndex));
                            return;
                        }

                        if (result.MessageType == WebSocketMessageType.Binary)
                        {
                            await _webSocketHandler.ReceiveBufferAsync(socket, connectionId, result, buffer);
                            return;
                        }

                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            await _webSocketHandler.OnDisconnected(connectionId);
                            socketClosing = true;
                            
                            return;
                        }

                        bufferPool.Return(buffer);
                    });
                }
                catch (WebSocketException wsex)
                {
                    socketClosing = true;
                }
            }

            if (!string.IsNullOrEmpty(requestedProvider))
            {
                // Try to remove subscription to provider.
                _webSocketHandler.WebSocketConnectionManager.RemoveSubscriberForProvider(requestedProvider,
                    connectionId);
            }

            // The provider has closed the Websocket connection, remove from list of providers.
            _webSocketHandler.WebSocketConnectionManager.RemoveProvider(connectionId);

            cts.Cancel();

            await _webSocketHandler.OnDisconnected(connectionId);
        }
        
        private async Task Receive(WebSocket socket, byte[] buffer, Action<WebSocketReceiveResult, byte[]> handleMessage)
        {
            var result = await socket.ReceiveAsync(buffer: new ArraySegment<byte>(buffer),
                cancellationToken: CancellationToken.None);

            handleMessage(result, buffer);
        }

        private void SpawnPingWorker(WebSocket socket, string connectionId, CancellationTokenSource cts)
        {
            // This worker is responsible for sending a ping request to one of the Pi camera clients.
            Task.Run(async () =>
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    await Task.Delay(5000);

                    try
                    {
                        await _webSocketHandler.SendMessageAsync(socket, "__ping__");
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, $"An error occurred when pinging socket {connectionId}.");
                        break;
                    }
                }
            }, cts.Token);
        }

        private async Task<string> HandleProviderRequest(string path, string connectionId)
        {
            // This should give us the provider we want to receive from.
            var requestedProviderSplit = path.Split(WebsocketUrlPrefix);
            var requestedProvider = string.Empty;

            if (requestedProviderSplit.Length > 1 && !string.IsNullOrEmpty(requestedProviderSplit[1]))
            {
                requestedProvider = requestedProviderSplit[1];

                if (requestedProvider.StartsWith("-"))
                {
                    requestedProvider = requestedProvider.TrimStart('-');
                }

                _webSocketHandler.WebSocketConnectionManager.InsertSubscriberForProvider(
                    requestedProvider,
                    connectionId);
            }
            else
            {
                // If we haven't requested a provider, assume we're a provider ourselves.
                if (!_webSocketHandler.WebSocketConnectionManager.InsertProvider(connectionId))
                {
                    _logger.LogError($"Unable to add connectionId {connectionId} to list of providers.");

                    await _webSocketHandler.OnDisconnected(connectionId);
                }
            }

            return requestedProvider;
        }
    }
}
