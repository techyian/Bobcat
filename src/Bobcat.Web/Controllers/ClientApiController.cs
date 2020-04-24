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
