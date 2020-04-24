using System;
using System.Net.WebSockets;
using ProtoBuf;

namespace Bobcat.Common.Network
{
    [ProtoContract]
    public class CamClient
    {
        [ProtoMember(1)]
        public string Id { get; set; }
        [ProtoMember(2)]
        public string Hostname { get; set; }
        [ProtoMember(3)]
        public DateTime? LastSeen { get; set; }
        [ProtoMember(4)]
        public CamClientType ClientType { get; set; }
        public string ConnectionId { get; set; }
        public WebSocket ActiveSocket { get; set; }
    }

    public enum CamClientType
    {
        Consumer,
        Provider
    }
}
