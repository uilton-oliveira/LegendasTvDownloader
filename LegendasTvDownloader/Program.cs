using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Net;

using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading;

using Microsoft.VisualBasic.ApplicationServices;

namespace LegendasTvDownloader
{
    //static class Program
    //{
    //    //public static Form1 form1;               
    //    /// <summary>
    //    /// The main entry point for the application.
    //    /// </summary>
    //    [STAThread]
    //    static void Main()
    //    {
    //        Application.EnableVisualStyles();
    //        Application.SetCompatibleTextRenderingDefault(false);
    //        Form1 form1 = new Form1();
    //        Application.Run(form1);

            
    //    }        
    //}


    static class Program
    {
        static Form1 MainForm;
        [STAThread]
        static void Main(params string[] Arguments)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            MainForm = new Form1();
            SingleInstanceApplication.Run(MainForm, NewInstanceHandler);
        }

        public static void NewInstanceHandler(object sender, StartupNextInstanceEventArgs e)
        {
            string param = e.CommandLine[1];
            //MessageBox.Show(imageLocation);
            e.BringToForeground = false;
            MainForm.StartWorkThread(e.CommandLine.ToArray());
            //ControlPanel.uploadImage(imageLocation);
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
