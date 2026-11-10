using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace SnapLayouts10
{
    internal sealed class TrayApp : ApplicationContext
    {
        private readonly AppSettings _settings;
        private readonly NotifyIcon _tray;
        private readonly Icon _trayIcon;
        private readonly ContextMenuStrip _menu;
        private readonly HoverWatcher _watcher;
        private readonly ToolStripMenuItem _statusItem;
        private readonly ToolStripMenuItem _startupItem;
        private readonly Timer _firstRunTimer;
        private SettingsForm _settingsForm;
        private bool _exiting;

        private SnapAssistForm _assist;
        private LayoutDef _sessionLayout;
        private bool[] _sessionFilled;
        private List<IntPtr> _sessionPlaced;
        private Rectangle _sessionWorkArea;
        private int _sessionGap;
        private int _sessionDpi;

        public TrayApp(bool openSettings)
        {
            _settings = AppSettings.Load();
            bool firstRun = _settings.FirstRun || !AppSettings.Exists;

            if (_settings.StartWithWindows)
                AutoStart.SetEnabled(true);
            else
                AutoStart.SetEnabled(false);
            _settings.StartWithWindows = AutoStart.IsEnabled();

            _trayIcon = IconFactory.CreateAppIcon(32);
            _tray = new NotifyIcon();
            _tray.Icon = _trayIcon;
            _tray.Text = "SnapLayouts 10 — hover Maximize";
            _tray.Visible = true;
            _tray.DoubleClick += delegate { OpenSettings(); };

            _menu = new ContextMenuStrip();
            _menu.ShowImageMargin = false;
            _menu.ShowCheckMargin = false;
            _menu.Renderer = new DarkMenuRenderer();
            _menu.Font = Theme.FontPoints(9.5f, FontStyle.Regular);

            _statusItem = CreateMenuItem(string.Empty, delegate { ToggleEnabled(); });
            _menu.Items.Add(_statusItem);
            _startupItem = CreateMenuItem(string.Empty, delegate { ToggleStartup(); });
            _menu.Items.Add(_startupItem);
            _menu.Items.Add(new ToolStripSeparator());
            _menu.Items.Add(CreateMenuItem("Settings...", delegate { OpenSettings(); }));
            _menu.Items.Add(CreateMenuItem("About SnapLayouts 10", delegate { ShowAbout(); }));
            _menu.Items.Add(CreateMenuItem("Open data folder", delegate { OpenDataFolder(); }));
            _menu.Items.Add(new ToolStripSeparator());
            _menu.Items.Add(CreateMenuItem("Exit", delegate { ExitApp(); }));
            _tray.ContextMenuStrip = _menu;

            _watcher = new HoverWatcher(_settings);
            _watcher.SnapRequested += SnapRequested;
            _watcher.SettingsRequested += delegate { OpenSettings(); };
            UpdateMenuText();

            _firstRunTimer = new Timer();
            _firstRunTimer.Interval = 800;
            _firstRunTimer.Tick += delegate
            {
                _firstRunTimer.Stop();
                if (firstRun && _settings.Enabled)
                {
                    _tray.ShowBalloonTip(
                        6000,
                        "SnapLayouts 10",
                        "Rest the pointer on a window's Maximize button for about 300 ms, then choose a zone.",
                        ToolTipIcon.Info);
                }
            };
            _firstRunTimer.Start();

            if (openSettings)
            {
                Timer settingsTimer = new Timer();
                settingsTimer.Interval = 250;
                settingsTimer.Tick += delegate
                {
                    settingsTimer.Stop();
                    settingsTimer.Dispose();
                    OpenSettings();
                };
                settingsTimer.Start();
            }

            if (_settings.FirstRun)
            {
                _settings.FirstRun = false;
                _settings.Save();
            }

            Log.Info("SnapLayouts 10 v" + Util.VersionString() + " started. Enabled=" + _settings.Enabled +
                     ", delay=" + _settings.HoverDelayMs + "ms, gap=" + _settings.Gap + "px, layouts=" +
                     _settings.EnabledLayoutCount + ", debug=" + Log.DebugEnabled + ".");
        }

        private ToolStripMenuItem CreateMenuItem(string text, EventHandler click)
        {
            ToolStripMenuItem item = new ToolStripMenuItem(text);
            item.ForeColor = Theme.Text;
            item.Padding = new Padding(2, 3, 2, 3);
            item.Click += click;
            return item;
        }

        private void UpdateMenuText()
        {
            _statusItem.Text = _settings.Enabled ? "Snap Layouts: enabled" : "Snap Layouts: disabled";
            _startupItem.Text = _settings.StartWithWindows ? "Start with Windows: yes" : "Start with Windows: no";
        }

        private void ToggleEnabled()
        {
            _settings.Enabled = !_settings.Enabled;
            _settings.Save();
            if (!_settings.Enabled)
                EndSession();
            _watcher.ApplySettings();
            UpdateMenuText();
        }

        private void ToggleStartup()
        {
            bool target = !_settings.StartWithWindows;
            if (!AutoStart.SetEnabled(target))
            {
                MessageBox.Show(
                    "Could not update automatic startup. Please try again.",
                    "SnapLayouts 10",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }
            _settings.StartWithWindows = AutoStart.IsEnabled();
            _settings.Save();
            UpdateMenuText();
        }

        private void OpenSettings()
        {
            if (_settingsForm != null && !_settingsForm.IsDisposed)
            {
                _settingsForm.Activate();
                return;
            }

            EndSession();
            _watcher.HideFlyout();
            _settingsForm = new SettingsForm(_settings);
            DialogResult result = DialogResult.Cancel;
            try
            {
                result = _settingsForm.ShowDialog();
            }
            finally
            {
                _settingsForm.Dispose();
                _settingsForm = null;
            }

            if (result == DialogResult.OK)
            {
                bool startupOk = AutoStart.SetEnabled(_settings.StartWithWindows);
                _settings.StartWithWindows = AutoStart.IsEnabled();
                _settings.Save();
                _watcher.ApplySettings();
                UpdateMenuText();
                if (!startupOk)
                {
                    MessageBox.Show(
                        "Settings were saved, but automatic startup could not be configured.",
                        "SnapLayouts 10",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
            }
        }

        private void ShowAbout()
        {
            string text =
                "SnapLayouts 10  v" + Util.VersionString() + Environment.NewLine +
                "Snap Layouts for Windows 10" + Environment.NewLine + Environment.NewLine +
                "Rest the pointer for about " + _settings.HoverDelayMs + " ms on the Maximize button " +
                "of the active window, then click the desired zone." + Environment.NewLine + Environment.NewLine +
                "• six layouts with 2, 3 or 4 zones" + Environment.NewLine +
                "• works across multiple monitors" + Environment.NewLine +
                "• Snap Assist fills the remaining zones" + Environment.NewLine +
                "• System Tray app with optional startup" + Environment.NewLine + Environment.NewLine +
                "Data and log: " + Log.DataDirectory;
            MessageBox.Show(text, "About SnapLayouts 10", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void OpenDataFolder()
        {
            try
            {
                Process.Start("explorer.exe", "\"" + Log.DataDirectory + "\"");
            }
            catch (Exception ex)
            {
                Log.Error("Failed to open the data folder.", ex);
            }
        }

        private void SnapRequested(object sender, SnapRequestedEventArgs e)
        {
            if (_exiting || e.TargetWindow == IntPtr.Zero)
                return;

            EndSession();
            Rectangle placed;
            if (!SnapEngine.SnapWindow(e.TargetWindow, e.Layout, e.ZoneIndex, e.WorkArea, e.Gap, out placed))
            {
                Log.Info("Failed to snap the target window.");
                return;
            }

            if (!_settings.SnapAssist || e.Layout.Zones.Length <= 1)
                return;

            _sessionLayout = e.Layout;
            _sessionFilled = new bool[e.Layout.Zones.Length];
            _sessionFilled[e.ZoneIndex] = true;
            _sessionPlaced = new List<IntPtr>();
            _sessionPlaced.Add(e.TargetWindow);
            _sessionWorkArea = e.WorkArea;
            _sessionGap = e.Gap;
            _sessionDpi = e.Dpi;
            ShowNextAssist();
        }

        private void ShowNextAssist()
        {
            if (_sessionLayout == null || _assist != null)
                return;

            int zoneIndex = NextUnfilledZone();
            if (zoneIndex < 0)
            {
                ClearSession();
                return;
            }

            List<WindowInfo> windows = WindowEnumerator.GetEligibleWindows(_sessionPlaced, 6);
            if (windows.Count == 0)
            {
                ClearSession();
                return;
            }

            int filled = 0;
            for (int i = 0; i < _sessionFilled.Length; i++)
                if (_sessionFilled[i])
                    filled++;

            string subtitle = string.Format(
                "Zone \"{0}\" · {1} of {2} zones filled",
                _sessionLayout.Zones[zoneIndex].Name,
                filled,
                _sessionLayout.Zones.Length);
            string hint = "Click a window · Esc to cancel";

            Rectangle zoneRect = SnapEngine.CalculateZoneRect(
                _sessionLayout, zoneIndex, _sessionWorkArea, _sessionGap);
            if (zoneRect.Width < 120 || zoneRect.Height < 100)
            {
                ClearSession();
                return;
            }

            SnapAssistForm form = new SnapAssistForm(windows, subtitle, hint, zoneRect, _sessionDpi);
            _assist = form;
            form.WindowChosen += AssistWindowChosen;
            form.FormClosed += AssistClosed;
            form.ShowAssist();
        }

        private void AssistWindowChosen(object sender, WindowChosenEventArgs e)
        {
            if (_sessionLayout == null)
                return;

            int zoneIndex = NextUnfilledZone();
            if (zoneIndex < 0)
            {
                ClearSession();
                return;
            }

            Log.Info("Snap Assist: selected window hwnd=" + e.Window.Handle.ToInt64() + " title=\"" + e.Window.Title + "\".");
            Rectangle placed;
            if (SnapEngine.SnapWindow(e.Window.Handle, _sessionLayout, zoneIndex, _sessionWorkArea, _sessionGap, out placed))
            {
                _sessionFilled[zoneIndex] = true;
                _sessionPlaced.Add(e.Window.Handle);
            }
            else
            {
                Log.Info("Failed to snap the window chosen from Snap Assist.");
                ClearSession();
                return;
            }

            ShowNextAssist();
        }

        private void AssistClosed(object sender, FormClosedEventArgs e)
        {
            SnapAssistForm form = sender as SnapAssistForm;
            if (form != null)
            {
                form.WindowChosen -= AssistWindowChosen;
                form.FormClosed -= AssistClosed;
            }
            if (_assist == form)
                _assist = null;

            if (form == null || !form.Chosen)
                ClearSession();
        }

        private int NextUnfilledZone()
        {
            if (_sessionFilled == null)
                return -1;
            for (int i = 0; i < _sessionFilled.Length; i++)
            {
                if (!_sessionFilled[i])
                    return i;
            }
            return -1;
        }

        private void EndSession()
        {
            SnapAssistForm form = _assist;
            _assist = null;
            if (form != null)
            {
                form.WindowChosen -= AssistWindowChosen;
                form.FormClosed -= AssistClosed;
                try
                {
                    form.Close();
                    form.Dispose();
                }
                catch
                {
                }
            }
            ClearSession();
        }

        private void ClearSession()
        {
            _sessionLayout = null;
            _sessionFilled = null;
            _sessionPlaced = null;
            _sessionDpi = 0;
            _assist = null;
        }

        private void ExitApp()
        {
            if (_exiting)
                return;
            _exiting = true;
            EndSession();
            _watcher.SnapRequested -= SnapRequested;
            _watcher.Dispose();
            _tray.Visible = false;
            _tray.Dispose();
            _trayIcon.Dispose();
            _menu.Dispose();
            _firstRunTimer.Stop();
            _firstRunTimer.Dispose();
            Log.Info("SnapLayouts 10 stopped.");
            ExitThread();
        }
    }

    internal sealed class DarkMenuRenderer : ToolStripProfessionalRenderer
    {
        public DarkMenuRenderer()
            : base(new DarkColorTable())
        {
            RoundedEdges = false;
        }

        protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
        {
            using (Pen pen = new Pen(Theme.Border, 1))
            {
                int y = e.Item.Height / 2;
                e.Graphics.DrawLine(pen, 8, y, e.Item.Width - 8, y);
            }
        }
    }

    internal sealed class DarkColorTable : ProfessionalColorTable
    {
        public override Color ToolStripDropDownBackground { get { return Theme.MenuBackground; } }
        public override Color MenuItemSelected { get { return Theme.MenuHover; } }
        public override Color MenuItemBorder { get { return Theme.MenuHover; } }
        public override Color ImageMarginGradientBegin { get { return Theme.MenuBackground; } }
        public override Color ImageMarginGradientMiddle { get { return Theme.MenuBackground; } }
        public override Color ImageMarginGradientEnd { get { return Theme.MenuBackground; } }
        public override Color MenuBorder { get { return Theme.Border; } }
        public override Color SeparatorDark { get { return Theme.Border; } }
        public override Color SeparatorLight { get { return Theme.Border; } }
    }
}
