using System;

namespace Kit.Sql.Attributes
{
    [AttributeUsage (AttributeTargets.Property)]
    public class UniqueAttribute : IndexedAttribute
    {
        public override bool Unique {
            get { return true; }
            set { /* throw?  */ }
        }
    }
}