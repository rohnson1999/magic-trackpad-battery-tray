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
    internal sealed class BatteryTray : ApplicationContext
    {
        private readonly NotifyIcon tray = new NotifyIcon();
        private readonly ContextMenuStrip menu = new ContextMenuStrip();
        private readonly ToolStripMenuItem batteryItem = new ToolStripMenuItem("正在读取电量…");
        private readonly ToolStripMenuItem timeItem = new ToolStripMenuItem("每 60 秒自动刷新");
        private readonly ToolStripMenuItem refreshItem = new ToolStripMenuItem("立即刷新");
        private readonly Control dispatcher = new Control();
        private readonly System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
        private readonly LowBatteryPolicy alerts = new LowBatteryPolicy();
        private readonly string statePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MagicTrackpadBattery", "low-battery-alert.txt");
        private readonly string statusPath;
        private Icon icon;
        private Task<Reading> pending;
        private DateTime nextPoll = DateTime.MinValue;
        private DateTime started;
        private bool timedOut;
        private bool exiting;
        private int? shownPercent;
        private bool shownCharging;
        private bool iconInitialized;

        public BatteryTray(string diagnostics)
        {
            statusPath = diagnostics;
            try { alerts.Warned = File.Exists(statePath) && File.ReadAllText(statePath).Trim() == "warned"; }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
            IntPtr handle = dispatcher.Handle;
            batteryItem.Enabled = timeItem.Enabled = false;
            menu.Items.Add(batteryItem);
            menu.Items.Add(timeItem);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(refreshItem);
            menu.Items.Add("退出", null, delegate { ExitThread(); });
            refreshItem.Click += delegate { Refresh(); };
            tray.ContextMenuStrip = menu;
            tray.Text = "Magic Trackpad：正在读取电量";
            SetIcon(null, false);
            tray.Visible = true;
            tray.MouseClick += delegate(object sender, MouseEventArgs e) { if (e.Button == MouseButtons.Left) Refresh(); };
            timer.Interval = 1000;
            timer.Tick += delegate {
                if (pending != null && !pending.IsCompleted && !timedOut && (DateTime.UtcNow - started).TotalSeconds > 10)
                {
                    timedOut = true;
                    ShowUnknown("读取超时，等待设备响应");
                }
                if (pending == null && DateTime.UtcNow >= nextPoll) Refresh();
            };
            timer.Start();
            Refresh();
        }

        private void SetIcon(int? percent, bool charging)
        {
            if (iconInitialized && percent == shownPercent && charging == shownCharging) return;
            Icon replacement = TrayArt.Create(percent, charging);
            tray.Icon = replacement;
            if (icon != null) icon.Dispose();
            icon = replacement;
            shownPercent = percent;
            shownCharging = charging;
            iconInitialized = true;
        }

        private void Refresh()
        {
            if (pending != null || exiting) return;
            refreshItem.Enabled = false;
            started = DateTime.UtcNow;
            timedOut = false;
            pending = Task.Factory.StartNew<Reading>(delegate { return HidBattery.Read(); });
            pending.ContinueWith(delegate(Task<Reading> task) {
                try {
                    dispatcher.BeginInvoke((Action)delegate {
                        if (exiting) return;
                        pending = null;
                        nextPoll = DateTime.UtcNow.AddSeconds(60);
                        refreshItem.Enabled = true;
                        if (task.IsFaulted) {
                            // Observe the exception, keep the tray responsive, and retry next minute.
                            Exception ignored = task.Exception;
                            ShowUnknown("暂时无法读取电量");
                        }
                        else if (task.Result == null) ShowUnknown("未连接、休眠或暂时无法读取");
                        else ApplyReading(task.Result);
                    });
                }
                catch (InvalidOperationException) { if (task.IsFaulted) { Exception ignored = task.Exception; } }
            });
        }

        private void ApplyReading(Reading reading)
        {
            string state = reading.Charging ? "正在充电" : reading.Full ? "已充满" : "未充电";
            tray.Text = "Magic Trackpad：" + reading.Percent + "%（" + state + "）";
            batteryItem.Text = "Magic Trackpad  " + reading.Percent + "% · " + state;
            timeItem.Text = "更新于 " + DateTime.Now.ToString("HH:mm:ss") + " · 每分钟刷新";
            SetIcon(reading.Percent, reading.Charging);
            bool previous = alerts.Warned;
            bool notify = alerts.Observe(reading);
            if (previous != alerts.Warned) SaveAlertState();
            if (notify)
                tray.ShowBalloonTip(12000, "Magic Trackpad 电量不足",
                    "剩余电量 " + reading.Percent + "%，请连接 Lightning 充电线。", ToolTipIcon.Warning);
            WriteStatus("{\"state\":\"connected\",\"battery_percent\":" + reading.Percent +
                ",\"charging\":" + reading.Charging.ToString().ToLowerInvariant() +
                ",\"low_battery_warned\":" + alerts.Warned.ToString().ToLowerInvariant() + "}");
        }

        private void ShowUnknown(string reason)
        {
            tray.Text = "Magic Trackpad：" + reason;
            batteryItem.Text = reason;
            timeItem.Text = "上次尝试 " + DateTime.Now.ToString("HH:mm:ss");
            SetIcon(null, false);
            WriteStatus("{\"state\":\"unavailable\",\"battery_percent\":null}");
        }

        private void SaveAlertState()
        {
            try {
                Directory.CreateDirectory(Path.GetDirectoryName(statePath));
                File.WriteAllText(statePath, alerts.Warned ? "warned" : "ready");
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }

        private void WriteStatus(string text)
        {
            if (statusPath == null) return;
            try { File.WriteAllText(statusPath, text, Encoding.UTF8); }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }

        protected override void ExitThreadCore()
        {
            exiting = true;
            timer.Stop();
            timer.Dispose();
            tray.Visible = false;
            tray.Dispose();
            menu.Dispose();
            if (icon != null) icon.Dispose();
            dispatcher.Dispose();
            base.ExitThreadCore();
        }
    }
}
