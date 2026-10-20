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
    internal static class LogicTests
    {
        private static void Require(bool condition, string name)
        {
            if (!condition) throw new InvalidOperationException("FAILED: " + name);
        }

        private static int Main(string[] args)
        {
            string output = args.Length == 1 ? args[0] : "test-results.txt";
            return Run(output);
        }

        private static int Run(string output)
        {
            try {
                Require(Reading.Parse(new byte[] { 0x90, 0, 47 }, "test").Percent == 47, "real report decoding");
                Require(Reading.Parse(new byte[] { 0x90, 3, 99 }, "test").Charging, "charging flags");
                Require(Reading.Parse(new byte[] { 0x90, 0, 0 }, "test") == null, "unknown zero payload");
                Require(Reading.Parse(new byte[] { 0x90, 0, 255 }, "test") == null, "invalid capacity");
                Require(Reading.Parse(new byte[] { 0x80, 0, 47 }, "test") == null, "wrong report ID");
                Require(Reading.Parse(new byte[] { 0x90 }, "test") == null, "short report");
                LowBatteryPolicy policy = new LowBatteryPolicy();
                Require(!policy.Observe(new Reading { Percent = 10 }), "exactly 10 percent stays quiet");
                Require(policy.Observe(new Reading { Percent = 9 }), "below 10 warns");
                Require(!policy.Observe(new Reading { Percent = 8 }), "no repeated warning");
                Require(!policy.Observe(null) && policy.Warned, "disconnect preserves warning latch");
                Require(!policy.Observe(new Reading { Percent = 8, Charging = true }) && !policy.Warned, "charging suppresses and resets");
                Require(policy.Observe(new Reading { Percent = 8 }), "unplugged while low warns again");
                Require(!policy.Observe(new Reading { Percent = 20 }) && !policy.Warned, "recovery resets");
                policy.Warned = true;
                Require(!policy.Observe(new Reading { Percent = 9 }), "persisted latch suppresses restart duplicate");
                Require(Marshal.SizeOf(typeof(IntPtr)) == 8, "x64 build");
                File.WriteAllText(output, "PASS: battery report validation, threshold, charge, disconnect, recovery and restart deduplication.", Encoding.UTF8);
                return 0;
            }
            catch (Exception error) { File.WriteAllText(output, error.ToString(), Encoding.UTF8); return 1; }
        }
    }
}
