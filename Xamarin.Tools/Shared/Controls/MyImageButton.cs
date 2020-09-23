using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace Plugin.Xamarin.Tools.Shared.Controls
{
    public partial class MyImageButton : ImageButton
    {
        public static readonly BindableProperty MySourceProperty = BindableProperty.Create(
            propertyName: nameof(MySource), returnType: typeof(string), declaringType: typeof(MyImageButton),
            propertyChanged: OnMySourceChanged);

        public static void OnMySourceChanged(BindableObject bindable, object oldVal, object newVal)
        {
            MyImageButton image = (MyImageButton)bindable;
            if (oldVal as string != newVal as string)
            {
                if (Device.RuntimePlatform == Device.UWP)
                {
                    image.Source = ImageSource.FromFile($"Resources/{newVal}");
                    return;
                }
                image.Source = ImageSource.FromFile(newVal as string);
            }
        }

        public string MySource
        {
            get { return (string)GetValue(MySourceProperty); }
            set { SetValue(MySourceProperty, value); }
        }

        public MyImageButton()
        {

        }
        public object Tag { get; set; }
    }
}
