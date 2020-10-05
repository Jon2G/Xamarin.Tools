using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Tools.Forms.Controls.NotificationBar
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CustomToolBar : ContentView, INotificaciones
    {
        public static readonly BindableProperty IsProgressVisibleProperty = BindableProperty.Create(
            propertyName: nameof(IsProgressVisible), returnType: typeof(bool), declaringType: typeof(CustomToolBar), defaultValue: false);
        public bool IsProgressVisible
        {
            get { return (bool)GetValue(IsProgressVisibleProperty); }
            set
            {
                SetValue(IsProgressVisibleProperty, value);
                OnPropertyChanged();
            }
        }
        public static readonly BindableProperty IsSandwichVisibleProperty = BindableProperty.Create(
            propertyName: nameof(IsSandwichVisible), returnType: typeof(bool), declaringType: typeof(CustomToolBar), defaultValue: false);
        public bool IsSandwichVisible
        {
            get { return (bool)GetValue(IsSandwichVisibleProperty); }
            set
            {
                SetValue(IsSandwichVisibleProperty, value);
                OnPropertyChanged();
            }
        }

        public static readonly BindableProperty ProgressSourceProperty = BindableProperty.Create(
            propertyName: nameof(IsProgressVisible), returnType: typeof(Binding), declaringType: typeof(CustomToolBar), defaultValue: null);
        public Binding ProgressSource
        {
            get { return (Binding)GetValue(ProgressSourceProperty); }
            set
            {
                SetValue(ProgressSourceProperty, value);
                OnPropertyChanged();
            }
        }

        private void ToogleMenu(object sender, EventArgs e)
        {
            if (Application.Current.MainPage is MasterDetailPage master)
            {
                master.IsPresented = !master.IsPresented;
            }
        }
        public CustomToolBar()
        {
            InitializeComponent();
        }

        private void WarningOffLine(VisualElement sender, TouchEffect.EventArgs.TouchCompletedEventArgs args)
        {
            Acr.UserDialogs.UserDialogs.Instance.Alert("No fue posible conectarse al servidor,esto puede impedir o dificultar la operación.", "Alerta", "Ok");
        }

        private void Click(object sender, EventArgs e)
        {
            if (sender is Grid grid && grid.BindingContext is Notificacion notificacion)
            {
                notificacion.Command.Execute(sender);
            }
        }

        void INotificaciones.Refresh()
        {
            this.BackgroundColor = Color.FromHex(Notificaciones.Instance.Color);
        }
    }
}