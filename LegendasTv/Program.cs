using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

using Microsoft.VisualBasic.ApplicationServices;

namespace LegendasTv
{
    static class Program
    {
        static Form1 MainForm;
        [STAThread]
        static void Main(params string[] Arguments)
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            MainForm = new Form1();
            SingleInstanceApplication.Run(MainForm, NewInstanceHandler);
        }

        public static void NewInstanceHandler(object sender, StartupNextInstanceEventArgs e)
        {
            string param = e.CommandLine[1];
            e.BringToForeground = false;
            MainForm.StartWorkThread(e.CommandLine.ToArray());
        }

        public class SingleInstanceApplication : WindowsFormsApplicationBase
        {
            private SingleInstanceApplication()
            {
                base.IsSingleInstance = true;
            }

            public static void Run(Form f, StartupNextInstanceEventHandler startupHandler)
            {
                SingleInstanceApplication app = new SingleInstanceApplication();
                app.MainForm = f;
                app.StartupNextInstance += startupHandler;
                app.Run(Environment.GetCommandLineArgs());
            }
        }
    }
}
