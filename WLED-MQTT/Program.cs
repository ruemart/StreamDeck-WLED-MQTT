﻿using BarRaider.SdTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WLED_MQTT
{
    class Program
    {
        static void Main(string[] args)
        {
            // Uncomment this line of code to allow for debugging
            //while (!System.Diagnostics.Debugger.IsAttached) { System.Threading.Thread.Sleep(100); }

            SDWrapper.Run(args);
        }
    }
}
