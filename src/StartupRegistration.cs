using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Xml;
using Microsoft.Win32;

namespace MagicTrackpadBattery
{
    internal static class StartupRegistration
    {
        private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string ValueName = "MagicTrackpadBattery";
        private const string TaskNamespace = "http://schemas.microsoft.com/windows/2004/02/mit/task";
        private static string CurrentSid { get { return WindowsIdentity.GetCurrent().User.Value; } }

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

        internal static string BuildTaskXml(string executablePath, string userSid)
        {
            BuildCommand(executablePath);
            if (string.IsNullOrEmpty(userSid)) throw new ArgumentException("A user SID is required.", "userSid");
            StringBuilder xml = new StringBuilder();
            using (XmlWriter writer = XmlWriter.Create(xml, new XmlWriterSettings { OmitXmlDeclaration = true }))
            {
                writer.WriteStartElement("Task", TaskNamespace); writer.WriteAttributeString("version", "1.2");
                writer.WriteStartElement("RegistrationInfo");
                writer.WriteElementString("Description", "Start the Magic Trackpad battery tray after the current user signs in.");
                writer.WriteEndElement();
                writer.WriteStartElement("Triggers"); writer.WriteStartElement("LogonTrigger");
                writer.WriteElementString("Enabled", "true"); writer.WriteElementString("UserId", userSid);
                writer.WriteElementString("Delay", "PT15S");
                writer.WriteEndElement(); writer.WriteEndElement();
                writer.WriteStartElement("Principals"); writer.WriteStartElement("Principal");
                writer.WriteAttributeString("id", "CurrentUser"); writer.WriteElementString("UserId", userSid);
                writer.WriteElementString("LogonType", "InteractiveToken"); writer.WriteElementString("RunLevel", "LeastPrivilege");
                writer.WriteEndElement(); writer.WriteEndElement();
                writer.WriteStartElement("Settings");
                writer.WriteElementString("MultipleInstancesPolicy", "IgnoreNew");
                writer.WriteElementString("DisallowStartIfOnBatteries", "false");
                writer.WriteElementString("StopIfGoingOnBatteries", "false");
                writer.WriteElementString("StartWhenAvailable", "true");
                writer.WriteElementString("AllowStartOnDemand", "true"); writer.WriteElementString("Enabled", "true");
                writer.WriteElementString("ExecutionTimeLimit", "PT0S");
                writer.WriteStartElement("RestartOnFailure");
                writer.WriteElementString("Interval", "PT1M"); writer.WriteElementString("Count", "3");
                writer.WriteEndElement(); writer.WriteEndElement();
                writer.WriteStartElement("Actions"); writer.WriteAttributeString("Context", "CurrentUser");
                writer.WriteStartElement("Exec"); writer.WriteElementString("Command", Path.GetFullPath(executablePath));
                writer.WriteElementString("Arguments", "--startup");
                writer.WriteElementString("WorkingDirectory", Path.GetDirectoryName(Path.GetFullPath(executablePath)));
                writer.WriteEndElement(); writer.WriteEndElement(); writer.WriteEndElement();
            }
            return xml.ToString();
        }

        internal static bool MatchesTaskXml(string xml, string executablePath, string userSid)
        {
            XmlDocument document = new XmlDocument { XmlResolver = null };
            document.LoadXml(xml);
            XmlNamespaceManager ns = new XmlNamespaceManager(document.NameTable); ns.AddNamespace("t", TaskNamespace);
            XmlNode command = document.SelectSingleNode("/t:Task/t:Actions/t:Exec/t:Command", ns);
            XmlNode arguments = document.SelectSingleNode("/t:Task/t:Actions/t:Exec/t:Arguments", ns);
            XmlNode user = document.SelectSingleNode("/t:Task/t:Triggers/t:LogonTrigger/t:UserId", ns);
            XmlNode triggerEnabled = document.SelectSingleNode("/t:Task/t:Triggers/t:LogonTrigger/t:Enabled", ns);
            return command != null && arguments != null && user != null &&
                string.Equals(command.InnerText, Path.GetFullPath(executablePath), StringComparison.OrdinalIgnoreCase) &&
                arguments.InnerText == "--startup" && MatchesUser(user.InnerText, userSid) &&
                (triggerEnabled == null || triggerEnabled.InnerText == "true");
        }

        private static bool MatchesUser(string registeredUser, string userSid)
        {
            if (registeredUser == userSid) return true;
            if (registeredUser.StartsWith("S-1-", StringComparison.OrdinalIgnoreCase)) return false;
            // Task Scheduler normalizes a trigger SID to DOMAIN\user on some systems.
            try { return ((SecurityIdentifier)new NTAccount(registeredUser).Translate(typeof(SecurityIdentifier))).Value == userSid; }
            catch (IdentityNotMappedException) { return false; }
            catch (ArgumentException) { return false; }
        }

        private static void Release(object value)
        {
            if (value != null && Marshal.IsComObject(value)) Marshal.FinalReleaseComObject(value);
        }

        private static object FindTask(dynamic folder)
        {
            try { return folder.GetTask(ValueName); }
            catch (COMException error) {
                uint code = unchecked((uint)error.ErrorCode);
                if (code == 0x80070002 || code == 0x8004130F) return null;
                throw;
            }
        }

        public static bool IsEnabled(string executablePath)
        {
            dynamic service = null, folder = null, task = null;
            try {
                service = Activator.CreateInstance(Type.GetTypeFromProgID("Schedule.Service", true));
                service.Connect(); folder = service.GetFolder(@"\"); task = FindTask(folder);
                return task != null && (bool)task.Enabled && MatchesTaskXml((string)task.Xml, executablePath, CurrentSid);
            }
            finally { Release(task); Release(folder); Release(service); }
        }

        private static bool LegacyEnabled(string executablePath)
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RunKey, false))
                return key != null && MatchesCommand(key.GetValue(ValueName) as string, executablePath);
        }

        private static void RemoveLegacy()
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RunKey, true))
                if (key != null) key.DeleteValue(ValueName, false);
        }

        public static void MigrateLegacy(string executablePath)
        {
            if (LegacyEnabled(executablePath)) SetEnabled(executablePath, true);
        }

        public static void SetEnabled(string executablePath, bool enabled)
        {
            BuildCommand(executablePath);
            if (enabled && !File.Exists(executablePath)) throw new FileNotFoundException("Executable not found.", executablePath);
            dynamic service = null, folder = null, task = null;
            try {
                service = Activator.CreateInstance(Type.GetTypeFromProgID("Schedule.Service", true));
                service.Connect(); folder = service.GetFolder(@"\");
                if (enabled) task = folder.RegisterTask(ValueName, BuildTaskXml(executablePath, CurrentSid), 6, CurrentSid, null, 3, null);
                else {
                    task = FindTask(folder);
                    if (task != null) folder.DeleteTask(ValueName, 0);
                }
            }
            finally { Release(task); Release(folder); Release(service); }
            if (IsEnabled(executablePath) != enabled) throw new IOException("自启动计划任务保存失败。");
            RemoveLegacy();
            StartupLog.Write(enabled ? "Login task enabled (15 second delay)." : "Login task disabled.");
        }
    }
}
