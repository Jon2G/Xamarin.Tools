using System;
using System.Collections.Generic;
using System.Text;

namespace SQLHelper.Interfaces
{
    public class BaseSQLHelper:IDisposable
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
        public string FormatTime(TimeSpan TimeSpan)
        {
            return $"{TimeSpan:hh}:{TimeSpan:mm}:{TimeSpan:ss}";
        }
        public string FormatTime(DateTime TimeSpan)
        {
            //using (SQLiteConnection lite = Conecction())
            //{
            //'2020-09-17T12:27:55'  Formato universal de fecha y hora sql server
            return TimeSpan.ToString("yyyy-MM-ddTHH:mm:ss");
            //}
        }

        public void Dispose()
        {
         
        }
    }
}
