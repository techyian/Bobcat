// <copyright file="CamClientHeader.cs" company="Techyian">
// Copyright (c) Ian Auty. All rights reserved.
// Licensed under the MIT License. Please see LICENSE.txt for License info.
// </copyright>

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
