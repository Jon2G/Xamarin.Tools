using System;
using System.Collections.Generic;
using System.Text;
using Kit.Sql.Attributes;

namespace Kit.Sql.Tables
{
    public class SyncHistory
    {
        [PrimaryKey,AutoIncrement]
        public int Id { get; set; }
        public int ChangeId { get; set; }
        public int DevideId { get; set; }
    }
}
