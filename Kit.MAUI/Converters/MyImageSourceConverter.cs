using System;
using Microsoft.Maui;using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Xamarin.Forms.Xaml;

namespace Kit.MAUI.Converters
{
    [TypeConversion(typeof(ImageSource))]
    public class MyImageSourceConverter :Xamarin.Forms.TypeConverter
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
