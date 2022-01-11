using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Kit.MAUI.Controls.LogsViewer
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LogViewer : ContentView
    {
        public LogViewer()
        {
            InitializeComponent();
        }

        protected override void OnParentSet()
        {
            base.OnParentSet();
            if (Parent is null)
            {
                Model?.Dispose();
            }
        }
    }
}