using SQLite;
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Kit.Sql.SQLiteNetExtensions
{
    public static class SQLiteCommandReaderExtensions
    {
        internal const string DateTimeExactStoreFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff";

        /// <summary>
        /// Execute a query on the database
        /// </summary>
        /// <param name="sqliteConn">Connection to the database</param>
        /// <param name="cmdText">sql command</param>
        /// <param name="ps">parameters</param>
        /// <returns></returns>
        public static IEnumerable<ReaderItem> ExecuteReader(this SQLiteConnection sqliteConn, string cmdText)
        // TODO : Add params into method :
        //, params object[] ps)
        {
            SQLiteCommand command = sqliteConn.CreateCommand(cmdText);

            if (sqliteConn.Trace)
            {
                Debug.WriteLine("Executing Query: " + sqliteConn);
            }

            return command.ExecuteReader(sqliteConn);
        }

        private static sqlite3_stmt Prepare(SQLiteConnection connection, string commandText)
        {
            sqlite3_stmt stmt = SQLite3.Prepare2(connection.Handle, commandText);
            return stmt;
        }

        private static object ReadCol(SQLiteConnection connection, sqlite3_stmt stmt, int index, SQLite3.ColType type, Type clrType)
        {
            if (type == SQLite3.ColType.Null)
            {
                return null;
            }
            else
            {
                if (clrType == typeof(String))
                {
                    return SQLite3.ColumnString(stmt, index);
                }
                else if (clrType == typeof(Int32))
                {
                    return (int)SQLite3.ColumnInt(stmt, index);
                }
                else if (clrType == typeof(Boolean))
                {
                    return SQLite3.ColumnInt(stmt, index) == 1;
                }
                else if (clrType == typeof(double))
                {
                    return SQLite3.ColumnDouble(stmt, index);
                }
                else if (clrType == typeof(float))
                {
                    return (float)SQLite3.ColumnDouble(stmt, index);
                }
                else if (clrType == typeof(TimeSpan))
                {
                    return new TimeSpan(SQLite3.ColumnInt64(stmt, index));
                }
                else if (clrType == typeof(DateTime))
                {
                    if (connection.StoreDateTimeAsTicks)
                    {
                        return new DateTime(SQLite3.ColumnInt64(stmt, index));
                    }
                    else
                    {
                        string text = SQLite3.ColumnString(stmt, index);
                        DateTime resultDate;
                        if (!DateTime.TryParseExact(text, DateTimeExactStoreFormat, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out resultDate))
                        {
                            resultDate = DateTime.Parse(text);
                        }
                        return resultDate;
                    }
                }
                //                else if (clrType == typeof(DateTimeOffset))
                //                {
                //                    return new DateTimeOffset(SQLite3.ColumnInt64(stmt, index), TimeSpan.Zero);
                //#if !USE_NEW_REFLECTION_API
                //                }
                //                else if (clrType.IsEnum)
                //                {
                //#else
                //				} else if (clrType.GetTypeInfo().IsEnum) {
                //#endif
                //                    if (type == SQLite3.ColType.Text)
                //                    {
                //                        var value = SQLite3.ColumnString(stmt, index);
                //                        return Enum.Parse(clrType, value.ToString(), true);
                //                    }
                //                    else
                //                        return SQLite3.ColumnInt(stmt, index);
                //                }
                else if (clrType == typeof(Int64))
                {
                    return SQLite3.ColumnInt64(stmt, index);
                }
                else if (clrType == typeof(UInt32))
                {
                    return (uint)SQLite3.ColumnInt64(stmt, index);
                }
                else if (clrType == typeof(decimal))
                {
                    return (decimal)SQLite3.ColumnDouble(stmt, index);
                }
                else if (clrType == typeof(Byte))
                {
                    return (byte)SQLite3.ColumnInt(stmt, index);
                }
                else if (clrType == typeof(UInt16))
                {
                    return (ushort)SQLite3.ColumnInt(stmt, index);
                }
                else if (clrType == typeof(Int16))
                {
                    return (short)SQLite3.ColumnInt(stmt, index);
                }
                else if (clrType == typeof(sbyte))
                {
                    return (sbyte)SQLite3.ColumnInt(stmt, index);
                }
                else if (clrType == typeof(byte[]))
                {
                    return SQLite3.ColumnByteArray(stmt, index);
                }
                else if (clrType == typeof(Guid))
                {
                    string text = SQLite3.ColumnString(stmt, index);
                    return new Guid(text);
                }
                else
                {
                    throw new NotSupportedException("Don't know how to read " + clrType);
                }
            }
        }

        private static IEnumerable<ReaderItem> ExecuteReader(this SQLiteCommand sqliteCommand, SQLiteConnection connection)
        {
            sqlite3_stmt stmt = Prepare(connection, sqliteCommand.CommandText);
            try
            {
                // We need to manage columns dynamically in order to create columns 
                ColumnLite[] cols = new ColumnLite[SQLite3.ColumnCount(stmt)];


                while (SQLite3.Step(stmt) == SQLite3.Result.Row)
                {
                    ReaderItem obj = new ReaderItem();
                    for (int i = 0; i < cols.Length; i++)
                    {
                        if (cols[i] == null)
                        {
                            // We try to create column mapping if it's not already created : 
                            string name = SQLite3.ColumnName16(stmt, i);
                            cols[i] = new ColumnLite(name, SQLite3.ColumnType(stmt, i).ToType());
                        }
                        SQLite3.ColType colType = SQLite3.ColumnType(stmt, i);
                        object val = ReadCol(connection, stmt, i, colType, cols[i].ColumnType);
                        obj[cols[i].Name] = val;
                    }
                    yield return obj;
                }
            }
            finally
            {
                SQLite3.Finalize(stmt);
            }
        }

        /// <summary> 
        /// Get a typeof() element from ColType enumeration. 
        /// </summary> 
        /// <param name="colType"></param> 
        /// <returns></returns> 
        private static Type ToType(this SQLite3.ColType colType)
        {
            // Prepare evolution where column can be nullable 
            bool nullable = false;

            switch (colType)
            {
                case SQLite3.ColType.Blob:
                    return typeof(byte[]);
                case SQLite3.ColType.Float:
                    return nullable ? typeof(double?) : typeof(double);
                case SQLite3.ColType.Integer:
                    return nullable ? typeof(long?) : typeof(long);
                default:
                    return typeof(string);
            }
        }
    }
}
