using Kit.Daemon;
using Kit.Sql;
using Kit.Sql.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaemonTest
{
    public static class AppData
    {
        public static SqlServer SQLH
        {
            get
            {
                return (Daemon.Current.DaemonConfig.Local as SqlServer);
            }
        }
        public static SqLite SQLHLite
        {
            get
            {
                return (Daemon.Current.DaemonConfig.Remote as SqLite);
            }
        }
    }
}
