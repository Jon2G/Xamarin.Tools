using Kit.Daemon;
using SQLHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaemonTest
{
    public static class AppData
    {
        public static SQLH SQLH
        {
            get
            {
                return (Daemon.Current.DaemonConfig.Local as SQLH);
            }
        }
        public static SQLHLite SQLHLite
        {
            get
            {
                return (Daemon.Current.DaemonConfig.Remote as SQLHLite);
            }
        }
    }
}
