using System;

namespace Kit.Sql.Attributes
{
    /// <summary>
    /// Select the collating sequence to use on a column.
    /// "BINARY", "NOCASE", and "RTRIM" are supported.
    /// "BINARY" is the default.
    /// </summary>
    [AttributeUsage (AttributeTargets.Property)]
    public class CollationAttribute : Attribute
    {
        public string Value { get; private set; }

        public CollationAttribute (string collation)
        {
            Value = collation;
        }
    }
}