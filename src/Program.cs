using System;
using System.Threading;
using System.Windows.Forms;

namespace SnapLayouts10
{
    internal static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            bool debug = false;
            bool openSettings = false;
            for (int i = 0; i < args.Length; i++)
            {
                if (string.Equals(args[i], "--debug", StringComparison.OrdinalIgnoreCase))
                    debug = true;
                if (string.Equals(args[i], "--settings", StringComparison.OrdinalIgnoreCase))
                    openSettings = true;
            }
            Log.DebugEnabled = debug;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += delegate(object sender, ThreadExceptionEventArgs e)
            {
                Log.Error("Unhandled exception on the UI thread.", e.Exception);
            };
            AppDomain.CurrentDomain.UnhandledException += delegate(object sender, UnhandledExceptionEventArgs e)
            {
                Log.Error("Unhandled exception.", e.ExceptionObject as Exception);
            };

            bool createdNew;
            using (Mutex mutex = new Mutex(true, @"Local\SnapLayouts10.SingleInstance", out createdNew))
            {
                if (!createdNew)
                {
                    MessageBox.Show(
                        "SnapLayouts 10 is already running. Look for its icon in the system tray.",
                        "SnapLayouts 10",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                try
                {
                    Log.Info("=== SnapLayouts 10 v" + Util.VersionString() + " starting ===");
                    Application.Run(new TrayApp(openSettings));
                }
                catch (Exception ex)
                {
                    Log.Error("The application stopped because of an error.", ex);
                    MessageBox.Show(
                        "SnapLayouts 10 encountered an error. See the log file:\n" + Log.LogPath,
                        "SnapLayouts 10",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
                finally
                {
                    try
                    {
                        mutex.ReleaseMutex();
                    }
                    catch
                    {
                    }
                }
            }
        }
    }
}
