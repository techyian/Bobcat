// <copyright file="ClientApiController.cs" company="Techyian">
// Copyright (c) Ian Auty. All rights reserved.
// Licensed under the MIT License. Please see LICENSE.txt for License info.
// </copyright>

using Microsoft.AspNetCore.Mvc;
using Bobcat.Web.Websockets;

namespace Bobcat.Web.Controllers
{
    public class ClientApiController : ControllerBase
    {
        private readonly PiConnectionHandler _piConnectionHandler;

        public ClientApiController(PiConnectionHandler piConnectionHandler)
        {
            _piConnectionHandler = piConnectionHandler;
        }

        public IActionResult GetProviders()
        {
            return Ok(_piConnectionHandler.GetProviders());
        }
    }
}
