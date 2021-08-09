using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GurbaniDesktopApp
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            bool result;
            var mutex = new System.Threading.Mutex(true, "GurbaniDesktopApp", out result);

            if (!result)
            {
                MessageBox.Show("Another instance is already running.");
                return;
            }
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new FormConsole());
                GC.KeepAlive(mutex);
            }
            finally
            {
                mutex.ReleaseMutex();
            }


        }
    }
}
