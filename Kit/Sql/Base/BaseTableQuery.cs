using System.Linq.Expressions;
using System.Text;

namespace Kit.Sql.Base
{
    public abstract class BaseTableQuery
    {
        public TableMapping Table { get; protected set; }

        protected class Ordering
        {
            public string ColumnName { get; set; }
            public bool Ascending { get; set; }
        }

        public class Condition
        {
            public Condition()
            {
            }

            public Condition(string ColumnName)
            {
                this.ColumnName = ColumnName;
                this.IsComplete = false;
            }

            public Condition(object value)
            {
                this.Value = value;
                this.IsComplete = false;
            }

            public Condition(Condition left, Condition right)
            {
                if (right is null || left is null)
                {
                    this.IsComplete = false;
                    return;
                }
                this.ColumnName = left.ColumnName;
                this.Value = right.Value;
                this.IsComplete = true;
                if (string.IsNullOrEmpty(ColumnName))
                {
                    this.IsComplete = false;
                }
            }

            public Condition(string columnName, object Value)
            {
                this.ColumnName = columnName;
                this.Value = Value;
                if (!string.IsNullOrEmpty(ColumnName))
                {
                    this.IsComplete = true;
                }
            }

            public void SetValue(object Value)
            {
            }

            public void SetColumnName(string ColumnName)
            {
            }

            public string ColumnName
            {
                get;
                private set;
            }

            public object Value
            {
                get;
                private set;
            }

            public bool IsComplete { get; private set; }

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder().Append('[').Append(this.ColumnName).Append(']').Append('{').Append(this.Value).Append('}');
                return sb.ToString();
            }
        }
    }
}