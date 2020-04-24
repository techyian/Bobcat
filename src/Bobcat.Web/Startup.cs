using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MMALSharp.Common.Utility;
using NLog.Extensions.Logging;
using System;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebSockets;
using Bobcat.Web;
using Bobcat.Web.Websockets;

namespace Bobcat
{
    public class Startup
    {
        /*
         *  To configure application on your Pi:
         *  1) Run `dotnet publish -r linux-arm`
         *  2) Copy contents to your Pi
         *  3) TESTING ONLY: On your Pi and in a terminal, run `export ASPNETCORE_URLS=http://*:5000/` so that kestrel will listen for incoming requests to port 5000.
         *     In production you should use a webserver such a Nginx.
         *  4) Run `./Bobcat` and it should start the web application.
         *  5) Browse to your Pi on port 5000 via your web browser.
         */
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddWebSocketManager();

            //services.AddCors(options => options.AddPolicy("CorsPolicy", builder =>
            //{
            //    builder
            //        .AllowAnyMethod()
            //        .AllowAnyHeader()
            //        .AllowAnyOrigin();
            //}));

            //services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
            services.AddControllersWithViews().AddRazorRuntimeCompilation();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            
            //app.UseHttpsRedirection();
            app.UseStaticFiles();
            //app.UseAuthorization();

            var serviceScopeFactory = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>();
            var serviceProvider = serviceScopeFactory.CreateScope().ServiceProvider;

            //app.UseCors("CorsPolicy");
            app.UseWebSockets();
            app.UseRouting();
            app.MapWebSocketManager($"/{WebSocketManagerMiddleware.WebsocketUrlPrefix}", serviceProvider.GetService<PiConnectionHandler>());

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
