using Plugin.Xamarin.Tools.Shared.Enums;
using Plugin.Xamarin.Tools.Shared.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Plugin.Xamarin.Tools.Droid.Services
{
    internal class CustomMessageBoxService : ICustomMessageBox
    {
        public CustomMessageBoxResult Show(string messageBoxText)
        {
            Acr.UserDialogs.UserDialogs.Instance.Alert(messageBoxText);
            return CustomMessageBoxResult.OK;
        }

        public CustomMessageBoxResult Show(string messageBoxText, string caption)
        {
            throw new NotImplementedException();
        }

        public CustomMessageBoxResult Show(string messageBoxText, string caption, CustomMessageBoxButton button)
        {
            throw new NotImplementedException();
        }

        public CustomMessageBoxResult Show(string messageBoxText, string caption, CustomMessageBoxButton button, CustomMessageBoxImage icon)
        {
            string text = null;
            switch (button)
            {
                case CustomMessageBoxButton.OK:
                    text = "Ok";
                    break;
            }
            Acr.UserDialogs.UserDialogs.Instance.Alert(messageBoxText, caption, text);
            return CustomMessageBoxResult.OK;
        }

        public CustomMessageBoxResult ShowOK(string messageBoxText, string caption, string okButtonText)
        {
            throw new NotImplementedException();
        }

        public CustomMessageBoxResult ShowOK(string messageBoxText, string caption, string okButtonText, CustomMessageBoxImage icon)
        {
            Acr.UserDialogs.UserDialogs.Instance.Alert(messageBoxText, caption, okButtonText);
            return CustomMessageBoxResult.OK;
        }

        public CustomMessageBoxResult ShowOKCancel(string messageBoxText, string caption, string okButtonText, string cancelButtonText)
        {
            throw new NotImplementedException();
        }

        public CustomMessageBoxResult ShowOKCancel(string messageBoxText, string caption, string okButtonText, string cancelButtonText, CustomMessageBoxImage icon)
        {
            throw new NotImplementedException();
        }

        public CustomMessageBoxResult ShowYesNo(string messageBoxText, string caption, string yesButtonText, string noButtonText)
        {
            throw new NotImplementedException();
        }

        public CustomMessageBoxResult ShowYesNo(string messageBoxText, string caption, string yesButtonText, string noButtonText, CustomMessageBoxImage icon)
        {
            throw new NotImplementedException();
        }

        public CustomMessageBoxResult ShowYesNoCancel(string messageBoxText, string caption, string yesButtonText, string noButtonText, string cancelButtonText)
        {
            throw new NotImplementedException();
        }

        public CustomMessageBoxResult ShowYesNoCancel(string messageBoxText, string caption, string yesButtonText, string noButtonText, string cancelButtonText, CustomMessageBoxImage icon)
        {
            throw new NotImplementedException();
        }
    }
}
