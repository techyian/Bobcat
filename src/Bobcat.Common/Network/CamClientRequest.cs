using System;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;

namespace Bobcat.Common.Network
{
    [ProtoContract]
    public class CamClientRequest
    {
        [ProtoMember(1)]
        public CamClientHeader Header { get; set; }
        [ProtoMember(2)]
        public CamClient ClientData { get; set; }
        [ProtoMember(3)]
        public byte[] Data { get; set; }
    }
}
