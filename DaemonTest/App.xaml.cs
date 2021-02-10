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
using Kit;
using Kit.Sql.Helpers;
using OpenTK;
using Realms;

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
        public class Persona : RealmObject, MongoRealm.IPrimaryKeyId
        {
            [PrimaryKey]
            public long Id { get; set; }
            [Required]
            public string Nombre { get; set; }
        }
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            AllocConsole();
            Console.WriteLine("test");
            Kit.WPF.Tools.Init();
            Sqlh.Init(Tools.Instance.LibraryPath, Tools.Instance.Debugging);
            MongoRealm realm = new MongoRealm("Invis", Init.Version, true);
            Persona p = new Persona()
            {
                Nombre = "Juan"
            };
            realm.Add(p);

            var juon = realm.Instance.All<Persona>().ToList();

            var frost = realm.Detach(juon[0]);
            frost.Nombre = "Killer frost";
            realm.Update(frost);

            //Start();
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
