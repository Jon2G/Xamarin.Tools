using Kit.Daemon.Enums;
using Kit.Sql;
using Kit.Sql.Helpers;
using Kit.Sql.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kit.Daemon
{
    public class DaemonConfig
    {
        public readonly BaseSQLHelper Local;
        public readonly BaseSQLHelper Remote;
        public readonly ulong DbVersion;

        /// <summary>
        /// Maximo que puede creceer el tiempo de descanso en segundos
        /// </summary>
        public int MaxSleep { get; private set; }
        internal DaemonConfig(ulong DbVersion, BaseSQLHelper Local, BaseSQLHelper Remote, int MaxSleep = 30)
        {
            this.DbVersion = DbVersion;
            this.Local = Local;
            this.Remote = Remote;
            this.MaxSleep = MaxSleep;
        }
        public BaseSQLHelper this[SyncDirecction direcction]
        {
            get
            {
                switch (direcction)
                {
                    case SyncDirecction.Remote:
                        return Remote;
                    case SyncDirecction.Local:
                        return Local;
                }
                return null;
            }
        }

        public void SetMaxSleep(int MaxSleep = 30)
        {
            this.MaxSleep = MaxSleep;
        }

        public SqLite GetSqlLiteConnection()
        {
            if (Local is SqLite sql)
            {
                return sql;
            }
            if (Remote is SqLite sql1)
            {
                return sql1;
            }
            return null;
        }
        internal SqlServer GetSqlServerConnection()
        {
            if (Local is SqlServer sql)
            {
                return sql;
            }
            if (Remote is SqlServer sql1)
            {
                return sql1;
            }
            return null;
        }

    }
}
