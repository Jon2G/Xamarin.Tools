using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using Kit.Sql.Base;
using Kit.Sql.Helpers;
using Kit.Sql.Readers;
using SQLitePCL;

namespace Kit.Sql.SqlServer
{
    public partial class SQLServerCommand : CommandBase
    {
        private SQLServerConnection _conn;
        private List<Binding> _bindings;
        public string CommandText { get; set; }
        public List<SqlParameter> Parameters { get; internal set; }

        public SQLServerCommand(SQLServerConnection conn, string cmd, params SqlParameter[] parameters)
        {
            this.Parameters = new List<SqlParameter>(parameters);
            CommandText = cmd;
            _conn = conn;
        }

        public SQLServerCommand(SQLServerConnection conn)
        {
            _conn = conn;
            _bindings = new List<Binding>();
            CommandText = "";
        }

        public override int ExecuteNonQuery()
        {
            if (_conn.Trace)
            {
                _conn.Tracer?.Invoke("Executing: " + this);
            }
            Log.Logger.Debug("Executing:[{0}]", CommandText);
            if (_conn.IsClosed)
            {
                _conn.RenewConnection();
            }
            using (var con = _conn.Connection)
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(CommandText, _conn.Connection))
                {
                    if (this.Parameters.Any())
                    {
                        cmd.Parameters.AddRange(this.Parameters.ToArray());
                    }

                    int affected = cmd.ExecuteNonQuery();
                    Log.Logger.Debug("Affected: {0} rows", affected);
                    return affected;
                }
            }
        }

        public override IEnumerable<T> ExecuteDeferredQuery<T>()
        {
            return ExecuteDeferredQuery<T>(_conn.GetMapping(typeof(T)));
        }

        public override List<T> ExecuteQuery<T>()
        {
            return ExecuteDeferredQuery<T>(_conn.GetMapping(typeof(T))).ToList();
        }

        public override List<T> ExecuteQuery<T>(Base.TableMapping map)
        {
            return ExecuteDeferredQuery<T>(map).ToList();
        }

        public override IReader ExecuteReader()
        {
            var cmd = new SqlCommand(this.CommandText, this._conn.Con());
            cmd.Parameters.AddRange(this.Parameters.ToArray());
            var reader = new Reader(cmd);
            return reader;
        }

        /// <summary>
        /// Invoked every time an instance is loaded from the database.
        /// </summary>
        /// <param name='obj'>
        /// The newly created object.
        /// </param>
        /// <remarks>
        /// This can be overridden in combination with the <see cref="SQLServerConnection.NewCommand"/>
        /// method to hook into the life-cycle of objects.
        /// </remarks>
        protected virtual void OnInstanceCreated(object obj)
        {
            // Can be overridden.
        }

        public override IEnumerable<T> ExecuteDeferredQuery<T>(Base.TableMapping map)
        {
            List<T> result = new List<T>();
            if (_conn.Trace)
            {
                _conn.Tracer?.Invoke("Executing Query: " + this);
            }
            Log.Logger.Debug("Executing:[{0}]", CommandText);
            if (_conn.IsClosed)
            {
                _conn.RenewConnection();
            }
            try
            {
                using (var con = _conn.Connection)
                {
                    con.Open();
                    using (var cmd = new SqlCommand(this.CommandText, con))
                    {
                        if (this.Parameters.Any())
                        {
                            cmd.Parameters.AddRange(Parameters.ToArray());
                        }
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var cols = new Base.TableMapping.Column[reader.FieldCount];
                                var fastColumnSetters = new Action<T, SqlDataReader, int>[reader.FieldCount];
                                for (int i = 0; i < cols.Length; i++)
                                {
                                    var name = reader.GetName(i);
                                    cols[i] = map.FindColumn(name);
                                    if (cols[i] != null)
                                        fastColumnSetters[i] = FastColumnSetter.GetFastSetter<T>(cols[i]);
                                }

                                do
                                {
                                    var obj = Activator.CreateInstance(map.MappedType);
                                    for (int i = 0; i < cols.Length; i++)
                                    {
                                        if (cols[i] == null)
                                            continue;

                                        if (fastColumnSetters[i] != null)
                                        {
                                            fastColumnSetters[i].Invoke((T)obj, reader, i);
                                        }
                                        else
                                        {
                                            var colType = reader.GetFieldType(i);
                                            var val = ReadCol(reader, i, colType, cols[i].ColumnType);
                                            cols[i].SetValue(obj, val);
                                        }
                                    }
                                    OnInstanceCreated(obj);
                                    result.Add((T)obj);
                                } while ((reader.Read()));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (Log.IsDBConnectionError(ex))
                {
                    Daemon.Daemon.OffLine = false;
                }
            }
            return result;
        }

        public override T ExecuteScalar<T>()
        {
            if (_conn.Trace)
            {
                _conn.Tracer?.Invoke("Executing Query: " + this);
            }

            T val = default(T);

            if (_conn.IsClosed)
            {
                _conn.RenewConnection();
            }
            using (var con = _conn.Connection)
            {
                con.Open();
                using (var cmd = new SqlCommand(this.CommandText + " select SCOPE_IDENTITY();", con))
                {
                    if (Parameters?.Any() ?? false)
                    {
                        cmd.Parameters.AddRange(Parameters.ToArray());
                    }
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var colval = ReadCol(reader, 0, reader.GetFieldType(0), typeof(T));
                            if (colval != null)
                            {
                                val = (T)colval;
                            }
                        }
                    }
                }
            }

            //var stmt = Prepare ();

            //try {
            //	var r = SQLite3.Step (stmt);
            //	if (r == SQLite3.Result.Row) {
            //		var colType = SQLite3.ColumnType (stmt, 0);
            //		var colval = ReadCol (stmt, 0, colType, typeof (T));
            //		if (colval != null) {
            //			val = (T)colval;
            //		}
            //	}
            //	else if (r == SQLite3.Result.Done) {
            //	}
            //	else {
            //		throw SQLiteException.New (r, SQLite3.GetErrmsg (_conn.Handle));
            //	}
            //}
            //finally {
            //	Finalize (stmt);
            //}

            return val;
        }

        public override IEnumerable<T> ExecuteQueryScalars<T>()
        {
            if (_conn.Trace)
            {
                _conn.Tracer?.Invoke("Executing Query: " + this);
            }
            //var stmt = Prepare ();
            //try {
            //	if (SQLite3.ColumnCount (stmt) < 1) {
            //		throw new InvalidOperationException ("QueryScalars should return at least one column");
            //	}
            //	while (SQLite3.Step (stmt) == SQLite3.Result.Row) {
            //		var colType = SQLite3.ColumnType (stmt, 0);
            //		var val = ReadCol (stmt, 0, colType, typeof (T));
            //		if (val == null) {
            //			yield return default (T);
            //		}
            //		else {
            //			yield return (T)val;
            //		}
            //	}
            //}
            //finally {
            //	Finalize (stmt);
            //}
            return null;
        }

        //public void Bind (string name, object val)
        //{
        //	_bindings.Add (new Binding {
        //		Name = name,
        //		Value = val
        //	});
        //}

        //public void Bind (object val)
        //{
        //	Bind (null, val);
        //}

        public override string ToString()
        {
            var parts = new string[1 + _bindings.Count];
            parts[0] = CommandText;
            var i = 1;
            foreach (var b in _bindings)
            {
                parts[i] = string.Format("  {0}: {1}", i - 1, b.Value);
                i++;
            }
            return string.Join(Environment.NewLine, parts);
        }

        public override void Dispose()
        {
            this._conn?.Close();
        }

        private SqlCommand Prepare(SqlCommand cmd)
        {
            cmd.Prepare();
            return cmd;
            //var stmt = SQLite3.Prepare2 (_conn.Handle, CommandText);
            //BindAll (stmt);
            //return stmt;
        }

        private void Finalize(SqlCommand cmd)
        {
            cmd.Dispose();
            //SQLite3.Finalize (stmt);
        }

        private void BindAll(sqlite3_stmt stmt)
        {
            int nextIdx = 1;
            foreach (var b in _bindings)
            {
                if (b.Name != null)
                {
                    b.Index = SQLite3.BindParameterIndex(stmt, b.Name);
                }
                else
                {
                    b.Index = nextIdx++;
                }

                BindParameter(stmt, b.Index, b.Value);
            }
        }

        private static IntPtr NegativePointer = new IntPtr(-1);

        internal static void BindParameter(sqlite3_stmt stmt, int index, object value)
        {
            if (value == null)
            {
                SQLite3.BindNull(stmt, index);
            }
            else
            {
                if (value is Int32)
                {
                    SQLite3.BindInt(stmt, index, (int)value);
                }
                else if (value is String)
                {
                    SQLite3.BindText(stmt, index, (string)value, -1, NegativePointer);
                }
                else if (value is Byte || value is UInt16 || value is SByte || value is Int16)
                {
                    SQLite3.BindInt(stmt, index, Convert.ToInt32(value));
                }
                else if (value is Boolean)
                {
                    SQLite3.BindInt(stmt, index, (bool)value ? 1 : 0);
                }
                else if (value is UInt32 || value is Int64)
                {
                    SQLite3.BindInt64(stmt, index, Convert.ToInt64(value));
                }
                else if (value is Single || value is Double || value is Decimal)
                {
                    SQLite3.BindDouble(stmt, index, Convert.ToDouble(value));
                }
                else if (value is TimeSpan)
                {
                    SQLite3.BindText(stmt, index, ((TimeSpan)value).ToString(), -1, NegativePointer);
                }
                else if (value is DateTime)
                {
                    SQLite3.BindText(stmt, index, ((DateTime)value).ToString(System.Globalization.CultureInfo.InvariantCulture), -1, NegativePointer);
                }
                else if (value is DateTimeOffset)
                {
                    SQLite3.BindInt64(stmt, index, ((DateTimeOffset)value).UtcTicks);
                }
                else if (value is byte[])
                {
                    SQLite3.BindBlob(stmt, index, (byte[])value, ((byte[])value).Length, NegativePointer);
                }
                else if (value is Guid)
                {
                    SQLite3.BindText(stmt, index, ((Guid)value).ToString(), 72, NegativePointer);
                }
                else if (value is Uri)
                {
                    SQLite3.BindText(stmt, index, ((Uri)value).ToString(), -1, NegativePointer);
                }
                else if (value is StringBuilder)
                {
                    SQLite3.BindText(stmt, index, ((StringBuilder)value).ToString(), -1, NegativePointer);
                }
                else if (value is UriBuilder)
                {
                    SQLite3.BindText(stmt, index, ((UriBuilder)value).ToString(), -1, NegativePointer);
                }
                else
                {
                    // Now we could possibly get an enum, retrieve cached info
                    var valueType = value.GetType();
                    var enumInfo = EnumCache.GetInfo(valueType);
                    if (enumInfo.IsEnum)
                    {
                        var enumIntValue = Convert.ToInt32(value);
                        if (enumInfo.StoreAsText)
                            SQLite3.BindText(stmt, index, enumInfo.EnumValues[enumIntValue], -1, NegativePointer);
                        else
                            SQLite3.BindInt(stmt, index, enumIntValue);
                    }
                    else
                    {
                        throw new NotSupportedException("Cannot store type: " + Orm.GetType(value));
                    }
                }
            }
        }

        private class Binding
        {
            public string Name { get; set; }

            public object Value { get; set; }

            public int Index { get; set; }
        }

        private object ReadCol(SqlDataReader reader, int index, Type coltype, Type clrType)
        {
            if (coltype == typeof(DBNull))
            {
                return null;
            }
            else
            {
                var clrTypeInfo = clrType.GetTypeInfo();
                if (clrTypeInfo.IsGenericType && clrTypeInfo.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    clrType = clrTypeInfo.GenericTypeArguments[0];
                    clrTypeInfo = clrType.GetTypeInfo();
                }

                if (clrType == typeof(String))
                {
                    return Convert.ToString(reader[index]);
                }
                else if (clrType == typeof(Int32))
                {
                    return (int)Convert.ToInt64(reader[index]);
                }
                else if (clrType == typeof(Boolean))
                {
                    return Convert.ToBoolean(reader[index]);
                }
                else if (clrType == typeof(double))
                {
                    return Convert.ToDouble(reader[index]);
                }
                else if (clrType == typeof(float))
                {
                    return (float)Convert.ToDouble(reader[index]);
                }
                else if (clrType == typeof(TimeSpan))
                {
                    var text = Convert.ToString(reader[index]);
                    TimeSpan resultTime;
                    if (!TimeSpan.TryParseExact(text, "c", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.TimeSpanStyles.None, out resultTime))
                    {
                        resultTime = TimeSpan.Parse(text);
                    }
                    return resultTime;
                }
                else if (clrType == typeof(DateTime))
                {
                    var text = Convert.ToString(reader[index]);
                    DateTime resultDate = DateTime.Now;

                    //if (!DateTime.TryParseExact (text, System.Globalization.CultureInfo.InvariantCulture, out resultDate)) {
                    //	resultDate = DateTime.Parse (text);
                    //}
                    return resultDate;
                }
                else if (clrType == typeof(DateTimeOffset))
                {
                    return new DateTimeOffset(Convert.ToInt64(reader[index]), TimeSpan.Zero);
                }
                else if (clrTypeInfo.IsEnum)
                {
                    if (coltype == typeof(String))
                    {
                        var value = Convert.ToString(reader[index]);
                        try
                        {
                            if (value == string.Empty)
                            {
                                return Enum.Parse(clrType, "0");
                            }
                            return Enum.Parse(clrType, value.ToString(), true);
                        }
                        catch (Exception e)
                        {
                            Log.Logger.Error(e, "Al intentar convertir [{0}] en [{1}]", value, clrType);
                        }

                        return null;
                    }
                    else
                        return Convert.ToInt32(reader[index]);
                }
                else if (clrType == typeof(Int64))
                {
                    return Convert.ToInt64(reader[index]);
                }
                else if (clrType == typeof(UInt32))
                {
                    return (uint)Convert.ToInt32(reader[index]);
                }
                else if (clrType == typeof(decimal))
                {
                    return (decimal)Convert.ToDouble(reader[index]);
                }
                else if (clrType == typeof(Byte))
                {
                    return (byte)Convert.ToInt32(reader[index]);
                }
                else if (clrType == typeof(UInt16))
                {
                    return (ushort)Convert.ToInt32(reader[index]);
                }
                else if (clrType == typeof(Int16))
                {
                    return (short)Convert.ToInt32(reader[index]);
                }
                else if (clrType == typeof(sbyte))
                {
                    return (sbyte)Convert.ToInt32(reader[index]);
                }
                else if (clrType == typeof(byte[]))
                {
                    return (byte[])(reader[index]);
                }
                else if (clrType == typeof(Guid))
                {
                    var text = Convert.ToString(reader[index]);
                    return new Guid(text);
                }
                else if (clrType == typeof(Uri))
                {
                    var text = Convert.ToString(reader[index]);
                    return new Uri(text);
                }
                else if (clrType == typeof(StringBuilder))
                {
                    var text = Convert.ToString(reader[index]);
                    return new StringBuilder(text);
                }
                else if (clrType == typeof(UriBuilder))
                {
                    var text = Convert.ToString(reader[index]);
                    return new UriBuilder(text);
                }
                else
                {
                    throw new NotSupportedException("Don't know how to read " + clrType);
                }
            }
        }
    }
}