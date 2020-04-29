// <copyright file="MiddlewareExtensions.cs" company="Techyian">
// Copyright (c) Ian Auty. All rights reserved.
// Licensed under the MIT License. Please see LICENSE.txt for License info.
// </copyright>

using System;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Bobcat.Web.Websockets;

namespace Bobcat.Web
{
    public static class MiddlewareExtensions
    {
        // Credit to Radu Matei: https://radu-matei.com/blog/aspnet-core-websockets-middleware/
        public static IApplicationBuilder MapWebSocketManager(this IApplicationBuilder app,
            PathString path,
            WebSocketHandler handler)
        {
            return app.MapWhen(x => x.Request.Path.Value.StartsWith(path, StringComparison.OrdinalIgnoreCase),
                (_app) => _app.UseMiddleware<WebSocketManagerMiddleware>(handler));
        }

        public static IServiceCollection AddWebSocketManager(this IServiceCollection services)
        {
            services.AddTransient<ConnectionManager>();

            foreach (var type in Assembly.GetEntryAssembly().ExportedTypes)
            {
                if (type.GetTypeInfo().BaseType == typeof(WebSocketHandler))
                {
                    services.AddSingleton(type);
                }
            }

            return services;
        }
    }
}
