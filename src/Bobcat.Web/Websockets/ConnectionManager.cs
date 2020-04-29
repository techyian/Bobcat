// <copyright file="ConnectionManager.cs" company="Techyian">
// Copyright (c) Ian Auty. All rights reserved.
// Licensed under the MIT License. Please see LICENSE.txt for License info.
// </copyright>

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Bobcat.Common.Network;

namespace Bobcat.Web.Websockets
{
    // Credit to Radu Matei: https://radu-matei.com/blog/aspnet-core-websockets-middleware/
    public class ConnectionManager
    {
        private ConcurrentDictionary<string, CamClient> _sockets = new ConcurrentDictionary<string, CamClient>();
        private ConcurrentDictionary<string, List<string>> _providerSubscribers = new ConcurrentDictionary<string, List<string>>();
        
        public WebSocket GetSocketById(string id)
        {
            if (_sockets.TryGetValue(id, out CamClient client))
            {
                return client.ActiveSocket;
            }

            return null;
        }

        public ConcurrentDictionary<string, CamClient> GetAll()
        {
            return _sockets;
        }

        public ConcurrentDictionary<string, List<string>> GetAllProviders()
        {
            return _providerSubscribers;
        }

        public List<string> GetSubscribersForProvider(string providerConnectionId)
        {
            if (_providerSubscribers.TryGetValue(providerConnectionId, out List<string> list))
            {
                return list;
            }

            return null;
        }

        public bool InsertProvider(string providerConnectionId)
        {
            return _providerSubscribers.TryAdd(providerConnectionId, new List<string>());
        }

        public bool RemoveProvider(string providerConnectionId)
        {
            return _providerSubscribers.TryRemove(providerConnectionId, out List<string> value);
        }

        public void InsertSubscriberForProvider(string providerConnectionId, string subscriber)
        {
            if (!_providerSubscribers.ContainsKey(providerConnectionId))
            {
                this.InsertProvider(providerConnectionId);
            }

            if (_providerSubscribers.TryGetValue(providerConnectionId, out List<string> list))
            {
                if (!list.Contains(subscriber))
                {
                    list.Add(subscriber);
                }
            }
        }

        public bool RemoveSubscriberForProvider(string providerConnectionId, string subscriber)
        {
            if (_providerSubscribers.TryGetValue(providerConnectionId, out List<string> list))
            {
                if (list.Contains(subscriber))
                {
                    return list.Remove(subscriber);
                }
            }

            return false;
        }
        
        public string GetId(WebSocket socket)
        {
            return _sockets.FirstOrDefault(p => p.Value?.ActiveSocket == socket).Key;
        }

        public string AddSocket(CamClient client)
        {
            var connectionId = this.CreateConnectionId();

            client.ConnectionId = connectionId;

            if (_sockets.TryAdd(connectionId, client))
            {
                return connectionId;
            }
            
            return null;
        }

        public async Task RemoveSocket(string id)
        {
            CamClient client;

            if (_sockets.TryRemove(id, out client))
            {
                if (client?.ActiveSocket != null)
                {
                    await client.ActiveSocket.CloseAsync(closeStatus: WebSocketCloseStatus.NormalClosure,
                        statusDescription: "Closed by the ConnectionManager",
                        cancellationToken: CancellationToken.None);
                }
            }
        }

        private string CreateConnectionId()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
