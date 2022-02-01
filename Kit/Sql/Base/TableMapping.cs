using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kit.Daemon.Sync;
using Kit.Sql.Attributes;
using Kit.Sql.Enums;
using Kit.Sql.Interfaces;
using Kit.Sql.Sqlite;
using static Kit.Sql.Base.BaseOrm;

namespace Kit.Sql.Base
{
    public abstract class TableMapping : IComparable, IEquatable<TableMapping>,IGuid
    {
        public SyncMode SyncMode { get; private set; }
        public SyncDirection SyncDirection => SyncMode?.Direction ?? SyncDirection.NoSync;
        public Type MappedType { get; private set; }

        public string TableName { get; private set; }

        public bool WithoutRowId { get; private set; }

        public Column[] Columns { get; protected set; }

        public Column PK { get; protected set; }

        public string GetByPrimaryKeySql { get; protected set; }

        protected abstract string _GetByPrimaryKeySql();

        public CreateFlags CreateFlags { get; private set; }

        protected Column _autoPk;
        protected Column[] _insertColumns;
        protected Column[] _insertOrReplaceColumns;
        public Guid Guid { get; set; }
        public TableMapping(Type type, CreateFlags createFlags = CreateFlags.None)
        {
            Guid = Guid.NewGuid();
            MappedType = type;
            CreateFlags = createFlags;

            var typeInfo = GetTypeInfo(type);
#if ENABLE_IL2CPP
			var tableAttr = typeInfo.GetCustomAttribute<TableAttribute> ();
#else
            var tableAttr = GetTableAttributes(typeInfo);
#endif
            TableName = GetTableName(tableAttr, type);

            WithoutRowId = tableAttr != null ? tableAttr.WithoutRowId : false;

            this.SyncMode = GetSyncModeAttribute(typeInfo) ?? new SyncMode();
            if (SyncDirection != SyncDirection.NoSync)
            {
                if (!type.IsSubclassOf(typeof(ISync)))
                {
                    throw new Exception($"{type} - Must implement ISync");
                }
            }
            ReadColumns(type, createFlags);
        }
        protected virtual void ReadColumns(Type type, CreateFlags createFlags)
        {
            var props = new List<PropertyInfo>();
            var baseType = type;
            var propNames = new HashSet<string>();
            while (baseType != typeof(object))
            {
                var ti = baseType.GetTypeInfo();
                var newProps = (
                    from p in ti.DeclaredProperties
                    where
                        !propNames.Contains(p.Name) &&
                        !props.Any(x => x.Name == p.Name) &&
                        p.CanRead && p.CanWrite &&
                        (p.GetMethod != null) && (p.SetMethod != null) &&
                        (p.GetMethod.IsPublic && p.SetMethod.IsPublic) &&
                        (!p.GetMethod.IsStatic) && (!p.SetMethod.IsStatic)
                    select p).ToList();
                foreach (var p in newProps)
                {
                    propNames.Add(p.Name);
                }
                props.AddRange(newProps);
                baseType = ti.BaseType;
            }

            var cols = new List<Column>();
            foreach (var p in props)
            {
                var ignore = p.IsDefined(typeof(IgnoreAttribute), true);
                if (!ignore)
                {
                    cols.Add(new Column(p, createFlags));
                }
            }

            var pks = cols.Where(x => x.IsPK).ToList();
            if (pks.Count > 1)
            {
                pks.Remove(pks.First());
                pks.ForEach(pk => cols.Remove(pk));
            }

            foreach (var c in cols)
            {
                if (c.IsAutoInc && c.IsPK)
                {
                    _autoPk = c;
                }
                if (c.IsPK)
                {
                    PK = c;
                }
            }

            HasAutoIncPK = _autoPk != null;

            GetByPrimaryKeySql = _GetByPrimaryKeySql();

            //if (cols.FirstOrDefault(x => x.Name == "SyncGuid") is Column syncguidcol)
            //{
            //    this.SyncGuid = syncguidcol;
            //}
            //else
            //{
            //    this.SyncGuid = new GuidColumn();
            //    cols.Add(this.SyncGuid);
            //}

            Columns = cols.ToArray();

            _insertColumns = Columns.Where(c => !c.IsAutoInc && !c.IsAutomatic).ToArray();
            _insertOrReplaceColumns = Columns.Where(c => !c.IsAutomatic).ToArray();
        }

        internal void Merge(TableMapping tableMapping)
        {
            List<Column> columns = new List<Column>(this.Columns);
            columns.AddRange(tableMapping.Columns.Where(x => !this.Columns.Any(c => c.Name == x.Name)));
            this.Columns = columns.ToArray();
        }

        protected static TypeInfo GetTypeInfo(Type type)
        {
            return type.GetTypeInfo();
        }

        public static string GetTableName(Type type)
        {
            return GetTableName(GetTableAttributes(GetTypeInfo(type)), type);
        }

        protected static TableAttribute GetTableAttributes(TypeInfo typeInfo)
        {
            return typeInfo.CustomAttributes
                .Where(x => x.AttributeType == typeof(TableAttribute))
                .Select(x => (TableAttribute)InflateAttribute(x))
                .FirstOrDefault();
        }

        protected static SyncMode GetSyncModeAttribute(object obj)
        {
            return GetSyncModeAttribute(obj.GetType().GetTypeInfo());
        }

        protected static SyncMode GetSyncModeAttribute(TypeInfo typeInfo)
        {
            return typeInfo.CustomAttributes
                .Where(x => x.AttributeType == typeof(SyncMode))
                .Select(x => (SyncMode)InflateAttribute(x))
                .FirstOrDefault();
        }

        protected static string GetTableName(TableAttribute tableAttr, Type MappedType)
        {
            return (tableAttr != null && !string.IsNullOrEmpty(tableAttr.Name)) ? tableAttr.Name : MappedType.Name;
        }

        public bool HasAutoIncPK { get; protected set; }

        public void SetAutoIncPK(object obj, long id)
        {
            if (_autoPk != null)
            {
                _autoPk.SetValue(obj, Convert.ChangeType(id, _autoPk.ColumnType, null));
            }
        }

        public Column[] InsertColumns
        {
            get
            {
                return _insertColumns;
            }
        }

        public Column[] InsertOrReplaceColumns
        {
            get
            {
                return _insertOrReplaceColumns;
            }
        }

        private string _SelectionList;

        public string SelectionList
        {
            get
            {
                if (string.IsNullOrEmpty(_SelectionList))
                {
                    _SelectionList = string.Join(",", Columns.Select(c => c.Name.ToUpper()));
                }

                return _SelectionList;
            }
        }



        public Column FindColumnWithPropertyName(string propertyName)
        {
            var exact = Columns.FirstOrDefault(c => c.PropertyName == propertyName);
            return exact;
        }

        public Column FindColumn(string columnName)
        {
            var exact = Columns.FirstOrDefault(c => c.Name.ToLower() == columnName.ToLower());
            return exact;
        }

        public int CompareTo(object obj)
        {
            if (obj is TableMapping map)
            {
                var thisType = this.GetType();
                var remoteType = map.GetType();
                if (remoteType.Equals(thisType))
                {
                    return map.TableName.CompareTo(this.TableName);
                }
            }
            return -1;
        }

        public bool Equals(TableMapping other)
        {
            return other?.CompareTo(this) == 0;
        }
        public override bool Equals(object obj)
        {
            if (obj is TableMapping map)
                return Equals(other: map);
            return false;
        }
        public override int GetHashCode()
        {
            return this.Guid.GetHashCode();
        }
    }
}