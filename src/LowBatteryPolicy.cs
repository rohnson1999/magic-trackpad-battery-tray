using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32.SafeHandles;

namespace MagicTrackpadBattery
{
    internal sealed class LowBatteryPolicy
    {
        public bool Warned;
        public bool Observe(Reading reading)
        {
            if (reading == null) return false;
            if (reading.Percent >= 10 || reading.Charging || reading.Full)
            {
                Warned = false;
                return false;
            }
            if (Warned) return false;
            Warned = true;
            return true;
        }
    }
}
