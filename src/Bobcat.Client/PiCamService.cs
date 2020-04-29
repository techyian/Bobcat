using Microsoft.Extensions.Logging;
using MMALSharp;
using MMALSharp.Components;
using MMALSharp.Native;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bobcat.Common;
using MMALSharp.Common;
using MMALSharp.Common.Utility;
using ProtoBuf;
using Bobcat.Common.Network;
using Newtonsoft.Json;
using Websocket.Client;

namespace Bobcat.Client
{
    public class PiCamService : IDisposable
    {
        private readonly ILogger _logger;
        private readonly CancellationTokenSource _applicationTokenSource;
        private readonly MMALCamera _cam = MMALCamera.Instance;
        private readonly string _hostname;
        private readonly ClientConfiguration _clientConfig;
        
        private static readonly object ReadLock = new object();

        private BobcatCaptureHandler _captureHandler;
        private CancellationTokenSource _cameraTokenSource;
        private WebsocketClient _client;
        private bool _running;
        private DateTime _lastPingSent;
     
        public PiCamService(ILogger<PiCamService> logger, CancellationTokenSource applicationTokenSource, ClientConfiguration clientConfig)
        {
            _clientConfig = clientConfig;
            _logger = logger;
            _applicationTokenSource = applicationTokenSource;
            _hostname = Dns.GetHostName();
        }

        public async Task InitialiseClient()
        {
            var url = new Uri($"{_clientConfig.RelayServerHostname}/bobcat");

            var factory = new Func<ClientWebSocket>(() => new ClientWebSocket
            {
                Options =
                {
                    KeepAliveInterval = TimeSpan.FromSeconds(5),
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

                    Task.Run(async () =>
                    {
                        await this.ProcessBinaryReceived(msg.Binary);
                    });
                }
            });

            _logger.LogInformation("Starting client...");

            _client = client;

            await client.Start();
        }

        public void ConfigureCaptureHandler()
        {
            MMALCameraConfig.VideoResolution = new Resolution(640, 480);
            MMALCameraConfig.VideoFramerate = new MMAL_RATIONAL_T(24, 1);
            MMALCameraConfig.VideoEncoding = MMALEncoding.I420;

            var width = MMALCameraConfig.VideoResolution.Width;
            var height = MMALCameraConfig.VideoResolution.Height;
            var fr = MMALCameraConfig.VideoFramerate.Num;
            var argument = $"-f rawvideo -pix_fmt yuv420p -s {width}x{height} -r {fr} -i - -f mpegts -c:v mpeg1video -c:a none -b:v 800k -bf 0 -r {fr} -s {width}x{height} -";

            var handler = new BobcatCaptureHandler(argument);

            _captureHandler = handler;
        }

        public async Task InitialiseCamera()
        {
            if (_running)
            {
                lock (ReadLock)
                {
                    _cameraTokenSource.Cancel();
                }
                
                // Wait for camera to cancel.
                await Task.Delay(500);
            }

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
            
            using (var preview = new MMALNullSinkComponent())
            {
                _cam.ConfigureCameraSettings(null, _captureHandler);

                // Create our component pipeline.         
                _cam.Camera.PreviewPort.ConnectTo(preview);

                // Camera warm up time
                await Task.Delay(2000).ConfigureAwait(false);

                try
                {
                    _running = true;
                    
                    var cameraTask = _cam.ProcessAsync(_cam.Camera.VideoPort, stoppingToken);
                    var ffmpegTask = Task.Run(this.ProcessFFmpegStream, stoppingToken);

                    await Task.WhenAny(cameraTask, ffmpegTask);
                }
                catch (Exception e)
                {
                    _logger.LogError($"An error occurred: {e.Message}");
                    _running = false;
                }
            }

            _logger.LogInformation("Stop camera running");

            _running = false;
        }

