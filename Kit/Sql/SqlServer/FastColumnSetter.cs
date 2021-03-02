using System;
using System.Data.SqlClient;
using System.Reflection;
using System.Text;
using Kit;
using Kit.Daemon.Sync;
using Kit.Sql.Attributes;
using Kit.Sql.Base;
using Kit.Sql.Helpers;

namespace SQLServer
{
    internal class FastColumnSetter
    {
        /// <summary>
        /// Creates a delegate that can be used to quickly set object members from query columns.
        ///
        /// Note that this frontloads the slow reflection-based type checking for columns to only happen once at the beginning of a query,
        /// and then afterwards each row of the query can invoke the delegate returned by this function to get much better performance (up to 10x speed boost, depending on query size and platform).
        /// </summary>
        /// <typeparam name="T">The type of the destination object that the query will read into</typeparam>
        /// <param name="conn">The active connection.  Note that this is primarily needed in order to read preferences regarding how certain data types (such as TimeSpan / DateTime) should be encoded in the database.</param>
        /// <param name="column">The table mapping used to map the statement column to a member of the destination object type</param>
        /// <returns>
        /// A delegate for fast-setting of object members from statement columns.
        ///
        /// If no fast setter is available for the requested column (enums in particular cause headache), then this function returns null.
        /// </returns>
        internal static Action<T, SqlDataReader, int> GetFastSetter<T>(TableMapping.Column column)
        {
            Action<T, SqlDataReader, int> fastSetter = null;
            if (column.PropertyInfo is null)
            {
                return null;
            }
            Type clrType = column.PropertyInfo.PropertyType;

            var clrTypeInfo = clrType.GetTypeInfo();
            if (clrTypeInfo.IsGenericType && clrTypeInfo.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                clrType = clrTypeInfo.GenericTypeArguments[0];
                clrTypeInfo = clrType.GetTypeInfo();
            }

            if (clrType == typeof(String))
            {
                fastSetter = CreateTypedSetterDelegate<T, string>(column, (reader, index) =>
                {
                    return Sqlh.Parse<string>(reader[index]);
                    //return Convert.ToString(reader[index]);
                });
            }
            else if (clrType == typeof(Int32))
            {
                fastSetter = CreateNullableTypedSetterDelegate<T, int>(column, (reader, index) =>
                {
                    return Convert.ToInt32(reader[index]);
                });
            }
            else if (clrType == typeof(Boolean))
            {
                fastSetter = CreateNullableTypedSetterDelegate<T, bool>(column, (reader, index) =>
                {
                    return Sqlh.Parse<Boolean>(reader[index], false);
                });
            }
            else if (clrType == typeof(double))
            {
                fastSetter = CreateNullableTypedSetterDelegate<T, double>(column, (reader, index) =>
                {
                    return Sqlh.Parse<double>(reader[index], 0);
                });
            }
            else if (clrType == typeof(float))
            {
                fastSetter = CreateNullableTypedSetterDelegate<T, float>(column, (reader, index) =>
                {
                    return Sqlh.Parse<float>(reader[index], 0);
                });
            }
            else if (clrType == typeof(TimeSpan))
            {

                fastSetter = CreateNullableTypedSetterDelegate<T, TimeSpan>(column, (reader, index) =>
                {
                    var text = Convert.ToString(reader[index]);
                    TimeSpan resultTime;
                    if (!TimeSpan.TryParseExact(text, "c", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.TimeSpanStyles.None, out resultTime))
                    {
                        resultTime = TimeSpan.Parse(text);
                    }
                    return resultTime;
                });

            }
            else if (clrType == typeof(DateTime))
            {

                fastSetter = CreateNullableTypedSetterDelegate<T, DateTime>(column, (reader, index) =>
                {
                    var text = Convert.ToString(reader[index]);
                    DateTime resultDate = DateTime.Now;
                    //if (!DateTime.TryParseExact (text, "", System.Globalization.CultureInfo.InvariantCulture, out resultDate)) {
                    //	resultDate = DateTime.Parse (text);
                    //}
                    return resultDate;
                });

            }
            else if (clrType == typeof(DateTimeOffset))
            {
                fastSetter = CreateNullableTypedSetterDelegate<T, DateTimeOffset>(column, (reader, index) =>
                {
                    return new DateTimeOffset(Convert.ToInt64(reader[index]), TimeSpan.Zero);
                });
            }
            else if (clrTypeInfo.IsEnum)
            {
                // NOTE: Not sure of a good way (if any?) to do a strongly-typed fast setter like this for enumerated types -- for now, return null and column sets will revert back to the safe (but slow) Reflection-based method of column prop.Set()
            }
            else if (clrType == typeof(Int64))
            {
                fastSetter = CreateNullableTypedSetterDelegate<T, Int64>(column, (reader, index) =>
                {
                    return Convert.ToInt64(reader[index]);
                });
            }
            else if (clrType == typeof(UInt32))
            {
                fastSetter = CreateNullableTypedSetterDelegate<T, UInt32>(column, (reader, index) =>
                {
                    return (uint)Convert.ToInt64(reader[index]);
                });
            }
            else if (clrType == typeof(decimal))
            {
                fastSetter = CreateNullableTypedSetterDelegate<T, decimal>(column, (reader, index) =>
                {
                    return (decimal)reader[index];
                });
            }
            else if (clrType == typeof(Byte))
            {
                fastSetter = CreateNullableTypedSetterDelegate<T, Byte>(column, (reader, index) =>
                {
                    return (byte)reader[index];
                });
            }
            else if (clrType == typeof(UInt16))
            {
                fastSetter = CreateNullableTypedSetterDelegate<T, UInt16>(column, (reader, index) =>
                {
                    return (ushort)reader[index];
                });
            }
            else if (clrType == typeof(Int16))
            {
                fastSetter = CreateNullableTypedSetterDelegate<T, Int16>(column, (reader, index) =>
                {
                    return (short)reader[index];
                });
            }
            else if (clrType == typeof(sbyte))
            {
                fastSetter = CreateNullableTypedSetterDelegate<T, sbyte>(column, (reader, index) =>
                {
                    return (sbyte)reader[index];
                });
            }
            else if (clrType == typeof(byte[]))
            {
                fastSetter = CreateTypedSetterDelegate<T, byte[]>(column, (reader, index) =>
                {
                    return (byte[])reader[index];
                });
            }
            else if (clrType == typeof(Guid))
            {
                fastSetter = CreateTypedSetterDelegate<T, Guid>(column, (reader, index) =>
                {
                    try
                    {
                        return (Guid)reader[index];
                    }
                    catch (Exception ex)
                    {
                        Log.Logger.Error(ex, "Al convertir {0} en Guid", reader[index]);
                    }

                    return Guid.NewGuid();
                });
            }
            else if (clrType == typeof(Uri))
            {
                fastSetter = CreateTypedSetterDelegate<T, Uri>(column, (reader, index) =>
                {
                    var text = Convert.ToString(reader[index]);
                    return new Uri(text);
                });
            }
            else if (clrType == typeof(StringBuilder))
            {
                fastSetter = CreateTypedSetterDelegate<T, StringBuilder>(column, (reader, index) =>
                {
                    var text = Convert.ToString(reader[index]);
                    return new StringBuilder(text);
                });
            }
            else if (clrType == typeof(UriBuilder))
            {
                fastSetter = CreateTypedSetterDelegate<T, UriBuilder>(column, (reader, index) =>
                {
                    var text = Convert.ToString(reader[index]);
                    return new UriBuilder(text);
                });
            }
            else
            {
                // NOTE: Will fall back to the slow setter method in the event that we are unable to create a fast setter delegate for a particular column type
            }
            return fastSetter;
        }

