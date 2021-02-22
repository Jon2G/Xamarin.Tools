namespace Kit.Sql.Base
{
    public abstract class BaseTableQuery
    {
        protected class Ordering
        {
            public string ColumnName { get; set; }
            public bool Ascending { get; set; }
        }
        public class Condition
        {
            public Condition() { }

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
                this.ColumnName = left.ColumnName;
                this.Value = right.Value;
                this.IsComplete = true;
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
        }


    }
}