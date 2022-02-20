using Kit.Sql.Base;
using System;
using System.Linq;
using System.Reflection;

namespace Kit.Sql.Attributes
{
    public class InitTableMethodInfo
    {
        private readonly MethodInfo MethodInfo;
        public InitTableMethodInfo(MethodInfo methodInfo)
        {
            this.MethodInfo = methodInfo;
        }
        public void Execute(SqlBase con)
        {
            MethodInfo.Invoke(null, new object[] { con });
        }
    }
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class InitTableAttribute : Attribute
    {
        internal static InitTableMethodInfo Find(Type mappedType)
        {
            var InitTableAttributetype = typeof(InitTableAttribute);

            if (mappedType
                    .GetMethods().FirstOrDefault(m => m.GetCustomAttributes(InitTableAttributetype, false).Any()) is { } method)
            {
                if (!method.IsStatic)
                {
                    throw new Exception($"Init table method must be static at {mappedType}");
                }
                return new InitTableMethodInfo(method);
            }
            return null;

        }
    }
}
