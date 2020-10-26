using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Tools.WPF.Pages
{
    /// <summary>
    /// Lógica de interacción para SplashScreen.xaml
    /// </summary>
    public partial class SplashScreen : Window
    {
        public event EventHandler OnShowed;
        private bool IsActivated;
        public SplashScreen()
        {
            this.IsActivated = false;
            this.Activated += Window_Activated;
            InitializeComponent();
        }

        private async void Window_Activated(object sender, EventArgs e)
        {
            await Task.Yield();
            if (!IsActivated)
            {
                this.IsActivated = true;
                this.Activated -= Window_Activated;
                await Task.Delay(TimeSpan.FromSeconds(2));
                OnShowed?.Invoke(sender, e);
            }
        }

        public void Failure(string Reason)
        {
            this.Background = new LinearGradientBrush()
            {
                StartPoint = new System.Windows.Point(0.5, 0),
                EndPoint = new System.Windows.Point(0.5, 1),
                GradientStops = new GradientStopCollection()
                {
                    new GradientStop((Color)ColorConverter.ConvertFromString("#FF420000"),1),
                    new GradientStop(System.Windows.Media.Colors.White,0)
                }
            };


            this.Progress.IsIndeterminate = false;
            this.Progress.Value = this.Progress.Maximum;
            this.Progress.Foreground = System.Windows.Media.Brushes.IndianRed;
            SetLabelText(Reason);
            this.WindowStyle = WindowStyle.ToolWindow;

        }
        public void SetLabelText(string Text)
        {
            this.Label.Text = Text;
        }
    }
}
