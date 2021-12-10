using System;
using System.Collections.Generic;
using System.Text;
using Kit.Enums;

namespace Kit.Sql.SqlServer
{
    public struct SqlServerInformation
    {
        public SqlServerVersion Version { get; }
        public string Level { get; }
        public string Edition { get; }

        public SqlServerInformation(SqlServerVersion Version, string Level, string Edition)
        {
            this.Version = Version;
            this.Level = Level;
            this.Edition = Edition;
        }
    }
}
