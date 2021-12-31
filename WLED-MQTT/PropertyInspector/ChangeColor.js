document.addEventListener('websocketCreate', function () {
    console.log("Websocket created!");
    showHideSettings(actionInfo.payload.settings);
    var minusButton = document.getElementById('removeColor');
    minusButton.disabled = true;

    websocket.addEventListener('message', function (event) {
        console.log("Got message event!");

        // Received message from Stream Deck
        var jsonObj = JSON.parse(event.data);

        if (jsonObj.event === 'sendToPropertyInspector') {
            console.log("sendToPropertyInspector");
            var payload = jsonObj.payload;
            showHideSettings(payload);
            restoreColors();
        }
        else if (jsonObj.event === 'didReceiveSettings') {
            console.log("didReceiveSettings");
            var payload = jsonObj.payload;
            showHideSettings(payload.settings);
            restoreColors();
        }
    });
});

function showHideSettings(payload) {
    console.log("Show Hide Settings Called");
    setAutomaticSettings("none");
    if (payload['mode'] == 1) {
        setAutomaticSettings("");
    }

    setWebSocketSettings("none");
    var dropdown = document.getElementById('connectionType');
    if (dropdown.value == 2) {
        setWebSocketSettings("");
    }
}

function setAutomaticSettings(displayValue) {
    var dvAutomaticSettings = document.getElementById('dvAutomaticSettings');
    dvAutomaticSettings.style.display = displayValue;
}
function setWebSocketSettings(displayValue) {
    var dvWebSocketSettings = document.getElementById('dvWebSocketSettings');
    dvWebSocketSettings.style.display = displayValue;
}

function restoreColors() {
    console.log("restoreColors");
    var colorGrid = document.getElementById('colorGrid');
    if (colorGrid.childElementCount > 0) {
        return;
    }

    var colorsToRestore = document.getElementById('colors').value.split(',');

    if (colorsToRestore.length == 0) {
        addColor(); // add random default color.
    }
    else {
        for (var i = 0; i < colorsToRestore.length; ++i) {
            var newColorItem = document.createElement('input');
            newColorItem.type = 'color';
            newColorItem.value = colorsToRestore[i];
            newColorItem.addEventListener('input', updateColorArray);
            colorGrid.appendChild(newColorItem);
        }
    }
}

function addColor() {
    var colorGrid = document.getElementById('colorGrid');
    var newColorItem = document.createElement('input');
    newColorItem.type = 'color';
    newColorItem.value = '#' + ((1 << 24) * Math.random() | 0).toString(16); // set a random color
    newColorItem.addEventListener('input', updateColorArray);
    colorGrid.appendChild(newColorItem);

    if (colorGrid.childElementCount > 1) {
        var minusButton = document.getElementById('removeColor');
        minusButton.disabled = false;
    }
    updateColorArray();
}

function removeColor() {
    var colorGrid = document.getElementById('colorGrid');
    colorGrid.removeChild(colorGrid.lastChild);

    if (colorGrid.childElementCount <= 1) {
        var minusButton = document.getElementById('removeColor');
        minusButton.disabled = true;
    }
    updateColorArray();
}

function updateColorArray() {
    var colorGrid = document.getElementById('colorGrid');
    var colorsElements = colorGrid.children;
    var colorValues = [];
    for (var i = 0; i < colorsElements.length; ++i) {
        colorValues.push(colorsElements[i].value);
    }

    var colorContainer = document.getElementById('colors');
    colorContainer.value = colorValues;
    setSettings();
}