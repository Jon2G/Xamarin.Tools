using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Kit.Daemon.Enums;
using Kit.Model;
using Kit.Sql.Attributes;
using Kit.Sql.Base;
using Kit.Sql.Enums;
using Kit.Sql.Interfaces;
using Kit.Sql.Sqlite;
using Kit.Sql.Tables;

namespace Kit.Daemon.Sync
{
    public abstract class ISync : ModelBase, IConvertible, IGuid
    {
        //[Ignore]
        //public Guid SyncGuid { get => Guid; set => Guid = value; }
        /// <summary>
        /// This guid identifies the row where the change is made
        /// </summary>
        [Unique, NotNull, AutoIncrement, Column("SyncGuid")]
        public virtual Guid Guid { get; set; }

        public virtual bool CustomUpload(SqlBase con, SqlBase targecon)
        {
            return false;
        }

        public virtual bool Affects(Kit.Sql.Sqlite.SQLiteConnection con, object PreviousId)
        {
            return false;
        }

        public virtual bool ShouldSync(SqlBase source_con, SqlBase target_con)
        {
            return true;
        }

        public virtual void OnDownloaded(NotifyTableChangedAction action)
        {
        }

        public virtual void OnUploaded(NotifyTableChangedAction action)
        {
        }

        /// <summary>
        /// Mandatory for TwoWay Sync
        /// </summary>
        /// <returns></returns>
        public virtual object GetPk()
        {
            return Guid;
        }

        public virtual SyncStatus GetSyncStatus(SqlBase source_con)
        {
            return GetSyncStatus(source_con, this.Guid);
        }

        public static SyncStatus GetSyncStatus(SqlBase source_con, Guid guid)
        {
            var history = source_con.Table<SyncHistory>().FirstOrDefault(x => x.Guid == guid);
            if (history is null)
            {
                ChangesHistory changes = source_con.Table<ChangesHistory>().FirstOrDefault(x => x.Guid == guid);
                if (changes is null)
                {
                    //Has been downloaded by Daemon service since daemon downloads dont get inserted here
                    return SyncStatus.Done;
                }
                return SyncStatus.Pending;
            }
            return SyncStatus.Done;
        }

        protected static Guid GetGuid<T>(SqlBase source_con, object id)
        {
            Guid result = Guid.NewGuid();
            var map = source_con.GetMapping<T>();
            string text = source_con.Single<string>($"SELECT SyncGuid FROM {map.TableName} where {map.PK.Name}='{id}'");
            if (!string.IsNullOrEmpty(text))
            {
                Guid.TryParse(text, out result);
            }
            return result;
        }

        public static bool IsSynced<T>(SqlBase source_con, int id)
        {
            Guid guid = GetGuid<T>(source_con, id);
            return GetSyncStatus(source_con, guid) == SyncStatus.Done;
        }

        public TypeCode GetTypeCode()
        {
            return TypeCode.Object;
        }

        public bool ToBoolean(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }

        public byte ToByte(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }

        public char ToChar(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }

        public DateTime ToDateTime(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }

        public decimal ToDecimal(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }

        public double ToDouble(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }

        public short ToInt16(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }

        public int ToInt32(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }

        public long ToInt64(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }

        public sbyte ToSByte(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }

        public float ToSingle(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }

        public string ToString(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }

        public object ToType(Type conversionType, IFormatProvider provider)
        {
            if (conversionType != typeof(ISync) && !conversionType.IsSubclassOf(typeof(ISync)))
            {
                throw new InvalidCastException();
            }
            return (ISync)this;
        }

        public ushort ToUInt16(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }

        public uint ToUInt32(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }

        public ulong ToUInt64(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }

        internal void OnSynced(SyncDirecction direccion, NotifyTableChangedAction action)
        {
            switch (direccion)
            {
                case SyncDirecction.Local:
                    OnDownloaded(action);
                    break;

                case SyncDirecction.Remote:
                    OnUploaded(action);
                    break;
            }
        }
        public virtual ISync GetBySyncGuid(SqlBase con, Guid syncguid)
        {
            var table = con.Table(this.GetType()).Table;
            string selection_list = table.SelectionList;
            string condition = (con is SQLiteConnection ? "SyncGuid=?" : "SyncGuid=@SyncGuid");
            CommandBase command = con.CreateCommand($"SELECT {selection_list} FROM {table.TableName} WHERE {condition}",
             new BaseTableQuery.Condition("SyncGuid", syncguid));

            MethodInfo method = command.GetType().GetMethod(nameof(CommandBase.ExecuteDeferredQuery), new[] { typeof(Kit.Sql.Base.TableMapping) });
            method = method.MakeGenericMethod(table.MappedType);
            IEnumerable<dynamic> result = (IEnumerable<dynamic>)method.Invoke(command, new object[] { table });

            dynamic i_result = result.FirstOrDefault();
            ISync read = Convert.ChangeType(i_result, typeof(ISync));

            return read;
        }
    }
}