using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Kit;
using Kit.Sql.Base;

namespace Kit.Sql.SqlServer
{
    public class SQLServerTableQuery<T> : TableQuery<T>
    {
        public SQLServerTableQuery(SqlBase conn) : base(conn)
        {
        }

        protected SQLServerTableQuery(SqlBase conn, Kit.Sql.Base.TableMapping table) : base(conn, table)
        {
        }

        public override TableQuery<U> Clone<U>()
        {
            var q = new SQLServerTableQuery<U>(Connection, Table);
            q._where = _where;
            q._deferred = _deferred;
            if (_orderBys != null)
            {
                q._orderBys = new List<Ordering>(_orderBys);
            }
            q._limit = _limit;
            q._offset = _offset;
            q._joinInner = _joinInner;
            q._joinInnerKeySelector = _joinInnerKeySelector;
            q._joinOuter = _joinOuter;
            q._joinOuterKeySelector = _joinOuterKeySelector;
            q._joinSelector = _joinSelector;
            q._selector = _selector;
            return q;
        }

        protected override CommandBase GenerateCommand(string selectionList)
        {
            if (_joinInner != null && _joinOuter != null)
            {
                throw new NotSupportedException("Joins are not supported.");
            }
            else
            {
                var cmdText = string.Empty;
                if (_limit.HasValue)
                {
                    cmdText += " top " + _limit.Value + " ";
                }
                cmdText += selectionList + " from \"" + Table.TableName + "\"";
                var args = new List<object>();
                var conditions = new List<Condition>();
                if (_where != null)
                {
                    var w = CompileExpr(_where, args, conditions);
                    conditions.RemoveAll(x => x.Value is Kit.Sql.Base.TableMapping);
                    conditions.RemoveAll(x => x.ColumnName == "?");
                    conditions.RemoveAll(x => !x.IsComplete);
                    conditions = conditions.DistinctBy(x => x.ColumnName).ToList();
                    cmdText += " where " + w.CommandText;
                }
                if ((_orderBys != null) && (_orderBys.Count > 0))
                {
                    var t = string.Join(", ", _orderBys.Select(o => "\"" + o.ColumnName + "\"" + (o.Ascending ? "" : " desc")).ToArray());
                    cmdText += " order by " + t;
                }
                if (_offset.HasValue)
                {
                    if (!_limit.HasValue)
                    {
                        cmdText = " top -1 " + cmdText;
                    }
                    cmdText += " offset " + _offset.Value;
                }

                cmdText = "select " + cmdText;
                if (Connection.IsClosed)
                    Connection.RenewConnection();

                return Connection.CreateCommand(cmdText, conditions.ToArray());
            }
        }
    }
}