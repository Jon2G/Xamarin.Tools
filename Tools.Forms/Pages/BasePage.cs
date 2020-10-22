using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tools.Enums;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform;



namespace Tools.Forms.Controls.Pages
{
    public class BasePage : ContentPage
    {
        public object Auxiliar { get; set; }
        public class PageOrientationEventArgs : EventArgs
        {
            public PageOrientationEventArgs(PageOrientation orientation)
            {
                Orientation = orientation;
            }

            public PageOrientation Orientation { get; }
        }

        public enum PageOrientation
        {
            Horizontal = 0,
            Vertical = 1,
        }

        private double _width;
        private double _height;
        public event EventHandler<PageOrientationEventArgs> OnOrientationChanged = (e, a) => { };
        private void InitOrientationPage()
        {
            _width = this.Width;
            _height = this.Height;
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            var oldWidth = _width;
            const double sizenotallocated = -1;

            base.OnSizeAllocated(width, height);
            if (Equals(_width, width) && Equals(_height, height)) return;

            _width = width;
            _height = height;

            // ignore if the previous height was size unallocated
            if (Equals(oldWidth, sizenotallocated)) return;

            // Has the device been rotated ?
            if (!Equals(width, oldWidth))
                OnOrientationChanged.Invoke(this, new PageOrientationEventArgs((width < height) ? PageOrientation.Vertical : PageOrientation.Horizontal));
        }
        public PageOrientation ActualOrientation
        {
            get => ((this.Width < this.Height) ? PageOrientation.Vertical : PageOrientation.Horizontal);
        }

        public BasePage()
        {
            this.LockedOrientation = DeviceOrientation.Other;
            this.IsModalLocked = false;
            InitOrientationPage();
            //IdentidadVisual.Inicializar(this);
        }
        public DeviceOrientation LockedOrientation { get; private set; }
        protected BasePage LockOrientation(DeviceOrientation Orientation)
        {
            this.LockedOrientation = Orientation;
            if (Device.RuntimePlatform == Device.Android)
            {
                MessagingCenter.Send(this, this.LockedOrientation.ToString());
            }
            return this;
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
        protected bool IsModalLocked { get; private set; }
        public BasePage LockModal()
        {
            this.IsModalLocked = !this.IsModalLocked;
            return this;
        }
        protected override bool OnBackButtonPressed()
        {
            if (Rg.Plugins.Popup.Services.PopupNavigation.Instance.PopupStack.LastOrDefault() is BasePopUp popUp)
            {
                popUp.BackButtonPressed();
                return true;
            }
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
        public void SetScreenMode(ScreenMode Screen)
        {
            Plugin.Xamarin.Tools.Shared.Services.ScreenManager.Current.SetScreenMode(Screen);
        }
    }
}
