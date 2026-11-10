using System;
using System.Windows.Forms;
using Microsoft.Win32;

namespace SnapLayouts10
{
    internal static class AutoStart
    {
        private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string ValueName = "SnapLayouts10";

        public static bool IsEnabled()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RunKey, false))
                {
                    if (key == null)
                        return false;
                    string value = key.GetValue(ValueName) as string;
                    if (string.IsNullOrEmpty(value))
                        return false;
                    string expected = Quote(Application.ExecutablePath);
                    return string.Equals(value.Trim(), expected, StringComparison.OrdinalIgnoreCase);
                }
            }
            catch
            {
                return false;
            }
        }

        public static bool SetEnabled(bool enabled)
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(RunKey))
                {
                    if (key == null)
                        return false;
                    if (enabled)
                        key.SetValue(ValueName, Quote(Application.ExecutablePath), RegistryValueKind.String);
                    else if (key.GetValue(ValueName) != null)
                        key.DeleteValue(ValueName, false);
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Error("Nu am putut modifica pornirea automată.", ex);
                return false;
            }
        }

        private static string Quote(string path)
        {
            return "\"" + path + "\"";
        }
    }
}
