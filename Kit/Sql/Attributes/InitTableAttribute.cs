using System;
using System.Collections.Generic;
using System.Text;

namespace Kit.Sql.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class InitTableAttribute : Attribute
    {
    }
}
