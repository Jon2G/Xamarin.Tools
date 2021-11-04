using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Kit.WPF.Converters
{
	/// <summary>
	/// Returns the value of the DescriptionAttribute applied to an enum value, or an empty string
	/// if the enum value is not decorated with the attribute.
	/// </summary>
	[ValueConversion(typeof(Enum), typeof(string))]
	public class EnumToDescriptionConverter : IValueConverter
	{
		object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			Debug.Assert(value is Enum, "value should be an Enum");
			Debug.Assert(targetType.IsAssignableFrom(typeof(string)), "targetType should assignable from a String");

			FieldInfo field = value.GetType().GetField(value.ToString());
			object[] attrs = field.GetCustomAttributes(typeof(DescriptionAttribute), false);
			if (attrs.Length > 0)
			{
				DescriptionAttribute attr = attrs[0] as DescriptionAttribute;
				return attr.Description;
			}

			return "";
		}

		object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException("ConvertBack not supported.");
		}
	}
}
