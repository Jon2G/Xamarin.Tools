using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kit.Sql.Attributes;
using Kit.Sql.Enums;
using Kit.Sql.Sqlite;

namespace Kit.Sql.Base
{
    public class TableMapping
    {
        public Type MappedType { get; private set; }

        public string TableName { get; private set; }

        public bool WithoutRowId { get; private set; }

        public Column[] Columns { get; private set; }

        public Column PK { get; private set; }
        public Column SyncGuid { get; private set; }

        public string GetByPrimaryKeySql { get; private set; }

        public CreateFlags CreateFlags { get; private set; }

        readonly Column _autoPk;
        readonly Column[] _insertColumns;
        readonly Column[] _insertOrReplaceColumns;

        public TableMapping (Type type, CreateFlags createFlags = CreateFlags.None)
        {
            MappedType = type;
            CreateFlags = createFlags;

            var typeInfo = type.GetTypeInfo ();
#if ENABLE_IL2CPP
			var tableAttr = typeInfo.GetCustomAttribute<TableAttribute> ();
#else
            var tableAttr =
                typeInfo.CustomAttributes
                    .Where (x => x.AttributeType == typeof (TableAttribute))
                    .Select (x => (TableAttribute)Orm.InflateAttribute (x))
                    .FirstOrDefault ();
#endif

            TableName = (tableAttr != null && !string.IsNullOrEmpty (tableAttr.Name)) ? tableAttr.Name : MappedType.Name;
            WithoutRowId = tableAttr != null ? tableAttr.WithoutRowId : false;

            var props = new List<PropertyInfo> ();
            var baseType = type;
            var propNames = new HashSet<string> ();
            while (baseType != typeof (object)) {
                var ti = baseType.GetTypeInfo ();
                var newProps = (
                    from p in ti.DeclaredProperties
                    where
                        !propNames.Contains (p.Name) &&
                        p.CanRead && p.CanWrite &&
                        (p.GetMethod != null) && (p.SetMethod != null) &&
                        (p.GetMethod.IsPublic && p.SetMethod.IsPublic) &&
                        (!p.GetMethod.IsStatic) && (!p.SetMethod.IsStatic)
                    select p).ToList ();
                foreach (var p in newProps) {
                    propNames.Add (p.Name);
                }
                props.AddRange (newProps);
                baseType = ti.BaseType;
            }

            var cols = new List<Column> ();
            foreach (var p in props) {
                var ignore = p.IsDefined (typeof (IgnoreAttribute), true);
                if (!ignore) {
                    cols.Add (new Column (p, createFlags));
                }
            }

            foreach (var c in cols) {
                if (c.IsAutoInc && c.IsPK) {
                    _autoPk = c;
                }
                if (c.IsPK) {
                    PK = c;
                }
            }

            HasAutoIncPK = _autoPk != null;

            if (PK != null) {
                GetByPrimaryKeySql = string.Format ("select * from \"{0}\" where \"{1}\" = ?", TableName, PK.Name);
            }
            else {
                // People should not be calling Get/Find without a PK
                GetByPrimaryKeySql = string.Format ("select * from \"{0}\" limit 1", TableName);
            }

            if (cols.FirstOrDefault (x => x.Name == "SyncGuid") is Column syncguidcol) {
                this.SyncGuid = syncguidcol;
            }
            else {
                this.SyncGuid = new GuidColumn ();
                cols.Add (this.SyncGuid);
            }

            Columns = cols.ToArray ();

            _insertColumns = Columns.Where (c => !c.IsAutoInc).ToArray ();
            _insertOrReplaceColumns = Columns.ToArray ();
        }

        public bool HasAutoIncPK { get; private set; }

        public void SetAutoIncPK (object obj, long id)
        {
            if (_autoPk != null) {
                _autoPk.SetValue (obj, Convert.ChangeType (id, _autoPk.ColumnType, null));
            }
        }

        public Column[] InsertColumns {
            get {
                return _insertColumns;
            }
        }

        public Column[] InsertOrReplaceColumns {
            get {
                return _insertOrReplaceColumns;
            }
        }

        public Column FindColumnWithPropertyName (string propertyName)
        {
            var exact = Columns.FirstOrDefault (c => c.PropertyName == propertyName);
            return exact;
        }

        public Column FindColumn (string columnName)
        {
            var exact = Columns.FirstOrDefault (c => c.Name.ToLower () == columnName.ToLower ());
            return exact;
        }

