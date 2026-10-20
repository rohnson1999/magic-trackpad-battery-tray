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
    internal static class HidBattery
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct RawDevice { public IntPtr Handle; public uint Type; }
        [StructLayout(LayoutKind.Sequential)]
        private struct Caps
        {
            public ushort Usage, Page, InputLength, OutputLength, FeatureLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)] public ushort[] Reserved;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)] public ushort[] Counts;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetRawInputDeviceList([Out] RawDevice[] list, ref uint count, uint size);
        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern uint GetRawInputDeviceInfoW(IntPtr device, uint command, StringBuilder data, ref uint size);
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern SafeFileHandle CreateFileW(string name, uint access, uint share, IntPtr security,
            uint creation, uint flags, IntPtr template);
        [DllImport("hid.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool HidD_GetPreparsedData(SafeFileHandle device, out IntPtr data);
        [DllImport("hid.dll")]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool HidD_FreePreparsedData(IntPtr data);
        [DllImport("hid.dll")]
        private static extern int HidP_GetCaps(IntPtr data, out Caps caps);
        [DllImport("hid.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool HidD_GetInputReport(SafeFileHandle device, [In, Out] byte[] report, uint length);

        private static List<string> FindPaths()
        {
            uint elementSize = (uint)Marshal.SizeOf(typeof(RawDevice));
            for (int retry = 0; retry < 3; retry++)
            {
                uint count = 0;
                if (GetRawInputDeviceList(null, ref count, elementSize) == uint.MaxValue)
                    throw new System.ComponentModel.Win32Exception();
                if (count == 0) return new List<string>();
                RawDevice[] list = new RawDevice[count];
                uint found = GetRawInputDeviceList(list, ref count, elementSize);
                if (found == uint.MaxValue) continue;
                List<string> result = new List<string>();
                for (int i = 0; i < found; i++)
                {
                    if (list[i].Type != 2) continue;
                    uint length = 0;
                    if (GetRawInputDeviceInfoW(list[i].Handle, 0x20000007, null, ref length) == uint.MaxValue) continue;
                    StringBuilder name = new StringBuilder((int)length + 1);
                    if (GetRawInputDeviceInfoW(list[i].Handle, 0x20000007, name, ref length) == uint.MaxValue) continue;
                    string path = name.ToString();
                    string lower = path.ToLowerInvariant();
                    if ((lower.Contains("pid&0265") || lower.Contains("pid_0265")) &&
                        (lower.Contains("vid&0001004c") || lower.Contains("vid_05ac") || lower.Contains("vid&05ac")))
                        result.Add(path);
                }
                result.Sort(delegate(string a, string b) {
                    int ap = a.ToLowerInvariant().Contains("col02#") ? 0 : 1;
                    int bp = b.ToLowerInvariant().Contains("col02#") ? 0 : 1;
                    return ap != bp ? ap.CompareTo(bp) : string.CompareOrdinal(a, b);
                });
                return result;
            }
            throw new IOException("设备列表正在变化，请稍后重试。");
        }

        public static Reading Read()
        {
            foreach (string path in FindPaths())
            {
                foreach (uint access in new uint[] { 0, 0x80000000 })
                {
                    using (SafeFileHandle handle = CreateFileW(path, access, 3, IntPtr.Zero, 3, 0, IntPtr.Zero))
                    {
                        if (handle.IsInvalid) continue;
                        IntPtr pp;
                        if (!HidD_GetPreparsedData(handle, out pp)) continue;
                        Caps caps;
                        try { if (HidP_GetCaps(pp, out caps) < 0) continue; }
                        finally { HidD_FreePreparsedData(pp); }
                        if (caps.InputLength < 3) continue;
                        byte[] buffer = new byte[caps.InputLength];
                        buffer[0] = 0x90;
                        if (!HidD_GetInputReport(handle, buffer, (uint)buffer.Length)) continue;
                        Reading reading = Reading.Parse(buffer, path);
                        if (reading != null) return reading;
                    }
                }
            }
            return null;
        }
    }
}
