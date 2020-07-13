using Plugin.Xamarin.Tools.Shared;
using Plugin.Xamarin.Tools.Shared.Blumitech;
using SQLHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Xamarin.Forms;

namespace WpfSample
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Xamarin.Forms.Forms.Init();
            Prueba();
        }
        private async void Prueba()
        {
            Plugin.Xamarin.Tools.UWP.Tools.Init();
            ProjectActivationState state
                = await Plugin.Xamarin.Tools.Shared.Blumitech.Licence.Instance(Tools.Instance).Autheticate("InventarioFisico");
            switch (state)
            {
                case ProjectActivationState.Active:
                    Log.LogMe("Project is active");
                    break;
                case ProjectActivationState.Expired:
                    //await Acr.UserDialogs.UserDialogs.Instance.AlertAsync("La licencia para usar esta aplicación ha expirado", "Acceso denegado");
                    //TODO: Abrir pagina de planes en el navegador,mostrar la fecha del vencimiento
                    return;
                case ProjectActivationState.Denied:
                    Log.LogMe("Acces denied");
                    //await Acr.UserDialogs.UserDialogs.Instance.AlertAsync("Este dispositivo no cuenta con la licencia para usar esta aplicación", "Acceso denegado");
                    return;
                case ProjectActivationState.LoginRequired:
                    //await Acr.UserDialogs.UserDialogs.Instance.AlertAsync("Este dispositivo debe ser registrado con una licencia valida antes de poder acceder a la aplicación", "Acceso denegado");
                    break;
                case ProjectActivationState.ConnectionFailed:
                    //Revisa tu coneccion a internet
                    break;
            }
        }
    }
}
