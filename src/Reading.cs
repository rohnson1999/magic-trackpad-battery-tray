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
    internal sealed class Reading
    {
        public int Percent;
        public bool Charging;
        public bool Full;
        public string Device;

        public static Reading Parse(byte[] report, string device)
        {
            if (report == null || report.Length < 3 || report[0] != 0x90 || report[2] > 100)
                return null;
            if (report[1] == 0 && report[2] == 0) return null;
            return new Reading { Percent = report[2], Charging = (report[1] & 2) != 0,
                Full = (report[1] & 4) != 0, Device = device };
        }
    }
}
