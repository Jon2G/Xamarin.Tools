using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Kit.Daemon.Sync;
using Kit.Sql.Attributes;
using Kit.Sql.Enums;
using Kit.Sql.Sqlite;
using static Kit.Sql.Base.BaseOrm;

namespace Kit.Sql.Base
{
    public abstract class TableMapping
    {
        public SyncMode SyncMode { get; private set; }
        public SyncDirection SyncDirection => SyncMode?.Direction ?? SyncDirection.NoSync;
        public Type MappedType { get; private set; }

        public string TableName { get; private set; }

        public bool WithoutRowId { get; private set; }

        public Column[] Columns { get; private set; }

        public Column PK { get; private set; }

        public string GetByPrimaryKeySql { get; protected set; }

        protected abstract string _GetByPrimaryKeySql();

        public CreateFlags CreateFlags { get; private set; }

        private readonly Column _autoPk;
        private readonly Column[] _insertColumns;
        private readonly Column[] _insertOrReplaceColumns;

        public TableMapping(Type type, CreateFlags createFlags = CreateFlags.None)
        {
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

        public bool HasAutoIncPK { get; private set; }

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

        public class Column
        {
            private PropertyInfo _prop;

            public string Name { get; protected set; }

            public PropertyInfo PropertyInfo => _prop;

            public string PropertyName { get { return _prop.Name; } }

            public Type ColumnType { get; protected set; }

            public string Collation { get; protected set; }

            public bool IsAutoInc { get; private set; }
            public bool IsAutoGuid { get; protected set; }
            public bool IsAutomatic { get; protected set; }

            public bool IsPK { get; protected set; }

            public IEnumerable<IndexedAttribute> Indices { get; set; }

            public bool IsNullable { get; protected set; }

            public int? MaxStringLength { get; protected set; }

            public bool StoreAsText { get; protected set; }

            protected Column()
            {
            }

            public Column(PropertyInfo prop, CreateFlags createFlags = CreateFlags.None)
            {
                var colAttr = prop.CustomAttributes.FirstOrDefault(x => x.AttributeType == typeof(ColumnAttribute));

                _prop = prop;
#if ENABLE_IL2CPP
                var ca = prop.GetCustomAttribute(typeof(ColumnAttribute)) as ColumnAttribute;
				Name = ca == null ? prop.Name : ca.Name;
#else
                Name = (colAttr != null && colAttr.ConstructorArguments.Count > 0) ?
                    colAttr.ConstructorArguments[0].Value?.ToString() :
                    prop.Name;
#endif
                //If this type is Nullable<T> then Nullable.GetUnderlyingType returns the T, otherwise it returns null, so get the actual type instead
                ColumnType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                Collation = Orm.Collation(prop);

                IsPK = Orm.IsPK(prop) ||
                       (((createFlags & CreateFlags.ImplicitPK) == CreateFlags.ImplicitPK) &&
                        string.Compare(prop.Name, Orm.ImplicitPkName, StringComparison.OrdinalIgnoreCase) == 0);

                var isAuto = Orm.IsAutoInc(prop) || (IsPK && ((createFlags & CreateFlags.AutoIncPK) == CreateFlags.AutoIncPK));
                IsAutoGuid = isAuto && ColumnType == typeof(Guid);
                IsAutoInc = isAuto && !IsAutoGuid;
                IsAutomatic = Orm.IsAutomatic(prop);

                Indices = Orm.GetIndices(prop);
                if (!Indices.Any()
                    && !IsPK
                    && ((createFlags & CreateFlags.ImplicitIndex) == CreateFlags.ImplicitIndex)
                    && Name.EndsWith(Orm.ImplicitIndexSuffix, StringComparison.OrdinalIgnoreCase)
                )
                {
                    Indices = new IndexedAttribute[] { new IndexedAttribute() };
                }
                IsNullable = !(IsPK || Orm.IsMarkedNotNull(prop));
                MaxStringLength = Orm.MaxStringLength(prop);

                StoreAsText = prop.PropertyType.GetTypeInfo().CustomAttributes.Any(x => x.AttributeType == typeof(StoreAsTextAttribute));
            }

            public virtual void SetValue(object obj, object val)
            {
                if (val != null && ColumnType.GetTypeInfo().IsEnum)
                {
                    _prop.SetValue(obj, Enum.ToObject(ColumnType, val));
                }
                else
                {
                    _prop.SetValue(obj, val, null);
                }
            }

            public virtual object GetValue(object obj)
            {
                return _prop.GetValue(obj, null);
            }
        }
    }
}