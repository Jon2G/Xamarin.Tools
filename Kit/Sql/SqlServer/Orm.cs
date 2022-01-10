using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Kit.Sql.Attributes;
using Kit.Sql.Base;

namespace Kit.Sql.SqlServer
{
    public static class Orm
    {
        public const int DefaultMaxStringLength = 140;
        public const string ImplicitPkName = "Id";
        public const string ImplicitIndexSuffix = "Id";

        public static Type GetType(object obj)
        {
            if (obj == null)
                return typeof(object);
            var rt = obj as IReflectableType;
            if (rt != null)
                return rt.GetTypeInfo().AsType();
            return obj.GetType();
        }

        public static string SqlDecl(Column p)
        {
            string type = SqlType(p);
            string decl = "\"" + p.Name + "\" " +type + " ";

            if (p.IsPK)
            {
                if(p.ColumnType == typeof(string)&&decl.Contains("varchar(MAX)"))
                {
                    decl= "\"" + p.Name + "\" varchar(500) "; 
                }
                decl += "primary key ";
            }
            if (p.IsAutoInc)
            {
                decl += "IDENTITY(1,1) ";
            }
            if (!p.IsNullable)
            {
                decl += "not null ";
            }

            if (p.IsAutoGuid)
            {
                decl += "DEFAULT NEWID() ";
            }

            if (p.Indices.Any())
            {
                foreach (var i in p.Indices)
                {
                    if (i.Unique)
                    {
                        decl += "UNIQUE ";
                    }
                }
                //var indexes = new Dictionary<string, SQLServerConnection.IndexInfo> ();

                //foreach (var i in p.Indices) {
                //	//var iname = i.Name ?? map.TableName + "_" + c.Name;
                //	SQLServerConnection.IndexInfo iinfo;
                //	if (!indexes.TryGetValue (iname, out iinfo)) {
                //		iinfo = new SQLServerConnection.IndexInfo {
                //			IndexName = iname,
                //			TableName = map.TableName,
                //			Unique = i.Unique,
                //			Columns = new List<SQLServerConnection.IndexedColumn> ()
                //		};
                //		indexes.Add (iname, iinfo);
                //	}

                //	if (i.Unique != iinfo.Unique)
                //		throw new Exception ("All the columns in an index must have the same value for their Unique property");

                //	iinfo.Columns.Add (new SQLServerConnection.IndexedColumn {
                //		Order = i.Order,
                //		ColumnName = p.Name
                //	});
                //}

                //foreach (var indexName in indexes.Keys) {
                //	var index = indexes[indexName];
                //	var columns = index.Columns.OrderBy (i => i.Order).Select (i => i.ColumnName).ToArray ();
                //	CreateIndex (indexName, index.TableName, columns, index.Unique);
                //}
            }

            if (!string.IsNullOrEmpty(p.Collation))
            {
                decl += "collate " + p.Collation + " ";
            }

            return decl;
        }

        public static string SqlType(Column p)
        {
            var clrType = p.ColumnType;
            if (clrType == typeof(Boolean) || clrType == typeof(Byte) || clrType == typeof(UInt16) || clrType == typeof(SByte) || clrType == typeof(Int16) || clrType == typeof(Int32) || clrType == typeof(UInt32) || clrType == typeof(Int64))
            {
                return "integer";
            }
            else if (clrType == typeof(Single) || clrType == typeof(Double) || clrType == typeof(Decimal))
            {
                return "float";
            }
            else if (clrType == typeof(String) || clrType == typeof(StringBuilder) || clrType == typeof(Uri) || clrType == typeof(UriBuilder))
            {
                int? len = p.MaxStringLength;

                if (len.HasValue)
                    return "varchar(" + len.Value + ")";

                return "varchar(MAX)";
            }
            else if (clrType == typeof(TimeSpan))
            {
                return "time";
            }
            else if (clrType == typeof(DateTime))
            {
                return "datetime";
            }
            else if (clrType == typeof(DateTimeOffset))
            {
                return "bigint";
            }
            else if (clrType.GetTypeInfo().IsEnum)
            {
                if (p.StoreAsText)
                    return "varchar(MAX)";
                else
                    return "integer";
            }
            else if (clrType == typeof(byte[]))
            {
                return "VARBINARY(MAX)";
            }
            else if (clrType == typeof(Guid))
            {
                return "UNIQUEIDENTIFIER";
            }
            else
            {
                throw new NotSupportedException("Don't know about " + clrType);
            }
        }

