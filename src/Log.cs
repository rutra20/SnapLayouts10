using System;
using System.IO;
using System.Text;

namespace SnapLayouts10
{
    internal static class Log
    {
        private static readonly object Sync = new object();
        private static string _path;

        public static bool DebugEnabled;

        public static string DataDirectory
        {
            get
            {
                string dir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "SnapLayouts10");
                Directory.CreateDirectory(dir);
                return dir;
            }
        }

        public static string LogPath
        {
            get
            {
                if (_path == null)
                    _path = Path.Combine(DataDirectory, "snap.log");
                return _path;
            }
        }

        public static void Info(string message)
        {
            Write("INFO", message);
        }

        public static void Debug(string message)
        {
            if (DebugEnabled)
                Write("DEBUG", message);
        }

        public static void Error(string message, Exception exception)
        {
            Write("ERROR", message + (exception == null ? string.Empty : Environment.NewLine + exception));
        }

        private static void Write(string level, string message)
        {
            try
            {
                lock (Sync)
                {
                    string path = LogPath;
                    FileInfo info = new FileInfo(path);
                    if (info.Exists && info.Length > 262144)
                    {
                        string old = Path.Combine(DataDirectory, "snap.old.log");
                        try
                        {
                            if (File.Exists(old))
                                File.Delete(old);
                            File.Move(path, old);
                        }
                        catch
                        {
                        }
                    }

                    string line = string.Format(
                        "{0:yyyy-MM-dd HH:mm:ss.fff} [{1}] {2}{3}",
                        DateTime.Now, level, message, Environment.NewLine);
                    File.AppendAllText(path, line, Encoding.UTF8);
                }
            }
            catch
            {
            }
        }
    }
}
