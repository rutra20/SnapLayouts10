using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading;

namespace SnapLayouts10
{
    internal static class SnapEngine
    {
        public static Rectangle CalculateZoneRect(LayoutDef layout, int zoneIndex, Rectangle workArea, int gap)
        {
            if (layout == null || zoneIndex < 0 || zoneIndex >= layout.Zones.Length)
                return Rectangle.Empty;

            RectangleF normalized = layout.Zones[zoneIndex].Bounds;
            int left = workArea.Left + (int)Math.Round(normalized.X * workArea.Width);
            int top = workArea.Top + (int)Math.Round(normalized.Y * workArea.Height);
            int right = workArea.Left + (int)Math.Round((normalized.X + normalized.Width) * workArea.Width);
            int bottom = workArea.Top + (int)Math.Round((normalized.Y + normalized.Height) * workArea.Height);

            int halfGap = Math.Max(0, gap) / 2;
            left += halfGap;
            top += halfGap;
            right -= halfGap;
            bottom -= halfGap;

            if (right - left < 80)
            {
                int center = (left + right) / 2;
                left = center - 40;
                right = center + 40;
            }
            if (bottom - top < 60)
            {
                int center = (top + bottom) / 2;
                top = center - 30;
                bottom = center + 30;
            }

            Rectangle result = Rectangle.FromLTRB(left, top, right, bottom);
            result.Intersect(workArea);
            return result;
        }

        public static bool SnapWindow(IntPtr hwnd, LayoutDef layout, int zoneIndex, Rectangle workArea, int gap, out Rectangle placedRect)
        {
            placedRect = Rectangle.Empty;
            if (hwnd == IntPtr.Zero || !Native.IsWindow(hwnd))
                return false;
            if (layout == null || zoneIndex < 0 || zoneIndex >= layout.Zones.Length)
                return false;

            Rectangle visible = CalculateZoneRect(layout, zoneIndex, workArea, gap);
            if (visible.Width < 40 || visible.Height < 40)
                return false;

            try
            {
                if (Native.IsIconic(hwnd) || Native.IsZoomed(hwnd))
                {
                    Native.ShowWindowAsync(hwnd, Native.SW_RESTORE);
                    Thread.Sleep(35);
                    Native.DwmFlush();
                }

                Native.RECT windowRect = new Native.RECT();
                Native.RECT extendedRect = new Native.RECT();
                bool hasExtended = Native.TryGetExtendedFrame(hwnd, out extendedRect) && Native.GetWindowRect(hwnd, out windowRect);

                int x = visible.Left;
                int y = visible.Top;
                int width = visible.Width;
                int height = visible.Height;

                if (hasExtended)
                {
                    int borderLeft = extendedRect.Left - windowRect.Left;
                    int borderTop = extendedRect.Top - windowRect.Top;
                    int borderRight = extendedRect.Right - windowRect.Right;
                    int borderBottom = extendedRect.Bottom - windowRect.Bottom;

                    x = visible.Left - borderLeft;
                    y = visible.Top - borderTop;
                    width = visible.Width - (borderRight - borderLeft);
                    height = visible.Height - (borderBottom - borderTop);
                }

                if (width < 40 || height < 40)
                {
                    x = visible.Left;
                    y = visible.Top;
                    width = visible.Width;
                    height = visible.Height;
                }

                bool moved = Native.SetWindowPos(
                    hwnd,
                    IntPtr.Zero,
                    x,
                    y,
                    width,
                    height,
                    Native.SWP_NOZORDER | Native.SWP_NOACTIVATE | Native.SWP_NOOWNERZORDER);

                if (!moved)
                {
                    Log.Info("SetWindowPos failed for hwnd=" + hwnd.ToInt64() + ".");
                    return false;
                }

                Native.SetForegroundWindow(hwnd);
                placedRect = visible;
                Log.Info(string.Format(
                    "Window snapped: hwnd={0}, layout={1}, zone={2}, rect={3},{4},{5},{6}",
                    hwnd.ToInt64(), layout.Name, layout.Zones[zoneIndex].Name,
                    visible.Left, visible.Top, visible.Width, visible.Height));
                return true;
            }
            catch (Exception ex)
            {
                Log.Error("Failed to snap the window.", ex);
                return false;
            }
        }

        public static bool IsEligibleForAssist(IntPtr hwnd, ICollection<IntPtr> exclude)
        {
            if (hwnd == IntPtr.Zero || !Native.IsWindow(hwnd))
                return false;
            if (exclude != null && exclude.Contains(hwnd))
                return false;
            if (!Native.IsWindowVisible(hwnd) || Native.IsIconic(hwnd) || Native.IsCloaked(hwnd))
                return false;
            if (Native.GetWindow(hwnd, Native.GW_OWNER) != IntPtr.Zero)
                return false;
            if (Util.IsShellWindow(hwnd) || Util.IsOwnProcess(hwnd))
                return false;
            long exStyle = Native.GetWindowExStyle(hwnd);
            if ((exStyle & Native.WS_EX_TOOLWINDOW) != 0 || (exStyle & Native.WS_EX_NOACTIVATE) != 0)
                return false;
            string title = Native.GetWindowTitle(hwnd);
            if (string.IsNullOrEmpty(title))
                return false;
            return true;
        }
    }

    internal sealed class WindowInfo
    {
        public IntPtr Handle;
        public string Title;
        public string ProcessName;
        public Icon Icon;
        public Rectangle Frame;

        public void DisposeIcon()
        {
            if (Icon != null)
            {
                Icon.Dispose();
                Icon = null;
            }
        }
    }

    internal static class WindowEnumerator
    {
        public static List<WindowInfo> GetEligibleWindows(ICollection<IntPtr> exclude, int maximum)
        {
            List<WindowInfo> result = new List<WindowInfo>();
            Native.EnumWindowsProc callback = delegate(IntPtr hwnd, IntPtr lParam)
            {
                if (result.Count >= maximum)
                    return false;
                if (!SnapEngine.IsEligibleForAssist(hwnd, exclude))
                    return true;

                Rectangle frame = Util.GetVisibleFrame(hwnd);
                if (frame.Width < 80 || frame.Height < 60)
                    return true;

                WindowInfo info = new WindowInfo();
                info.Handle = hwnd;
                info.Title = Util.Truncate(Native.GetWindowTitle(hwnd), 90);
                info.Frame = frame;
                try
                {
                    uint pid = Native.GetWindowProcessId(hwnd);
                    using (Process process = Process.GetProcessById((int)pid))
                        info.ProcessName = process.ProcessName;
                }
                catch
                {
                    info.ProcessName = string.Empty;
                }
                info.Icon = GetWindowIcon(hwnd);
                result.Add(info);
                return true;
            };

            Native.EnumWindows(callback, IntPtr.Zero);
            return result;
        }

        private static Icon GetWindowIcon(IntPtr hwnd)
        {
            try
            {
                IntPtr iconHandle = Native.SendMessage(hwnd, Native.WM_GETICON, new IntPtr(Native.ICON_BIG), IntPtr.Zero);
                if (iconHandle == IntPtr.Zero)
                    iconHandle = Native.SendMessage(hwnd, Native.WM_GETICON, new IntPtr(Native.ICON_SMALL2), IntPtr.Zero);
                if (iconHandle == IntPtr.Zero)
                    iconHandle = Native.GetClassIcon(hwnd, true);
                if (iconHandle != IntPtr.Zero)
                {
                    using (Icon temporary = Icon.FromHandle(iconHandle))
                        return (Icon)temporary.Clone();
                }
            }
            catch
            {
            }
            return null;
        }
    }
}
