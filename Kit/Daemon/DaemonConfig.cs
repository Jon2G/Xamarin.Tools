using System.Data;
using Kit.Daemon.Enums;




namespace Kit.Daemon
{
    public class DaemonConfig
    {
        public readonly IDbConnection Local;
        public readonly IDbConnection Remote;
        public readonly int DbVersion;

        /// <summary>
        /// Maximo que puede creceer el tiempo de descanso en segundos
        /// </summary>
        public int MaxSleep { get; private set; }
        internal DaemonConfig(int DbVersion, IDbConnection Local, IDbConnection Remote, int MaxSleep = 30)
        {
            this.DbVersion = DbVersion;
            this.Local = Local;
            this.Remote = Remote;
            this.MaxSleep = MaxSleep;
        }
        public IDbConnection this[SyncTarget direcction]
        {
            get
            {
                switch (direcction)
                {
                    case SyncTarget.Remote:
                        return Remote;
                    case SyncTarget.Local:
                        return Local;
                }
                return null;
            }
        }

        public void SetMaxSleep(int MaxSleep = 30)
        {
            this.MaxSleep = MaxSleep;
        }

        public IDbConnection GetSqlLiteConnection()
        {
            if (Local is IDbConnection sql)
            {
                return sql;
            }
            if (Remote is IDbConnection sql1)
            {
                return sql1;
            }
            return null;
        }
        internal IDbConnection GetSqlServerConnection()
        {
            if (Local is IDbConnection sql)
            {
                return sql;
            }
            if (Remote is IDbConnection sql1)
            {
                return sql1;
            }
            return null;
        }

    }
}
