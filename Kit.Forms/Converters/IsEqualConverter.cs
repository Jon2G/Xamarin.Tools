using System;
using System.Globalization;
using Xamarin.Forms;

namespace Kit.Forms.Converters
{
    public class IsEqualConverter : IValueConverter
    {
        public static readonly IValueConverter Instance = new IsEqualConverter();
        public bool IsReversed { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return IsReversed ? !value.Equals(parameter) : (object)value.Equals(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}