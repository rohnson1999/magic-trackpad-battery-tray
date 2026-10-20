using System;
using System.IO;
using Microsoft.Win32;

namespace MagicTrackpadBattery
{
    internal static class StartupRegistration
    {
        private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string ValueName = "MagicTrackpadBattery";

        internal static string BuildCommand(string executablePath)
        {
            if (string.IsNullOrEmpty(executablePath) || !Path.IsPathRooted(executablePath) || executablePath.IndexOf('"') >= 0)
                throw new ArgumentException("An absolute executable path is required.", "executablePath");
            return "\"" + Path.GetFullPath(executablePath) + "\"";
        }

        internal static bool MatchesCommand(string registeredCommand, string executablePath)
        {
            return string.Equals(registeredCommand, BuildCommand(executablePath), StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsEnabled(string executablePath)
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RunKey, false))
                return key != null && MatchesCommand(key.GetValue(ValueName) as string, executablePath);
        }

        public static void SetEnabled(string executablePath, bool enabled)
        {
            string command = BuildCommand(executablePath);
            if (enabled && !File.Exists(executablePath)) throw new FileNotFoundException("Executable not found.", executablePath);
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(RunKey))
            {
                if (key == null) throw new IOException("无法打开当前用户的自启动设置。");
                if (enabled) key.SetValue(ValueName, command, RegistryValueKind.String);
                else key.DeleteValue(ValueName, false);
            }
            if (IsEnabled(executablePath) != enabled) throw new IOException("自启动设置保存失败。");
        }
    }
}
