using FFImageLoading.Forms;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace Kit.Forms.Controls
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

        [TypeConverter(typeof(Converters.MyImageSourceConverter))]
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
