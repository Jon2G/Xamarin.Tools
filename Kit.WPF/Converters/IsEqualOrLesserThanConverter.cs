using System;
using System.Globalization;
using System.Windows.Data;

namespace Kit.WPF.Converters
{
    public class IsEqualOrLesserThanConverter : IValueConverter
    {
        public static readonly IValueConverter Instance = new IsEqualOrLesserThanConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double intValue = System.Convert.ToDouble(value);
            double compareToValue = System.Convert.ToDouble(parameter);

            return intValue <= compareToValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}