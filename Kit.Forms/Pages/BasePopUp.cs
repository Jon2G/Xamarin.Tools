using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Kit.Services.Interfaces;
using Rg.Plugins.Popup.Animations;
using Rg.Plugins.Popup.Enums;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;

namespace Kit.Forms.Pages
{
    public class BasePopUp : PopupPage, ICrossWindow
    {
        #region ICrossWindow

        Task ICrossWindow.Close() => Close();

        Task ICrossWindow.Show() => Show();

        Task ICrossWindow.ShowDialog() => ShowDialog();

        #endregion ICrossWindow

        public ICommand ClosedCommad;
        private readonly AutoResetEvent ShowDialogCallback;

        public BasePopUp()
        {
            this.ShowDialogCallback = new AutoResetEvent(false);
            this.Visual = VisualMarker.Material;
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

        public virtual async Task<BasePopUp> Close()
        {
            Closing();
            await PopupNavigation.Instance.RemovePageAsync(this, true);
            this.ShowDialogCallback.Set();
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