using System;
using System.IO;
using System.Text;

namespace MagicTrackpadBattery
{
    internal static class StartupLog
    {
        private static readonly object Gate = new object();
        public static void Write(string message)
        {
            try {
                lock (Gate) {
                    string directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MagicTrackpadBattery");
                    Directory.CreateDirectory(directory);
                    string path = Path.Combine(directory, "startup.log");
                    if (File.Exists(path) && new FileInfo(path).Length > 65536) {
                        File.Copy(path, Path.Combine(directory, "startup.previous.log"), true);
                        File.WriteAllText(path, "", Encoding.UTF8);
                    }
                    File.AppendAllText(path, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + message + Environment.NewLine, Encoding.UTF8);
                }
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }
    }
}
