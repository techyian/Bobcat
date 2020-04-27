using System.Collections.Generic;
using Bobcat.Common.Network;

namespace Bobcat.Web.Models
{
    public class CamClientDto
    {
        public string Id { get; set; }
        public string ConnectionId { get; set; }
        public string Hostname { get; set; }
        public CamClientType ClientType { get; set; }
        public List<CameraConfig> ClientConfig { get; set; }
    }
}
