using System;
using System.Collections.Generic;
using System.Text;
using Kit.Daemon.Enums;
using Kit.Model;
using Kit.Sql.Attributes;
using Kit.Sql.Base;
using Kit.Sql.Tables;

namespace Kit.Daemon.Sync
{
    public abstract class ISync : ModelBase, IConvertible
    { /// <summary>
      /// This guid identifies the row where the change is made
      /// </summary>
        [Unique, NotNull, AutoIncrement]
        public Guid SyncGuid { get; set; }

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
        /// <summary>
        /// Mandatory for TwoWay Sync
        /// </summary>
        /// <returns></returns>
        public virtual object GetPk()
        {
            return SyncGuid;
        }

        public virtual SyncStatus GetSyncStatus(SqlBase source_con)
        {
            var history = source_con.Table<SyncHistory>().FirstOrDefault(x => x.SyncGuid == this.SyncGuid);
            if (history is null)
            {
                return SyncStatus.Pending;
            }
            return SyncStatus.Done;
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
    }
}
