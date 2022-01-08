using Kit.Sql.Attributes;
using Kit.Sql.Enums;
using Kit.Sql.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using static Kit.Sql.Base.BaseOrm;

namespace Kit.Sql.Base
{
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

        public override string ToString()
        {
            return $"{Name}-{PropertyInfo}";
        }
    }
}
