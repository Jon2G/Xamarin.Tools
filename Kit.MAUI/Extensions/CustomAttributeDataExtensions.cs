using System.Reflection;

namespace Kit.MAUI
{
    public static class CustomAttributeDataExtensions
    {
        public static object InflateAttribute(this CustomAttributeData x)
        {
            var atype = x.AttributeType;
            var typeInfo = atype.GetTypeInfo();
#if ENABLE_IL2CPP
			var r = Activator.CreateInstance (x.AttributeType);
#else
            var args = x.ConstructorArguments.Select(a => a.Value).ToArray();
            var r = Activator.CreateInstance(x.AttributeType, args);
            foreach (var arg in x.NamedArguments)
            {
                if (arg.IsField)
                {
                    typeInfo.GetField(arg.MemberName).SetValue(r, arg.TypedValue.Value);
                }
                else
                {
                    typeInfo.GetProperty(arg.MemberName).SetValue(r, arg.TypedValue.Value);
                }
            }
#endif
            return r;
        }
    }
}
