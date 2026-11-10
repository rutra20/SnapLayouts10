using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;

namespace SnapLayouts10
{
    internal static class IconFactory
    {
        public static Icon CreateAppIcon(int size)
        {
            using (Bitmap bitmap = new Bitmap(size, size, PixelFormat.Format32bppArgb))
            {
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                    g.Clear(Color.Transparent);

                    float pad = size * 0.055f;
                    RectangleF background = new RectangleF(pad, pad, size - pad * 2, size - pad * 2);
                    using (GraphicsPath path = Util.RoundedRect(background, size * 0.22f))
                    using (SolidBrush fill = new SolidBrush(Color.FromArgb(250, 32, 32, 32)))
                        g.FillPath(fill, path);

                    Color accent = Color.FromArgb(76, 194, 255);
                    float zoneGap = Math.Max(1.2f, size * 0.055f);
                    float left = size * 0.23f;
                    float top = size * 0.23f;
                    float right = size * 0.77f;
                    float bottom = size * 0.77f;
                    float midX = (left + right) / 2f;
                    float midY = (top + bottom) / 2f;

                    DrawZone(g, new RectangleF(left, top, midX - left - zoneGap / 2f, midY - top - zoneGap / 2f), accent, zoneGap);
                    DrawZone(g, new RectangleF(midX + zoneGap / 2f, top, right - midX - zoneGap / 2f, midY - top - zoneGap / 2f), accent, zoneGap);
                    DrawZone(g, new RectangleF(left, midY + zoneGap / 2f, midX - left - zoneGap / 2f, bottom - midY - zoneGap / 2f), accent, zoneGap);
                    DrawZone(g, new RectangleF(midX + zoneGap / 2f, midY + zoneGap / 2f, right - midX - zoneGap / 2f, bottom - midY - zoneGap / 2f), accent, zoneGap);
                }

                IntPtr handle = bitmap.GetHicon();
                try
                {
                    using (Icon temporary = Icon.FromHandle(handle))
                        return (Icon)temporary.Clone();
                }
                finally
                {
                    Native.DestroyIcon(handle);
                }
            }
        }

        private static void DrawZone(Graphics g, RectangleF rect, Color color, float radius)
        {
            if (rect.Width <= 1 || rect.Height <= 1)
                return;
            using (GraphicsPath path = Util.RoundedRect(rect, Math.Max(1f, radius / 2f)))
            using (SolidBrush fill = new SolidBrush(color))
                g.FillPath(fill, path);
        }
    }
}
