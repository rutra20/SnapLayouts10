using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace SnapLayouts10
{
    internal sealed class AppSettings
    {
        public bool Enabled = true;
        public int HoverDelayMs = 300;
        public int Gap = 8;
        public bool SnapAssist = true;
        public bool StartWithWindows = false;
        public bool FirstRun = true;
        public bool[] LayoutEnabled = new bool[Layouts.Count];

        public AppSettings()
        {
            for (int i = 0; i < LayoutEnabled.Length; i++)
                LayoutEnabled[i] = true;
        }

        public static string SettingsPath
        {
            get { return Path.Combine(Log.DataDirectory, "settings.ini"); }
        }

        public static bool Exists
        {
            get { return File.Exists(SettingsPath); }
        }

        public static AppSettings Load()
        {
            AppSettings settings = new AppSettings();
            try
            {
                if (!File.Exists(SettingsPath))
                    return settings;

                string[] lines = File.ReadAllLines(SettingsPath, Encoding.UTF8);
                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i].Trim();
                    if (line.Length == 0 || line.StartsWith("#"))
                        continue;
                    int equals = line.IndexOf('=');
                    if (equals <= 0)
                        continue;
                    string key = line.Substring(0, equals).Trim().ToLowerInvariant();
                    string value = line.Substring(equals + 1).Trim();
                    switch (key)
                    {
                        case "enabled":
                            settings.Enabled = ParseBool(value, settings.Enabled);
                            break;
                        case "hoverdelayms":
                            settings.HoverDelayMs = ParseInt(value, settings.HoverDelayMs, 100, 2000);
                            break;
                        case "gap":
                            settings.Gap = ParseInt(value, settings.Gap, 0, 24);
                            break;
                        case "snapassist":
                            settings.SnapAssist = ParseBool(value, settings.SnapAssist);
                            break;
                        case "startwithwindows":
                            settings.StartWithWindows = ParseBool(value, settings.StartWithWindows);
                            break;
                        case "firstrun":
                            settings.FirstRun = ParseBool(value, settings.FirstRun);
                            break;
                        case "layouts":
                            ParseLayouts(settings, value);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("Failed to read settings; using defaults.", ex);
            }
            return settings;
        }

        public void Save()
        {
            try
            {
                StringBuilder builder = new StringBuilder();
                builder.AppendLine("# SnapLayouts 10 - settings");
                builder.AppendLine("enabled=" + (Enabled ? "1" : "0"));
                builder.AppendLine("hoverdelayms=" + HoverDelayMs.ToString(CultureInfo.InvariantCulture));
                builder.AppendLine("gap=" + Gap.ToString(CultureInfo.InvariantCulture));
                builder.AppendLine("snapassist=" + (SnapAssist ? "1" : "0"));
                builder.AppendLine("startwithwindows=" + (StartWithWindows ? "1" : "0"));
                builder.AppendLine("firstrun=" + (FirstRun ? "1" : "0"));
                builder.Append("layouts=");
                for (int i = 0; i < LayoutEnabled.Length; i++)
                {
                    if (i > 0)
                        builder.Append(',');
                    builder.Append(LayoutEnabled[i] ? '1' : '0');
                }
                builder.AppendLine();

                string path = SettingsPath;
                string temp = path + ".tmp";
                File.WriteAllText(temp, builder.ToString(), new UTF8Encoding(true));
                if (File.Exists(path))
                    File.Replace(temp, path, null);
                else
                    File.Move(temp, path);
            }
            catch (Exception ex)
            {
                Log.Error("Failed to save settings.", ex);
            }
        }

        public bool IsLayoutEnabled(int index)
        {
            if (index < 0 || index >= LayoutEnabled.Length)
                return false;
            return LayoutEnabled[index];
        }

        public int EnabledLayoutCount
        {
            get
            {
                int count = 0;
                for (int i = 0; i < LayoutEnabled.Length; i++)
                    if (LayoutEnabled[i])
                        count++;
                return count;
            }
        }

        public void ResetLayouts()
        {
            for (int i = 0; i < LayoutEnabled.Length; i++)
                LayoutEnabled[i] = true;
        }

        private static bool ParseBool(string value, bool fallback)
        {
            if (value == "1" || value.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("yes", StringComparison.OrdinalIgnoreCase) || value.Equals("da", StringComparison.OrdinalIgnoreCase))
                return true;
            if (value == "0" || value.Equals("false", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("no", StringComparison.OrdinalIgnoreCase) || value.Equals("nu", StringComparison.OrdinalIgnoreCase))
                return false;
            return fallback;
        }

        private static int ParseInt(string value, int fallback, int min, int max)
        {
            int parsed;
            if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsed))
                return fallback;
            return Util.Clamp(parsed, min, max);
        }

        private static void ParseLayouts(AppSettings settings, string value)
        {
            string[] parts = value.Split(',');
            for (int i = 0; i < settings.LayoutEnabled.Length; i++)
            {
                if (i < parts.Length)
                    settings.LayoutEnabled[i] = ParseBool(parts[i].Trim(), settings.LayoutEnabled[i]);
                else
                    settings.LayoutEnabled[i] = false;
            }
            if (settings.EnabledLayoutCount == 0)
                settings.ResetLayouts();
        }
    }
}
