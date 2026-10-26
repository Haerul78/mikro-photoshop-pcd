using System;
using System.Threading;
using System.Windows.Forms;

namespace PengolahanCitra
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Error yang tidak tertangkap ditampilkan sebagai pesan, bukan menutup aplikasi
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += OnThreadException;

            Application.Run(new MainForm());
        }

        private static void OnThreadException(object sender, ThreadExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception.Message, "Terjadi kesalahan", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
