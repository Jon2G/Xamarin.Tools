using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Kit.WPF.Converters
{
    public class EqualityToVisibilityConverter : IValueConverter
    {        /// <summary>
             /// If set to True, conversion is reversed: True will become Collapsed.
             /// </summary>
        public bool IsReversed { get; set; }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && value.Equals(parameter)) return IsReversed ? Visibility.Collapsed : Visibility.Visible;

            return IsReversed ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
