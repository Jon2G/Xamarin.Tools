using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.Xaml;

namespace Kit.Forms.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ArrowButton : ContentView
    {
        public static readonly BindableProperty ArrowColorProperty = BindableProperty.Create(
            propertyName: nameof(ArrowColor), returnType: typeof(Color), declaringType: typeof(ArrowButton), defaultValue: Color.Accent);

        public Color ArrowColor
        {
            get => (Color)GetValue(ArrowColorProperty);
            set
            {
                SetValue(ArrowColorProperty, value);
                OnPropertyChanged();
            }
        }

        public static readonly BindableProperty TitleColorProperty = BindableProperty.Create(
            propertyName: nameof(TitleColor), returnType: typeof(Color), declaringType: typeof(ArrowButton), defaultValue: Color.Black);

        public Color TitleColor
        {
            get => (Color)GetValue(TitleColorProperty);
            set
            {
                SetValue(TitleColorProperty, value);
                OnPropertyChanged();
            }
        }

        public static readonly BindableProperty SubTitleColorProperty = BindableProperty.Create(
            propertyName: nameof(SubTitleColor), returnType: typeof(Color), declaringType: typeof(ArrowButton), defaultValue: Color.Black);

        public Color SubTitleColor
        {
            get => (Color)GetValue(SubTitleColorProperty);
            set
            {
                SetValue(SubTitleColorProperty, value);
                OnPropertyChanged();
            }
        }

        public static readonly BindableProperty TitleProperty = BindableProperty.Create(
            propertyName: nameof(Title), returnType: typeof(string), declaringType: typeof(ArrowButton), defaultValue: string.Empty);

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set
            {
                SetValue(TitleProperty, value);
                OnPropertyChanged();
            }
        }

        public static readonly BindableProperty SubTitleProperty = BindableProperty.Create(
            propertyName: nameof(SubTitle), returnType: typeof(string), declaringType: typeof(ArrowButton), defaultValue: string.Empty);

        public string SubTitle
        {
            get => (string)GetValue(SubTitleProperty);
            set
            {
                SetValue(SubTitleProperty, value);
                OnPropertyChanged();
            }
        }

        public static readonly BindableProperty SubTitleFontSizeProperty = BindableProperty.Create(
            propertyName: nameof(SubTitleFontSize), returnType: typeof(int), declaringType: typeof(ArrowButton), defaultValue: 12);

        [TypeConverter(typeof(FontSizeConverter))]
        public int SubTitleFontSize
        {
            get => (int)GetValue(SubTitleFontSizeProperty);
            set
            {
                SetValue(SubTitleFontSizeProperty, value);
                OnPropertyChanged();
            }
        }

        public static readonly BindableProperty TitleFontSizeProperty = BindableProperty.Create(
            propertyName: nameof(TitleFontSize), returnType: typeof(int), declaringType: typeof(ArrowButton), defaultValue: 14);

        [TypeConverter(typeof(FontSizeConverter))]
        public int TitleFontSize
        {
            get => (int)GetValue(TitleFontSizeProperty);
            set
            {
                SetValue(TitleFontSizeProperty, value);
                OnPropertyChanged();
            }
        }

        private bool IsEnabledCore { set; get; }

        public static readonly BindableProperty CommandProperty =
            BindableProperty.Create(nameof(ArrowButton.Command), typeof(ICommand),
            typeof(ArrowButton), null,
            propertyChanging: OnCommandChanging, propertyChanged: OnCommandChanged);

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        private static void OnCommandChanged(BindableObject bo, object o, object n)
        {
            ArrowButton button = (ArrowButton)bo;
            if (n is ICommand newCommand)
                newCommand.CanExecuteChanged += button.OnCommandCanExecuteChanged;

            CommandChanged(button);
        }

        private void OnCommandCanExecuteChanged(object sender, EventArgs e)
        {
            bool can = this.Command?.CanExecute(this.CommandParameter) ?? true;
            if (!can)
            {
            }
        }

        private static void OnCommandChanging(BindableObject bo, object o, object n)
        {
            ArrowButton button = (ArrowButton)bo;
            if (o != null)
            {
                (o as ICommand).CanExecuteChanged -= button.OnCommandCanExecuteChanged;
            }
        }

        public static void CommandChanged(ArrowButton sender)
        {
            if (sender.Command != null)
            {
                CommandCanExecuteChanged(sender, EventArgs.Empty);
            }
            else
            {
                sender.IsEnabledCore = true;
            }
        }

        public static void CommandCanExecuteChanged(object sender, EventArgs e)
        {
            ArrowButton ButtonElementManager = (ArrowButton)sender;
            ICommand cmd = ButtonElementManager.Command;
            if (cmd != null)
            {
                ButtonElementManager.IsEnabledCore = cmd.CanExecute(ButtonElementManager.CommandParameter);
            }
        }

        public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(
            propertyName: nameof(CommandParameter), returnType: typeof(object), declaringType: typeof(ArrowButton), defaultValue: null);

        public object CommandParameter
        {
            get => (object)GetValue(CommandParameterProperty);
            set
            {
                SetValue(CommandProperty, value);
                OnPropertyChanged();
            }
        }

        public static readonly BindableProperty IconProperty = BindableProperty.Create(
            propertyName: nameof(Icon), returnType: typeof(ImageSource), declaringType: typeof(ArrowButton), defaultValue: null);

        [TypeConverter(typeof(Converters.MyImageSourceConverter))]
        public ImageSource Icon
        {
            get => (ImageSource)GetValue(IconProperty);
            set
            {
                SetValue(CommandProperty, value);
                OnPropertyChanged();
            }
        }

        public static readonly BindableProperty IconHeightRequestProperty = BindableProperty.Create(
            propertyName: nameof(IconHeightRequest), returnType: typeof(double), declaringType: typeof(ArrowButton), defaultValue: 20d);

        public double IconHeightRequest
        {
            get => (double)GetValue(IconHeightRequestProperty);
            set
            {
                SetValue(IconHeightRequestProperty, value);
                OnPropertyChanged();
            }
        }

        public static readonly BindableProperty IsArrowVisibleProperty = BindableProperty.Create(
            propertyName: nameof(IsArrowVisible), returnType: typeof(bool), declaringType: typeof(ArrowButton), defaultValue: true);

        public bool IsArrowVisible
        {
            get => (bool)GetValue(IsArrowVisibleProperty);
            set
            {
                SetValue(IsArrowVisibleProperty, value);
                OnPropertyChanged();
            }
        }

        public ArrowButton()
        {
            InitializeComponent();
        }

        private void TapGestureRecognizer_OnTapped(object sender, EventArgs e)
        {
            HapticFeedback.Perform(HapticFeedbackType.Click);
            this.Command?.Execute(this.CommandParameter);
        }
    }
}