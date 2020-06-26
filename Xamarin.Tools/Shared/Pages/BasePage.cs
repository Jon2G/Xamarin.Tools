using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace Plugin.Xamarin.Tools.Shared.Pages
{
    public class BasePage : ContentPage
    {
        public BasePage()
        {
            this.LockedOrientation = DeviceOrientation.Other;
            this.IsModalLocked = false;
            //IdentidadVisual.Inicializar(this);
        }
        public DeviceOrientation LockedOrientation { get; private set; }
        protected void LockOrientation(DeviceOrientation Orientation)
        {
            this.LockedOrientation = Orientation;
            if (Device.RuntimePlatform == Device.Android)
            {
                MessagingCenter.Send(this, this.LockedOrientation.ToString());
            }
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
            //if (App.Current.MainPage is MainPage MP)
            //{
            //    MP.ActualizarSandwich(this);
            //}
        }
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            //if (App.Current.MainPage is MainPage MP)
            //{
            //    MP.ActualizarSandwich(this);
            //}
            if (LockedOrientation != DeviceOrientation.Other)
            {
                if (Device.RuntimePlatform == Device.Android)
                {
                    MessagingCenter.Send(this, nameof(DeviceOrientation.Other));
                }
                else if (Device.RuntimePlatform == Device.iOS)
                {

                }
            }

        }
        private bool IsModalLocked { get; set; }
        public void LockModal()
        {
            this.IsModalLocked = !this.IsModalLocked;
        }
        protected override bool OnBackButtonPressed()
        {
            if (this.IsModalLocked)
            {
                return true;
            }
            return base.OnBackButtonPressed();
        }
        public async void MenuPrincipal()
        {
            await Navigation.PopToRootAsync(true);

        }
    }
}