        private void ProcessFFmpegStream()
        {
            try
            {
                if (_captureHandler.CurrentProcess.StandardOutput.BaseStream.CanRead)
                {
                    var arrayPool = ArrayPool<byte>.Shared;

                    while (!_cameraTokenSource.Token.IsCancellationRequested &&
                           !_applicationTokenSource.Token.IsCancellationRequested)
                    {
                        // ArrayPool should hopefully be quicker for larger allocations.
                        var buffer = arrayPool.Rent(32768);
                        var ms = new MemoryStream();
                        
                        lock (ReadLock)
                        {
                            if (!_cameraTokenSource.Token.IsCancellationRequested &&
                                !_applicationTokenSource.Token.IsCancellationRequested)
                            {
                                var read = _captureHandler.CurrentProcess.StandardOutput.BaseStream.Read(buffer, 0, buffer.Length);

                                // The buffer we send to the server should only be the length of the amount of data we were able to extract from
                                // the StandardOutput stream so we need to copy that chunk into a smaller buffer.
                                var accurateBuffer = new byte[read];

                                Array.Copy(buffer, 0, accurateBuffer, 0, read);

                                var request = this.GenerateRequest(accurateBuffer);

                                Serializer.Serialize(ms, request);

                                var byteArr = Utility.ApplyFrameToMessage(ms);

                                //_logger.LogInformation($"Sending data {read}");

                                Task.Run(() => _client.Send(byteArr));
                            }
                        }

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

            _logger.LogInformation("Finish ProcessFFmpegStream.");
        }

        private CamClientRequest GenerateRequest(byte[] buffer)
        {
            return new CamClientRequest()
            {
                ClientData = new CamClient()
                {
                    ClientType = CamClientType.Provider,
                    Id = _clientConfig.UniqueId,
                    Hostname = _hostname,
                    ClientConfig = this.GenerateCurrentConfiguration()
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
        
        private async Task ProcessBinaryReceived(byte[] buffer)
        {
            try
            {
                // Extract frame size which will be in the first 4 bytes of the buffer. We can then 
                // deserialize the protobuf buffer.
                var ms = Utility.ExtractMessage(buffer);

                var request = Serializer.Deserialize<CamClientRequest>(ms);

                if (request == null)
                {
                    _logger.LogError("Unable to deserialise buffer message.");
                    return;
                }

                switch (request.Header.HeaderType)
                {
                    case CamClientHeaderType.Start:
                        _logger.LogInformation("Received start request.");
                        
                        await this.InitialiseCamera();
                        
                        break;
                    case CamClientHeaderType.Stop:
                        _logger.LogInformation("Received stop request.");

                        if (_running)
                        {
                            _cameraTokenSource.Cancel();
                        }

                        break;
                    case CamClientHeaderType.Config:
                        _logger.LogInformation("Received config change request.");
                        
                        this.ProcessConfigChangeRequest(Encoding.Unicode.GetString(request.Data));
                        await this.InitialiseCamera();

                        break;
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Could not process binary websocket request, possibly not of correct type.");
            }
        }

        private List<CameraConfig> GenerateCurrentConfiguration()
        {
            var cameraConfigList = new List<CameraConfig>
            {
                new CameraConfig()
                {
                    ConfigType = nameof(MMALCameraConfig.Brightness),
                    ConfigValue = MMALCameraConfig.Brightness.ToString(CultureInfo.InvariantCulture)
                },
                new CameraConfig()
                {
                    ConfigType = nameof(MMALCameraConfig.Sharpness),
                    ConfigValue = MMALCameraConfig.Sharpness.ToString(CultureInfo.InvariantCulture)
                },
                new CameraConfig()
                {
                    ConfigType = nameof(MMALCameraConfig.Contrast),
                    ConfigValue = MMALCameraConfig.Contrast.ToString(CultureInfo.InvariantCulture)
                },
                new CameraConfig()
                {
                    ConfigType = nameof(MMALCameraConfig.Saturation),
                    ConfigValue = MMALCameraConfig.Saturation.ToString(CultureInfo.InvariantCulture)
                },
                new CameraConfig()
                {
                    ConfigType = nameof(MMALCameraConfig.ImageFx),
                    ConfigValue = MMALCameraConfig.ImageFx.ToString()
                }
            };

            return cameraConfigList;
        }

        private void ProcessConfigChangeRequest(string changeRequestString)
        {
            _logger.LogInformation($"Received JSON {changeRequestString}");

            var changeRequestList = JsonConvert.DeserializeObject<List<CameraConfig>>(changeRequestString);

            if (changeRequestList?.Count == 0)
            {
                _logger.LogError("Could not deserialise JSON from config change request.");
                return;
            }

            _logger.LogInformation($"Received {changeRequestList.Count} items in payload.");

            foreach (var changeRequest in changeRequestList)
            {
                if (string.IsNullOrEmpty(changeRequest.ConfigType) || string.IsNullOrEmpty(changeRequest.ConfigValue))
                {
                    _logger.LogError($"Received invalid data in change request JSON. {changeRequest.ConfigType} {changeRequest.ConfigValue}");
                    return;
                }

                var configTypeObj = typeof(MMALCameraConfig);
                var property =
                    configTypeObj.GetProperty(changeRequest.ConfigType, BindingFlags.Static | BindingFlags.Public);

                if (property == null)
                {
                    _logger.LogError("Could not parse PropertyInfo from config change request.");
                    return;
                }

                try
                {
                    if (property.PropertyType.IsEnum)
                    {
                        _logger.LogInformation("Config request is enum type.");

                        if (Enum.TryParse(property.PropertyType, changeRequest.ConfigValue, out object enumFromString))
                        {
                            property.SetValue(property.GetValue(changeRequest.ConfigType), enumFromString);
                        }
                    }
                    else if (property.PropertyType == typeof(int) || property.PropertyType == typeof(double))
                    {
                        _logger.LogInformation("Config request is numeric type.");

                        if (int.TryParse(changeRequest.ConfigValue, out int intFromString))
                        {
                            property.SetValue(property.GetValue(changeRequest.ConfigType), intFromString);
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Could not set global config from config change request.");
                    return;
                }
            }
        }

        public void Dispose()
        {
            _logger.LogInformation("Disposing and cleaning up native resources.");
            _applicationTokenSource?.Dispose();
            _captureHandler?.Dispose();
            _cameraTokenSource?.Dispose();
            _client?.Dispose();
            _cam.Cleanup();
        }
    }
}
