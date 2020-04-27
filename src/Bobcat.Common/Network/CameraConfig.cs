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
