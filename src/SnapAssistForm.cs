using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using Microsoft.Win32;

namespace SnapLayouts10
{
    /// <summary>
    /// Controller for the Windows 11-style Snap Assist: instead of a panel,
    /// each candidate is shown as a floating live window preview inside the empty zone.
    /// </summary>
    internal sealed class SnapAssistForm : Form
    {
        private readonly List<WindowInfo> _windows;
        private readonly Rectangle _zoneRect;
        private readonly int _dpi;
        private readonly float _scale;
        private readonly List<WindowPreviewForm> _previews;
        private readonly Timer _timer;
        private readonly int _shownAt;
        private readonly Bitmap _wallpaper;
        private readonly Bitmap _backdrop;
        private readonly Rectangle _monitorRect;
        private bool _closedByChoice;
        private bool _cleanedUp;

        public event EventHandler<WindowChosenEventArgs> WindowChosen;

        public bool Chosen
        {
            get { return _closedByChoice; }
        }

        public SnapAssistForm(List<WindowInfo> windows, string subtitle, string hint, Rectangle zoneRect, int dpi)
        {
            _windows = windows;
            _zoneRect = zoneRect;
            _dpi = dpi >= 72 && dpi <= 480 ? dpi : 96;
            _scale = _dpi / 96f;
            _previews = new List<WindowPreviewForm>();

            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            ShowInTaskbar = false;
            TopMost = true;
            AutoScaleMode = AutoScaleMode.None;
            BackColor = Theme.Background;
            Text = "SnapAssist";
            Bounds = zoneRect;
            DoubleBuffered = true;
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.UserPaint |
                     ControlStyles.ResizeRedraw, true);

            _wallpaper = LoadWallpaper();
            _monitorRect = GetMonitorRect(zoneRect);
            _backdrop = CreateBackdrop();

            _timer = new Timer();
            _timer.Interval = 45;
            _timer.Tick += TimerTick;
            _shownAt = Environment.TickCount;
        }

