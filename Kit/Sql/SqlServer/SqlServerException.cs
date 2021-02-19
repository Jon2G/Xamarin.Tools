using System;

namespace SQLServer
{

#if WINDOWS_PHONE && !USE_WP8_NATIVE_SQLITE
#define USE_CSHARP_SQLITE
#endif

    using System;
#if !USE_SQLITEPCL_RAW
	using System.Runtime.InteropServices;
#endif
    using System.Linq.Expressions;
#if USE_CSHARP_SQLITE
using Sqlite3 = Community.CsharpSqlite.Sqlite3;
using Sqlite3DatabaseHandle = Community.CsharpSqlite.Sqlite3.sqlite3;
using Sqlite3Statement = Community.CsharpSqlite.Sqlite3.Vdbe;
#elif USE_WP8_NATIVE_SQLITE
using Sqlite3 = Sqlite.Sqlite3;
using Sqlite3DatabaseHandle = Sqlite.Database;
using Sqlite3Statement = Sqlite.Statement;
#elif USE_SQLITEPCL_RAW

#else
	using Sqlite3DatabaseHandle = System.IntPtr;
	using Sqlite3BackupHandle = System.IntPtr;
	using Sqlite3Statement = System.IntPtr;
	using System.Data.SqlClient;
#endif

#pragma warning disable 1591 // XML Doc Comments


    public class SqlServerException : Exception
    {
        public SQLite3.Result Result { get; private set; }

        protected SqlServerException(SQLite3.Result r, string message) : base(message)
        {
            Result = r;
        }

        public static SqlServerException New(SQLite3.Result r, string message)
        {
            return new SqlServerException(r, message);
        }
    }
}


