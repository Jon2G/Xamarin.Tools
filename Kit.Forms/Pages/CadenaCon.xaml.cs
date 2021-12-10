extern alias SharedZXingNet;

using SharedZXingNet::ZXing;

using Acr.UserDialogs;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;
using Kit.Forms.Controls;
using Kit.Forms.Fonts;
using Kit.Forms.Services;
using Kit.Services.BarCode;
using Xamarin.Essentials;
using Kit.SetUpConnectionString;
using AsyncAwaitBestPractices.MVVM;
using System.Windows.Input;

namespace Kit.Forms.Pages
{
    extern alias SharedZXingNet;

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CadenaCon : BasePage
    {
        public Lector Leector { get; set; }

        public ICommand Confirmado { get; set; }

        public SetUpConnectionStringViewModelBase Model { get; set; }

        public static bool IsActive()
        {
            if ((Application.Current.MainPage.Navigation.ModalStack.Any() && Application.Current.MainPage.Navigation.ModalStack.LastOrDefault() is CadenaCon)
                || (Application.Current.MainPage.Navigation.NavigationStack.Any() && Application.Current.MainPage.Navigation.NavigationStack.LastOrDefault() is CadenaCon))
            {
                return true;
            }
            return false;
        }

        private Kit.Extensions.Command<CadenaCon> OnAppearingCommand;
        private readonly CancellationTokenSource CancellationTokenSource;

        public CadenaCon(IDbConnection DBConection) : this(DBConection, null, null)
        {
        }

        public CadenaCon(IDbConnection DBConection, Exception exception = null) : this(DBConection, null, exception)
        {
        }

        public CadenaCon(IDbConnection DBConection, Configuracion Configuracion) : this(DBConection, Configuracion, null)
        {
        }

        public CadenaCon(IDbConnection DBConection, Configuracion Configuracion, Exception exception)
        {
            try
            {
                this.CancellationTokenSource = new CancellationTokenSource();
                var config = Configuracion ?? Configuracion.ObtenerConfiguracion(DBConection, Daemon.Devices.Device.Current.DeviceId);
                this.Model = new SetUpConnectionStringViewModelBase(DBConection, new SqlConnection(config.CadenaCon), config);
                this.BindingContext = this.Model;
                InitializeComponent();
                ToogleStatus(exception);
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Al leer la cadena de conexion para editar desde cadenacon.cs");
            }
        }

        public async void FocusOnImport()
        {
            await Task.Delay(500);
            await ScrollView.ScrollToAsync(BtnImportConnection, ScrollToPosition.End, true);
            GlowAnimation(BtnImportConnection, this.CancellationTokenSource.Token);
        }

        private static async void GlowAnimation(VisualElement element, CancellationToken cancellation)
        {
            while (!cancellation.IsCancellationRequested)
            {
                await Task.Delay(200);
                await element.FadeTo(.7, 100);
                await element.ScaleTo(1);
                await Task.Delay(200);
                await element.FadeTo(1, 250);
                await element.ScaleTo(1.1);
            }
        }

        public async Task ShowAndExecute(Kit.Extensions.Command<CadenaCon> Command)
        {
            this.OnAppearingCommand = Command;
            await Xamarin.Forms.Application.Current.MainPage.Navigation.PushModalAsync(this);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            this.LockOrientation(DeviceOrientation.Portrait);
            this.Model.Configuration.RefreshConnectionString();
            this.OnAppearingCommand?.Execute(this);
        }

        protected override void OnDisappearing()
        {
            this.CancellationTokenSource.Cancel();
            base.OnDisappearing();
        }

        private async void Guardar(object sender, EventArgs e)
        {
            using (Acr.UserDialogs.UserDialogs.Instance.Loading("Espere un momento..."))
            {
                await GuardarAsync();
            }
        }

        private async Task GuardarAsync()
        {
            await Task.Yield();
            try
            {
                Exception ex;
                //using (Acr.UserDialogs.UserDialogs.Instance.Loading("Intentando conectar..."))
                //{
                ex = this.Model.TestConnection();
                //}
                ToogleStatus(ex);
                if (ex != null)
                {
                    await Acr.UserDialogs.UserDialogs.Instance.AlertAsync($"La conexión actual no es valida.\n{ex.Message}", "Mensaje informativo");
                }
                else
                {
                    this.Model.Save();

                    if (this.Navigation.ModalStack.Count > 0)
                    {
                        await Navigation.PopModalAsync();
                    }
                    Confirmado?.Execute(this);
                    Confirmado = null;
                }
            }
            catch (Exception exx)
            {
                Log.Logger.Error(exx, "Al intentar cambiar la cadena de conexión desde CadenaCon.cs GuardarAsync");
                await Acr.UserDialogs.UserDialogs.Instance.AlertAsync(exx.Message, "Alerta");
            }
        }

        private void Cancelar(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        protected override bool OnBackButtonPressed()
        {
            if (this.IsModalLocked)
            {
                return true;
            }
            Navigation.PopModalAsync(true);
            Confirmado?.Execute(this);
            Confirmado = null;
            return true;
        }

        private async void ProbarConexion(object sender, EventArgs e)
        {
            UserDialogs.Instance.ShowLoading("Intentando conectar...", MaskType.Black);
            await ProbarConexionAsync();
            UserDialogs.Instance.HideLoading();
        }

        private async Task<bool> ProbarConexionAsync(bool MostrarMensajesStatus = true)
        {
            await Task.Yield();

            Exception ex = this.Model.TestConnection();
            ToogleStatus(ex);
            if (ex != null)
            {
                if (MostrarMensajesStatus)
                {
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        await Acr.UserDialogs.UserDialogs.Instance.AlertAsync($"La cadena de conexión no es valida.\n{ex.Message}", "Mensaje informativo");
                    });
                }
                return false;
            }
            if (MostrarMensajesStatus)
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await Acr.UserDialogs.UserDialogs.Instance.AlertAsync("Conexión correcta", "Mensaje informativo");
                });
            }
            return true;
        }

        private void ToogleStatus(Exception ex)
        {
            TxtEstado.Text = ex?.Message ?? "Correcto";
            LblIcon.Text = ex is null ? FontelloIcons.Ok : FontelloIcons.Cross;
            LblIcon.TextColor = ex is null ? Color.Green : Color.Red;
        }

        private async void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            await Acr.UserDialogs.UserDialogs.Instance.AlertAsync(TxtEstado.Text, "Información", "Ok");
        }

        private async void GuardarRenovar_Touched(object sender, EventArgs e)
        {
            using (Acr.UserDialogs.UserDialogs.Instance.Loading("Espere un momento..."))
            {
                if (EliminarDB(this.Model.SqLite.Database))
                {
                    Log.Logger.Warning("Se elimino la base de datos");
                    await Acr.UserDialogs.UserDialogs.Instance.AlertAsync("Se elimino la base de datos local", "Atención", "Ok");
                    Guardar(sender, e);
                }
            }
        }

        public bool EliminarDB(string RutaDb)
        {
            try
            {
                FileInfo file = new FileInfo(RutaDb);
                if (file.Exists)
                {
                    File.Delete(RutaDb);
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Al eliminar la base de datos de sqlite");
                return false;
            }
        }

        private void Importar_Touched(object sender, EventArgs e)
        {
            ActionSheetConfig config = new ActionSheetConfig()
            {
                Title = "Importar cadena de conexión",
                Cancel = new ActionSheetOption("Cancelar"),
                Options = new List<ActionSheetOption>()
                {
                    new ActionSheetOption( "Usar camara",()=>Task.Run(FromCamera)),
                    new ActionSheetOption("Seleccionar desde la galería",()=>Task.Run(FromGallery))
                },
                UseBottomSheet = true
            };
            UserDialogs.Instance.ActionSheet(config);
        }

        private async void FromGallery()
        {
            FileResult qr = await MediaPicker.PickPhotoAsync(new MediaPickerOptions()
            {
                Title = "Importar cadena de conexión"
            });
            if (qr is null)
            {
                return;
            }
            await Task.Delay(500);
            using (Acr.UserDialogs.UserDialogs.Instance.Loading("Leyendo qr..."))
            {
                await ReadFromGallery(qr);
            }
        }

        private async Task ReadFromGallery(FileResult qr)
        {
            SharedZXingNet::ZXing.Result result = null;
            var qr_file = await qr.LoadPhotoAsync();

            using (MemoryStream memory = new MemoryStream())
            {
                using (FileStream file = new FileStream(qr_file.FullName, FileMode.Open, FileAccess.Read))
                {
                    await file.CopyToAsync(memory);
                }
                BarcodeDecoding reader = new BarcodeDecoding();

                result = reader.Decode(memory, SharedZXingNet::ZXing.BarcodeFormat.QR_CODE
                   , new[]
                   {
                    new KeyValuePair<SharedZXingNet::ZXing.DecodeHintType, object>(SharedZXingNet::ZXing.DecodeHintType.TRY_HARDER,null)
                   });
            }
            await Deserialize(result?.Text);
        }

        private void FromCamera()
        {
            if (this.Leector is null)
            {
                this.Leector = new Lector(BarcodeFormat.QR_CODE)
                {
                    OnCodeReadCommand = new AsyncCommand<string>(Deserialize)
                };
            }
            this.Leector.Abrir();
        }

        private async Task Deserialize(string json)
        {
            await Task.Yield();
            if (!string.IsNullOrEmpty(json))
            {
                Configuracion configuracion_qr = Configuracion.DeSerialize(json);
                if (configuracion_qr != null)
                {
                    this.Model.Configuration = configuracion_qr;
                    this.Model.Configuration.RefreshConnectionString();
                }
                else
                {
                    Acr.UserDialogs.UserDialogs.Instance.Alert("Formato Qr incorrecto", "Incorrecto", "Ok");
                }
            }
        }
    }
}