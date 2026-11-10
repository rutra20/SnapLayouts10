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
                Log.Error("Excepție necontrolată pe firul UI.", e.Exception);
            };
            AppDomain.CurrentDomain.UnhandledException += delegate(object sender, UnhandledExceptionEventArgs e)
            {
                Log.Error("Excepție necontrolată.", e.ExceptionObject as Exception);
            };

            bool createdNew;
            using (Mutex mutex = new Mutex(true, @"Local\SnapLayouts10.SingleInstance", out createdNew))
            {
                if (!createdNew)
                {
                    MessageBox.Show(
                        "SnapLayouts 10 rulează deja. Căutați iconița din System Tray.",
                        "SnapLayouts 10",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                try
                {
                    Log.Info("=== Pornire SnapLayouts 10 v" + Util.VersionString() + " ===");
                    Application.Run(new TrayApp(openSettings));
                }
                catch (Exception ex)
                {
                    Log.Error("Aplicația s-a oprit din cauza unei erori.", ex);
                    MessageBox.Show(
                        "SnapLayouts 10 a întâmpinat o eroare. Detalii în jurnalul din:\n" + Log.LogPath,
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
