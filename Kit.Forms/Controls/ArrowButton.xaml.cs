using System;
using System.Threading.Tasks;
using System.Windows.Input;
using AsyncAwaitBestPractices.MVVM;
using FFImageLoading;
using Kit.Forms.Extensions;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ContentView = Xamarin.Forms.ContentView;
using ImageSource = Xamarin.Forms.ImageSource;
using Brush = Xamarin.Forms.Brush;
using Xamarin.CommunityToolkit.Effects;

namespace Kit.Forms.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile), Preserve()]
    public partial class ArrowButton
    {

        public static readonly BindableProperty PressedBackgroundColorProperty = BindableProperty.Create(
            propertyName: nameof(TextColor),
            returnType: typeof(Color),
            declaringType: typeof(ArrowButton),
            defaultValue: Color.LightGray, BindingMode.OneWay,
            propertyChanged: (e, o, n) =>
            {
                if (e is ArrowButton arrow)
                {
                    Color color = (Color)n;
                    arrow.PressedBackgroundColor = color;
                    TouchEffect.SetPressedBackgroundColor(arrow, color);
                }
            });

        public Color PressedBackgroundColor
        {
            get => (Color)GetValue(PressedBackgroundColorProperty);
            set
            {
                SetValue(PressedBackgroundColorProperty, value);
                TouchEffect.SetPressedBackgroundColor(this, value);
                OnPropertyChanged();
            }
        }
        public static new readonly BindableProperty BackgroundColorProperty = BindableProperty.Create(
            propertyName: nameof(BackgroundColor),
            returnType: typeof(Color),
            declaringType: typeof(ArrowButton),
            defaultValue: Color.WhiteSmoke, BindingMode.OneWay,
            propertyChanged: (e, o, n) =>
            {
                if (e is ArrowButton arrow)
                {
                    Color color = (Color)n;
                    arrow.BackgroundColor = color;
                    TouchEffect.SetNormalBackgroundColor(arrow, color);
                }
            });

        public new Color BackgroundColor
        {
            get => (Color)base.GetValue(BackgroundColorProperty);
            set
            {
                base.SetValue(BackgroundColorProperty, value);
                OnPropertyChanged();
                TouchEffect.SetNormalBackgroundColor(this.arrow, value);
            }
        }

        public static readonly BindableProperty TextColorProperty = BindableProperty.Create(
            propertyName: nameof(TextColor),
            returnType: typeof(Color),
            declaringType: typeof(ArrowButton),
            defaultValue: Color.Black, BindingMode.OneWay,
            propertyChanged: (e, o, n) =>
            {
                if (e is ArrowButton arrow)
                {
                    Color color = (Color)n;
                    arrow.TitleColor = color;
                    arrow.SubTitleColor = color;
                    arrow.TextColor = color;
                }
            });

        public Color TextColor
        {
            get => (Color)GetValue(TextColorProperty);
            set
            {
                SetValue(TextColorProperty, value);
                SetValue(SubTitleColorProperty, value);
                SetValue(TitleColorProperty, value);
                OnPropertyChanged();
                OnPropertyChanged(nameof(TitleColor));
                OnPropertyChanged(nameof(SubTitleColor));
            }
        }

        public static readonly BindableProperty ArrowColorProperty = BindableProperty.Create(
            propertyName: nameof(ArrowColor), returnType: typeof(Color),
            declaringType: typeof(ArrowButton), defaultValue: Color.Accent, BindingMode.OneWay,
            propertyChanged: (e, o, n) =>
            {
                if (e is ArrowButton arrow) arrow.ArrowColor = (Color)n;
            });

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
            propertyName: nameof(TitleColor), returnType: typeof(Color),
            declaringType: typeof(ArrowButton), defaultValue: Color.Black, BindingMode.OneWay,
            propertyChanged: (e, o, n) =>
            {
                if (e is ArrowButton arrow) arrow.TitleColor = (Color)n;
            });

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
            propertyName: nameof(SubTitleColor), returnType: typeof(Color),
            declaringType: typeof(ArrowButton), defaultValue: Color.Black, BindingMode.OneWay,
            propertyChanged: (e, o, n) =>
            {
                if (e is ArrowButton arrow) arrow.SubTitleColor = (Color)n;
            });

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
            propertyName: nameof(Title), returnType: typeof(string),
            declaringType: typeof(ArrowButton), defaultValue: string.Empty, BindingMode.OneWay,
            propertyChanged: (e, o, n) => { if (e is ArrowButton arrow) arrow.Title = n?.ToString(); });

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
            propertyName: nameof(SubTitle), returnType: typeof(string),
            declaringType: typeof(ArrowButton), defaultValue: string.Empty, BindingMode.OneWay,
            propertyChanged: (e, o, n) => { if (e is ArrowButton arrow) arrow.SubTitle = n?.ToString(); });

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
            propertyName: nameof(SubTitleFontSize), returnType: typeof(double),
            declaringType: typeof(ArrowButton), defaultValue: 12d, BindingMode.OneWay,
            propertyChanged: (e, o, n) =>
            {
                if (e is ArrowButton arrow) arrow.SubTitleFontSize = (double)n;
            });

        [TypeConverter(typeof(FontSizeConverter))]
        public double SubTitleFontSize
        {
            get => (double)GetValue(SubTitleFontSizeProperty);
            set
            {
                SetValue(SubTitleFontSizeProperty, value);
                OnPropertyChanged();
            }
        }

        public static readonly BindableProperty TitleFontSizeProperty = BindableProperty.Create(
            propertyName: nameof(TitleFontSize), returnType: typeof(double),
            declaringType: typeof(ArrowButton), defaultValue: 14d, BindingMode.OneWay,
            propertyChanged: (e, o, n) =>
            {
                if (e is ArrowButton arrow) arrow.TitleFontSize = (double)n;
            });

        [TypeConverter(typeof(FontSizeConverter))]
        public double TitleFontSize
        {
            get => (double)GetValue(TitleFontSizeProperty);
            set
            {
                SetValue(TitleFontSizeProperty, value);
                OnPropertyChanged();
            }
        }

        public static readonly BindableProperty TitleFontAttributesProperty = BindableProperty.Create(
            propertyName: nameof(TitleFontAttributes), returnType: typeof(FontAttributes),
            declaringType: typeof(ArrowButton), defaultValue: FontAttributes.Bold, BindingMode.OneWay,
            propertyChanged: (e, o, n) =>
            {
                if (e is ArrowButton arrow) arrow.TitleFontAttributes = (FontAttributes)n;
            });
        public FontAttributes TitleFontAttributes
        {
            get { return (FontAttributes)GetValue(TitleFontAttributesProperty); }
            set { SetValue(TitleFontAttributesProperty, value); OnPropertyChanged(); }
        }
        public static readonly BindableProperty SubTitleFontAttributesProperty = BindableProperty.Create(
            propertyName: nameof(SubTitleFontAttributes), returnType: typeof(FontAttributes),
            declaringType: typeof(ArrowButton), defaultValue: FontAttributes.Bold, BindingMode.OneWay,
            propertyChanged: (e, o, n) =>
            {
                if (e is ArrowButton arrow) arrow.SubTitleFontAttributes = (FontAttributes)n;
            });
        public FontAttributes SubTitleFontAttributes
        {
            get { return (FontAttributes)GetValue(SubTitleFontAttributesProperty); }
            set { SetValue(SubTitleFontAttributesProperty, value); OnPropertyChanged(); }
        }

        public static readonly BindableProperty TitleFontFamilyProperty = BindableProperty.Create(
            propertyName: nameof(TitleFontFamily), returnType: typeof(string),
            declaringType: typeof(ArrowButton), defaultValue: null, BindingMode.OneWay,
            propertyChanged: (e, o, n) =>
            {
                if (e is ArrowButton arrow) arrow.TitleFontFamily = n?.ToString();
            });

        public string TitleFontFamily
        {
            get { return (string)GetValue(TitleFontFamilyProperty); }
            set { SetValue(TitleFontFamilyProperty, value); OnPropertyChanged(); }
        }

        public static readonly BindableProperty SubtitleFontFamilyProperty = BindableProperty.Create(
            propertyName: nameof(SubtitleFontFamily), returnType: typeof(string),
            declaringType: typeof(ArrowButton), defaultValue: null, BindingMode.OneWay,
            propertyChanged: (e, o, n) =>
            {
                if (e is ArrowButton arrow) arrow.TitleFontFamily = n?.ToString();
            });

        public string SubtitleFontFamily
        {
            get { return (string)GetValue(SubtitleFontFamilyProperty); }
            set { SetValue(SubtitleFontFamilyProperty, value); OnPropertyChanged(); }
        }

        private bool IsEnabledCore { set; get; }

        public static readonly BindableProperty CommandProperty =
            BindableProperty.Create(nameof(ArrowButton.Command), typeof(ICommand),
            typeof(ArrowButton), null, BindingMode.OneWay,
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
            propertyName: nameof(CommandParameter), returnType: typeof(object),
            declaringType: typeof(ArrowButton), defaultValue: null, BindingMode.OneWay,
            propertyChanged: (e, o, n) =>
            {
                if (e is ArrowButton arrow) arrow.CommandParameter = n;
            });

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
            propertyName: nameof(Icon), returnType: typeof(ImageSource), declaringType: typeof(ArrowButton),
            defaultValue: null, BindingMode.OneWay,
            propertyChanged: (e, o, n) =>
            {
                if (e is ArrowButton arrow) arrow.Icon = n as ImageSource;
            });

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
            propertyName: nameof(IconHeightRequest), returnType: typeof(double),
            declaringType: typeof(ArrowButton), defaultValue: 20d, BindingMode.OneWay,
            propertyChanged: (e, o, n) =>
            {
                if (e is ArrowButton arrow) arrow.IconHeightRequest = (double)n;
            });

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
            propertyName: nameof(IsArrowVisible), returnType: typeof(bool),
            declaringType: typeof(ArrowButton), defaultValue: true, BindingMode.OneWay,
            propertyChanged: (e, o, n) =>
            {
                if (e is ArrowButton arrow) arrow.IsArrowVisible = (bool)n;
            });

        public bool IsArrowVisible
        {
            get => (bool)GetValue(IsArrowVisibleProperty);
            set
            {
                SetValue(IsArrowVisibleProperty, value);
                OnPropertyChanged();
            }
        }

        public ICommand TouchedCommand { get; }

        public ArrowButton()
        {
            this.TouchedCommand = new AsyncCommand(Touched);
            InitializeComponent();
            TouchEffect.SetNormalBackgroundColor(this.arrow, this.arrow.BackgroundColor);
            TouchEffect.SetCommand(this, TouchedCommand);
            //xct: TouchEffect.Command = "{Binding TouchedCommand,Source={x:Reference arrow}}"
        }

        private async Task Touched()
        {
            await Task.Yield();
            if (await Permisos.CanVibrate())
            {
                HapticFeedback.Perform(HapticFeedbackType.Click);
            }

            this.Command?.Execute(this.CommandParameter);
        }
    }
}