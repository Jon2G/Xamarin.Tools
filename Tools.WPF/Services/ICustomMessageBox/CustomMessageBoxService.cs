using Tools.Enums;
using inter= Tools.Services.Interfaces.ICustomMessageBox;
using SQLHelper;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace Tools.WPF.Services.ICustomMessageBox
{
    public class CustomMessageBoxService : inter
    {
        /// <summary>
        /// Displays a message box that has a message and returns a result.
        /// </summary>
        /// <param name="messageBoxText">A System.String that specifies the text to display.</param>
        /// <returns>A System.Windows.CustomMessageBoxResult value that specifies which message box button is clicked by the user.</returns>
        public CustomMessageBoxResult Show(string messageBoxText)
        {
            CustomMessageBoxWindow msg = new CustomMessageBoxWindow(messageBoxText);
            msg.ShowDialog();

            return msg.Result;
        }

        /// <summary>
        /// Displays a message box that has a message and a title bar caption; and that returns a result.
        /// </summary>
        /// <param name="messageBoxText">A System.String that specifies the text to display.</param>
        /// <param name="caption">A System.String that specifies the title bar caption to display.</param>
        /// <returns>A System.Windows.CustomMessageBoxResult value that specifies which message box button is clicked by the user.</returns>
        public CustomMessageBoxResult Show(string messageBoxText, string caption)
        {
            CustomMessageBoxWindow msg = new CustomMessageBoxWindow(messageBoxText, caption);
            msg.ShowDialog();

            return msg.Result;
        }

        /// <summary>
        /// Displays a message box in front of the specified window. The message box displays a message and returns a result.
        /// </summary>
        /// <param name="owner">A System.Windows.Window that represents the owner window of the message box.</param>
        /// <param name="messageBoxText">A System.String that specifies the text to display.</param>
        /// <returns>A System.Windows.CustomMessageBoxResult value that specifies which message box button is clicked by the user.</returns>
        public CustomMessageBoxResult Show(Window owner, string messageBoxText)
        {
            CustomMessageBoxWindow msg = new CustomMessageBoxWindow(messageBoxText)
            {
                Owner = owner
            };
            msg.ShowDialog();

            return msg.Result;
        }

        /// <summary>
        /// Displays a message box in front of the specified window. The message box displays a message and title bar caption; and it returns a result.
        /// </summary>
        /// <param name="owner">A System.Windows.Window that represents the owner window of the message box.</param>
        /// <param name="messageBoxText">A System.String that specifies the text to display.</param>
        /// <param name="caption">A System.String that specifies the title bar caption to display.</param>
        /// <returns>A System.Windows.CustomMessageBoxResult value that specifies which message box button is clicked by the user.</returns>
        public CustomMessageBoxResult Show(Window owner, string messageBoxText, string caption)
        {
            CustomMessageBoxWindow msg = new CustomMessageBoxWindow(messageBoxText, caption)
            {
                Owner = owner
            };
            msg.ShowDialog();

            return msg.Result;
        }

        /// <summary>
        /// Displays a message box that has a message, title bar caption, and button; and that returns a result.
        /// </summary>
        /// <param name="messageBoxText">A System.String that specifies the text to display.</param>
        /// <param name="caption">A System.String that specifies the title bar caption to display.</param>
        /// <param name="button">A System.Windows.CustomMessageBoxButton value that specifies which button or buttons to display.</param>
        /// <returns>A System.Windows.CustomMessageBoxResult value that specifies which message box button is clicked by the user.</returns>
        public CustomMessageBoxResult Show(string messageBoxText, string caption, CustomMessageBoxButton button)
        {
            CustomMessageBoxWindow msg = new CustomMessageBoxWindow(messageBoxText, caption, button);
            msg.ShowDialog();

            return msg.Result;
        }

        /// <summary>
        /// Displays a message box that has a message, title bar caption, button, and icon; and that returns a result.
        /// </summary>
        /// <param name="messageBoxText">A System.String that specifies the text to display.</param>
        /// <param name="caption">A System.String that specifies the title bar caption to display.</param>
        /// <param name="button">A System.Windows.CustomMessageBoxButton value that specifies which button or buttons to display.</param>
        /// <param name="icon">A System.Windows.CustomMessageBoxImage value that specifies the icon to display.</param>
        /// <returns>A System.Windows.CustomMessageBoxResult value that specifies which message box button is clicked by the user.</returns>
        public CustomMessageBoxResult Show(string messageBoxText, string caption, CustomMessageBoxButton button, CustomMessageBoxImage icon)
        {
            Application.Current.Dispatcher.Invoke(delegate
            {
                try
                {
                    CustomMessageBoxWindow msg = new CustomMessageBoxWindow(messageBoxText, caption, button, icon);

                    msg.ShowDialog();
                }
                catch (System.ComponentModel.Win32Exception ex)
                {
                    Log.LogMe(ex, "Al mostar un mensaje personalizado");
                }
            });
            return CustomMessageBoxResult.OK;
        }

        /// <summary>
        /// Displays a message box that has a message, title bar caption, and OK button with a custom System.String value for the button's text; and that returns a result.
        /// </summary>
        /// <param name="messageBoxText">A System.String that specifies the text to display.</param>
        /// <param name="caption">A System.String that specifies the title bar caption to display.</param>
        /// <param name="okButtonText">A System.String that specifies the text to display within the OK button.</param>
        /// <returns>A System.Windows.CustomMessageBoxResult value that specifies which message box button is clicked by the user.</returns>
        public CustomMessageBoxResult ShowOK(string messageBoxText, string caption, string okButtonText)
        {
            CustomMessageBoxWindow msg = new CustomMessageBoxWindow(messageBoxText, caption, CustomMessageBoxButton.OK)
            {
                OkButtonText = okButtonText
            };

            msg.ShowDialog();

            return msg.Result;
        }

        /// <summary>
        /// Displays a message box that has a message, title bar caption, OK button with a custom System.String value for the button's text, and icon; and that returns a result.
        /// </summary>
        /// <param name="messageBoxText">A System.String that specifies the text to display.</param>
        /// <param name="caption">A System.String that specifies the title bar caption to display.</param>
        /// <param name="okButtonText">A System.String that specifies the text to display within the OK button.</param>
        /// <param name="icon">A System.Windows.CustomMessageBoxImage value that specifies the icon to display.</param>
        /// <returns>A System.Windows.CustomMessageBoxResult value that specifies which message box button is clicked by the user.</returns>
        public CustomMessageBoxResult ShowOK(string messageBoxText, string caption, string okButtonText, CustomMessageBoxImage icon)
        {
            CustomMessageBoxWindow msg = new CustomMessageBoxWindow(messageBoxText, caption, CustomMessageBoxButton.OK, icon)
            {
                OkButtonText = okButtonText
            };

            msg.ShowDialog();

            return msg.Result;
        }

        /// <summary>
        /// Displays a message box that has a message, caption, and OK/Cancel buttons with custom System.String values for the buttons' text;
        /// and that returns a result.
        /// </summary>
        /// <param name="messageBoxText">A System.String that specifies the text to display.</param>
        /// <param name="caption">A System.String that specifies the title bar caption to display.</param>
        /// <param name="okButtonText">A System.String that specifies the text to display within the OK button.</param>
        /// <param name="cancelButtonText">A System.String that specifies the text to display within the Cancel button.</param>
        /// <returns>A System.Windows.CustomMessageBoxResult value that specifies which message box button is clicked by the user.</returns>
        public CustomMessageBoxResult ShowOKCancel(string messageBoxText, string caption, string okButtonText, string cancelButtonText)
        {
            CustomMessageBoxWindow msg = new CustomMessageBoxWindow(messageBoxText, caption, CustomMessageBoxButton.OKCancel)
            {
                OkButtonText = okButtonText,
                CancelButtonText = cancelButtonText
            };

            msg.ShowDialog();

            return msg.Result;
        }

        /// <summary>
        /// Displays a message box that has a message, caption, OK/Cancel buttons with custom System.String values for the buttons' text, and icon;
        /// and that returns a result.
        /// </summary>
        /// <param name="messageBoxText">A System.String that specifies the text to display.</param>
        /// <param name="caption">A System.String that specifies the title bar caption to display.</param>
        /// <param name="okButtonText">A System.String that specifies the text to display within the OK button.</param>
        /// <param name="cancelButtonText">A System.String that specifies the text to display within the Cancel button.</param>
        /// <param name="icon">A System.Windows.CustomMessageBoxImage value that specifies the icon to display.</param>
        /// <returns>A System.Windows.CustomMessageBoxResult value that specifies which message box button is clicked by the user.</returns>
        public CustomMessageBoxResult ShowOKCancel(string messageBoxText, string caption, string okButtonText, string cancelButtonText, CustomMessageBoxImage icon)
        {
            CustomMessageBoxWindow msg = new CustomMessageBoxWindow(messageBoxText, caption, CustomMessageBoxButton.OKCancel, icon)
            {
                OkButtonText = okButtonText,
                CancelButtonText = cancelButtonText
            };

            msg.ShowDialog();

            return msg.Result;
        }

        /// <summary>
        /// Displays a message box that has a message, caption, and Yes/No buttons with custom System.String values for the buttons' text;
        /// and that returns a result.
        /// </summary>
        /// <param name="messageBoxText">A System.String that specifies the text to display.</param>
        /// <param name="caption">A System.String that specifies the title bar caption to display.</param>
        /// <param name="yesButtonText">A System.String that specifies the text to display within the Yes button.</param>
        /// <param name="noButtonText">A System.String that specifies the text to display within the No button.</param>
        /// <returns>A System.Windows.CustomMessageBoxResult value that specifies which message box button is clicked by the user.</returns>
        public CustomMessageBoxResult ShowYesNo(string messageBoxText, string caption, string yesButtonText, string noButtonText)
        {
            CustomMessageBoxWindow msg = new CustomMessageBoxWindow(messageBoxText, caption, CustomMessageBoxButton.YesNo)
            {
                YesButtonText = yesButtonText,
                NoButtonText = noButtonText
            };

            msg.ShowDialog();

            return msg.Result;
        }

        /// <summary>
        /// Displays a message box that has a message, caption, Yes/No buttons with custom System.String values for the buttons' text, and icon;
        /// and that returns a result.
        /// </summary>
        /// <param name="messageBoxText">A System.String that specifies the text to display.</param>
        /// <param name="caption">A System.String that specifies the title bar caption to display.</param>
        /// <param name="yesButtonText">A System.String that specifies the text to display within the Yes button.</param>
        /// <param name="noButtonText">A System.String that specifies the text to display within the No button.</param>
        /// <param name="icon">A System.Windows.CustomMessageBoxImage value that specifies the icon to display.</param>
        /// <returns>A System.Windows.CustomMessageBoxResult value that specifies which message box button is clicked by the user.</returns>
        public CustomMessageBoxResult ShowYesNo(string messageBoxText, string caption, string yesButtonText, string noButtonText, CustomMessageBoxImage icon)
        {
            CustomMessageBoxWindow msg = new CustomMessageBoxWindow(messageBoxText, caption, CustomMessageBoxButton.YesNo, icon)
            {
                YesButtonText = yesButtonText,
                NoButtonText = noButtonText
            };

            msg.ShowDialog();

            return msg.Result;
        }

        /// <summary>
        /// Displays a message box that has a message, caption, and Yes/No/Cancel buttons with custom System.String values for the buttons' text;
        /// and that returns a result.
        /// </summary>
        /// <param name="messageBoxText">A System.String that specifies the text to display.</param>
        /// <param name="caption">A System.String that specifies the title bar caption to display.</param>
        /// <param name="yesButtonText">A System.String that specifies the text to display within the Yes button.</param>
        /// <param name="noButtonText">A System.String that specifies the text to display within the No button.</param>
        /// <param name="cancelButtonText">A System.String that specifies the text to display within the Cancel button.</param>
        /// <returns>A System.Windows.CustomMessageBoxResult value that specifies which message box button is clicked by the user.</returns>
        public CustomMessageBoxResult ShowYesNoCancel(string messageBoxText, string caption, string yesButtonText, string noButtonText, string cancelButtonText)
        {
            CustomMessageBoxWindow msg = new CustomMessageBoxWindow(messageBoxText, caption, CustomMessageBoxButton.YesNoCancel)
            {
                YesButtonText = yesButtonText,
                NoButtonText = noButtonText,
                CancelButtonText = cancelButtonText
            };

            msg.ShowDialog();

            return msg.Result;
        }

        /// <summary>
        /// Displays a message box that has a message, caption, Yes/No/Cancel buttons with custom System.String values for the buttons' text, and icon;
        /// and that returns a result.
        /// </summary>
        /// <param name="messageBoxText">A System.String that specifies the text to display.</param>
        /// <param name="caption">A System.String that specifies the title bar caption to display.</param>
        /// <param name="yesButtonText">A System.String that specifies the text to display within the Yes button.</param>
        /// <param name="noButtonText">A System.String that specifies the text to display within the No button.</param>
        /// <param name="cancelButtonText">A System.String that specifies the text to display within the Cancel button.</param>
        /// <param name="icon">A System.Windows.CustomMessageBoxImage value that specifies the icon to display.</param>
        /// <returns>A System.Windows.CustomMessageBoxResult value that specifies which message box button is clicked by the user.</returns>
        public CustomMessageBoxResult ShowYesNoCancel(string messageBoxText, string caption, string yesButtonText, string noButtonText, string cancelButtonText, CustomMessageBoxImage icon)
        {
            CustomMessageBoxWindow msg = new CustomMessageBoxWindow(messageBoxText, caption, CustomMessageBoxButton.YesNoCancel, icon)
            {
                YesButtonText = yesButtonText,
                NoButtonText = noButtonText,
                CancelButtonText = cancelButtonText
            };

            msg.ShowDialog();

            return msg.Result;
        }


    }
}
