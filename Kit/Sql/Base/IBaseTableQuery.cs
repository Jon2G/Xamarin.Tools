using System;
using System.Collections.Generic;
using System.Text;

namespace Kit.Sql.Base
{
    public interface IBaseTableQuery<T>
    {
        /// <summary>
        /// Returns the first element of this query, or null if no element is found.
        /// </summary>
        public T FirstOrDefault();
        /// <summary>
        /// Returns the first element of this query, or a new element if no element is found.
        /// </summary>
        public T FirstOrNew<T>() where T : new();
    }
}
