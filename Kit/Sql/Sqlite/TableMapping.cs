using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Kit.Sql.Attributes;
using Kit.Sql.Enums;

namespace Kit.Sql.Sqlite
{
    public class TableMapping : Kit.Sql.Base.TableMapping
    {
        public TableMapping(Type type, CreateFlags createFlags = CreateFlags.None) : base(type, createFlags)
        {
        }

        protected override string _GetByPrimaryKeySql()
        {
            if (PK != null)
            {
                return string.Format("select * from \"{0}\" where \"{1}\" = ?", TableName, PK.Name);
            }
            else
            {
                // People should not be calling Get/Find without a PK
                return string.Format("select * from \"{0}\" limit 1", TableName);
            }
        }
    }
}