using System;
using System.Collections.Generic;
using System.Text;

namespace SQLHelper
{
    public class DBError : Exception
    {
        public readonly string ConnectionString;
        public readonly Exception Exception;
        public DBError(string ConnectionString, Exception Exception)
        {
            this.Exception = Exception;
            this.ConnectionString = ConnectionString;
        }
    }
}
