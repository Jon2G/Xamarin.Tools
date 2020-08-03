using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using XFGloss;
using Xamarin.Forms.Platform;


#if WINDOWS_UWP
using Windows.UI.Xaml.Media;
using Xamarin.Forms.Platform.UWP;
using UWPRender = Xamarin.Forms.Platform.UWP.PageRenderer;
#endif



namespace Plugin.Xamarin.Tools.Shared.Pages
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
        public BasePage LockModal()
        {
            this.IsModalLocked = !this.IsModalLocked;
            return this;
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

        public void SetGradientBackground(Gradient gradient)
        {
            if (Device.RuntimePlatform == Device.UWP)
            {
#if WINDOWS_UWP
                LinearGradientBrush gradiente = new LinearGradientBrush();
                foreach (GradientStep step in gradient.Steps)
                {
                    gradiente.GradientStops.Add(new GradientStop()
                    {
                        Color = step.StepColor.ToWindowsColor(),
                        Offset = step.StepPercentage
                    });
                }
                IVisualElementRenderer render = this.GetOrCreateRenderer();
                (render as UWPRender).Background = gradiente;
#endif
            }
            else
            {
                ContentPageGloss.SetBackgroundGradient(this, gradient);
            }
        }
    }
}
