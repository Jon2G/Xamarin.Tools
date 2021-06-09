using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using Kit.Sql.Enums;

namespace Kit.Sql.SqlServer
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
                return string.Format("select * from \"{0}\" where \"{1}\" = @{1}", TableName, PK.Name);
            }
            else
            {
                // People should not be calling Get/Find without a PK
                return $"select top 1 * from \"{TableName}\"";
            }
        }
    }
}