function ClientViewModel() {

    let self = this;

    self.camClients = ko.observableArray([]);
    self.selectedClients = ko.observableArray([]);

    self.getProviders = function() {
        $.ajax({
            type: 'GET',
            url: '/ClientApi/GetProviders',
            success: function (data) {
                if (data && data.length > 0) {
                    self.camClients([]);

                    for (let i = 0; i < data.length; i++) {
                        self.camClients.push(new CamClient(data[i].id, data[i].connectionId, data[i].hostname, 'Provider'));
                    }
                }
            },
            error: function (jqXHR, status, errorThrown) {
                toastr.error(errorThrown, 'Error');
            }
        });
    };

    self.createConnection = function() {

    };

    self.saveProviders = function() {
        self.selectedClients([]);

        ko.utils.arrayForEach(self.camClients(), function(client) {
            if (client.selected()) {
                self.selectedClients.push(client);
            }
        });
    };

    self.setCameraConfig = function(camClient, event, configType, configSubtype) {
        if (camClient.player) {
            var cameraConfig = new CameraConfig(configType, configSubtype);
            var json = JSON.stringify(cameraConfig);

            camClient.player.source.socket.send('__config__' + json);
        }
    }

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

            if (currentObject.player) {
                currentObject.player.source.destroy();
            } 

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

                // Ping server every 5 seconds.
                interval = setInterval(ping, 5000);
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
                    handleReconnect();
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

