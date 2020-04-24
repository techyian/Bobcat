using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MMALSharp.Common.Utility;
using NLog.Extensions.Logging;

namespace Bobcat.Client
{
    class Program
    {
        private static CancellationTokenSource _applicationTokenSource;
        private static PiCamService _service;

        public static async Task Main(string[] args)
        {
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .ClearProviders()
                    .SetMinimumLevel(LogLevel.Trace)
                    .AddNLog("NLog.config");
            });

            MMALLog.LoggerFactory = loggerFactory;

            _applicationTokenSource = new CancellationTokenSource();
            
            _service = new PiCamService(loggerFactory.CreateLogger<PiCamService>(), _applicationTokenSource);

            Console.CancelKeyPress += Console_OnCancelKeyPress;
            
            _service.InitialiseClient();

            await _service.InitialiseCamera();
        }

        private static void Console_OnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
            _applicationTokenSource?.Cancel();
            _service.Dispose();
        }
    }
}
