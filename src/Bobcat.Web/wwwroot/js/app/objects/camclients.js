function CamClient(id, connectionId, hostname, type) {
    this.id = id;
    this.connectionId = connectionId;
    this.hostname = hostname;
    this.type = type;
    this.player = null;
    this.selected = ko.observable(false);
}