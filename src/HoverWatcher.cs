using System;
using System.Drawing;
using System.Windows.Forms;

namespace SnapLayouts10
{
    internal sealed class HoverWatcher : IDisposable
    {
        private readonly AppSettings _settings;
        private readonly Timer _timer;
        private readonly SnapFlyout _flyout;
        private IntPtr _candidate;
        private int _candidateSince;
        private Point _candidatePoint;
        private int _outsideSince;
        private IntPtr _activeTarget;
        private Rectangle _activeWorkArea;
        private int _activeDpi;
        private string _lastDiagnostic;
        private bool _disposed;

        public event EventHandler<SnapRequestedEventArgs> SnapRequested;
        public event EventHandler SettingsRequested;

        public HoverWatcher(AppSettings settings)
        {
            _settings = settings;
            _flyout = new SnapFlyout(settings);
            _flyout.ZoneSelected += FlyoutZoneSelected;
            _flyout.SettingsRequested += FlyoutSettingsRequested;

            _timer = new Timer();
            _timer.Interval = 40;
            _timer.Tick += TimerTick;
            if (settings.Enabled)
                _timer.Start();
        }

        public bool IsRunning
        {
            get { return _timer.Enabled; }
        }

        public void ApplySettings()
        {
            _flyout.ReloadLayouts(_settings);
            if (_settings.Enabled)
            {
                if (!_timer.Enabled)
                    _timer.Start();
            }
            else
            {
                _timer.Stop();
                HideFlyout();
            }
            ResetCandidate();
        }

        public void HideFlyout()
        {
            if (_flyout.Visible)
                _flyout.HideFlyout();
            _activeTarget = IntPtr.Zero;
            _outsideSince = 0;
            ResetCandidate();
        }

        private void TimerTick(object sender, EventArgs e)
        {
            if (_disposed || !_settings.Enabled)
                return;

            Native.POINT cursor;
            if (!Native.GetCursorPos(out cursor))
                return;
            Point point = new Point(cursor.X, cursor.Y);

            if (_flyout.Visible)
            {
                TickFlyoutOpen(point);
                return;
            }

            if (Native.IsKeyDown(Native.VK_LBUTTON))
            {
                ResetCandidate();
                return;
            }

            if (!_flyout.HasLayouts)
                return;

            IntPtr hwnd;
            string detail;
            if (!TryGetMaxButton(point, out hwnd, out detail))
            {
                ResetCandidate();
                return;
            }

            int now = Environment.TickCount;
            if (hwnd != _candidate)
            {
                _candidate = hwnd;
                _candidateSince = now;
                _candidatePoint = point;
                Log.Debug("Candidat Maximize: hwnd=" + hwnd.ToInt64() + " " + detail);
                return;
            }

            if (unchecked(now - _candidateSince) < _settings.HoverDelayMs)
                return;

            if (!Native.IsWindow(hwnd) || !Native.IsWindowVisible(hwnd))
            {
                ResetCandidate();
                return;
            }

            ShowFlyout(hwnd, point);
        }

        private void TickFlyoutOpen(Point point)
        {
            if (_flyout.Bounds.Contains(point))
            {
                _outsideSince = 0;
                return;
            }

            if (Native.IsKeyDown(Native.VK_LBUTTON))
            {
                HideFlyout();
                return;
            }

            if (!Native.IsWindow(_activeTarget) ||
                !Native.IsWindowVisible(_activeTarget) ||
                Native.IsIconic(_activeTarget) ||
                Util.RootWindow(Native.GetForegroundWindow()) != _activeTarget)
            {
                HideFlyout();
                return;
            }

            if (IsPointOverMaxButton(_activeTarget, point))
            {
                _outsideSince = 0;
                return;
            }

            int now = Environment.TickCount;
            if (_outsideSince == 0)
                _outsideSince = now;
            else if (unchecked(now - _outsideSince) > 420)
                HideFlyout();
        }

        private bool TryGetMaxButton(Point point, out IntPtr hwnd, out string detail)
        {
            hwnd = IntPtr.Zero;
            detail = string.Empty;

            Native.POINT nativePoint = new Native.POINT(point.X, point.Y);
            IntPtr underCursor = Native.WindowFromPoint(nativePoint);
            IntPtr root = Util.RootWindow(underCursor);
            if (root == IntPtr.Zero)
                return false;

            IntPtr foreground = Util.RootWindow(Native.GetForegroundWindow());
            if (foreground != root)
                return false;

            if (!Util.IsCandidateWindow(root))
                return false;

            bool timedOut;
            int hit = Native.SendHitTest(root, point.X, point.Y, 80, out timedOut);
            string cls = Native.GetWindowClass(root);
            string title = Util.Truncate(Native.GetWindowTitle(root), 55);
            detail = "class=" + cls + " title=\"" + title + "\" hit=" + (timedOut ? "timeout" : hit.ToString());

            if (!timedOut && hit == Native.HTMAXBUTTON)
            {
                hwnd = root;
                string diagnostic = root.ToInt64() + "|" + hit;
                if (diagnostic != _lastDiagnostic)
                {
                    _lastDiagnostic = diagnostic;
                    Log.Debug("Buton Maximize detectat (WM_NCHITTEST): hwnd=" + detail);
                }
                return true;
            }

            if (timedOut)
            {
                bool fallback = IsPointInFallbackButton(root, point);
                if (fallback)
                {
                    hwnd = root;
                    string diagnostic = root.ToInt64() + "|fallback";
                    if (diagnostic != _lastDiagnostic)
                    {
                        _lastDiagnostic = diagnostic;
                        Log.Debug("Buton Maximize detectat (geometrie de rezervă): hwnd=" + detail);
                    }
                }
                return fallback;
            }

            return false;
        }

