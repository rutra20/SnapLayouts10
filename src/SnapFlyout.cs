using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SnapLayouts10
{
    internal sealed class SnapFlyout : Form
    {
        private const int BasePad = 20;
        private const int BaseGap = 18;
        private const int BaseCardWidth = 150;
        private const int BaseCardHeight = 102;
        private const int BaseRadius = 14;
        private const int BaseGearRail = 34;

        private readonly AppSettings _settings;
        private readonly Timer _fadeTimer;
        private readonly List<LayoutDef> _layouts;
        private RectangleF[] _cardRects;
        private RectangleF[][] _zoneRects;
        private int _hoverCard = -1;
        private int _hoverZone = -1;
        private int _dpi = 96;
        private float _scale = 1f;
        private int _columns;
        private int _rows;
        private RectangleF _gearRect;
        private bool _gearHover;
        private Bitmap _acrylicBackdrop;

        public event EventHandler<ZoneSelectedEventArgs> ZoneSelected;
        public event EventHandler SettingsRequested;

        public SnapFlyout(AppSettings settings)
        {
            _settings = settings;
            _layouts = Layouts.GetEnabled(settings);

            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            ShowInTaskbar = false;
            TopMost = true;
            Text = "SnapLayouts10 Flyout";
            AutoScaleMode = AutoScaleMode.None;
            BackColor = Theme.Background;
            DoubleBuffered = true;
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.UserPaint |
                     ControlStyles.ResizeRedraw, true);

            _fadeTimer = new Timer();
            _fadeTimer.Interval = 15;
            _fadeTimer.Tick += FadeTimerTick;
        }

        public bool HasLayouts
        {
            get { return _layouts.Count > 0; }
        }

        public void ReloadLayouts(AppSettings settings)
        {
            if (Visible)
                HideFlyout();
            _layouts.Clear();
            _layouts.AddRange(Layouts.GetEnabled(settings));
            _hoverCard = -1;
            _hoverZone = -1;
        }

        public IntPtr TargetWindow { get; private set; }

        public Size Measure(int dpi)
        {
            CalculateGrid(dpi);
            float scale = dpi / 96f;
            int width = (int)Math.Round((BasePad * 2 + BaseGearRail + _columns * BaseCardWidth + (_columns - 1) * BaseGap) * scale);
            int height = (int)Math.Round((BasePad * 2 + _rows * BaseCardHeight + (_rows - 1) * BaseGap) * scale);
            return new Size(width, height);
        }

        public void ShowAt(int x, int y, int dpi, IntPtr target)
        {
            TargetWindow = target;
            _dpi = dpi;
            CalculateGrid(dpi);
            Size size = Measure(dpi);
            Bounds = new Rectangle(x, y, size.Width, size.Height);
            BuildGeometry();
            _hoverCard = -1;
            _hoverZone = -1;
            if (_acrylicBackdrop != null)
            {
                _acrylicBackdrop.Dispose();
                _acrylicBackdrop = null;
            }
            _acrylicBackdrop = CaptureBackdrop();
            Opacity = 0.05;
            Show();
            Native.SetWindowPos(Handle, new IntPtr(-1), x, y, size.Width, size.Height,
                Native.SWP_NOACTIVATE | Native.SWP_SHOWWINDOW);
            _fadeTimer.Start();
        }

        public void HideFlyout()
        {
            _fadeTimer.Stop();
            if (Visible)
                Hide();
            _hoverCard = -1;
            _hoverZone = -1;
            TargetWindow = IntPtr.Zero;
            if (_acrylicBackdrop != null)
            {
                _acrylicBackdrop.Dispose();
                _acrylicBackdrop = null;
            }
        }

        private Bitmap CaptureBackdrop()
        {
            try
            {
                int width = Math.Max(1, Width);
                int height = Math.Max(1, Height);
                int smallWidth = Math.Max(10, width / 10);
                int smallHeight = Math.Max(10, height / 10);
                using (Bitmap full = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
                {
                    using (Graphics graphics = Graphics.FromImage(full))
                        graphics.CopyFromScreen(Left, Top, 0, 0, new Size(width, height));
                    Bitmap small = new Bitmap(smallWidth, smallHeight);
                    using (Graphics graphics = Graphics.FromImage(small))
                    {
                        graphics.InterpolationMode = InterpolationMode.HighQualityBilinear;
                        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        graphics.DrawImage(full, 0, 0, smallWidth, smallHeight);
                    }
                    return small;
                }
            }
            catch
            {
                return null;
            }
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

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (!Visible)
            {
                _fadeTimer.Stop();
                TargetWindow = IntPtr.Zero;
            }
        }

        private void FadeTimerTick(object sender, EventArgs e)
        {
            if (!Visible)
            {
                _fadeTimer.Stop();
                return;
            }
            double next = Opacity + 0.22;
            if (next >= 1.0)
            {
                Opacity = 1.0;
                _fadeTimer.Stop();
            }
            else
            {
                Opacity = next;
            }
        }

        private void CalculateGrid(int dpi)
        {
            _scale = dpi / 96f;
            int count = _layouts.Count;
            _columns = Math.Max(1, Math.Min(2, count));
            _rows = (int)Math.Ceiling(count / (double)_columns);
        }

        private void BuildGeometry()
        {
            _scale = _dpi / 96f;
            int pad = (int)Math.Round(BasePad * _scale);
            int gap = (int)Math.Round(BaseGap * _scale);
            int gearRail = (int)Math.Round(BaseGearRail * _scale);
            int cardWidth = (int)Math.Round(BaseCardWidth * _scale);
            int cardHeight = (int)Math.Round(BaseCardHeight * _scale);

            Size size = Measure(_dpi);
            ClientSize = size;

            int gearSize = (int)Math.Round(30 * _scale);
            _gearRect = new RectangleF(
                pad + (gearRail - gearSize) / 2f,
                (size.Height - gearSize) / 2f,
                gearSize,
                gearSize);

            _cardRects = new RectangleF[_layouts.Count];
            _zoneRects = new RectangleF[_layouts.Count][];

            for (int i = 0; i < _layouts.Count; i++)
            {
                int column = i % _columns;
                int row = i / _columns;
                RectangleF card = new RectangleF(
                    pad + gearRail + column * (cardWidth + gap),
                    pad + row * (cardHeight + gap),
                    cardWidth,
                    cardHeight);
                _cardRects[i] = card;

                RectangleF preview = new RectangleF(
                    card.X + 12 * _scale,
                    card.Y + 12 * _scale,
                    card.Width - 24 * _scale,
                    card.Height - 24 * _scale);

                ZoneDef[] zones = _layouts[i].Zones;
                _zoneRects[i] = new RectangleF[zones.Length];
                for (int z = 0; z < zones.Length; z++)
                {
                    RectangleF normalized = zones[z].Bounds;
                    RectangleF zone = new RectangleF(
                        preview.X + normalized.X * preview.Width,
                        preview.Y + normalized.Y * preview.Height,
                        normalized.Width * preview.Width,
                        normalized.Height * preview.Height);
                    float inset = 3.8f * _scale;
                    zone.Inflate(-inset, -inset);
                    _zoneRects[i][z] = zone;
                }
            }

            int radius = (int)Math.Round(BaseRadius * _scale);
            using (GraphicsPath path = Util.RoundedRect(new RectangleF(0, 0, size.Width, size.Height), radius))
            {
                Region old = Region;
                Region = new Region(path);
                if (old != null)
                    old.Dispose();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.Clear(Theme.Background);

            if (_acrylicBackdrop != null)
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(_acrylicBackdrop, new Rectangle(0, 0, ClientSize.Width, ClientSize.Height));
                Color tintColor = Theme.IsLight
                    ? Color.FromArgb(215, 246, 246, 246)
                    : Color.FromArgb(238, 24, 24, 24);
                using (SolidBrush tint = new SolidBrush(tintColor))
                    g.FillRectangle(tint, 0, 0, ClientSize.Width, ClientSize.Height);
            }

            float borderWidth = Math.Max(1f, _scale);
            using (Pen borderPen = new Pen(Theme.Border, borderWidth))
            using (GraphicsPath borderPath = Util.RoundedRect(
                new RectangleF(0.5f * _scale, 0.5f * _scale, ClientSize.Width - _scale, ClientSize.Height - _scale),
                BaseRadius * _scale))
            {
                g.DrawPath(borderPen, borderPath);
            }

            DrawAmbientShade(g);
            DrawGear(g);

            for (int i = 0; i < _layouts.Count; i++)
                DrawCard(g, i);
        }

        private void DrawGear(Graphics g)
        {
            if (_gearHover)
            {
                using (SolidBrush fill = new SolidBrush(Theme.CardHover))
                using (GraphicsPath path = Util.RoundedRect(_gearRect, 7f * _scale))
                    g.FillPath(fill, path);
            }

            float cx = _gearRect.X + _gearRect.Width / 2f;
            float cy = _gearRect.Y + _gearRect.Height / 2f;
            Color color = _gearHover ? Theme.Text : Theme.SubText;
            using (Pen pen = new Pen(color, Math.Max(1.3f, 1.7f * _scale)))
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;
                for (int i = 0; i < 8; i++)
                {
                    double angle = Math.PI * 2 * i / 8d;
                    float inner = 4.2f * _scale;
                    float outer = 7.2f * _scale;
                    g.DrawLine(pen,
                        cx + (float)Math.Cos(angle) * inner,
                        cy + (float)Math.Sin(angle) * inner,
                        cx + (float)Math.Cos(angle) * outer,
                        cy + (float)Math.Sin(angle) * outer);
                }
                float ring = 4.2f * _scale;
                g.DrawEllipse(pen, cx - ring, cy - ring, ring * 2, ring * 2);
            }
        }

        private void DrawAmbientShade(Graphics g)
        {
            if (Theme.IsLight)
                return;
            using (LinearGradientBrush brush = new LinearGradientBrush(
                new Rectangle(0, 0, Math.Max(1, ClientSize.Width), Math.Max(1, (int)(70 * _scale))),
                Color.FromArgb(20, 255, 255, 255),
                Color.FromArgb(0, 255, 255, 255),
                LinearGradientMode.Vertical))
            {
                g.FillRectangle(brush, 1, 1, ClientSize.Width - 2, (int)(70 * _scale));
            }
        }

        private void DrawCard(Graphics g, int cardIndex)
        {
            RectangleF card = _cardRects[cardIndex];
            bool cardHover = _hoverCard == cardIndex;

            if (cardHover)
            {
                using (GraphicsPath hoverPath = Util.RoundedRect(card, 7f * _scale))
                using (SolidBrush hoverFill = new SolidBrush(Theme.IsLight
                    ? Color.FromArgb(12, 0, 0, 0)
                    : Color.FromArgb(14, 255, 255, 255)))
                {
                    g.FillPath(hoverFill, hoverPath);
                }
            }

            for (int z = 0; z < _zoneRects[cardIndex].Length; z++)
            {
                RectangleF zone = _zoneRects[cardIndex][z];
                bool hover = cardHover && _hoverZone == z;
                Color fillColor = hover ? Color.FromArgb(225, Theme.Accent) : (cardHover ? Theme.ZoneHover : Theme.Zone);
                Color borderColor = hover ? Theme.AccentBorder : Theme.ZoneBorder;
                float radius = 4f * _scale;

                using (GraphicsPath path = Util.RoundedRect(zone, radius))
                using (SolidBrush fill = new SolidBrush(fillColor))
                using (Pen pen = new Pen(borderColor, hover ? Math.Max(1.6f, _scale) : Math.Max(1f, _scale)))
                {
                    g.FillPath(fill, path);
                    g.DrawPath(pen, path);
                }

                if (hover)
                {
                    using (SolidBrush shine = new SolidBrush(Color.FromArgb(28, 255, 255, 255)))
                    using (GraphicsPath topPath = Util.RoundedRect(
                        new RectangleF(zone.X + _scale, zone.Y + _scale, Math.Max(1, zone.Width - 2 * _scale), Math.Max(1, zone.Height * 0.45f)),
                        radius))
                    {
                        g.FillPath(shine, topPath);
                    }
                }
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            int card;
            int zone;
            HitTestPoint(e.Location, out card, out zone);
            bool gearHover = _gearRect.Contains(e.Location);
            if (card != _hoverCard || zone != _hoverZone || gearHover != _gearHover)
            {
                _hoverCard = card;
                _hoverZone = zone;
                _gearHover = gearHover;
                Cursor = zone >= 0 || gearHover ? Cursors.Hand : Cursors.Default;
                Invalidate();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (_hoverCard != -1 || _hoverZone != -1 || _gearHover)
            {
                _hoverCard = -1;
                _hoverZone = -1;
                _gearHover = false;
                Cursor = Cursors.Default;
                Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (e.Button != MouseButtons.Left)
                return;
            if (_gearRect.Contains(e.Location))
            {
                EventHandler settingsHandler = SettingsRequested;
                HideFlyout();
                if (settingsHandler != null)
                    settingsHandler(this, EventArgs.Empty);
                return;
            }
            int card;
            int zone;
            HitTestPoint(e.Location, out card, out zone);
            if (card < 0 || zone < 0)
                return;

            LayoutDef layout = _layouts[card];
            EventHandler<ZoneSelectedEventArgs> handler = ZoneSelected;
            HideFlyout();
            if (handler != null)
                handler(this, new ZoneSelectedEventArgs(layout, zone));
        }

        private void HitTestPoint(Point point, out int cardIndex, out int zoneIndex)
        {
            cardIndex = -1;
            zoneIndex = -1;
            if (_cardRects == null)
                return;
            for (int i = 0; i < _cardRects.Length; i++)
            {
                if (!_cardRects[i].Contains(point))
                    continue;
                cardIndex = i;
                for (int z = 0; z < _zoneRects[i].Length; z++)
                {
                    if (_zoneRects[i][z].Contains(point))
                    {
                        zoneIndex = z;
                        break;
                    }
                }
                return;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_fadeTimer != null)
                {
                    _fadeTimer.Stop();
                    _fadeTimer.Dispose();
                }
                if (_acrylicBackdrop != null)
                {
                    _acrylicBackdrop.Dispose();
                    _acrylicBackdrop = null;
                }
            }
            base.Dispose(disposing);
        }
    }

    internal sealed class ZoneSelectedEventArgs : EventArgs
    {
        public readonly LayoutDef Layout;
        public readonly int ZoneIndex;

        public ZoneSelectedEventArgs(LayoutDef layout, int zoneIndex)
        {
            Layout = layout;
            ZoneIndex = zoneIndex;
        }
    }
}
