using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;


namespace Kit.WPF.Pages
{
    /// <summary>
    /// Lógica de interacción para SplashScreen.xaml
    /// </summary>
    public partial class SplashScreen : Window
    {
        public event EventHandler OnShowed;
        private bool IsActivated;
        public SplashScreen() : this(null)
        {

        }
        public SplashScreen(string Title)
        {
            this.IsActivated = false;
            this.Activated += Window_Activated;
            InitializeComponent();
            this.Title = Title;
        }
        private async void Window_Activated(object sender, EventArgs e)
        {
            await Task.Yield();
            if (!IsActivated)
            {
                this.IsActivated = true;
                this.Activated -= Window_Activated;
                await Task.Delay(TimeSpan.FromSeconds(1));
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
