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
            if (args.Length == 2 && args[0] == "--autostart")
            {
                if (args[1] != "enable" && args[1] != "disable") return 2;
                try {
                    StartupRegistration.SetEnabled(Application.ExecutablePath, args[1] == "enable");
                    return 0;
                }
                catch (Exception error) { StartupLog.Write("Startup registration error: " + error); return 1; }
            }
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
            bool fromStartup = args.Length == 1 && args[0] == "--startup";
            StartupLog.Write("Process started (" + (fromStartup ? "login task" : "manual") + "), v" + Application.ProductVersion + ".");
            using (Mutex mutex = new Mutex(true, @"Local\MagicTrackpadBattery.Tray.v1", out first))
            {
                if (!first) { StartupLog.Write("Existing instance detected; duplicate exited."); return 0; }
                Application.EnableVisualStyles();
                Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);
                Application.SetCompatibleTextRenderingDefault(false);
                string status = args.Length == 2 && args[0] == "--status-file" ? args[1] : null;
                try {
                    try { StartupRegistration.MigrateLegacy(Application.ExecutablePath); }
                    catch (Exception error) { StartupLog.Write("Legacy startup migration error: " + error); }
                    BatteryTray context = new BatteryTray(status);
                    StartupLog.Write("Tray initialized successfully.");
                    Application.Run(context);
                    StartupLog.Write("Application exited normally.");
                }
                catch (Exception error) {
                    StartupLog.Write("Application error: " + error);
                    if (!fromStartup) MessageBox.Show(error.Message, "Magic Trackpad 电量", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return 1;
                }
                finally { mutex.ReleaseMutex(); }
            }
            return 0;
        }
    }
}
