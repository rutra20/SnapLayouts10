using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SnapLayouts10
{
    internal sealed class SettingsForm : Form
    {
        private readonly AppSettings _settings;
        private ToggleSwitch _enabledSwitch;
        private ToggleSwitch _startupSwitch;
        private ToggleSwitch _assistSwitch;
        private DarkComboBox _delayCombo;
        private DarkComboBox _gapCombo;
        private LayoutToggle[] _layoutToggles;
        private readonly int[] _delays = new int[] { 200, 300, 400, 500, 700 };
        private readonly int[] _gaps = new int[] { 0, 4, 6, 8, 12, 16 };

        public SettingsForm(AppSettings settings)
        {
            _settings = settings;
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "SnapLayouts 10 — Settings";
            BackColor = Theme.Background;
            ForeColor = Theme.Text;
            ClientSize = new Size(620, 700);
            Font = Theme.FontPoints(9.5f, FontStyle.Regular);
            try
            {
                Icon = IconFactory.CreateAppIcon(32);
            }
            catch
            {
            }

            Panel buttonBar = new Panel();
            buttonBar.Dock = DockStyle.Bottom;
            buttonBar.Height = 62;
            buttonBar.BackColor = Theme.Panel;
            buttonBar.Paint += ButtonBarPaint;
            Controls.Add(buttonBar);

            Panel content = new Panel();
            content.Dock = DockStyle.Fill;
            content.AutoScroll = true;
            content.BackColor = Theme.Background;
            Controls.Add(content);

            BuildContent(content);
            BuildButtons(buttonBar);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            if (!Theme.IsLight)
                Native.EnableDarkTitleBar(Handle);
        }

        private void BuildContent(Panel content)
        {
            Label title = new Label();
            title.Text = "SnapLayouts 10";
            title.Font = Theme.FontPoints(17f, FontStyle.Bold);
            title.ForeColor = Theme.Text;
            title.AutoSize = true;
            title.Location = new Point(28, 20);
            content.Controls.Add(title);

            Label subtitle = new Label();
            subtitle.Text = "The layout menu appears when the pointer rests on the Maximize button.";
            subtitle.Font = Theme.FontPoints(9.25f, FontStyle.Regular);
            subtitle.ForeColor = Theme.SubText;
            subtitle.AutoSize = true;
            subtitle.Location = new Point(28, 52);
            content.Controls.Add(subtitle);

            AddSection(content, "Behavior", 92);
            _enabledSwitch = AddToggleRow(content, "Enable Snap Layouts on hover", 126, _settings.Enabled);
            _startupSwitch = AddToggleRow(content, "Start with Windows", 164, _settings.StartWithWindows);
            _assistSwitch = AddToggleRow(content, "Show Snap Assist for remaining zones", 202, _settings.SnapAssist);

            AddSection(content, "Appearance and spacing", 250);
            Label delayLabel = AddLabel(content, "Menu delay", 28, 284, 190);
            string[] delayItems = new string[_delays.Length];
            for (int i = 0; i < _delays.Length; i++)
                delayItems[i] = _delays[i] + " ms";
            _delayCombo = AddChoice(content, 220, 280, 110, delayItems, ClosestIndex(_delays, _settings.HoverDelayMs));

            Label gapLabel = AddLabel(content, "Window gap", 370, 284, 130);
            string[] gapItems = new string[_gaps.Length];
            for (int i = 0; i < _gaps.Length; i++)
                gapItems[i] = _gaps[i] + " px";
            _gapCombo = AddChoice(content, 620 - 28 - 110, 280, 110, gapItems, ClosestIndex(_gaps, _settings.Gap));

            AddSection(content, "Active layouts", 330);
            Label hint = AddLabel(content,
                "Choose the layouts shown in the menu. Each zone accepts one window with a single click.",
                28, 358, 560);
            hint.Font = Theme.FontPoints(8.75f, FontStyle.Regular);
            hint.ForeColor = Theme.SubText;

            _layoutToggles = new LayoutToggle[Layouts.Count];
            int startX = 28;
            int startY = 390;
            int cardWidth = 272;
            int cardHeight = 58;
            int gapX = 20;
            int gapY = 10;
            for (int i = 0; i < Layouts.Count; i++)
            {
                LayoutToggle toggle = new LayoutToggle(Layouts.All[i], _settings.IsLayoutEnabled(i));
                toggle.Location = new Point(startX + (i % 2) * (cardWidth + gapX), startY + (i / 2) * (cardHeight + gapY));
                toggle.Size = new Size(cardWidth, cardHeight);
                toggle.BackColor = Color.Transparent;
                content.Controls.Add(toggle);
                _layoutToggles[i] = toggle;
            }

            Label note = AddLabel(content,
                "Keep at least one layout active. Settings are stored in the app data folder.",
                28, 600, 560);
            note.Font = Theme.FontPoints(8.5f, FontStyle.Regular);
            note.ForeColor = Theme.SubText;
        }

        private void BuildButtons(Panel bar)
        {
            LinkLabel dataLink = new LinkLabel();
            dataLink.Text = "Open data folder";
            dataLink.AutoSize = true;
            dataLink.LinkColor = Theme.Accent;
            dataLink.ActiveLinkColor = Theme.AccentBorder;
            dataLink.VisitedLinkColor = Theme.Accent;
            dataLink.Font = Theme.FontPoints(9f, FontStyle.Regular);
            dataLink.Location = new Point(28, 21);
            dataLink.LinkClicked += delegate
            {
                try
                {
                    Process.Start("explorer.exe", "\"" + Log.DataDirectory + "\"");
                }
                catch (Exception ex)
                {
                    Log.Error("Failed to open the data folder.", ex);
                }
            };
            bar.Controls.Add(dataLink);

            Button cancel = new Button();
            cancel.Text = "Cancel";
            cancel.FlatStyle = FlatStyle.Flat;
            cancel.FlatAppearance.BorderSize = 1;
            cancel.FlatAppearance.BorderColor = Theme.Border;
            cancel.FlatAppearance.MouseOverBackColor = Theme.CardHover;
            cancel.BackColor = Theme.Panel;
            cancel.ForeColor = Theme.Text;
            cancel.Font = Theme.FontPoints(9.5f, FontStyle.Regular);
            cancel.Size = new Size(94, 34);
            cancel.Location = new Point(ClientSize.Width - 104 - 20 - 94 - 10, 14);
            cancel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            cancel.DialogResult = DialogResult.Cancel;
            cancel.Click += delegate { Close(); };
            bar.Controls.Add(cancel);

            Button save = new Button();
            save.Text = "Save";
            save.FlatStyle = FlatStyle.Flat;
            save.FlatAppearance.BorderSize = 0;
            save.FlatAppearance.MouseOverBackColor = Theme.Lighten(Theme.Accent, 0.12f);
            save.BackColor = Theme.Accent;
            save.ForeColor = Color.White;
            save.Font = Theme.FontPoints(9.5f, FontStyle.Bold);
            save.Size = new Size(104, 34);
            save.Location = new Point(ClientSize.Width - 104 - 20, 14);
            save.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            save.Click += SaveClicked;
            bar.Controls.Add(save);

            AcceptButton = save;
            CancelButton = cancel;
        }

        private void SaveClicked(object sender, EventArgs e)
        {
            int enabledLayouts = 0;
            for (int i = 0; i < _layoutToggles.Length; i++)
            {
                if (_layoutToggles[i].Checked)
                    enabledLayouts++;
            }
            if (enabledLayouts == 0)
            {
                MessageBox.Show(this,
                    "Enable at least one layout.",
                    "SnapLayouts 10",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            _settings.Enabled = _enabledSwitch.Checked;
            _settings.StartWithWindows = _startupSwitch.Checked;
            _settings.SnapAssist = _assistSwitch.Checked;
            _settings.HoverDelayMs = _delays[Math.Max(0, _delayCombo.SelectedIndex)];
            _settings.Gap = _gaps[Math.Max(0, _gapCombo.SelectedIndex)];
            for (int i = 0; i < _layoutToggles.Length; i++)
                _settings.LayoutEnabled[i] = _layoutToggles[i].Checked;
            _settings.Save();

            DialogResult = DialogResult.OK;
            Close();
        }

        private void ButtonBarPaint(object sender, PaintEventArgs e)
        {
            using (Pen pen = new Pen(Theme.Border, 1))
                e.Graphics.DrawLine(pen, 0, 0, ((Panel)sender).Width, 0);
        }

        private void AddSection(Panel content, string text, int y)
        {
            Label label = new Label();
            label.Text = text;
            label.Font = Theme.FontPoints(10.25f, FontStyle.Bold);
            label.ForeColor = Theme.Text;
            label.AutoSize = true;
            label.Location = new Point(28, y);
            content.Controls.Add(label);

            Panel line = new Panel();
            line.BackColor = Theme.Border;
            line.Location = new Point(28, y + 27);
            line.Size = new Size(564, 1);
            content.Controls.Add(line);
        }

        private ToggleSwitch AddToggleRow(Panel content, string text, int y, bool value)
        {
            Label label = AddLabel(content, text, 28, y + 1, 480);
            ToggleSwitch toggle = new ToggleSwitch();
            toggle.Checked = value;
            toggle.Location = new Point(620 - 28 - 46, y);
            toggle.Size = new Size(46, 24);
            content.Controls.Add(toggle);
            return toggle;
        }

        private Label AddLabel(Panel content, string text, int x, int y, int width)
        {
            Label label = new Label();
            label.Text = text;
            label.AutoSize = false;
            label.AutoEllipsis = true;
            label.Location = new Point(x, y);
            label.Size = new Size(width, 22);
            label.ForeColor = Theme.Text;
            label.Font = Theme.FontPoints(9.5f, FontStyle.Regular);
            content.Controls.Add(label);
            return label;
        }

        private DarkComboBox AddChoice(Panel content, int x, int y, int width, string[] items, int selectedIndex)
        {
            DarkComboBox combo = new DarkComboBox(items, selectedIndex);
            combo.Location = new Point(x, y);
            combo.Size = new Size(width, 30);
            content.Controls.Add(combo);
            return combo;
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            Rectangle work = Screen.FromControl(this).WorkingArea;
            int maxHeight = work.Height - 50;
            if (Height > maxHeight)
                Height = maxHeight;
        }

        private static int ClosestIndex(int[] values, int value)
        {
            int best = 0;
            int distance = int.MaxValue;
            for (int i = 0; i < values.Length; i++)
            {
                int d = Math.Abs(values[i] - value);
                if (d < distance)
                {
                    distance = d;
                    best = i;
                }
            }
            return best;
        }
    }

    internal sealed class DarkComboBox : Control
    {
        private readonly string[] _items;
        private int _selectedIndex;
        private bool _hover;
        private readonly ContextMenuStrip _menu;

        public event EventHandler SelectedIndexChanged;

        public DarkComboBox(string[] items, int selectedIndex)
        {
            _items = items;
            _selectedIndex = Util.Clamp(selectedIndex, 0, items.Length - 1);
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.UserPaint |
                     ControlStyles.SupportsTransparentBackColor |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.Selectable, true);
            BackColor = Color.Transparent;
            Font = Theme.FontPoints(9.5f, FontStyle.Regular);
            Cursor = Cursors.Hand;
            TabStop = true;
            AccessibleRole = AccessibleRole.ComboBox;

            _menu = new ContextMenuStrip();
            _menu.ShowImageMargin = false;
            _menu.ShowCheckMargin = false;
            _menu.Renderer = new DarkMenuRenderer();
            _menu.Font = Font;
            _menu.Padding = new Padding(0, 4, 0, 4);
        }

        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set
            {
                int next = Util.Clamp(value, 0, _items.Length - 1);
                if (next == _selectedIndex)
                    return;
                _selectedIndex = next;
                Invalidate();
                EventHandler handler = SelectedIndexChanged;
                if (handler != null)
                    handler(this, EventArgs.Empty);
            }
        }

        public string SelectedItem
        {
            get
            {
                if (_selectedIndex < 0 || _selectedIndex >= _items.Length)
                    return string.Empty;
                return _items[_selectedIndex];
            }
        }

        private void OpenMenu()
        {
            BuildMenu();
            try
            {
                _menu.Show(this, new Point(0, Height + 2));
            }
            catch
            {
            }
        }

        private void BuildMenu()
        {
            _menu.Items.Clear();
            int rowHeight = Math.Max(24, (int)Math.Round(28f * DeviceDpi / 96f));
            for (int i = 0; i < _items.Length; i++)
            {
                ToolStripMenuItem item = new ToolStripMenuItem(_items[i]);
                item.ForeColor = Theme.Text;
                item.AutoSize = false;
                item.Size = new Size(Math.Max(80, Width - 2), rowHeight);
                item.Padding = new Padding(2, 0, 2, 0);
                int index = i;
                item.Click += delegate { SelectedIndex = index; };
                _menu.Items.Add(item);
            }
            _menu.AutoSize = false;
            _menu.Size = new Size(Math.Max(80, Width), rowHeight * _items.Length + 8);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            float s = DeviceDpi / 96f;
            RectangleF rect = new RectangleF(0.5f * s, 0.5f * s, Width - s, Height - s);
            using (GraphicsPath path = Util.RoundedRect(rect, 5f * s))
            using (SolidBrush fill = new SolidBrush(Theme.Panel))
            using (Pen border = new Pen(_hover || Focused ? Theme.AccentBorder : Theme.Border, Math.Max(1f, s)))
            {
                g.FillPath(fill, path);
                g.DrawPath(border, path);
            }

            TextRenderer.DrawText(g, SelectedItem, Font,
                new Rectangle((int)Math.Round(10 * s), 0, Width - (int)Math.Round(30 * s), Height),
                Theme.Text, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

            float cx = Width - 15 * s;
            float cy = Height / 2f;
            using (Pen chevron = new Pen(Theme.SubText, Math.Max(1.3f, 1.6f * s)))
            {
                chevron.StartCap = LineCap.Round;
                chevron.EndCap = LineCap.Round;
                g.DrawLine(chevron, cx - 4 * s, cy - 2 * s, cx, cy + 2 * s);
                g.DrawLine(chevron, cx, cy + 2 * s, cx + 4 * s, cy - 2 * s);
            }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            _hover = true;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _hover = false;
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Focus();
            if (e.Button == MouseButtons.Left)
                OpenMenu();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Space || e.KeyCode == Keys.Down || e.KeyCode == Keys.Up)
            {
                if (e.KeyCode == Keys.Down)
                    SelectedIndex = Math.Min(_items.Length - 1, _selectedIndex + 1);
                else if (e.KeyCode == Keys.Up)
                    SelectedIndex = Math.Max(0, _selectedIndex - 1);
                else
                    OpenMenu();
                e.Handled = true;
            }
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            Invalidate();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            Invalidate();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _menu.Dispose();
            base.Dispose(disposing);
        }
    }

    internal sealed class ToggleSwitch : Control
    {
        private bool _checked;
        private bool _hover;

        public event EventHandler CheckedChanged;

        public ToggleSwitch()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.UserPaint |
                     ControlStyles.SupportsTransparentBackColor |
                     ControlStyles.ResizeRedraw, true);
            BackColor = Color.Transparent;
            Cursor = Cursors.Hand;
            Size = new Size(46, 24);
        }

        public bool Checked
        {
            get { return _checked; }
            set
            {
                if (_checked == value)
                    return;
                _checked = value;
                Invalidate();
                EventHandler handler = CheckedChanged;
                if (handler != null)
                    handler(this, EventArgs.Empty);
            }
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            Checked = !Checked;
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            _hover = true;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _hover = false;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            float dpi = DeviceDpi;
            float s = dpi / 96f;
            RectangleF track = new RectangleF(1 * s, 3 * s, Width - 2 * s, Height - 6 * s);
            Color trackColor = _checked
                ? (_hover ? Theme.Lighten(Theme.Accent, 0.12f) : Theme.Accent)
                : (Theme.IsLight ? Color.FromArgb(190, 190, 190) : Color.FromArgb(76, 76, 76));
            using (GraphicsPath path = Util.RoundedRect(track, track.Height / 2f))
            using (SolidBrush fill = new SolidBrush(trackColor))
                g.FillPath(fill, path);

            float knobSize = track.Height - 4 * s;
            float knobX = _checked ? track.Right - knobSize - 2 * s : track.Left + 2 * s;
            RectangleF knob = new RectangleF(knobX, track.Top + 2 * s, knobSize, knobSize);
            using (SolidBrush knobBrush = new SolidBrush(Color.White))
                g.FillEllipse(knobBrush, knob);
        }
    }

    internal sealed class LayoutToggle : Control
    {
        private readonly LayoutDef _layout;
        private bool _checked;
        private bool _hover;
        private readonly Font _titleFont;
        private readonly Font _descriptionFont;

        public LayoutToggle(LayoutDef layout, bool isChecked)
        {
            _layout = layout;
            _checked = isChecked;
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.UserPaint |
                     ControlStyles.SupportsTransparentBackColor |
                     ControlStyles.ResizeRedraw, true);
            BackColor = Color.Transparent;
            Cursor = Cursors.Hand;
            float s = DeviceDpi / 96f;
            _titleFont = Theme.Font(13f * s, FontStyle.Regular);
            _descriptionFont = Theme.Font(10.5f * s, FontStyle.Regular);
        }

        public bool Checked
        {
            get { return _checked; }
            set
            {
                if (_checked == value)
                    return;
                _checked = value;
                Invalidate();
            }
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            _checked = !_checked;
            Invalidate();
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            _hover = true;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _hover = false;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            float s = DeviceDpi / 96f;
            RectangleF rect = new RectangleF(0.5f * s, 0.5f * s, Width - s, Height - s);

            using (GraphicsPath path = Util.RoundedRect(rect, 7f * s))
            using (SolidBrush fill = new SolidBrush(_hover ? Theme.CardHover : Theme.Card))
            using (Pen border = new Pen(_checked ? Theme.AccentBorder : Theme.Border, Math.Max(1f, s)))
            {
                g.FillPath(fill, path);
                g.DrawPath(border, path);
            }

            RectangleF preview = new RectangleF(10 * s, 9 * s, 58 * s, 40 * s);
            using (SolidBrush previewFill = new SolidBrush(Theme.IsLight ? Color.FromArgb(246, 246, 246) : Color.FromArgb(36, 36, 36)))
            using (GraphicsPath previewPath = Util.RoundedRect(preview, 4f * s))
                g.FillPath(previewFill, previewPath);

            for (int i = 0; i < _layout.Zones.Length; i++)
            {
                RectangleF zone = Util.NormalizedRect(preview, _layout.Zones[i].Bounds.X, _layout.Zones[i].Bounds.Y,
                    _layout.Zones[i].Bounds.Width, _layout.Zones[i].Bounds.Height);
                zone.Inflate(-1.5f * s, -1.5f * s);
                using (GraphicsPath path = Util.RoundedRect(zone, 2f * s))
                using (SolidBrush fill = new SolidBrush(_checked ? Color.FromArgb(105, Theme.Accent) : (Theme.IsLight ? Color.FromArgb(214, 214, 214) : Color.FromArgb(71, 71, 71))))
                {
                    g.FillPath(fill, path);
                }
            }

            using (StringFormat format = new StringFormat(StringFormatFlags.NoWrap))
            {
                format.Trimming = StringTrimming.EllipsisCharacter;
                using (SolidBrush titleBrush = new SolidBrush(Theme.Text))
                    g.DrawString(_layout.Name, _titleFont, titleBrush,
                        new RectangleF(80 * s, 11 * s, Width - 128 * s, 18 * s), format);
                using (SolidBrush descriptionBrush = new SolidBrush(Theme.SubText))
                    g.DrawString(_layout.Description, _descriptionFont, descriptionBrush,
                        new RectangleF(80 * s, 31 * s, Width - 128 * s, 16 * s), format);
            }

            RectangleF check = new RectangleF(Width - 30 * s, (Height - 18 * s) / 2f, 18 * s, 18 * s);
            using (GraphicsPath checkPath = Util.RoundedRect(check, 5f * s))
            using (SolidBrush checkFill = new SolidBrush(_checked ? Theme.Accent : Color.Transparent))
            using (Pen checkBorder = new Pen(_checked ? Theme.Accent : Theme.ZoneBorder, Math.Max(1f, s)))
            {
                g.FillPath(checkFill, checkPath);
                g.DrawPath(checkBorder, checkPath);
            }
            if (_checked)
            {
                using (Pen checkPen = new Pen(Color.White, Math.Max(1.6f, 1.8f * s)))
                {
                    checkPen.StartCap = LineCap.Round;
                    checkPen.EndCap = LineCap.Round;
                    g.DrawLine(checkPen, check.Left + 4 * s, check.Top + 9 * s, check.Left + 7.5f * s, check.Top + 12.5f * s);
                    g.DrawLine(checkPen, check.Left + 7.5f * s, check.Top + 12.5f * s, check.Right - 4 * s, check.Top + 5 * s);
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_titleFont != null) _titleFont.Dispose();
                if (_descriptionFont != null) _descriptionFont.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
