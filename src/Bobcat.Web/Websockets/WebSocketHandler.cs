// <copyright file="WebSocketHandler.cs" company="Techyian">
// Copyright (c) Ian Auty. All rights reserved.
// Licensed under the MIT License. Please see LICENSE.txt for License info.
// </copyright>

using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bobcat.Common.Network;

namespace Bobcat.Web.Websockets
{
    // Credit to Radu Matei: https://radu-matei.com/blog/aspnet-core-websockets-middleware/
    public abstract class WebSocketHandler
    {
        public ConnectionManager WebSocketConnectionManager { get; set; }

        protected WebSocketHandler(ConnectionManager webSocketConnectionManager)
        {
            WebSocketConnectionManager = webSocketConnectionManager;
        }

        public virtual async Task<string> OnConnected(WebSocket socket)
        {
            var client = new CamClient()
            {
                ActiveSocket = socket
            };

            return WebSocketConnectionManager.AddSocket(client);
        }

        public virtual async Task OnDisconnected(string connectionId)
        {
            await WebSocketConnectionManager.RemoveSocket(connectionId);
        }

        public async Task SendMessageAsync(WebSocket socket, string message)
        {
            if (socket?.State != WebSocketState.Open)
                return;

            await socket?.SendAsync(buffer: new ArraySegment<byte>(array: Encoding.ASCII.GetBytes(message),
                    offset: 0,
                    count: message.Length),
                messageType: WebSocketMessageType.Text,
                endOfMessage: true,
                cancellationToken: CancellationToken.None);
        }

        public async Task SendBufferAsync(WebSocket socket, byte[] buffer)
        {
            if (socket?.State != WebSocketState.Open)
                return;

            await socket?.SendAsync(buffer: new ArraySegment<byte>(array: buffer,
                    offset: 0,
                    count: buffer.Length),
                messageType: WebSocketMessageType.Binary,
                endOfMessage: true,
                cancellationToken: CancellationToken.None);
        }
        
        public async Task SendMessageAsync(string socketId, string message)
        {
            await SendMessageAsync(WebSocketConnectionManager.GetSocketById(socketId), message);
        }

        public async Task SendBufferAsync(string socketId, byte[] buffer)
        {
            await SendBufferAsync(WebSocketConnectionManager.GetSocketById(socketId), buffer);
        }

        public abstract Task ReceiveBufferAsync(WebSocket socket, string connectionId, WebSocketReceiveResult result, byte[] buffer);
        public abstract Task ReceiveTextAsync(WebSocket socket, string connectionId, string requestedProviderId, WebSocketReceiveResult result, string text);
    }
}
