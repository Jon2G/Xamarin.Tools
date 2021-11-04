using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using Kit.Sql.Attributes;
using static Kit.Sql.Base.BaseOrm;

namespace Kit.Sql.Sqlite
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

        public static string SqlDecl(TableMapping.Column p, bool storeDateTimeAsTicks, bool storeTimeSpanAsTicks)
        {
            string decl = "\"" + p.Name + "\" " + SqlType(p, storeDateTimeAsTicks, storeTimeSpanAsTicks) + " ";

            if (p.IsPK)
            {
                decl += "primary key ";
            }
            if (p.IsAutoInc)
            {
                decl += "autoincrement ";
            }
            if (!p.IsNullable)
            {
                decl += "not null ";
            }
            if (!string.IsNullOrEmpty(p.Collation))
            {
                decl += "collate " + p.Collation + " ";
            }

            return decl;
        }

        public static string SqlType(TableMapping.Column p, bool storeDateTimeAsTicks, bool storeTimeSpanAsTicks)
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

                return "varchar";
            }
            else if (clrType == typeof(TimeSpan))
            {
                return storeTimeSpanAsTicks ? "bigint" : "time";
            }
            else if (clrType == typeof(DateTime))
            {
                return storeDateTimeAsTicks ? "bigint" : "datetime";
            }
            else if (clrType == typeof(DateTimeOffset))
            {
                return "bigint";
            }
            else if (clrType.GetTypeInfo().IsEnum)
            {
                if (p.StoreAsText)
                    return "varchar";
                else
                    return "integer";
            }
            else if (clrType == typeof(byte[]))
            {
                return "blob";
            }
            else if (clrType == typeof(Guid))
            {
                return "varchar(36)";
            }
            else if (clrType == typeof(UInt64))
            {
                return "bigint ";
            }
            else
            {
                if (Tools.Debugging)
                {
                    Debugger.Break();
                    return "What?";
                }
                else
                {
                    throw new NotSupportedException($"Don't know about {clrType} on  property name:{p.Name}");
                }
            }
        }

        public static bool IsPK(MemberInfo p)
        {
            return p.CustomAttributes.Any(x => x.AttributeType == typeof(PrimaryKeyAttribute));
        }

        public static bool IsAutomatic(MemberInfo p)
        {
            return p.CustomAttributes.Any(x => x.AttributeType == typeof(AutomaticAttribute));
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