using System;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;

namespace Bobcat.Common.Network
{
    [ProtoContract]
    public class CamClientHeader
    {
        [ProtoMember(1)]
        public CamClientHeaderType HeaderType { get; set; }
    }

    public enum CamClientHeaderType
    {
        Unknown = -1,
        SendVideo,
        Start,
        Stop,
        Config
    }
}
