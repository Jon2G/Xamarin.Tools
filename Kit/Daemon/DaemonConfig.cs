using Kit.Daemon.Enums;
using Kit.Sql;
using Kit.Sql.Helpers;
using Kit.Sql.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using Kit.Sql.Base;
using Kit.Sql.Sqlite;
using SQLServer;

namespace Kit.Daemon
{
    public class DaemonConfig
    {
        public readonly SqlBase Local;
        public readonly SqlBase Remote;
        public readonly int DbVersion;

        /// <summary>
        /// Maximo que puede creceer el tiempo de descanso en segundos
        /// </summary>
        public int MaxSleep { get; private set; }
        internal DaemonConfig(int DbVersion, SqlBase Local, SqlBase Remote, int MaxSleep = 30)
        {
            this.DbVersion = DbVersion;
            this.Local = Local;
            this.Remote = Remote;
            this.MaxSleep = MaxSleep;
        }
        public SqlBase this[SyncDirecction direcction]
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

        public SQLiteConnection GetSqlLiteConnection()
        {
            if (Local is SQLiteConnection sql)
            {
                return sql;
            }
            if (Remote is SQLiteConnection sql1)
            {
                return sql1;
            }
            return null;
        }
        internal SQLServerConnection GetSqlServerConnection()
        {
            if (Local is SQLServerConnection sql)
            {
                return sql;
            }
            if (Remote is SQLServerConnection sql1)
            {
                return sql1;
            }
            return null;
        }

    }
}
