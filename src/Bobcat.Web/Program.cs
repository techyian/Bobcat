using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using Bobcat.Web;
using Bobcat.Web.Network;

namespace Bobcat
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                    logging.AddNLog("NLog.config");
                })
                //.UseKestrel(options =>
                //{
                //    // TCP 8007
                //    options.ListenLocalhost(8007, builder =>
                //    {
                //        builder.UseConnectionHandler<CamClientConnectionHandler>();
                //    });
                //})
                .UseStartup<Startup>();


        //.ConfigureServices(services =>
        //{
        //    services.AddHostedService<PiCamService>();
        //});
    }
}