        protected override bool ShowWithoutActivation
        {
            get { return true; }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams parameters = base.CreateParams;
                parameters.ExStyle |= (int)(Native.WS_EX_NOACTIVATE | Native.WS_EX_TOOLWINDOW);
                return parameters;
            }
        }

        public void ShowAssist()
        {
            Show();
            Native.SetWindowPos(Handle, new IntPtr(-1), Left, Top, Width, Height,
                Native.SWP_NOACTIVATE | Native.SWP_SHOWWINDOW);
            BuildPreviews();
            _timer.Start();
        }

        private static Bitmap LoadWallpaper()
        {
            try
            {
                string path = Registry.GetValue(@"HKEY_CURRENT_USER\Control Panel\Desktop", "WallPaper", string.Empty) as string;
                if (string.IsNullOrEmpty(path) || !File.Exists(path))
                {
                    string transcoded = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                        @"Microsoft\Windows\Themes\TranscodedWallpaper");
                    if (File.Exists(transcoded))
                        path = transcoded;
                }
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    using (Image image = Image.FromFile(path))
                        return new Bitmap(image);
                }
            }
            catch
            {
            }
            return null;
        }

        private static Rectangle GetMonitorRect(Rectangle zone)
        {
            Native.MONITORINFO info = new Native.MONITORINFO();
            info.cbSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(Native.MONITORINFO));
            IntPtr monitor = Native.MonitorFromPoint(
                new Native.POINT(zone.Left + 1, zone.Top + 1), Native.MONITOR_DEFAULTTONEAREST);
            if (monitor != IntPtr.Zero && Native.GetMonitorInfo(monitor, ref info))
                return Rectangle.FromLTRB(info.rcMonitor.Left, info.rcMonitor.Top, info.rcMonitor.Right, info.rcMonitor.Bottom);
            return zone;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Theme.Background);
            if (_backdrop != null)
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.DrawImage(_backdrop, new Rectangle(0, 0, Width, Height));
            }

            using (SolidBrush tint = new SolidBrush(Color.FromArgb(34, 0, 0, 0)))
                g.FillRectangle(tint, 0, 0, Width, Height);

            DrawPreviewShadows(g);
        }

        private Bitmap CreateBackdrop()
        {
            if (_wallpaper == null)
                return null;
            try
            {
                Rectangle monitor = _monitorRect.Width > 0 ? _monitorRect : _zoneRect;
                float scale = Math.Max(monitor.Width / (float)_wallpaper.Width, monitor.Height / (float)_wallpaper.Height);
                float drawWidth = _wallpaper.Width * scale;
                float drawHeight = _wallpaper.Height * scale;
                float offsetX = (monitor.Width - drawWidth) / 2f;
                float offsetY = (monitor.Height - drawHeight) / 2f;
                float zoneX = _zoneRect.Left - monitor.Left - offsetX;
                float zoneY = _zoneRect.Top - monitor.Top - offsetY;

                int smallWidth = Math.Max(12, Width / 9);
                int smallHeight = Math.Max(12, Height / 9);
                Bitmap small = new Bitmap(smallWidth, smallHeight);
                using (Graphics graphics = Graphics.FromImage(small))
                {
                    graphics.InterpolationMode = InterpolationMode.HighQualityBilinear;
                    graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    graphics.DrawImage(
                        _wallpaper,
                        new Rectangle(0, 0, smallWidth, smallHeight),
                        zoneX / scale,
                        zoneY / scale,
                        Width / scale,
                        Height / scale,
                        GraphicsUnit.Pixel);
                }
                return small;
            }
            catch
            {
                return null;
            }
        }

        private void DrawPreviewShadows(Graphics g)
        {
            if (_previews.Count == 0)
                return;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            for (int p = 0; p < _previews.Count; p++)
            {
                Rectangle bounds = _previews[p].Bounds;
                bounds.Offset(-Left, -Top);
                for (int layer = 10; layer >= 1; layer--)
                {
                    Rectangle shadow = bounds;
                    shadow.Inflate(layer + 3, layer + 3);
                    shadow.Offset(0, 4);
                    int alpha = 2 + (11 - layer) * 2;
                    using (GraphicsPath path = Util.RoundedRect(shadow, 11f * _scale))
                    using (SolidBrush brush = new SolidBrush(Color.FromArgb(alpha, 0, 0, 0)))
                    {
                        g.FillPath(brush, path);
                    }
                }
            }
        }

        private void BuildPreviews()
        {
            if (_windows.Count == 0)
                return;

            Rectangle area = _zoneRect;
            int margin = Math.Max(8, (int)Math.Round(18 * _scale));
            int gap = Math.Max(8, (int)Math.Round(16 * _scale));
            area.Inflate(-margin, -margin);
            if (area.Width < 80 || area.Height < 80)
                area = _zoneRect;

            int count = _windows.Count;
            int columns;
            if (count == 1)
                columns = 1;
            else if (count == 2)
                columns = area.Width > area.Height * 1.2 ? 2 : 1;
            else if (count <= 4)
                columns = 2;
            else
                columns = area.Width > area.Height * 1.1 ? 3 : 2;

            int rows = (int)Math.Ceiling(count / (double)columns);
            int cellWidth = Math.Max(60, (area.Width - (columns - 1) * gap) / columns);
            int cellHeight = Math.Max(50, (area.Height - (rows - 1) * gap) / rows);

            for (int i = 0; i < count; i++)
            {
                WindowInfo info = _windows[i];
                int column = i % columns;
                int row = i / columns;
                Rectangle cell = new Rectangle(
                    area.Left + column * (cellWidth + gap),
                    area.Top + row * (cellHeight + gap),
                    cellWidth,
                    cellHeight);

                Native.RECT sourceRect;
                int sourceWidth = 0;
                int sourceHeight = 0;
                if (Native.GetWindowRect(info.Handle, out sourceRect))
                {
                    sourceWidth = sourceRect.Width;
                    sourceHeight = sourceRect.Height;
                }
                if (sourceWidth < 40 || sourceHeight < 40)
                {
                    Rectangle frame = Util.GetVisibleFrame(info.Handle);
                    sourceWidth = frame.Width;
                    sourceHeight = frame.Height;
                }
                if (sourceWidth < 40 || sourceHeight < 40)
                {
                    sourceWidth = 16;
                    sourceHeight = 9;
                }

                float availableWidth = Math.Max(80, cell.Width - 6 * _scale);
                float availableHeight = Math.Max(60, cell.Height - 6 * _scale);
                float maxWidth = 470 * _scale;
                float maxHeight = 330 * _scale;
                availableWidth = Math.Min(availableWidth, maxWidth);
                availableHeight = Math.Min(availableHeight, maxHeight);

                float factor = Math.Min(availableWidth / sourceWidth, availableHeight / sourceHeight);
                int previewWidth = Math.Max(90, (int)Math.Round(sourceWidth * factor));
                int previewHeight = Math.Max(70, (int)Math.Round(sourceHeight * factor));
                if (previewWidth > cell.Width)
                    previewWidth = cell.Width;
                if (previewHeight > cell.Height)
                    previewHeight = cell.Height;

                int x = cell.Left + (cell.Width - previewWidth) / 2;
                int y = cell.Top + (cell.Height - previewHeight) / 2;
                Rectangle previewBounds = new Rectangle(x, y, previewWidth, previewHeight);

                WindowPreviewForm preview = new WindowPreviewForm(info, previewBounds, _dpi);
                preview.Selected += PreviewSelected;
                _previews.Add(preview);
                preview.ShowPreview();
                Native.SetWindowPos(preview.Handle, new IntPtr(-1), preview.Left, preview.Top,
                    preview.Width, preview.Height, Native.SWP_NOACTIVATE | Native.SWP_SHOWWINDOW);
            }

            Invalidate();
            Update();
        }

        private void PreviewSelected(object sender, WindowInfoEventArgs e)
        {
            if (_closedByChoice)
                return;
            _closedByChoice = true;
            EventHandler<WindowChosenEventArgs> handler = WindowChosen;
            Close();
            if (handler != null)
                handler(this, new WindowChosenEventArgs(e.Window));
        }

        private void TimerTick(object sender, EventArgs e)
        {
            if (!Visible)
                return;

            if (unchecked(Environment.TickCount - _shownAt) < 180)
                return;

            if (Native.IsKeyDown(0x1B)) // VK_ESCAPE
            {
                Close();
                return;
            }

            if (!Native.IsKeyDown(Native.VK_LBUTTON))
                return;

            Native.POINT cursor;
            if (!Native.GetCursorPos(out cursor))
                return;

            Point point = new Point(cursor.X, cursor.Y);
            for (int i = 0; i < _previews.Count; i++)
            {
                if (_previews[i].Bounds.Contains(point))
                    return;
            }
            Close();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            Cleanup();
            base.OnFormClosed(e);
        }

        private void Cleanup()
        {
            if (_cleanedUp)
                return;
            _cleanedUp = true;
            _timer.Stop();
            _timer.Dispose();

            for (int i = 0; i < _previews.Count; i++)
            {
                WindowPreviewForm preview = _previews[i];
                preview.Selected -= PreviewSelected;
                try
                {
                    preview.Close();
                    preview.Dispose();
                }
                catch
                {
                }
            }
            _previews.Clear();

            for (int i = 0; i < _windows.Count; i++)
                _windows[i].DisposeIcon();
            if (_backdrop != null)
                _backdrop.Dispose();
            if (_wallpaper != null)
                _wallpaper.Dispose();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                Cleanup();
            base.Dispose(disposing);
        }
    }

    internal sealed class WindowPreviewForm : Form
    {
        private readonly WindowInfo _info;
        private readonly int _dpi;
        private readonly float _scale;
        private IntPtr _thumbnail;
        private bool _hover;

        public event EventHandler<WindowInfoEventArgs> Selected;

        public WindowPreviewForm(WindowInfo info, Rectangle bounds, int dpi)
        {
            _info = info;
            _dpi = dpi >= 72 && dpi <= 480 ? dpi : 96;
            _scale = _dpi / 96f;
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            ShowInTaskbar = false;
            TopMost = true;
            AutoScaleMode = AutoScaleMode.None;
            BackColor = Theme.Card;
            Text = "SnapPreview";
            Bounds = bounds;
            DoubleBuffered = true;
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.UserPaint |
                     ControlStyles.ResizeRedraw, true);

            using (GraphicsPath path = Util.RoundedRect(
                new RectangleF(0, 0, Width, Height), 8f * _scale))
            {
                Region old = Region;
                Region = new Region(path);
                if (old != null)
                    old.Dispose();
            }
        }

        public void ShowPreview()
        {
            Show();
        }

        protected override bool ShowWithoutActivation
        {
            get { return true; }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams parameters = base.CreateParams;
                parameters.ExStyle |= (int)(Native.WS_EX_NOACTIVATE | Native.WS_EX_TOOLWINDOW);
                parameters.ClassStyle |= 0x00020000; // CS_DROPSHADOW
                return parameters;
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            try
            {
                int result = Native.DwmRegisterThumbnail(Handle, _info.Handle, out _thumbnail);
                if (result != 0)
                    _thumbnail = IntPtr.Zero;
                if (_thumbnail != IntPtr.Zero)
                {
                    int border = Math.Max(1, (int)Math.Round(2 * _scale));
                    Native.DWM_THUMBNAIL_PROPERTIES properties = new Native.DWM_THUMBNAIL_PROPERTIES();
                    properties.dwFlags = Native.DWM_TNP_RECTDESTINATION | Native.DWM_TNP_OPACITY | Native.DWM_TNP_VISIBLE;
                    properties.rcDestination = new Native.RECT();
                    properties.rcDestination.Left = border;
                    properties.rcDestination.Top = border;
                    properties.rcDestination.Right = Width - border;
                    properties.rcDestination.Bottom = Height - border;
                    properties.opacity = 255;
                    properties.fVisible = true;
                    properties.fSourceClientAreaOnly = false;
                    Native.DwmUpdateThumbnailProperties(_thumbnail, ref properties);
                }
            }
            catch
            {
                _thumbnail = IntPtr.Zero;
            }
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Theme.Card);

            if (_thumbnail == IntPtr.Zero)
            {
                if (_info.Icon != null)
                {
                    int iconSize = (int)Math.Max(24f, Math.Min(64f * _scale, Math.Min(Width, Height) / 3f));
                    try
                    {
                        g.DrawIcon(_info.Icon, new Rectangle((Width - iconSize) / 2, (Height - iconSize) / 2, iconSize, iconSize));
                    }
                    catch
                    {
                    }
                }
            }

            int borderWidth = Math.Max(1, (int)Math.Round((_hover ? 2.4f : 1.4f) * _scale));
            Color borderColor = _hover
                ? Theme.AccentBorder
                : (Theme.IsLight ? Color.FromArgb(80, 0, 0, 0) : Color.FromArgb(85, 255, 255, 255));
            using (Pen border = new Pen(borderColor, borderWidth))
            using (GraphicsPath path = Util.RoundedRect(
                new RectangleF(borderWidth / 2f, borderWidth / 2f,
                    Width - borderWidth, Height - borderWidth), 8f * _scale))
            {
                g.DrawPath(border, path);
            }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            _hover = true;
            Cursor = Cursors.Hand;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _hover = false;
            Cursor = Cursors.Default;
            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (e.Button != MouseButtons.Left)
                return;
            EventHandler<WindowInfoEventArgs> handler = Selected;
            if (handler != null)
                handler(this, new WindowInfoEventArgs(_info));
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            UnregisterThumbnail();
            base.OnFormClosed(e);
        }

        private void UnregisterThumbnail()
        {
            if (_thumbnail != IntPtr.Zero)
            {
                try
                {
                    Native.DwmUnregisterThumbnail(_thumbnail);
                }
                catch
                {
                }
                _thumbnail = IntPtr.Zero;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                UnregisterThumbnail();
            base.Dispose(disposing);
        }
    }

    internal sealed class WindowInfoEventArgs : EventArgs
    {
        public readonly WindowInfo Window;

        public WindowInfoEventArgs(WindowInfo window)
        {
            Window = window;
        }
    }

    internal sealed class WindowChosenEventArgs : EventArgs
    {
        public readonly WindowInfo Window;

        public WindowChosenEventArgs(WindowInfo window)
        {
            Window = window;
        }
    }
}
