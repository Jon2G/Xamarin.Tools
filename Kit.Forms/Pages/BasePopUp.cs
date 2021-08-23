using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Rg.Plugins.Popup.Animations;
using Rg.Plugins.Popup.Enums;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using Command = Kit.Extensions.Command;

namespace Kit.Forms.Pages
{
    public class BasePopUp : PopupPage
    {
        public ICommand ConfirmedCommand;

        [Obsolete("Use ConfirmedCommand")]
        public event EventHandler Confirmado;

        public ICommand ClosedCommad;

        [Obsolete("Use ClosedCommad")]
        public event EventHandler OnClosed;

        private readonly AutoResetEvent ShowDialogCallback;

        public BasePopUp()
        {
            this.ShowDialogCallback = new AutoResetEvent(false);
        }

      

        protected void InvokeConfirmado(object sender, EventArgs e)
        {
            Confirmado?.Invoke(sender, e);
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            Confirmado?.Invoke(this, null);
        }

        public virtual async Task<BasePopUp> ShowDialog()
        {
            await Show();
            await Task.Run(() => this.ShowDialogCallback.WaitOne());
            return this;
        }

        public virtual async Task<BasePopUp> Show()
        {
            ScaleAnimation scaleAnimation = new ScaleAnimation
            {
                PositionIn = MoveAnimationOptions.Right,
                PositionOut = MoveAnimationOptions.Left
            };
            this.Animation = scaleAnimation;
            await PopupNavigation.Instance.PushAsync(this, true);
            return this;
        }

        public virtual async void Confirm(object obj = null, bool close = true)
        {
            ConfirmedCommand?.Execute(obj ?? this);
            if (close)
            {
                await Close();
            }
        }

        public virtual async Task<BasePopUp> Close()
        {
            Closing();
            await PopupNavigation.Instance.RemovePageAsync(this, true);
            this.ShowDialogCallback.Set();
            OnClosed?.Invoke(this, EventArgs.Empty);
            ClosedCommad?.Execute(this);
            return this;
        }

        protected virtual void Closing()
        {
        }

        private bool IsModalLocked { get; set; }

        public BasePopUp LockModal()
        {
            this.IsModalLocked = !this.IsModalLocked;
            this.CloseWhenBackgroundIsClicked = !this.IsModalLocked;
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