using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Text;

namespace SnapLayouts10
{
    internal static class Util
    {
        public static int Clamp(int value, int min, int max)
        {
            if (value < min)
                return min;
            if (value > max)
                return max;
            return value;
        }

        public static float Clamp(float value, float min, float max)
        {
            if (value < min)
                return min;
            if (value > max)
                return max;
            return value;
        }

        public static RectangleF NormalizedRect(RectangleF area, float x, float y, float width, float height)
        {
            return new RectangleF(
                area.X + area.Width * x,
                area.Y + area.Height * y,
                area.Width * width,
                area.Height * height);
        }

        public static GraphicsPath RoundedRect(RectangleF rect, float radius)
        {
            GraphicsPath path = new GraphicsPath();
            if (radius <= 0.1f)
            {
                path.AddRectangle(rect);
                path.CloseFigure();
                return path;
            }

            float diameter = Math.Min(radius * 2f, Math.Min(rect.Width, rect.Height));
            RectangleF arc = new RectangleF(rect.X, rect.Y, diameter, diameter);
            path.AddArc(arc, 180, 90);
            arc.X = rect.Right - diameter;
            path.AddArc(arc, 270, 90);
            arc.Y = rect.Bottom - diameter;
            path.AddArc(arc, 0, 90);
            arc.X = rect.Left;
            path.AddArc(arc, 90, 90);
            path.CloseFigure();
            return path;
        }

        public static Rectangle GetMonitorWorkArea(Point screenPoint)
        {
            Native.MONITORINFO info = new Native.MONITORINFO();
            info.cbSize = Marshal.SizeOf(typeof(Native.MONITORINFO));
            IntPtr monitor = Native.MonitorFromPoint(new Native.POINT(screenPoint.X, screenPoint.Y), Native.MONITOR_DEFAULTTONEAREST);
            if (monitor != IntPtr.Zero && Native.GetMonitorInfo(monitor, ref info))
            {
                Native.RECT work = info.rcWork;
                return Rectangle.FromLTRB(work.Left, work.Top, work.Right, work.Bottom);
            }
            return System.Windows.Forms.Screen.FromPoint(screenPoint).WorkingArea;
        }

        public static Rectangle GetVisibleFrame(IntPtr hwnd)
        {
            Native.RECT ext;
            if (Native.TryGetExtendedFrame(hwnd, out ext))
                return Rectangle.FromLTRB(ext.Left, ext.Top, ext.Right, ext.Bottom);
            Native.RECT wr;
            if (Native.GetWindowRect(hwnd, out wr))
                return Rectangle.FromLTRB(wr.Left, wr.Top, wr.Right, wr.Bottom);
            return Rectangle.Empty;
        }

        public static IntPtr RootWindow(IntPtr hwnd)
        {
            if (hwnd == IntPtr.Zero)
                return IntPtr.Zero;
            return Native.GetAncestor(hwnd, Native.GA_ROOT);
        }

        public static bool IsOwnProcess(IntPtr hwnd)
        {
            uint pid = Native.GetWindowProcessId(hwnd);
            return pid == (uint)Process.GetCurrentProcess().Id;
        }

        public static bool IsShellWindow(IntPtr hwnd)
        {
            if (hwnd == IntPtr.Zero)
                return true;
            if (hwnd == Native.GetShellWindow() || hwnd == Native.GetDesktopWindow())
                return true;
            string cls = Native.GetWindowClass(hwnd);
            return cls == "Progman" || cls == "WorkerW" || cls == "Shell_TrayWnd" ||
                   cls == "Shell_SecondaryTrayWnd" || cls == "DV2ControlHost";
        }

        public static bool IsCandidateWindow(IntPtr hwnd)
        {
            if (hwnd == IntPtr.Zero || !Native.IsWindow(hwnd))
                return false;
            if (!Native.IsWindowVisible(hwnd) || Native.IsIconic(hwnd))
                return false;
            if (IsOwnProcess(hwnd) || IsShellWindow(hwnd))
                return false;
            long style = Native.GetWindowStyle(hwnd);
            if ((style & Native.WS_MAXIMIZEBOX) == 0)
                return false;
            if (Native.GetWindow(hwnd, Native.GW_OWNER) != IntPtr.Zero)
                return false;
            return true;
        }

        public static int DpiForWindow(IntPtr hwnd)
        {
            try
            {
                uint dpi = Native.GetDpiForWindow(hwnd);
                if (dpi >= 72 && dpi <= 480)
                    return (int)dpi;
            }
            catch
            {
            }
            try
            {
                using (Graphics graphics = Graphics.FromHwnd(IntPtr.Zero))
                    return (int)Math.Round(graphics.DpiX);
            }
            catch
            {
                return 96;
            }
        }

        public static string Truncate(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;
            text = text.Replace("\r", " ").Replace("\n", " ").Trim();
            if (text.Length <= maxLength)
                return text;
            return text.Substring(0, Math.Max(0, maxLength - 1)) + "\u2026";
        }

        public static string VersionString()
        {
            try
            {
                return typeof(Util).Assembly.GetName().Version.ToString(3);
            }
            catch
            {
                return "1.0.0";
            }
        }
    }
}
