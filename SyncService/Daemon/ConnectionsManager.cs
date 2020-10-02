using SQLHelper.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace SyncService.Daemon
{
    public class ConnectionsManager
    {
        private static ConnectionsManager _Instance;
        public static ConnectionsManager Instance
        {
            get
            {
                if (_Instance is null)
                {
                    throw new InvalidOperationException("Please Init ConnectionsManager with 'ConnectionsManager.Init()' before attempting to use it");
                }
                return _Instance;
            }
        }
        private readonly Dictionary<string, BaseSQLHelper> Connections;
        private ConnectionsManager()
        {
            this.Connections = new Dictionary<string, BaseSQLHelper>();
        }
        public static ConnectionsManager Init()
        {
            ConnectionsManager._Instance = new ConnectionsManager();
            return ConnectionsManager.Instance;
        }
        public ConnectionsManager AddConnection(string Name, BaseSQLHelper Connection)
        {
            this.Connections.Add(Name, Connection);
            return _Instance;
        }
        public BaseSQLHelper GetConnection(string Name)
        {
            return this.Connections[Name];
        }
    }
}
