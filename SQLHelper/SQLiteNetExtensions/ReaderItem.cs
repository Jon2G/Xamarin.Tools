using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kit.Sql.SQLiteNetExtensions
{
    public class ReaderItem
    {
        private readonly IDictionary<string, object> data;

        public object this[string propertyName]
        {
            get
            {
                if (data.ContainsKey(propertyName))
                    return data[propertyName];
                else
                    return null;
            }
            set
            {
                if (data.ContainsKey(propertyName))
                    data[propertyName] = value;
                else
                    data.Add(propertyName, value);
            }
        }

        /// <summary>
        /// Get column names
        /// </summary>
        public List<string> Fields
        {
            get
            {
                return data.Keys.ToList();
            }
        }

        public ReaderItem()
        {
            data = new Dictionary<string, object>();
        }
    }
}
