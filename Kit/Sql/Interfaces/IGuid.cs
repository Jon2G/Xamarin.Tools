using Kit.Sql.Attributes;
using System;

namespace Kit.Sql.Interfaces
{
    public interface IGuid
    {
        [Unique, NotNull, AutoIncrement]
        public Guid Guid { get; set; }
    }
}
