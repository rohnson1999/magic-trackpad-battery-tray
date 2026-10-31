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
                string pathWithSpaces = @"C:\Program Files\Magic Trackpad\MagicTrackpadBattery.exe";
                Require(StartupRegistration.BuildCommand(pathWithSpaces) == "\"" + pathWithSpaces + "\"", "quote startup path with spaces");
                Require(StartupRegistration.MatchesCommand("\"" + pathWithSpaces.ToLowerInvariant() + "\"", pathWithSpaces), "startup path case-insensitive match");
                Require(!StartupRegistration.MatchesCommand(null, pathWithSpaces), "missing startup registration");
                Require(!StartupRegistration.MatchesCommand("\"C:\\Old\\MagicTrackpadBattery.exe\"", pathWithSpaces), "stale location does not match");
                bool rejectedRelativePath = false;
                try { StartupRegistration.BuildCommand("MagicTrackpadBattery.exe"); }
                catch (ArgumentException) { rejectedRelativePath = true; }
                Require(rejectedRelativePath, "reject relative startup path");
                string sid = "S-1-5-21-111-222-333-1001";
                string taskXml = StartupRegistration.BuildTaskXml(pathWithSpaces, sid);
                Require(StartupRegistration.MatchesTaskXml(taskXml, pathWithSpaces, sid), "scheduled task round trip");
                Require(!StartupRegistration.MatchesTaskXml(taskXml, pathWithSpaces, sid + "0"), "task belongs to current user");
                Require(!StartupRegistration.MatchesTaskXml(taskXml.Replace("<Enabled>true</Enabled>", "<Enabled>false</Enabled>"), pathWithSpaces, sid), "disabled login trigger rejected");
                string escapedPath = @"C:\Apps\Battery & Tools\MagicTrackpadBattery.exe";
                Require(StartupRegistration.MatchesTaskXml(StartupRegistration.BuildTaskXml(escapedPath, sid), escapedPath, sid), "task XML escapes special characters");
                Require(taskXml.Contains("PT15S") && taskXml.Contains("InteractiveToken") && taskXml.Contains("LeastPrivilege"), "interactive unelevated delayed startup");
                Require(taskXml.Contains("<ExecutionTimeLimit>PT0S</ExecutionTimeLimit>") && taskXml.Contains("<DisallowStartIfOnBatteries>false</DisallowStartIfOnBatteries>"), "task can run continuously on battery");
                System.Security.Principal.WindowsIdentity identity = System.Security.Principal.WindowsIdentity.GetCurrent();
                string normalizedXml = StartupRegistration.BuildTaskXml(pathWithSpaces, identity.User.Value).Replace(identity.User.Value, identity.Name);
                Require(StartupRegistration.MatchesTaskXml(normalizedXml, pathWithSpaces, identity.User.Value), "Windows-normalized account name maps back to SID");
                File.WriteAllText(output, "PASS: battery reports, low-battery policy, restart deduplication and startup command validation.", Encoding.UTF8);
                return 0;
            }
            catch (Exception error) { File.WriteAllText(output, error.ToString(), Encoding.UTF8); return 1; }
        }
    }
}
