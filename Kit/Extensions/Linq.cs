using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Reflection;
using Kit.Sql.Base;

namespace Kit.Extensions
{
    public static class Linq
    {
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>
            (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> seenKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }

        public static List<T>[] Divide<T>(this IEnumerable<T> lista, int dividir)
        {
            if (dividir <= 0)
            {
                throw new ArgumentOutOfRangeException("No puede dividir una lista entre:" + dividir);
            }
            List<T>[] resultado = new List<T>[dividir];
            if (lista?.Count() < 0)
            {
                return resultado;
            }

            int xlista = lista.Count() / dividir;
            if (xlista <= 0) { xlista = 1; }

            int rango = 0;
            for (int i = 0; i < dividir; i++)
            {
                if (rango + xlista > lista.Count())
                {
                    continue;
                }

                resultado[i] = new List<T>(lista.GetRange(rango, xlista));
                rango += xlista;
            }
            if (rango < lista.Count())
            {
                resultado[0].AddRange(lista.GetRange(rango, lista.Count() - rango));
            }
            return resultado;
        }

        public static IEnumerable<T> GetRange<T>(this IEnumerable<T> input, int start, int end)
        {
            int i = 0;
            foreach (T item in input)
            {
                if (i < start) continue;
                if (i > end) break;

                yield return item;

                i++;
            }
        }

        public static List<T> Unir<T>(this List<T> lista, params List<T>[] listas)
        {
            foreach (List<T> l in listas)
            {
                lista.AddRange(l);
            }
            return lista;
        }

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

#if NETSTANDARD1_0 || NETSTANDARD2_0 || NET462

        public static DataTable ToTable<T>(this T obj)
        {
            TableMapping map = new Kit.Sql.Sqlite.TableMapping(typeof(T));
            DataTable data = new DataTable(map.TableName);
            foreach (var col in map.Columns)
            {
                data.Columns.Add(col.Name, col.ColumnType);
            }

            List<object> valores = new List<object>();
            foreach (var column in map.Columns)
            {
                valores.Add(column.GetValue(obj));
            }
            data.Rows.Add(valores.ToArray());

            return data;
        }

        public static DataTable ToTable<T>(this List<T> lista)
        {
            Type type = null;
            if (lista.Any())
            {
                type = lista.First().GetType();
            }
            else
            {
                type = typeof(T);
            }
            TableMapping map = new Kit.Sql.Sqlite.TableMapping(type);
            DataTable data = new DataTable(map.TableName);
            foreach (TableMapping.Column col in map.Columns)
            {
                data.Columns.Add(col.Name, col.ColumnType);
            }
            lista.ForEach(v =>
            {
                List<object> valores = new List<object>();
                foreach (var column in map.Columns)
                {
                    valores.Add(column.GetValue(v));
                }
                data.Rows.Add(valores.ToArray());
            });
            return data;
        }

        public static DataTable ToTable<T>(this IEnumerable<T> lista)
        {
            return lista.ToList().ToTable();
        }

        public static DataTable ToTable<T>(this ObservableCollection<T> lista)
        {
            return lista.ToList().ToTable();
        }

        public static DataTable ToTable<T>(this Collection<T> lista)
        {
            return lista.ToList().ToTable();
        }

        /// <summary>
        /// Convierte cualquier tabla a una Lista de objectos siempre que tengan campos publicos en común y un constructor publico sin parametros
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public static List<T> ToList<T>(this DataTable data)
        {
            List<T> lista = new List<T>();
            Type t = typeof(T);
            ConstructorInfo a = t.GetConstructor(new Type[0]); //Obtener el contructor por defecto
            foreach (DataRow row in data.Rows)
            {
                int i = 0;
                T valor = (T)Convert.ChangeType(a.Invoke(new object[0]), typeof(T)); //Debe regresar la instancia de clase
                foreach (DataColumn column in data.Columns)
                {
                    //Invocar la propiedad y establecer el valor que le corresponde
                    try
                    {
                        t.InvokeMember(
                            column.ColumnName,
                            BindingFlags.SetProperty |
                            BindingFlags.SetField |
                            BindingFlags.IgnoreCase |
                            BindingFlags.Public |
                            BindingFlags.Instance,
                            null,
                            valor,
                            new object[] { row[i] });
                    }
                    catch (Exception)
                    {
                        if (t.GetProperties().FirstOrDefault(x => x.Name.ToUpper() == column.ColumnName.ToUpper()) is PropertyInfo pr)
                        {
                            pr.SetValue(valor, Convert.ChangeType(row[i], pr.PropertyType));
                        }
                    }
                    i++;
                }
                lista.Add(valor);
            }
            return lista;
        }

        public static void InsertRow(this DataTable tabla, int index, DataRow fila)
        {
            DataRow dr = tabla.NewRow(); //Create New Row

            object[] array = new object[fila.ItemArray.Length];
            fila.ItemArray.CopyTo(array, 0);
            dr.ItemArray = array;

            tabla.Rows.InsertAt(dr, index); // InsertAt specified position
        }

#endif

        public static void Remove<T>(this ICollection<T> source, Func<T, bool> p)
        {
            for (int i=0;i<source.Count;i++)
            {
                T item = source.ElementAt(i);
                if (p.Invoke(item))
                {
                    source.Remove(item: item);
                }
            }
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source, Random rng)
        {
            T[] elements = source.ToArray();
            for (int i = elements.Length - 1; i >= 0; i--)
            {
                // Swap element "i" with a random earlier element it (or itself)
                // ... except we don't really need to swap it fully, as we can
                // return it immediately, and afterwards it's irrelevant.
                int swapIndex = rng.Next(i + 1);
                yield return elements[swapIndex];
                elements[swapIndex] = elements[i];
            }
        }

        public static Dictionary<Key, Value> Merge<Key, Value>(this Dictionary<Key, Value> left, Dictionary<Key, Value> right)
        {
            if (left == null)
            {
                throw new ArgumentNullException("Can't merge into a null dictionary");
            }
            else if (right == null)
            {
                return left;
            }

            foreach (var kvp in right)
            {
                if (!left.ContainsKey(kvp.Key))
                {
                    left.Add(kvp.Key, kvp.Value);
                }
            }

            return left;
        }
    }
}