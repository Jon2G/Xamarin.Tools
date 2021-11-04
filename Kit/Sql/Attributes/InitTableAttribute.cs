using System;

namespace Kit.Sql.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class InitTableAttribute : Attribute
    {
    }
}
