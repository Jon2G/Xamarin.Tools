using Kit.Sql.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kit.Sql.Interfaces
{
    public interface IGuid
    {
        [Unique, NotNull, AutoIncrement]
        public Guid Guid { get; set; }
    }
}
