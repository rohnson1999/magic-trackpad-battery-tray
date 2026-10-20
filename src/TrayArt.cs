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
    internal static class TrayArt
    {
        [DllImport("user32.dll")] private static extern bool DestroyIcon(IntPtr icon);

        public static Bitmap Draw(int? percent, bool charging, int size)
        {
            Bitmap bitmap = new Bitmap(size, size);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.ScaleTransform(size / 32f, size / 32f);
                Color color = !percent.HasValue ? Color.FromArgb(104, 113, 125) :
                    percent < 10 && !charging ? Color.FromArgb(194, 53, 59) :
                    charging ? Color.FromArgb(31, 110, 191) : Color.FromArgb(25, 130, 94);
                using (SolidBrush brush = new SolidBrush(color))
                using (GraphicsPath shape = new GraphicsPath())
                {
                    shape.AddArc(1, 4, 10, 10, 180, 90);
                    shape.AddArc(21, 4, 10, 10, 270, 90);
                    shape.AddArc(21, 21, 10, 10, 0, 90);
                    shape.AddArc(1, 21, 10, 10, 90, 90);
                    shape.CloseFigure();
                    g.FillPath(brush, shape);
                    g.FillRectangle(brush, 11, 0, 10, 5);
                }
                string text = percent.HasValue ? percent.Value.ToString() : "?";
                using (Font font = new Font("Segoe UI", text.Length == 3 ? 13 : 17, FontStyle.Bold, GraphicsUnit.Pixel))
                using (StringFormat format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                    g.DrawString(text, font, Brushes.White, new RectangleF(0, 4, 32, 27), format);
            }
            return bitmap;
        }

        public static Icon Create(int? percent, bool charging)
        {
            using (Bitmap bitmap = Draw(percent, charging, 32))
            {
                IntPtr handle = bitmap.GetHicon();
                try { using (Icon borrowed = Icon.FromHandle(handle)) return (Icon)borrowed.Clone(); }
                finally { DestroyIcon(handle); }
            }
        }
    }
}
