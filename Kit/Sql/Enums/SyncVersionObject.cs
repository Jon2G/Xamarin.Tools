using System;
using System.Collections.Generic;
using System.Text;
using Kit.Sql.Attributes;

namespace Kit.Sql.Enums
{
    [StoreAsText]
    public enum SyncVersionObject
    {
        Trigger,
        Table
    }
}
