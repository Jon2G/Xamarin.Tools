using System;
using System.Collections.Generic;
using System.Text;
using Kit.Forms.Extensions;
using Kit.Model;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;

namespace Kit.Forms.Services
{
    public class IKeyboardListenerService : ModelBase, IDisposable
    {
        public const string Message = "IKeyboardListenerService.OnKeyUp";
        private StringBuilder RecievedText;
        private MyTimer CountDown { get; set; }
        private Action<string> ReciveCode;

        private bool _IsEnabled;

        public bool IsEnabled
        {
            get => _IsEnabled;
            set
            {
                if (_IsEnabled != value)
                {
                    _IsEnabled = value;
                    if (IsEnabled)
                    {
                        Suscribe();
                    }
                    else
                    {
                        UnSuscribe();
                    }
                    Raise(() => IsEnabled);
                    Raise(() => IsDisabled);
                }
            }
        }

        public bool IsDisabled => !IsEnabled;

        private void UnSuscribe()
        {
            MessagingCenter.Unsubscribe<object, char>(this, Message);
        }

        private void Suscribe()
        {
            MessagingCenter.Subscribe<object, char>(this, Message, OnKeyUp);
        }

        public IKeyboardListenerService(Action<string> ReciveCode, bool IsEnabled = true)
        {
            this.IsEnabled = IsEnabled;
            this.ReciveCode = ReciveCode;
            if (this.ReciveCode is null)
            {
                this.IsEnabled = false;
            }
        }

        ~IKeyboardListenerService()
        {
            Dispose();
        }

        public IKeyboardListenerService SetDelay(long WaitMillis)
        {
            this.RecievedText = new StringBuilder();
            CountDown = new MyTimer(TimeSpan.FromMilliseconds(WaitMillis), Confirm);
            return this;
        }

        private void Confirm()
        {
            this.CountDown.Stop();
            ReciveCode?.Invoke(RecievedText?.ToString()?.Trim());
            this.RecievedText = new StringBuilder();
        }

        public void OnKeyUp(object sender, char character)
        {
            if (!IsEnabled)
            {
                return;
            }

            string text = character.ToString();
            if (string.IsNullOrEmpty(text) || text == "\0")
            {
                return;
            }
            RecievedText.Append(character);
            CountDown.Restart();
        }

        private void ReleaseUnmanagedResources()
        {
        }

        public void Dispose()
        {
            UnSuscribe();
            CountDown?.Stop();
            CountDown = null;
            ReciveCode = null;
        }
    }
}