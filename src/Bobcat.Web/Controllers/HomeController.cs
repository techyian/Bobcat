using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Bobcat.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConfiguration _configuration;

        public HomeController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            var relayHostname = _configuration["RelayServerHostname"];

            if (string.IsNullOrEmpty(relayHostname))
            {
                throw new NullReferenceException("Could not parse RelayServerHostname from appsettings.json");
            }

            return View("Index", relayHostname);
        }
    }
}
