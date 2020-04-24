using Microsoft.Extensions.Logging;
using MMALSharp;
using MMALSharp.Components;
using MMALSharp.Native;
using System;
using System.Buffers;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using MMALSharp.Common;
using MMALSharp.Common.Utility;
using ProtoBuf;
using Bobcat.Common.Network;
using Websocket.Client;

namespace Bobcat.Client
{
    public class PiCamService : IDisposable
    {
        private readonly ILogger _logger;
        private readonly CancellationTokenSource _applicationTokenSource;
        private readonly MMALCamera _cam = MMALCamera.Instance;
        private readonly string _uniqueId = Guid.NewGuid().ToString();

        private Process _captureHandlerProcess;
        private CancellationTokenSource _cameraTokenSource;
        private WebsocketClient _client;
        private bool _running;
        private string _hostname;
        private DateTime _lastPingSent;
        
        public PiCamService(ILogger<PiCamService> logger, CancellationTokenSource applicationTokenSource)
        {
            _logger = logger;
            _applicationTokenSource = applicationTokenSource;
            _hostname = Dns.GetHostName();
        }

        public void InitialiseClient()
        {
            var url = new Uri("ws://192.168.1.92:44369/bobcat");

            var factory = new Func<ClientWebSocket>(() => new ClientWebSocket
            {
                Options =
                {
                    KeepAliveInterval = TimeSpan.FromSeconds(5)
                }
            });

            var client = new WebsocketClient(url, factory);
            
            client.ReconnectTimeout = TimeSpan.FromSeconds(30);
            client.ReconnectionHappened.Subscribe(info =>
                _logger.LogInformation($"Reconnection happened, type: {info.Type}"));

            client.DisconnectionHappened.Subscribe(
                msg =>
                {
                    _logger.LogCritical($"Disconnect happened: {msg.Type} {msg.CloseStatus} {msg.Exception} {msg.Exception?.InnerException} {msg.CloseStatusDescription}");
                });
            
            client.MessageReceived.Subscribe(msg =>
            {
                if (msg.MessageType == WebSocketMessageType.Text)
                {
                    _logger.LogInformation($"Text Message received: {msg.Text}");
                    this.ProcessTextReceived(msg.Text);
                }

                if (msg.MessageType == WebSocketMessageType.Binary)
                {
                    _logger.LogInformation($"Binary Message received");
                    this.ProcessBinaryReceived(msg.Binary);
                }

                if (msg.MessageType == WebSocketMessageType.Close)
                {
                    // Attempt reconnect.
                    client?.Start();
                }
            });

            client.Start();
            
            _logger.LogInformation("Client started");

            _client = client;
        }

        public async Task InitialiseCamera()
        {
            if (!_running)
            {
                _cameraTokenSource = new CancellationTokenSource();

                await this.ExecuteCameraAsync(_cameraTokenSource.Token);
            }
            else
            {
                _logger.LogWarning("Camera is already started and received initialise request.");
            }
        }
        
        private async Task ExecuteCameraAsync(CancellationToken stoppingToken)
        {
            /*
             * Note: Tried using Splitter component attached to video port however this caused a strange issue whereby
             * a portion of the right side of the video was attached to the left. Directly processing off the video
             * port does not have this issue. 
             */
            
            //MMALCameraConfig.Debug = true;
            MMALCameraConfig.VideoResolution = new Resolution(640, 480);
            MMALCameraConfig.VideoFramerate = new MMAL_RATIONAL_T(24, 1);
            MMALCameraConfig.VideoEncoding = MMALEncoding.I420;
            
            var width = MMALCameraConfig.VideoResolution.Width;
            var height = MMALCameraConfig.VideoResolution.Height;
            var fr = MMALCameraConfig.VideoFramerate.Num;
            var argument = $"-f rawvideo -pix_fmt yuv420p -s {width}x{height} -r {fr} -i - -f mpegts -c:v mpeg1video -c:a none -b:v 800k -bf 0 -r {fr} -s {width}x{height} -";

            using (var handler = new BobcatCaptureHandler(argument))
            using (var preview = new MMALNullSinkComponent())
            {
                _cam.ConfigureCameraSettings(null, handler);

                // Create our component pipeline.         
                _cam.Camera.PreviewPort.ConnectTo(preview);

                // Camera warm up time
                await Task.Delay(2000).ConfigureAwait(false);

                _captureHandlerProcess = handler.CurrentProcess;

                try
                {
                    _running = true;

                    var cameraTask = _cam.ProcessAsync(_cam.Camera.VideoPort, stoppingToken);
                    this.ProcessFFmpegStream();

                    await cameraTask;
                }
                catch (Exception e)
                {
                    _logger.LogError($"An error occurred: {e.Message}");
                    _running = false;
                }
            }

            _running = false;
        }

