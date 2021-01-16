using Kit.Daemon;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;

namespace DaemonTest
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        [DllImport("Kernel32")]
        public static extern void AllocConsole();

        [DllImport("Kernel32")]
        public static extern void FreeConsole();


        private void button1_Click(object sender, EventArgs e)
        {

        }
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            AllocConsole();
            Console.WriteLine("test");
            Start();
        }
        static async void Start()
        {
            TextWriterTraceListener myListener = new TextWriterTraceListener(System.Console.Out);
            Debug.Listeners.Add(myListener);

            Kit.WPF.Tools.Init();
            Init init = await Init.GetInstance().Initializate();
            Daemon.Current.Awake();
            Daemon.Current.SetPackageSize(0);

            Console.ReadKey();
        }
    }
}
