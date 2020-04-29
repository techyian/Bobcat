// <copyright file="CameraConfig.cs" company="Techyian">
// Copyright (c) Ian Auty. All rights reserved.
// Licensed under the MIT License. Please see LICENSE.txt for License info.
// </copyright>

using Newtonsoft.Json;
using ProtoBuf;

namespace Bobcat.Common.Network
{
    [ProtoContract]
    public class CameraConfig
    {
        [JsonProperty("configType")]
        [ProtoMember(1)]
        public string ConfigType { get; set; }
        [JsonProperty("configValue")]
        [ProtoMember(2)]
        public string ConfigValue { get; set; }
    }
}
