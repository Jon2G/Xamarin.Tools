using Kit.Daemon.Enums;
using SQLHelper;
using SQLHelper.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kit.Daemon
{
    public class DaemonConfig
    {
        public readonly BaseSQLHelper Local;
        public readonly BaseSQLHelper Remote;
        public readonly string DbVersion;

        /// <summary>
        /// Maximo que puede creceer el tiempo de descanso en segundos
        /// </summary>
        public int MaxSleep { get; private set; }
        internal DaemonConfig(string DbVersion, BaseSQLHelper Local, BaseSQLHelper Remote, int MaxSleep = 30)
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

        public SQLHLite GetSqlLiteConnection()
        {
            if (Local is SQLHLite sql)
            {
                return sql;
            }
            if (Remote is SQLHLite sql1)
            {
                return sql1;
            }
            return null;
        }
        internal SQLH GetSqlServerConnection()
        {
            if (Local is SQLH sql)
            {
                return sql;
            }
            if (Remote is SQLH sql1)
            {
                return sql1;
            }
            return null;
        }

    }
}
