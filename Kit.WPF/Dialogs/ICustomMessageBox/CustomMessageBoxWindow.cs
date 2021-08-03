using System;
using System.Drawing;
using System.Windows;
using Kit.Enums;
using Kit.Sql;
using Kit.WPF;
using Kit;

using Kit.Extensions;

namespace Kit.WPF.Dialogs.ICustomMessageBox
{
    /// <summary>
    /// Interaction logic for ModalDialog.xaml
    /// </summary>
    public partial class CustomMessageBoxWindow : System.Windows.Window
    {
        internal string Caption
        {
            get => Title;
            set => Title = value;
        }

        internal string Message
        {
            get => TextBlock_Message.Text;
            set => TextBlock_Message.Text = value;
        }

        internal string OkButtonText
        {
            get => Label_Ok.Text.ToString();
            set => Label_Ok.Text = value;
        }

        internal string CancelButtonText
        {
            get => Label_Cancel.Text.ToString();
            set => Label_Cancel.Text = value;
            //.TryAddKeyboardAccellerator()
        }

        internal string YesButtonText
        {
            get => Label_Yes.Text.ToString();
            set => Label_Yes.Text = value;
        }

        internal string NoButtonText
        {
            get => Label_No.Text;
            set => Label_No.Text = value;
        }

        public CustomMessageBoxResult Result { get; set; }

        internal CustomMessageBoxWindow(string message)
        {
            InitializeComponent();
            Message = message;
            Image_MessageBox.Visibility = System.Windows.Visibility.Collapsed;
            DisplayButtons(CustomMessageBoxButton.OK);
            VentanaModal();
        }

        internal CustomMessageBoxWindow(string message, string caption)
        {
            InitializeComponent();
            Message = message;
            Caption = caption;
            Image_MessageBox.Visibility = System.Windows.Visibility.Collapsed;
            DisplayButtons(CustomMessageBoxButton.OK);
            VentanaModal();
        }

        internal CustomMessageBoxWindow(string message, string caption, CustomMessageBoxButton button)
        {
            InitializeComponent();
            Message = message;
            Caption = caption;
            Image_MessageBox.Visibility = System.Windows.Visibility.Collapsed;
            DisplayButtons(button);
            VentanaModal();
        }

        internal CustomMessageBoxWindow(string message, string caption, CustomMessageBoxImage image)
        {
            InitializeComponent();
            Message = message;
            Caption = caption;
            DisplayImage(image);
            DisplayButtons(CustomMessageBoxButton.OK);
            VentanaModal();
        }

        internal CustomMessageBoxWindow(string message, string caption, CustomMessageBoxButton button, CustomMessageBoxImage image)
        {
            InitializeComponent();
            Message = message;
            Caption = caption;
            Image_MessageBox.Visibility = System.Windows.Visibility.Collapsed;
            DisplayButtons(button);
            DisplayImage(image);
            VentanaModal();
        }

        internal void VentanaModal()
        {
            try
            {
                if (AbstractTools.IsInDesingMode)
                {
                    return;
                }
                this.Topmost = false;
                if (this.Owner is null)
                {
                    Window owner = ToolsImplementation.UWPInstance.VentanaPadre();
                    if (this != owner)
                    {
                        this.Owner = owner;
                    }
                }
                if (this.Owner is null)
                {
                    this.Topmost = true;
                }
                else
                {
                    if (Owner.Style != null)
                        if (Owner.Style.TargetType == typeof(Window))
                        {
                            this.Style = Owner.Style;
                        }
                }
                this.WindowStartupLocation = this.Owner != null ?
                    WindowStartupLocation.CenterOwner : WindowStartupLocation.CenterScreen;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);

                Log.Logger.Error(ex, "Al mostrar un mensaje personalizadp desde VentanaModal();", true);
            }
        }

        private void DisplayButtons(CustomMessageBoxButton button)
        {
            switch (button)
            {
                case CustomMessageBoxButton.OKCancel:
                    // Hide all but OK, Cancel
                    Button_OK.Visibility = System.Windows.Visibility.Visible;
                    Button_OK.Focus();
                    Button_Cancel.Visibility = System.Windows.Visibility.Visible;

                    Button_Yes.Visibility = System.Windows.Visibility.Collapsed;
                    Button_No.Visibility = System.Windows.Visibility.Collapsed;
                    break;

                case CustomMessageBoxButton.YesNo:
                    // Hide all but Yes, No
                    Button_Yes.Visibility = System.Windows.Visibility.Visible;
                    Button_Yes.Focus();
                    Button_No.Visibility = System.Windows.Visibility.Visible;

                    Button_OK.Visibility = System.Windows.Visibility.Collapsed;
                    Button_Cancel.Visibility = System.Windows.Visibility.Collapsed;
                    break;

                case CustomMessageBoxButton.YesNoCancel:
                    // Hide only OK
                    Button_Yes.Visibility = System.Windows.Visibility.Visible;
                    Button_Yes.Focus();
                    Button_No.Visibility = System.Windows.Visibility.Visible;
                    Button_Cancel.Visibility = System.Windows.Visibility.Visible;

                    Button_OK.Visibility = System.Windows.Visibility.Collapsed;
                    break;

                default:
                    // Hide all but OK
                    Button_OK.Visibility = System.Windows.Visibility.Visible;
                    Button_OK.Focus();
                    Button_Yes.Visibility = System.Windows.Visibility.Collapsed;
                    Button_No.Visibility = System.Windows.Visibility.Collapsed;
                    Button_Cancel.Visibility = System.Windows.Visibility.Collapsed;
                    break;
            }
        }

        private void DisplayImage(CustomMessageBoxImage image)
        {
            Icon icon;
            switch (image)
            {
                case CustomMessageBoxImage.Exclamation:       // Enumeration value 48 - also covers "Warning"
                    icon = SystemIcons.Exclamation;
                    break;

                case CustomMessageBoxImage.Error:             // Enumeration value 16, also covers "Hand" and "Stop"
                    icon = SystemIcons.Hand;
                    break;

                case CustomMessageBoxImage.Information:       // Enumeration value 64 - also covers "Asterisk"
                    icon = SystemIcons.Information;
                    break;

                case CustomMessageBoxImage.Question:
                    icon = SystemIcons.Question;
                    break;

                default:
                    icon = SystemIcons.Information;
                    break;
            }
            Image_MessageBox.Source = icon.ToImageSource();
            Image_MessageBox.Visibility = System.Windows.Visibility.Visible;
        }

        private void Button_OK_Click(object sender, RoutedEventArgs e)
        {
            if (!ClickHelper.EsValido())
            {
                return;
            }
            Result = CustomMessageBoxResult.OK;
            Close();
        }

        private void Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            if (!ClickHelper.EsValido())
            {
                return;
            }
            Result = CustomMessageBoxResult.Cancel;
            Close();
        }

        private void Button_Yes_Click(object sender, RoutedEventArgs e)
        {
            if (!ClickHelper.EsValido())
            {
                return;
            }
            Result = CustomMessageBoxResult.Yes;
            Close();
        }

        private void Button_No_Click(object sender, RoutedEventArgs e)
        {
            if (!ClickHelper.EsValido())
            {
                return;
            }
            Result = CustomMessageBoxResult.No;
            Close();
        }

        private void Button_OK_TouchDown(object sender, System.Windows.Input.TouchEventArgs e)
        {
            if (!ClickHelper.EsValido())
            {
                return;
            }
        }
    }
}