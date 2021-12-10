using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DocumentFormat.OpenXml.Office2010.ExcelAc;

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

        public static bool IsOverriden(this PropertyInfo property)
        {
            var getMethod = property.GetGetMethod(false);
            if (getMethod.GetBaseDefinition().DeclaringType == getMethod.DeclaringType)
            {
                return false;
            }
            return true;
        }

        public static bool IsPrimitive(this Type t)
        {
            return (t.IsPrimitive || t == typeof(Decimal) || t == typeof(String) || t.IsEnum || t == typeof(DateTime) ||
                    t == typeof(TimeSpan) || t == typeof(DateTimeOffset));
        }
        public static FieldInfo GetField(this TypeInfo t, string name)
        {
            var f = t.GetDeclaredField(name);
            if (f != null)
                return f;
            return GetField(t.BaseType.GetTypeInfo(), name);
        }

        public static Type GetGenericType(this Type t)
        => t.GetGenericTypes().FirstOrDefault();


        public static List<Type> GetGenericTypes(this Type t)
        {
            return new List<Type>(t.GenericTypeArguments);
        }
    }
}