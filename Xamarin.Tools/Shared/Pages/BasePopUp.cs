using Rg.Plugins.Popup.Animations;
using Rg.Plugins.Popup.Enums;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Plugin.Xamarin.Tools.Shared.Pages
{
    public class BasePopUp : PopupPage
    {
        public event EventHandler Confirmado;
        protected void InvokeConfirmado(object sender, EventArgs e)
        {
            Confirmado?.Invoke(sender, e);
        }
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            Confirmado?.Invoke(this, null);
        }
        public async Task<BasePopUp> Mostrar()
        {
            var scaleAnimation = new ScaleAnimation
            {
                PositionIn = MoveAnimationOptions.Right,
                PositionOut = MoveAnimationOptions.Left
            };
            this.Animation = scaleAnimation;
            await PopupNavigation.Instance.PushAsync(this, true);
            return this;
        }
        public virtual async Task<BasePopUp> Close()
        {
            await PopupNavigation.Instance.RemovePageAsync(this, true);
            return this;
        }

        private bool IsModalLocked { get; set; }
        public BasePopUp LockModal()
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
            else
            {
                return false;
            }
        }
        public async void BackButtonPressed()
        {
            if (!OnBackButtonPressed())
            {
                await this.Close();
            }
        }
    }
}
