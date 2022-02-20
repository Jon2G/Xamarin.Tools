using Kit.Sql.Base;
using Kit.Sql.Sqlite;
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using TableMapping = Kit.Sql.Base.TableMapping;

namespace Kit.Daemon.Abstractions
{
    [Preserve(AllMembers = true)]
    public class DaemonCompiledSetterTuple
    {
        public DaemonCompiledSetter CompiledSetter;
        public IEnumerable<dynamic> Results;
        public DaemonCompiledSetterTuple(IEnumerable<dynamic> Results, DaemonCompiledSetter CompiledSetter)
        {
            this.Results = Results;
            this.CompiledSetter = CompiledSetter;
        }
    }
    [Preserve(AllMembers = true)]
    public abstract class DaemonCompiledSetter
    {
        public readonly Column[] Columns;
        public DaemonCompiledSetter(Column[] Columns) : base()
        {
            this.Columns = Columns;
        }
    }
    [Preserve(AllMembers = true)]
    public class DaemonCompiledSetter<T, TReader>  : DaemonCompiledSetter
    {
       
        public readonly Action<T, TReader, int>[] Setters;

        public DaemonCompiledSetter(Column[] Columns, Action<T, TReader, int>[] Setters):base(Columns)
        {
            this.Setters = Setters;
        }
    }
}
