using System;
using System.Collections.Generic;
using System.Text;

namespace SQLHelper.Interfaces
{
    public class BaseSQLHelper
    {
        public event EventHandler OnConnectionStringChanged;
        public const int Error = -2;
        private string _ConnectionString;
        public string ConnectionString
        {
            get => _ConnectionString;
            protected set
            {
                if (_ConnectionString != value)
                {
                    _ConnectionString = value;
                    this.OnConnectionStringChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        protected BaseSQLHelper() { }
        public BaseSQLHelper(string ConnectionString)
        {
            this.ConnectionString = ConnectionString;
        }

    }
}
