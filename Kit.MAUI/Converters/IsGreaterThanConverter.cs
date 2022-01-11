﻿using System;
using System.Globalization;
using Microsoft.Maui;using Microsoft.Maui.Controls;

namespace Kit.MAUI.Converters
{
    public class IsGreaterThanConverter : IValueConverter
    {
        public IsGreaterThanConverter()
        {

        }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double intValue = System.Convert.ToDouble(value);
            double compareToValue = System.Convert.ToDouble(parameter);

            return intValue > compareToValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}