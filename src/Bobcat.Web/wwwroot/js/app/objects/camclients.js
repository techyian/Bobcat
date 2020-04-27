function CamClient(id, connectionId, hostname, type, currentConfig) {
    this.id = id;
    this.connectionId = connectionId;
    this.hostname = hostname;
    this.type = type;
    this.currentConfig = currentConfig;
    this.player = null;
    this.selected = ko.observable(false);
}