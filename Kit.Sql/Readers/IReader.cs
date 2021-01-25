using System;
using System.Collections.Generic;
using System.Text;

namespace Kit.Sql
{
    public interface IReader : IDisposable
    {
        public bool Read();
        public object this[int index] { get; }
        public object this[string columna] { get; }
        public int FieldCount { get; }
    }
}
