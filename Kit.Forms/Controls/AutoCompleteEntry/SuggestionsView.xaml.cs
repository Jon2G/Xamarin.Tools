using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using P42.Utils;
using Rg.Plugins.Popup.Animations;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Kit.Forms.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SuggestionsView
    {
        public bool IsDisplayed { get; private set; }
        public string Selected { get; private set; }

        public static readonly BindableProperty ItemsSourceProperty =
            BindableProperty.Create(nameof(ItemsSource), typeof(IEnumerable<string>),
                typeof(SuggestionsView), null);

        public IEnumerable<string> ItemsSource
        {
            get => (IEnumerable<string>)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        private AutoCompleteEntry AutoCompleteEntry;
        public SuggestionsView()
        {
            this.BindingContext = this;
            this.Animation = new FadeAnimation();
            InitializeComponent();
        }

        public async Task Hide()
        {
            await Rg.Plugins.Popup.Services.PopupNavigation.Instance.PopAsync(true);
            IsDisplayed = false;
        }
        public async Task Show(AutoCompleteEntry entry)
        {
            this.AutoCompleteEntry = entry;
            this.PancakeView.Margin = new Thickness(0, AbsoluteLocation(), 0, 0);
            this.WidthRequest = this.AutoCompleteEntry.Width;
            await Rg.Plugins.Popup.Services.PopupNavigation.Instance.PushAsync(this, true);
            IsDisplayed = true;
        }

        private double AbsoluteLocation()
        {
            var y = this.AutoCompleteEntry.Y + this.AutoCompleteEntry.Height;
            var parent = this.AutoCompleteEntry.ParentView;
            while (parent != null) { y += parent.Y; parent = parent.ParentView; }

            return y;
        }

        private void TapGestureRecognizer_OnTapped(object sender, EventArgs e)
        {
            HapticFeedback.Perform(HapticFeedbackType.Click);
            if (sender is ContentView view)
            {
                this.Selected = view.BindingContext?.ToString();
                this.AutoCompleteEntry.Text = this.Selected;
                this.AutoCompleteEntry.CursorPosition = this.Selected.Length;
            }
            this.AutoCompleteEntry.Focus();
        }
    }
}