        private void ProcessFFmpegStream()
        {
            try
            {
                if (_captureHandlerProcess.StandardOutput.BaseStream.CanRead)
                {
                    var arrayPool = ArrayPool<byte>.Shared;

                    while (!_cameraTokenSource.Token.IsCancellationRequested && 
                           !_applicationTokenSource.Token.IsCancellationRequested)
                    {
                        // ArrayPool should hopefully be quicker for larger allocations.
                        var buffer = arrayPool.Rent(32768);
                        var ms = new MemoryStream();

                        var read = _captureHandlerProcess.StandardOutput.BaseStream.Read(buffer, 0, buffer.Length);

                        // The buffer we send to the server should only be the length of the amount of data we were able to extract from
                        // the StandardOutput stream so we need to copy that chunk into a smaller buffer.
                        var accurateBuffer = new byte[read];

                        Array.Copy(buffer, 0, accurateBuffer, 0, read);

                        var request = this.GenerateRequest(accurateBuffer);
                        
                        Serializer.Serialize(ms, request);

                        var byteArr = new byte[(int)ms.Length + 4];

                        _logger.LogInformation($"Memorystream length {(int)ms.Length}");

                        byte[] intBytes = BitConverter.GetBytes((int)ms.Length);
                        if (BitConverter.IsLittleEndian)
                            Array.Reverse(intBytes);

                        Array.Copy(intBytes, byteArr, intBytes.Length);
                        Array.Copy(ms.ToArray(), 0, byteArr, 4, ms.Length);

                        _logger.LogInformation($"Sending data {read}");

                        Task.Run(() => _client.Send(byteArr));

                        arrayPool.Return(buffer);
                    }
                }
                else
                {
                    _logger.LogInformation("Can't read stream.");
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"An error occurred: {e.Message}");
            }
        }

        private CamClientRequest GenerateRequest(byte[] buffer)
        {
            return new CamClientRequest()
            {
                ClientData = new CamClient()
                {
                    ClientType = CamClientType.Provider,
                    Id = _uniqueId,
                    Hostname = _hostname
                },
                Header = new CamClientHeader()
                {
                    HeaderType = CamClientHeaderType.SendVideo
                },
                Data = buffer
            };
        }

        private void ProcessTextReceived(string text)
        {
            if (text == "__ping__")
            {
                Task.Run(async () =>
                {
                    await Task.Delay(3000);

                    _lastPingSent = DateTime.Now;

                    _client.Send("__pong__");
                });
            }
        }

        private void ProcessBinaryReceived(byte[] buffer)
        {
            try
            {
                // Extract frame size which will be in the first 4 bytes of the buffer. We can then 
                // deserialize the protobuf buffer.
                var frame = new byte[4];
                Array.Copy(buffer, 0, frame, 0, 4);

                if (BitConverter.IsLittleEndian)
                    Array.Reverse(frame);

                var dataLength = BitConverter.ToInt32(frame, 0);

                var ms = new MemoryStream(buffer, 4, dataLength);

                var request = Serializer.Deserialize<CamClientRequest>(ms);

                if (request?.ClientData == null)
                {
                    _logger.LogError("Received bad request.");
                    return;
                }

                switch (request.Header.HeaderType)
                {
                    case CamClientHeaderType.Start:
                        Task.Run(async () =>
                        {
                            await this.InitialiseCamera();
                        });
                        break;
                    case CamClientHeaderType.Stop:
                        if (_running)
                        {
                            _cameraTokenSource.Cancel();
                        }

                        break;
                    case CamClientHeaderType.Config:
                        break;
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Could not process binary websocket request, possibly not of correct type.");
            }
        }

        public void Dispose()
        {
            _logger.LogInformation("Disposing and cleaning up native resources.");
            _applicationTokenSource?.Dispose();
            _captureHandlerProcess?.Dispose();
            _cameraTokenSource?.Dispose();
            _client?.Dispose();
            _cam.Cleanup();
        }
    }
}
