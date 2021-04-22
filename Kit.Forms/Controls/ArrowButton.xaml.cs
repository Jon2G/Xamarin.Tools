using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Kit.Forms.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ArrowButton : ContentView
    {

        public static readonly BindableProperty ArrowColorProperty = BindableProperty.Create(
            propertyName: nameof(ArrowColor), returnType: typeof(Color), declaringType: typeof(ArrowButton), defaultValue: Color.Gray);

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

        public static readonly BindableProperty CommandProperty = BindableProperty.Create(
            propertyName: nameof(Command), returnType: typeof(ICommand), declaringType: typeof(ArrowButton), defaultValue: null);

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set
            {
                SetValue(CommandProperty, value);
                OnPropertyChanged();
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


        public ArrowButton()
        {
            this.BindingContext = this;
            InitializeComponent();
        }
    }
}