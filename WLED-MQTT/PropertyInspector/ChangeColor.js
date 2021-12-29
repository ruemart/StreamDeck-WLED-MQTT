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
    setAutomaticSettings("none");

    if (payload['mode'] == 1) {
        setAutomaticSettings("");
    }

    setCertificateSettings("none");
    setWebSocketSettings("none");

    if (payload['connectionType'] == 2) {
        setCertificateSettings("");
    }
    if (payload['connectionType'] == 3) {
        setWebSocketSettings("");
    }
}

function setAutomaticSettings(displayValue) {
    var dvAutomaticSettings = document.getElementById('dvAutomaticSettings');
    dvAutomaticSettings.style.display = displayValue;
}
function setCertificateSettings(displayValue) {
    var dvCertificateSettings = document.getElementById('dvCertificateSettings');
    dvCertificateSettings.style.display = displayValue;
}
function setWebSocketSettings(displayValue) {
    var dvWebSocketSettings = document.getElementById('dvWebSocketSettings');
    dvWebSocketSettings.style.display = displayValue;
}