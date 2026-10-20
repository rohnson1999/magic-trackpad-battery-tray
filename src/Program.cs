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
    internal static class Program
    {
        [STAThread]
        private static int Main(string[] args)
        {
            if (args.Length == 2 && args[0] == "--read-once")
            {
                Reading reading = HidBattery.Read();
                File.WriteAllText(args[1], reading == null ? "unavailable" :
                    "percent=" + reading.Percent + ";charging=" + reading.Charging, Encoding.UTF8);
                return reading == null ? 2 : 0;
            }
            if (args.Length == 2 && args[0] == "--preview")
            {
                using (Bitmap preview = TrayArt.Draw(47, false, 128)) preview.Save(args[1]);
                return 0;
            }
            bool first;
            using (Mutex mutex = new Mutex(true, @"Local\MagicTrackpadBattery.Tray.v1", out first))
            {
                if (!first) return 0;
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                string status = args.Length == 2 && args[0] == "--status-file" ? args[1] : null;
                try { Application.Run(new BatteryTray(status)); }
                catch (Exception error) { MessageBox.Show(error.Message, "Magic Trackpad 电量", MessageBoxButtons.OK, MessageBoxIcon.Error); return 1; }
                finally { mutex.ReleaseMutex(); }
            }
            return 0;
        }
    }
}
