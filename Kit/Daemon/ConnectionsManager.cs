using SQLHelper.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kit.Daemon
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
            Connections = new Dictionary<string, BaseSQLHelper>();
        }
        public static ConnectionsManager Init()
        {
            _Instance = new ConnectionsManager();
            return Instance;
        }
        public ConnectionsManager AddConnection(string Name, BaseSQLHelper Connection)
        {
            Connections.Add(Name, Connection);
            return _Instance;
        }
        public BaseSQLHelper GetConnection(string Name)
        {
            return Connections[Name];
        }
    }
}
