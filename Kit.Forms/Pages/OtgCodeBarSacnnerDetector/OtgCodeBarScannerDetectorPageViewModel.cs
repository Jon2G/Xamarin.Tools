using AsyncAwaitBestPractices;
using Kit.Forms.Services;
using Kit.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Kit.Forms.Pages.OtgCodeBarSacnnerDetector
{
    internal class OtgCodeBarScannerDetectorPageViewModel : ModelBase
    {
        private string _Code;

        public string Code
        {
            get => _Code;
            set
            {
                _Code = value;
                Raise(() => Code);
            }
        }
        private View EnterView { get; set; }

        internal void Init(View EnterView)
        {
            this.EnterView = EnterView;
            Services.IKeyboardListenerService listenerService = new Services.IKeyboardListenerService(ReadCode, ReadCharacter, IsEnabled: true);
        }

        private void ReadCode(object sender, string Code)
        {
            this.Code = Code;
            this.Jump().SafeFireAndForget();
        }

        private async Task Jump()
        {
            await Task.Yield();
            await this.EnterView.TranslateTo(0, 50, 250);
            await this.EnterView.TranslateTo(0, 0, 250);
        }

        private void ReadCharacter(object sender, string character)
        {
            IKeyboardListenerService service = (IKeyboardListenerService)sender;
            this.Code = service.Code;
        }
    }
}