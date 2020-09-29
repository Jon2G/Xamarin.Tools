using FFImageLoading.Forms;
using Plugin.Xamarin.Tools.Shared.Converters;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace Plugin.Xamarin.Tools.Shared.Controls
{
    public partial class MyImage : CachedImage
    {

        public static readonly BindableProperty MySourceProperty = BindableProperty.Create(
            propertyName: nameof(MySource), returnType: typeof(ImageSource), declaringType: typeof(MyImage), defaultValue: null,
          propertyChanged: ImgChanged);

        private static void ImgChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is MyImage img)
            {
                if (newValue != oldValue)
                {
                    img.Source = newValue as ImageSource;
                }
            }
        }

        [TypeConverter(typeof(MyImageSourceConverter))]
        public ImageSource MySource
        {
            get { return (ImageSource)GetValue(MySourceProperty); }
            set
            {
                SetValue(MySourceProperty, value);
                OnPropertyChanged();
            }
        }


        public MyImage()
        {

        }
        public object Tag { get; set; }
    }
}
