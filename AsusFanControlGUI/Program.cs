using System;
using System.Linq;
using System.Windows.Forms;

namespace AsusFanControlGUI
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            var args = Environment.GetCommandLineArgs();
            var isSilent = args.Contains("--silent");
            if (isSilent)
            {
                var service = BackgroundWorkerService.GetInstance();
                service.StartListener();
            }
            else
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form1());
            }
        }
    }
}