        /// <summary>
        /// This creates a strongly typed delegate that will permit fast setting of column values given a Sqlite3Statement and a column index.
        ///
        /// Note that this is identical to CreateTypedSetterDelegate(), but has an extra check to see if it should create a nullable version of the delegate.
        /// </summary>
        /// <typeparam name="ObjectType">The type of the object whose member column is being set</typeparam>
        /// <typeparam name="ColumnMemberType">The CLR type of the member in the object which corresponds to the given SQLite columnn</typeparam>
        /// <param name="column">The column mapping that identifies the target member of the destination object</param>
        /// <param name="getColumnValue">A lambda that can be used to retrieve the column value at query-time</param>
        /// <returns>A strongly-typed delegate</returns>
        private static Action<ObjectType, SqlDataReader, int> CreateNullableTypedSetterDelegate<ObjectType, ColumnMemberType>(TableMapping.Column column, Func<SqlDataReader, int, ColumnMemberType> getColumnValue) where ColumnMemberType : struct
        {
            var clrTypeInfo = column.PropertyInfo.PropertyType.GetTypeInfo();
            bool isNullable = false;

            if (clrTypeInfo.IsGenericType && clrTypeInfo.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                isNullable = true;
            }

            if (isNullable)
            {
                var setProperty = (Action<ObjectType, ColumnMemberType?>)Delegate.CreateDelegate(
                    typeof(Action<ObjectType, ColumnMemberType?>), null,
                    column.PropertyInfo.GetSetMethod());

                return (o, reader, i) =>
                {
                    var colType = reader.GetFieldType(i);
                    //if (colType != SQLite3.ColType.Null)
                    //	setProperty.Invoke (o, getColumnValue.Invoke (stmt, i));
                };
            }

            return CreateTypedSetterDelegate<ObjectType, ColumnMemberType>(column, getColumnValue);
        }

