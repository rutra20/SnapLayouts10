using System;
using System.Drawing;
using Microsoft.Win32;

namespace SnapLayouts10
{
    internal static class Theme
    {
        private static readonly bool Light = ReadLightMode();
        private static readonly Color AccentValue = ReadAccentColor();

        public static bool IsLight
        {
            get { return Light; }
        }

        public static Color Background
        {
            get { return Light ? Color.FromArgb(243, 243, 243) : Color.FromArgb(32, 32, 32); }
        }

        public static Color Card
        {
            get { return Light ? Color.FromArgb(255, 255, 255) : Color.FromArgb(44, 44, 44); }
        }

        public static Color CardHover
        {
            get { return Light ? Color.FromArgb(235, 235, 235) : Color.FromArgb(56, 56, 56); }
        }

        public static Color Border
        {
            get { return Light ? Color.FromArgb(206, 206, 206) : Color.FromArgb(68, 68, 68); }
        }

        public static Color CardBorder
        {
            get { return Light ? Color.FromArgb(218, 218, 218) : Color.FromArgb(74, 74, 74); }
        }

        public static Color Zone
        {
            get { return Light ? Color.FromArgb(218, 218, 218) : Color.FromArgb(70, 70, 70); }
        }

        public static Color ZoneHover
        {
            get { return Light ? Color.FromArgb(232, 232, 232) : Color.FromArgb(88, 88, 88); }
        }

        public static Color ZoneBorder
        {
            get { return Light ? Color.FromArgb(140, 140, 140) : Color.FromArgb(122, 122, 122); }
        }

        public static Color Text
        {
            get { return Light ? Color.FromArgb(28, 28, 28) : Color.FromArgb(245, 245, 245); }
        }

        public static Color SubText
        {
            get { return Light ? Color.FromArgb(96, 96, 96) : Color.FromArgb(176, 176, 176); }
        }

        public static Color Accent
        {
            get { return AccentValue; }
        }

        public static Color AccentBorder
        {
            get { return Light ? Darken(AccentValue, 0.18f) : Lighten(AccentValue, 0.35f); }
        }

        public static Color Panel
        {
            get { return Light ? Color.FromArgb(250, 250, 250) : Color.FromArgb(38, 38, 38); }
        }

        public static Color MenuBackground
        {
            get { return Light ? Color.FromArgb(249, 249, 249) : Color.FromArgb(32, 32, 32); }
        }

        public static Color MenuHover
        {
            get { return Light ? Color.FromArgb(226, 226, 226) : Color.FromArgb(52, 52, 52); }
        }

        public static Font Font(float pixelSize, FontStyle style)
        {
            return new Font("Segoe UI", pixelSize, style, GraphicsUnit.Pixel);
        }

        public static Font FontPoints(float points, FontStyle style)
        {
            return new Font("Segoe UI", points, style, GraphicsUnit.Point);
        }

        public static Color Lighten(Color color, float amount)
        {
            return Color.FromArgb(
                color.A,
                (int)Math.Min(255, color.R + (255 - color.R) * amount),
                (int)Math.Min(255, color.G + (255 - color.G) * amount),
                (int)Math.Min(255, color.B + (255 - color.B) * amount));
        }

        public static Color Darken(Color color, float amount)
        {
            return Color.FromArgb(
                color.A,
                (int)Math.Max(0, color.R * (1f - amount)),
                (int)Math.Max(0, color.G * (1f - amount)),
                (int)Math.Max(0, color.B * (1f - amount)));
        }

        private static bool ReadLightMode()
        {
            try
            {
                object value = Registry.GetValue(
                    @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize",
                    "AppsUseLightTheme", 1);
                if (value is int)
                    return (int)value != 0;
            }
            catch
            {
            }
            return false;
        }

        private static Color ReadAccentColor()
        {
            try
            {
                object value = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\DWM", "ColorizationColor", null);
                if (value is int)
                {
                    int argb = (int)value;
                    int rgb = argb & 0x00FFFFFF;
                    int r = (rgb >> 16) & 0xFF;
                    int g = (rgb >> 8) & 0xFF;
                    int b = rgb & 0xFF;
                    if (r + g + b > 30)
                    {
                        if (Light && r + g + b > 620)
                            return Color.FromArgb(0, 103, 192);
                        return Color.FromArgb(r, g, b);
                    }
                }
            }
            catch
            {
            }
            return Color.FromArgb(0, 120, 212);
        }
    }
}