        public class Column
        {
            PropertyInfo _prop;

            public string Name { get; protected set; }

            public PropertyInfo PropertyInfo => _prop;

            public string PropertyName { get { return _prop.Name; } }

            public Type ColumnType { get; protected set; }

            public string Collation { get; protected set; }

            public bool IsAutoInc { get; private set; }
            public bool IsAutoGuid { get; protected set; }

            public bool IsPK { get; protected set; }

            public IEnumerable<IndexedAttribute> Indices { get; set; }

            public bool IsNullable { get; protected set; }

            public int? MaxStringLength { get; protected set; }

            public bool StoreAsText { get; protected set; }

            protected Column () { }
            public Column (PropertyInfo prop, CreateFlags createFlags = CreateFlags.None)
            {
                var colAttr = prop.CustomAttributes.FirstOrDefault (x => x.AttributeType == typeof (ColumnAttribute));

                _prop = prop;
#if ENABLE_IL2CPP
                var ca = prop.GetCustomAttribute(typeof(ColumnAttribute)) as ColumnAttribute;
				Name = ca == null ? prop.Name : ca.Name;
#else
                Name = (colAttr != null && colAttr.ConstructorArguments.Count > 0) ?
                    colAttr.ConstructorArguments[0].Value?.ToString () :
                    prop.Name;
#endif
                //If this type is Nullable<T> then Nullable.GetUnderlyingType returns the T, otherwise it returns null, so get the actual type instead
                ColumnType = Nullable.GetUnderlyingType (prop.PropertyType) ?? prop.PropertyType;
                Collation = Orm.Collation (prop);

                IsPK = Orm.IsPK (prop) ||
                       (((createFlags & CreateFlags.ImplicitPK) == CreateFlags.ImplicitPK) &&
                        string.Compare (prop.Name, Orm.ImplicitPkName, StringComparison.OrdinalIgnoreCase) == 0);

                var isAuto = Orm.IsAutoInc (prop) || (IsPK && ((createFlags & CreateFlags.AutoIncPK) == CreateFlags.AutoIncPK));
                IsAutoGuid = isAuto && ColumnType == typeof (Guid);
                IsAutoInc = isAuto && !IsAutoGuid;

                Indices = Orm.GetIndices (prop);
                if (!Indices.Any ()
                    && !IsPK
                    && ((createFlags & CreateFlags.ImplicitIndex) == CreateFlags.ImplicitIndex)
                    && Name.EndsWith (Orm.ImplicitIndexSuffix, StringComparison.OrdinalIgnoreCase)
                ) {
                    Indices = new IndexedAttribute[] { new IndexedAttribute () };
                }
                IsNullable = !(IsPK || Orm.IsMarkedNotNull (prop));
                MaxStringLength = Orm.MaxStringLength (prop);

                StoreAsText = prop.PropertyType.GetTypeInfo ().CustomAttributes.Any (x => x.AttributeType == typeof (StoreAsTextAttribute));
            }

            public virtual void SetValue (object obj, object val)
            {
                if (val != null && ColumnType.GetTypeInfo ().IsEnum) {
                    _prop.SetValue (obj, Enum.ToObject (ColumnType, val));
                }
                else {
                    _prop.SetValue (obj, val, null);
                }
            }

            public virtual object GetValue (object obj)
            {
                return _prop.GetValue (obj, null);
            }
        }

        public class GuidColumn : Column
        {
            private Guid SyncGuid;
            public GuidColumn () : base ()
            {
                Name = "SyncGuid";
                //If this type is Nullable<T> then Nullable.GetUnderlyingType returns the T, otherwise it returns null, so get the actual type instead
                ColumnType = typeof (Guid);
                Collation = "";
                IsPK = false;
                IsAutoGuid = true;
                IsNullable = false;
                MaxStringLength = null;
                StoreAsText = ColumnType.CustomAttributes.Any (x => x.AttributeType == typeof (StoreAsTextAttribute));
                Indices = new IndexedAttribute[] { new IndexedAttribute (Name, -1) { Unique = true } };

            }

            public void SetValue ()
            {
                SyncGuid = Guid.NewGuid ();
            }
            public override void SetValue (object obj, object val)
            {
                if (val is Guid guid) {
                    SyncGuid = guid;
                    return;
                }
                SetValue ();
            }

            public Guid GetValue ()
            {
                return SyncGuid;
            }
            public override object GetValue (object obj)
            {
                return GetValue ();
            }
        }
    }
}