        /// <summary>
        /// This creates a strongly typed delegate that will permit fast setting of column values given a Sqlite3Statement and a column index.
        /// </summary>
        /// <typeparam name="ObjectType">The type of the object whose member column is being set</typeparam>
        /// <typeparam name="ColumnMemberType">The CLR type of the member in the object which corresponds to the given SQLite columnn</typeparam>
        /// <param name="column">The column mapping that identifies the target member of the destination object</param>
        /// <param name="getColumnValue">A lambda that can be used to retrieve the column value at query-time</param>
        /// <returns>A strongly-typed delegate</returns>
        private static Action<ObjectType, SqlDataReader, int> CreateTypedSetterDelegate<ObjectType, ColumnMemberType>(TableMapping.Column column, Func<SqlDataReader, int, ColumnMemberType> getColumnValue)
        {
            Action<ObjectType, ColumnMemberType> setProperty = null;
            try
            {
                if (column.Name == "SyncGuid")
                {
                    return (o, reader, i) =>
                    {
                        try
                        {
                            if (o is ISync isync)
                            {
                                isync.SyncGuid = (Guid)reader[i];
                                return;
                            }
                            column.PropertyInfo.GetSetMethod(false).Invoke(o,
                                    new object[] { reader[i] });

                        }
                        catch (Exception ex)
                        {
                            Log.Logger.Error(ex, "CreateTypedSetterDelegate");
                        }
                    };
                }
                //  var a = column.PropertyInfo.GetSetMethod(true);
                setProperty = (Action<ObjectType, ColumnMemberType>)Delegate.CreateDelegate(
               typeof(Action<ObjectType, ColumnMemberType>), null,
               column.PropertyInfo.GetSetMethod());
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "CreateTypedSetterDelegate");
            }

            return (o, reader, i) =>
            {
                try
                {
                    var colType = reader.GetFieldType(i);
                    if (colType != typeof(DBNull))
                        setProperty.Invoke(o, getColumnValue.Invoke(reader, i));
                }
                catch (Exception ex)
                {
                    Log.Logger.Error(ex, "CreateTypedSetterDelegate");
                }
            };
        }
    }
}