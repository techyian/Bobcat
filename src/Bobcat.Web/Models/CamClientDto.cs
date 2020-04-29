// <copyright file="CamClientDto.cs" company="Techyian">
// Copyright (c) Ian Auty. All rights reserved.
// Licensed under the MIT License. Please see LICENSE.txt for License info.
// </copyright>

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
