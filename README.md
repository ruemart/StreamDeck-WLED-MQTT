# ![CategoryIcon@2x](https://user-images.githubusercontent.com/18423789/148647624-630662d6-84b3-4952-bc08-2b3feda6895e.png) StreamDeck-WLED-MQTT Plugin
A StreamDeck plugin to control a WLED light strip via MQTT.

## Actions
The plugin contains 3 actions:

### ![ColorAction@2x](https://user-images.githubusercontent.com/18423789/148647538-dac4e1cd-7f8b-4c7b-9257-618066c20cc9.png) Change Color
Allows to setup a list of colors (use refresh button if no colors appear). 
This action has two modes:
- Manual: Each key press switches to the next color from the color list. If the last color is reached the next key press goes back to the beginning of the list.
- Automatic: The colors automatically change after a given time. A key press starts and stops the automatic mode.

### ![EffectAction@2x](https://user-images.githubusercontent.com/18423789/148647544-d571d19a-0d88-40c9-9295-f8cb170e10ae.png) Change Effect
Allows to activate an effect by pressing the key. The PropertyInspector has a list of all supported WLED effects where one can be selected.

### ![ToggleAction@2x](https://user-images.githubusercontent.com/18423789/148647552-523c7897-45fc-4b39-aacc-f42ffcdc6c69.png) Toggle Light
Switches the WLED light On/Off on key press.

## MQTT Setup
Each action also contains some MQTT setup fields in the PropertyInspector:

### Connection Type
There are three supported connection types:
- TCP
- SecureTCP (with TLS)
- Websockets (An input field appears where you can enter the web socket server adress)

### Host, Port, Topic and Timeout
You can leave all input fields with their default values if the MQTT broker is running on the same computer your StreamDeck is connected to. Otherwise you need to 
change Host by entering the IP (and maybe Port) to fit your setup.

If you have multiple WLED lights and only want to control one specific light with this plugin, you need to change the topic to wled/{device_id}

### Credentials and ClientId
If your MQTT broker user authentification via user name and password, you can enter both here.

## MQTT Status
Upon assigning an action to a key or by changing MQTT sttings the client will automatically try to reconnect to the broker with the new configuration.
The status of this current connection is visualized in the background image of the respective StreamDeck key:

![ToggleOffline@2x](https://user-images.githubusercontent.com/18423789/148647563-14548b46-3ae1-40d5-bf14-11009b9b0d30.png)

The client is not connected, yet.


![ToggleConnecting@2x](https://user-images.githubusercontent.com/18423789/148647566-6960d43b-8449-4923-a0b2-52a6cba9fd38.png)

The client is currently connecting to the broker.


![ToggleOnline@2x](https://user-images.githubusercontent.com/18423789/148647565-d5648b1e-1988-470b-881b-c408a17198cc.png)

The client is connected to the broker.


![ToggleError@2x](https://user-images.githubusercontent.com/18423789/148647561-53dd79cc-5c0b-433c-9100-fde8e1f0616d.png)

The client could not connect to the broker.
