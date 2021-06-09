using System;
using System.Collections.Generic;
using System.Text;

namespace Kit
{
    public static class TypeExtensions
    {
        public static object GetDefault(this Type type)
        {
            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }
            return null;
        }

        public static bool IsPrimitive(this Type t)
        {
            return (t.IsPrimitive || t == typeof(Decimal) || t == typeof(String) || t.IsEnum || t == typeof(DateTime) ||
                    t == typeof(TimeSpan) || t == typeof(DateTimeOffset));
        }
    }
}