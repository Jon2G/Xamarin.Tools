using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Xml;

namespace Kit.WPF.Converters
{
	/// <summary>
	/// Converts an XmlAttribute to its Value, as a string.
	/// </summary>
	[ValueConversion(typeof(XmlAttribute), typeof(string))]
	public class XmlAttributeToStringStateConverter : IValueConverter
	{
		object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			Debug.Assert(value is XmlAttribute, "value should be an XmlAttribute");
			Debug.Assert(targetType == typeof(string), "targetType should be String");

			XmlAttribute attr = value as XmlAttribute;
			return attr.Value;
		}

		object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException("ConvertBack not supported.");
		}
	}
}