        public static bool IsPK(MemberInfo p)
        {
            return p.CustomAttributes.Any(x => x.AttributeType == typeof(PrimaryKeyAttribute));
        }

        public static string Collation(MemberInfo p)
        {
#if ENABLE_IL2CPP
			return (p.GetCustomAttribute<CollationAttribute> ()?.Value) ?? "";
#else
            return
                (p.CustomAttributes
                    .Where(x => typeof(CollationAttribute) == x.AttributeType)
                    .Select(x =>
                    {
                        var args = x.ConstructorArguments;
                        return args.Count > 0 ? ((args[0].Value as string) ?? "") : "";
                    })
                    .FirstOrDefault()) ?? "";
#endif
        }

        public static bool IsAutoInc(MemberInfo p)
        {
            return p.CustomAttributes.Any(x => x.AttributeType == typeof(AutoIncrementAttribute));
        }

        public static FieldInfo GetField(TypeInfo t, string name)
        {
            var f = t.GetDeclaredField(name);
            if (f != null)
                return f;
            return GetField(t.BaseType.GetTypeInfo(), name);
        }

        public static PropertyInfo GetProperty(TypeInfo t, string name)
        {
            var f = t.GetDeclaredProperty(name);
            if (f != null)
                return f;
            return GetProperty(t.BaseType.GetTypeInfo(), name);
        }

        public static object InflateAttribute(CustomAttributeData x)
        {
            var atype = x.AttributeType;
            var typeInfo = atype.GetTypeInfo();
#if ENABLE_IL2CPP
			var r = Activator.CreateInstance (x.AttributeType);
#else
            var args = x.ConstructorArguments.Select(a => a.Value).ToArray();
            var r = Activator.CreateInstance(x.AttributeType, args);
            foreach (var arg in x.NamedArguments)
            {
                if (arg.IsField)
                {
                    GetField(typeInfo, arg.MemberName).SetValue(r, arg.TypedValue.Value);
                }
                else
                {
                    GetProperty(typeInfo, arg.MemberName).SetValue(r, arg.TypedValue.Value);
                }
            }
#endif
            return r;
        }

        public static IEnumerable<IndexedAttribute> GetIndices(MemberInfo p)
        {
#if ENABLE_IL2CPP
			return p.GetCustomAttributes<IndexedAttribute> ();
#else
            var indexedInfo = typeof(IndexedAttribute).GetTypeInfo();
            return
                p.CustomAttributes
                    .Where(x => indexedInfo.IsAssignableFrom(x.AttributeType.GetTypeInfo()))
                    .Select(x => (IndexedAttribute)InflateAttribute(x));
#endif
        }

        public static int? MaxStringLength(PropertyInfo p)
        {
#if ENABLE_IL2CPP
			return p.GetCustomAttribute<MaxLengthAttribute> ()?.Value;
#else
            var attr = p.CustomAttributes.FirstOrDefault(x => x.AttributeType == typeof(MaxLengthAttribute));
            if (attr != null)
            {
                var attrv = (MaxLengthAttribute)InflateAttribute(attr);
                return attrv.Value;
            }
            return null;
#endif
        }

        public static bool IsMarkedNotNull(MemberInfo p)
        {
            return p.CustomAttributes.Any(x => x.AttributeType == typeof(NotNullAttribute));
        }
    }
}