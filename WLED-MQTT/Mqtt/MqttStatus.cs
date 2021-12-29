using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WLED_MQTT.Mqtt
{
    public enum MqttStatus
    {
        NotRunning,
        Connecting,
        Faulty,
        Running,
    }
}
