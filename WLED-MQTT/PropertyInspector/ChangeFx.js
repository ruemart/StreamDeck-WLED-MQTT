document.addEventListener('websocketCreate', function () {
    console.log("Websocket created!");
    showHideSettings(actionInfo.payload.settings);

    websocket.addEventListener('message', function (event) {
        console.log("Got message event!");

        // Received message from Stream Deck
        var jsonObj = JSON.parse(event.data);

        if (jsonObj.event === 'sendToPropertyInspector') {
            var payload = jsonObj.payload;
            showHideSettings(payload);
        }
        else if (jsonObj.event === 'didReceiveSettings') {
            var payload = jsonObj.payload;
            showHideSettings(payload.settings);
        }
    });
});

function tryConnection() {
    var payload = {};
    payload.property_inspector = 'tryConnection';
    sendPayloadToPlugin(payload);
}

function showHideSettings(payload) {
    console.log("Show Hide Settings Called");
    setWebSocketSettings("none");

    var dropdown = document.getElementById('connectionType');
    if (dropdown.value == 2) {
        setWebSocketSettings("");
    }
}

function setWebSocketSettings(displayValue) {
    var dvWebSocketSettings = document.getElementById('dvWebSocketSettings');
    dvWebSocketSettings.style.display = displayValue;
}