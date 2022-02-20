using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Kit.Forms.Converters
{
    [TypeConversion(typeof(ImageSource))]
    public class MyImageSourceConverter : TypeConverter
    {
        public override object ConvertFromInvariantString(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }
            else
            {
                if (Device.RuntimePlatform == Device.UWP)
                {
                    return ImageSource.FromFile($"Resources/{value}");
                }
                return ImageSource.FromFile(value as string);
            }
            throw new InvalidOperationException(string.Format("Cannot convert \"{0}\" into {1}", value, typeof(ImageSource)));
        }
    }
}
