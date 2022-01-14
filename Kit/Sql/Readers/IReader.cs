using System;
using System.Data;

namespace Kit.Sql.Readers
{
    public interface IReader : IDisposable
    {
        public bool Read();
        public object this[int index] { get; }
        public object this[string columna] { get; }
        public int FieldCount { get; }
        public T Get<T>(int index) where T : IConvertible;
    }
}
