function ClientViewModel() {

    let self = this;

    self.camClients = ko.observableArray([]);
    self.selectedClients = ko.observableArray([]);
    self.currentSelectedClient = null;

    // Bottom container observables.
    self.clientHostname = ko.observable();
    self.clientBrightness = ko.observable(50);
    self.clientSharpness = ko.observable(0);
    self.clientContrast = ko.observable(0);
    self.clientSaturation = ko.observable(0);
    
    self.clientImageFxOptions = ko.observableArray([
        { imageFxName: 'None', imageFxValue: 'MMAL_PARAM_IMAGEFX_NONE' },
        { imageFxName: 'Cartoon', imageFxValue: 'MMAL_PARAM_IMAGEFX_CARTOON' },
        { imageFxName: 'Colour Balance', imageFxValue: 'MMAL_PARAM_IMAGEFX_COLOURBALANCE' },
        { imageFxName: 'Colour Point', imageFxValue: 'MMAL_PARAM_IMAGEFX_COLOURPOINT' },
        { imageFxName: 'Colour Swap', imageFxValue: 'MMAL_PARAM_IMAGEFX_COLOURSWAP' },
        { imageFxName: 'Emboss', imageFxValue: 'MMAL_PARAM_IMAGEFX_EMBOSS' },
        { imageFxName: 'Film', imageFxValue: 'MMAL_PARAM_IMAGEFX_FILM' },
        { imageFxName: 'GPen', imageFxValue: 'MMAL_PARAM_IMAGEFX_GPEN' },
        { imageFxName: 'Hatch', imageFxValue: 'MMAL_PARAM_IMAGEFX_HATCH' },
        { imageFxName: 'Negative', imageFxValue: 'MMAL_PARAM_IMAGEFX_NEGATIVE' },
        { imageFxName: 'Oilpaint', imageFxValue: 'MMAL_PARAM_IMAGEFX_OILPAINT' },
        { imageFxName: 'Pastel', imageFxValue: 'MMAL_PARAM_IMAGEFX_PASTEL' },
        { imageFxName: 'Posterise', imageFxValue: 'MMAL_PARAM_IMAGEFX_POSTERISE' },
        { imageFxName: 'Sketch', imageFxValue: 'MMAL_PARAM_IMAGEFX_SKETCH' },
        { imageFxName: 'Solarize', imageFxValue: 'MMAL_PARAM_IMAGEFX_SOLARIZE' },
        { imageFxName: 'Washedout', imageFxValue: 'MMAL_PARAM_IMAGEFX_WASHEDOUT' }
    ]);
    self.selectedImageFx = ko.observable('MMAL_PARAM_IMAGEFX_NONE');

    self.clientExposureModeOptions = ko.observableArray([
        { exposureModeName: 'Off', exposureModeValue: 'MMAL_PARAM_EXPOSUREMODE_OFF' },
        { exposureModeName: 'Auto', exposureModeValue: 'MMAL_PARAM_EXPOSUREMODE_AUTO' },
        { exposureModeName: 'Night', exposureModeValue: 'MMAL_PARAM_EXPOSUREMODE_NIGHT' },
        { exposureModeName: 'Night Preview', exposureModeValue: 'MMAL_PARAM_EXPOSUREMODE_NIGHTPREVIEW' },
        { exposureModeName: 'Backlight', exposureModeValue: 'MMAL_PARAM_EXPOSUREMODE_BACKLIGHT' },
        { exposureModeName: 'Spotlight', exposureModeValue: 'MMAL_PARAM_EXPOSUREMODE_SPOTLIGHT' },
        { exposureModeName: 'Sports', exposureModeValue: 'MMAL_PARAM_EXPOSUREMODE_SPORTS' },
        { exposureModeName: 'Snow', exposureModeValue: 'MMAL_PARAM_EXPOSUREMODE_SNOW' },
        { exposureModeName: 'Beach', exposureModeValue: 'MMAL_PARAM_EXPOSUREMODE_BEACH' },
        { exposureModeName: 'Very Long', exposureModeValue: 'MMAL_PARAM_EXPOSUREMODE_VERYLONG' },
        { exposureModeName: 'Fixed FPS', exposureModeValue: 'MMAL_PARAM_EXPOSUREMODE_FIXEDFPS' },
        { exposureModeName: 'Anti-shake', exposureModeValue: 'MMAL_PARAM_EXPOSUREMODE_ANTISHAKE' },
        { exposureModeName: 'Fireworks', exposureModeValue: 'MMAL_PARAM_EXPOSUREMODE_FIREWORKS' }
    ]);
    self.selectedExposureMode = ko.observable('MMAL_PARAM_EXPOSUREMODE_AUTO');

    self.clientExposureMeterModeOptions = ko.observableArray([
        { exposureMeterModeName: 'Average', exposureMeterModeValue: 'MMAL_PARAM_EXPOSUREMETERINGMODE_AVERAGE' },
        { exposureMeterModeName: 'Spot', exposureMeterModeValue: 'MMAL_PARAM_EXPOSUREMETERINGMODE_SPOT' },
        { exposureMeterModeName: 'Backlit', exposureMeterModeValue: 'MMAL_PARAM_EXPOSUREMETERINGMODE_BACKLIT' },
        { exposureMeterModeName: 'Matrix', exposureMeterModeValue: 'MMAL_PARAM_EXPOSUREMETERINGMODE_MATRIX' }
    ]);
    self.selectedExposureMeterMode = ko.observable('MMAL_PARAM_EXPOSUREMETERINGMODE_AVERAGE');

    self.clientAwbModeOptions = ko.observableArray([
        { awbModeName: 'Off', awbModeValue: 'MMAL_PARAM_AWBMODE_OFF' },
        { awbModeName: 'Auto', awbModeValue: 'MMAL_PARAM_AWBMODE_AUTO' },
        { awbModeName: 'Sunlight', awbModeValue: 'MMAL_PARAM_AWBMODE_SUNLIGHT' },
        { awbModeName: 'Cloudy', awbModeValue: 'MMAL_PARAM_AWBMODE_CLOUDY' },
        { awbModeName: 'Shade', awbModeValue: 'MMAL_PARAM_AWBMODE_SHADE' },
        { awbModeName: 'Tungsten', awbModeValue: 'MMAL_PARAM_AWBMODE_TUNGSTEN' },
        { awbModeName: 'Fluorescent', awbModeValue: 'MMAL_PARAM_AWBMODE_FLUORESCENT' },
        { awbModeName: 'Incandescent', awbModeValue: 'MMAL_PARAM_AWBMODE_INCANDESCENT' },
        { awbModeName: 'Flash', awbModeValue: 'MMAL_PARAM_AWBMODE_FLASH' },
        { awbModeName: 'Horizon', awbModeValue: 'MMAL_PARAM_AWBMODE_HORIZON' },
        { awbModeName: 'Greyworld', awbModeValue: 'MMAL_PARAM_AWBMODE_GREYWORLD' }
    ]);
    self.selectedAwbMode = ko.observable('MMAL_PARAM_AWBMODE_AUTO');

    self.showClientConfigContainer = ko.observable(false);
    
    self.getProviders = function() {
        $.ajax({
            type: 'GET',
            url: '/ClientApi/GetProviders',
            success: function (data) {
                if (data && data.length > 0) {
                    self.camClients([]);

                    for (let i = 0; i < data.length; i++) {
                        self.camClients.push(new CamClient(data[i].id, data[i].connectionId, data[i].hostname, 'Provider', data[i].clientConfig));
                    }
                }
            },
            error: function (jqXHR, status, errorThrown) {
                toastr.error(errorThrown, 'Error');
            }
        });
    };
    
    self.saveProviders = function() {
        self.selectedClients([]);

        ko.utils.arrayForEach(self.camClients(), function(client) {
            if (client.selected()) {
                self.selectedClients.push(client);
            }
        });
    };

    self.setCameraConfig = function() {
        if (self.currentSelectedClient && self.currentSelectedClient.player) {

            var cameraConfigArr = [];

            cameraConfigArr.push(new CameraConfig('Brightness', self.clientBrightness().toString()));
            cameraConfigArr.push(new CameraConfig('Sharpness', self.clientSharpness().toString()));
            cameraConfigArr.push(new CameraConfig('Contrast', self.clientContrast().toString()));
            cameraConfigArr.push(new CameraConfig('Saturation', self.clientSaturation().toString()));
            cameraConfigArr.push(new CameraConfig('ImageFx', self.selectedImageFx()));
            cameraConfigArr.push(new CameraConfig('ExposureMode', self.selectedExposureMode()));
            cameraConfigArr.push(new CameraConfig('ExposureMeterMode', self.selectedExposureMeterMode()));
            cameraConfigArr.push(new CameraConfig('AwbMode', self.selectedAwbMode()));

            var json = JSON.stringify(cameraConfigArr);

            self.currentSelectedClient.player.source.socket.send('__config__' + json);
        }
    };

    self.showClientConfig = function (camClient) {
        for (let i = 0; i < camClient.currentConfig.length; i++) {
            let currentConfigItem = camClient.currentConfig[i];

            if (currentConfigItem.configType === 'Brightness') {
                self.clientBrightness(parseInt(currentConfigItem.configValue));
            }

            if (currentConfigItem.configType === 'Sharpness') {
                self.clientSharpness(parseInt(currentConfigItem.configValue));
            }

            if (currentConfigItem.configType === 'Contrast') {
                self.clientContrast(parseInt(currentConfigItem.configValue));
            }

            if (currentConfigItem.configType === 'Saturation') {
                self.clientSaturation(parseInt(currentConfigItem.configValue));
            }

            if (currentConfigItem.configType === 'ImageFx') {
                self.selectedImageFx(currentConfigItem.configValue);
            }
        }

        self.currentSelectedClient = camClient;
        self.showClientConfigContainer(true);
    };

    /*
     * ViewModel Init     
     */


    ko.bindingHandlers.customConnectionBinding = {
        update: function (element, valueAccessor, allBindings, viewModel, bindingContext) {
            // This will be called once when the binding is first applied to an element,
            // and again whenever any observables/computeds that are accessed change
            // Update the DOM element based on the supplied values here.

            let tm = null;
            let interval = null;
            let parent = bindingContext.$parent;
            let currentObject = bindingContext.$data;
            let numReconnectAttempts = 0;

            if (currentObject.player) {
                currentObject.player.source.destroy();
            } 
            
            element.style.width = '100%';
            element.style.height = '100%';
            element.width = element.offsetWidth;
            element.height = element.offsetHeight;

            let player = new JSMpeg.Player(`ws://localhost:44369/bobcat-${currentObject.connectionId}`,
                {
                    canvas: element
                });

            currentObject.player = player;

            player.source.onMessage = function (ev) {

                let isFirstChunk = !player.source.established;

                player.source.established = true;

                if (isFirstChunk && player.source.onEstablishedCallback) {
                    player.source.onEstablishedCallback(player.source);
                }

                if (this.destination) {
                    if (ev.data === '__pong__') {
                        pong();
                        return;
                    }

                    player.source.destination.write(ev.data);
                }
            }

            player.source.onOpen = function() {
                player.source.progress = 1;
                
                if (numReconnectAttempts < 5) {
                    // Ping server every 5 seconds.
                    if (interval > 0) {
                        clearInterval(interval);
                    }

                    interval = setInterval(ping, 5000);
                }
            }

            player.source.onClose = function() {
                handleReconnect();
            }

            player.source.onError = function () {
                handleReconnect();
            }

            function ping() {
                player.source.socket.send(`__ping__`);
                
                tm = setTimeout(function () {
                    // If we haven't received a reply in 5 seconds, attempt reconnect.
                    player.source.established = false;
                    
                    clearInterval(interval);

                    if (numReconnectAttempts < 5) {
                        handleReconnect();
                        numReconnectAttempts++;
                    }
                }, 5000);
            }

            function pong() {
                clearTimeout(tm);
            }

            function extractConnectionId() {
                let currentUrl = player.source.url;
                let cIdSplit = currentUrl.split('bobcat-');
                
                if (cIdSplit.length > 0) {
                    return cIdSplit[cIdSplit.length - 1];
                }

                return '';
            }

            function handleReconnect() {
                if (player.source.shouldAttemptReconnect) {
                    clearTimeout(player.source.reconnectTimeoutId);

                    // Retrieve updated list of providers.
                    parent.getProviders();

                    let newClients = parent.camClients();
                    let currentSubscriptions = parent.selectedClients();
                    let extractedConnectionId = extractConnectionId();
                    let foundSubscription = null;

                    if (extractedConnectionId !== '') {
                        // Check if the client we're subscribed to has a new connectionId.
                        ko.utils.arrayForEach(currentSubscriptions, function (subscription) {
                            // Extract the connectionId from the url.
                            if (subscription.connectionId === extractedConnectionId) {
                                foundSubscription = subscription;
                            }
                        });

                        if (foundSubscription) {
                            ko.utils.arrayForEach(newClients, function (newClient) {
                                if (newClient.id === foundSubscription.id) {

                                    foundSubscription.connectionId = newClient.connectionId;
                                    
                                    player.source.url = `ws://localhost:44369/bobcat-${newClient.connectionId}`;
                                }
                            });

                            player.source.start();
                        }
                    }
                }
            }
        }
    };

}