        private bool IsPointOverMaxButton(IntPtr hwnd, Point point)
        {
            if (!Native.IsWindow(hwnd))
                return false;
            bool timedOut;
            int hit = Native.SendHitTest(hwnd, point.X, point.Y, 60, out timedOut);
            if (!timedOut && hit == Native.HTMAXBUTTON)
                return true;
            if (timedOut)
                return IsPointInFallbackButton(hwnd, point);
            return false;
        }

        private static bool IsPointInFallbackButton(IntPtr hwnd, Point point)
        {
            Rectangle frame = Util.GetVisibleFrame(hwnd);
            if (frame.IsEmpty)
                return false;

            int dpi = Util.DpiForWindow(hwnd);
            float scale = dpi / 96f;
            int buttonWidth = Math.Max(30, (int)Math.Round(40f * scale));
            int captionHeight = Math.Max(26, (int)Math.Round(32f * scale));
            Rectangle maximize = Rectangle.FromLTRB(
                frame.Right - 2 * buttonWidth,
                frame.Top,
                frame.Right - buttonWidth,
                frame.Top + captionHeight);
            return maximize.Contains(point);
        }

        private void ShowFlyout(IntPtr hwnd, Point point)
        {
            int dpi = Util.DpiForWindow(hwnd);
            float scale = dpi / 96f;
            Size size = _flyout.Measure(dpi);
            Rectangle work = Util.GetMonitorWorkArea(point);
            Rectangle frame = Util.GetVisibleFrame(hwnd);
            int margin = Math.Max(6, (int)Math.Round(8f * scale));

            int right = Math.Min(frame.Right - (int)Math.Round(4f * scale), work.Right - margin);
            int x = right - size.Width;
            if (x < work.Left + margin)
                x = work.Left + margin;
            if (x + size.Width > work.Right - margin)
                x = work.Right - size.Width - margin;

            int y = point.Y + (int)Math.Round(22f * scale);
            if (y + size.Height > work.Bottom - margin)
                y = point.Y - size.Height - (int)Math.Round(16f * scale);
            if (y < work.Top + margin)
                y = work.Bottom - size.Height - margin;
            y = Util.Clamp(y, work.Top + margin, Math.Max(work.Top + margin, work.Bottom - size.Height - margin));

            _activeTarget = hwnd;
            _activeWorkArea = work;
            _activeDpi = dpi;
            _outsideSince = 0;
            _flyout.ShowAt(x, y, dpi, hwnd);
            Log.Info(string.Format(
                "Flyout afișat: hwnd={0}, dpi={1}, poziție={2},{3}, dimensiune={4}x{5}, cursor={6},{7}",
                hwnd.ToInt64(), dpi, x, y, size.Width, size.Height, point.X, point.Y));
        }

        private void FlyoutSettingsRequested(object sender, EventArgs e)
        {
            HideFlyout();
            EventHandler handler = SettingsRequested;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        private void FlyoutZoneSelected(object sender, ZoneSelectedEventArgs e)
        {
            IntPtr target = _activeTarget;
            Rectangle work = _activeWorkArea;
            int dpi = _activeDpi;
            _activeTarget = IntPtr.Zero;
            ResetCandidate();

            EventHandler<SnapRequestedEventArgs> handler = SnapRequested;
            if (handler != null)
            {
                handler(this, new SnapRequestedEventArgs(
                    target, e.Layout, e.ZoneIndex, work, _settings.Gap, dpi));
            }
        }

        private void ResetCandidate()
        {
            _candidate = IntPtr.Zero;
            _candidateSince = 0;
            _lastDiagnostic = null;
        }

        public void Dispose()
        {
            if (_disposed)
                return;
            _disposed = true;
            _timer.Stop();
            _timer.Dispose();
            _flyout.ZoneSelected -= FlyoutZoneSelected;
            _flyout.SettingsRequested -= FlyoutSettingsRequested;
            _flyout.Dispose();
        }
    }

    internal sealed class SnapRequestedEventArgs : EventArgs
    {
        public readonly IntPtr TargetWindow;
        public readonly LayoutDef Layout;
        public readonly int ZoneIndex;
        public readonly Rectangle WorkArea;
        public readonly int Gap;
        public readonly int Dpi;

        public SnapRequestedEventArgs(IntPtr targetWindow, LayoutDef layout, int zoneIndex, Rectangle workArea, int gap, int dpi)
        {
            TargetWindow = targetWindow;
            Layout = layout;
            ZoneIndex = zoneIndex;
            WorkArea = workArea;
            Gap = gap;
            Dpi = dpi;
        }
    }
}
