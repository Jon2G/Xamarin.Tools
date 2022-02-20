using System;

namespace Kit.Sql.Attributes
{
    /// <summary>
    /// This value should never been inserted/updated beacause its provided by the database
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class AutomaticAttribute : Attribute
    {
    }
}