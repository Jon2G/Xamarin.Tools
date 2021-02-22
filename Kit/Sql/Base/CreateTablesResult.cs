using System;
using System.Collections.Generic;
using Kit.Sql.Enums;

namespace Kit.Sql.Base
{
    public class CreateTablesResult
    {
        public Dictionary<Type, CreateTableResult> Results { get; private set; }

        public CreateTablesResult ()
        {
            Results = new Dictionary<Type, CreateTableResult> ();
        }
    }
}