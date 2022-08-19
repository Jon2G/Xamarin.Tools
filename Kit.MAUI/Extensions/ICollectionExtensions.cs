using System.Collections.ObjectModel;

namespace Kit.MAUI
{
    public static class ICollectionExtensions
    {
        public static int FindIndexOf<T>(this ObservableCollection<T> modificadoresSeleccionados, Func<T, bool> p)
        {
            for (int i = 0; i < modificadoresSeleccionados.Count; i++)
            {
                T elemento = modificadoresSeleccionados[i];
                if (p.Invoke(elemento))
                {
                    return i;
                }
            }
            return -1;
        }

        public static int FindIndex<T>(this ObservableCollection<T> ts, Predicate<T> match)
        {
            return ts.FindIndex(0, ts.Count, match);
        }

        public static int FindIndex<T>(this ObservableCollection<T> ts, int startIndex, Predicate<T> match)
        {
            return ts.FindIndex(startIndex, ts.Count, match);
        }

        public static int FindIndex<T>(this ObservableCollection<T> ts, int startIndex, int count, Predicate<T> match)
        {
            if (startIndex < 0) startIndex = 0;
            if (count > ts.Count) count = ts.Count;

            for (int i = startIndex; i < count; i++)
            {
                if (match(ts[i])) return i;
            }

            return -1;
        }

        public static void AddRange<T>(this ICollection<T> ts, params T[] pelementos)
        {
            ts.AddRange<T>(elementos: pelementos);
        }

        public static void AddRange<T>(this ICollection<T> ts, IEnumerable<T> elementos)
        {
            foreach (T elemento in elementos)
            {
                ts.Add(elemento);
            }
        }
    }
}
