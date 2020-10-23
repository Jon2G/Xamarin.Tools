using SQLHelper;
using SQLHelper.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tools.Daemon
{
    public class DaemonConfig
    {
        public readonly BaseSQLHelper Source;
        public readonly BaseSQLHelper Destination;
        public readonly string DbVersion;

        /// <summary>
        /// Maximo que puede creceer el tiempo de descanso en segundos
        /// </summary>
        public readonly int MaxSleep;
        internal DaemonConfig(string DbVersion, BaseSQLHelper Source, BaseSQLHelper Destination, int MaxSleep = 30)
        {
            this.DbVersion = DbVersion;
            this.Source = Source;
            this.Destination = Destination;
            this.MaxSleep = MaxSleep;
        }
        internal IEnumerable<SQLH> GetSqlServerConnections()
        {
            List<SQLH> connections = new List<SQLH>();
            if (Source is SQLH sql)
            {
                connections.Add(sql);
            }
            if (Destination is SQLH sql1)
            {
                connections.Add(sql1);
            }
            return connections;
        }
        internal IEnumerable<SQLHLite> GetSqlLiteConnections()
        {
            List<SQLHLite> connections = new List<SQLHLite>();
            if (Source is SQLHLite sql)
            {
                connections.Add(sql);
            }
            if (Destination is SQLHLite sql1)
            {
                connections.Add(sql1);
            }
            return connections;
        }

    }
}
