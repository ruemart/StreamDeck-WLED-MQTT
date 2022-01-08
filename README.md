# StreamDeck-WLED-MQTT
A StreamDeck plugin to control a WLED light strip via MQTT.

## Actions
The plugin contains 3 actions:

### Change Color
Allows to setup a list of colors (use refresh button if no colors appear). 
This action has two modes:
- Manual: Each key press switches to the next color from the color list. If the last color is reached the next key press goes back to the beginning of the list.
- Automatic: The colors automatically change after a given time. A key press starts and stops the automatic mode.

### Change Effect
Allows to activate an effect by pressing the key. The PropertyInspector has a list of all supported WLED effects where one can be selected.

### Toggle Light
Switches the WLED light On/Off on key press.
