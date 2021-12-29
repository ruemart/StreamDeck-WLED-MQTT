using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WLED_MQTT.Mqtt
{
    public enum ConnectionType
    {
        TCP = 0,

        SecureTCP = 1,

        SecureTCPWithCertificate = 2,

        WebSockets = 3,
    }
}
