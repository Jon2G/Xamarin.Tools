using System;

namespace Kit.Sql.SQLiteNetExtensions
{
    internal class ColumnLite
    {
        public string Name { get; private set; }

        public Type ColumnType { get; private set; }

        /// <summary>
        /// Internal constructor used for Dynamic queries that returns IDictionary
        /// </summary>
        /// <param name="name"></param>
        /// <param name="columnType"></param>
        internal ColumnLite(string name, Type columnType)
        {
            Name = name;
            ColumnType = columnType;
        }
    }
}