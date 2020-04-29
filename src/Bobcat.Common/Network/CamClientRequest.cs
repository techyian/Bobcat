// <copyright file="CamClientRequest.cs" company="Techyian">
// Copyright (c) Ian Auty. All rights reserved.
// Licensed under the MIT License. Please see LICENSE.txt for License info.
// </copyright>

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
