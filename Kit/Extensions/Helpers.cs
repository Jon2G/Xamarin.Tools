using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Kit.Extensions;
using Xamarin.Forms;

namespace Kit.Extensions
{
    public static class Helpers
    {
        public static ImageSource ByteToImage(this byte[] ByteArray)
        {
            return ImageSource.FromStream(() => new MemoryStream(ByteArray));
        }
        public static byte[] GetByteArray(this Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }
        public static async Task<byte[]> GetByteArray(StreamImageSource streamImageSource)
        {
            var stream = await streamImageSource.Stream.Invoke(System.Threading.CancellationToken.None);
            return stream.GetByteArray();
        }
        public static byte[] ImageToByte(this ImageSource ImageSource)
        {
            StreamImageSource streamImageSource = (StreamImageSource)ImageSource;
            System.Threading.CancellationToken cancellationToken = System.Threading.CancellationToken.None;
            Task<Stream> task = streamImageSource.Stream(cancellationToken);
            MemoryStream stream = task.Result as MemoryStream;
            return stream.ToArray();
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
        public static string ToImageString(this byte[] bytes)
        {
            return string.Concat("data:image/png;base64,", Convert.ToBase64String(bytes, 0, bytes.Length));
        }
        public static string EnLetra(this decimal Numero, string Leyenda, bool bolDecimales, string strMoneda = "")
        {
            string dec;

            var entero = Convert.ToInt64(Math.Truncate(Numero));
            var decimales = Convert.ToInt32(Math.Round((Numero - entero) * 100, 2));
            if (decimales > 0)
            {
                //dec = " PESOS CON " + decimales.ToString() + "/100";
                dec = $" {strMoneda} {decimales:0,0} /100";
            }
            //Código agregado por mí
            else
            {
                //dec = " PESOS CON " + decimales.ToString() + "/100";
                dec = $" {strMoneda} {decimales:0,0} /100";
            }
            var res = NumeroALetras(Convert.ToDouble(entero)) + dec;
            return $"{res} {Leyenda}";
        }



        private static string NumeroALetras(double value)
        {
            string num2Text; value = Math.Truncate(value);
            if (value == 0) num2Text = "CERO";
            else if (value == 1) num2Text = "UNO";
            else if (value == 2) num2Text = "DOS";
            else if (value == 3) num2Text = "TRES";
            else if (value == 4) num2Text = "CUATRO";
            else if (value == 5) num2Text = "CINCO";
            else if (value == 6) num2Text = "SEIS";
            else if (value == 7) num2Text = "SIETE";
            else if (value == 8) num2Text = "OCHO";
            else if (value == 9) num2Text = "NUEVE";
            else if (value == 10) num2Text = "DIEZ";
            else if (value == 11) num2Text = "ONCE";
            else if (value == 12) num2Text = "DOCE";
            else if (value == 13) num2Text = "TRECE";
            else if (value == 14) num2Text = "CATORCE";
            else if (value == 15) num2Text = "QUINCE";
            else if (value < 20) num2Text = "DIECI" + NumeroALetras(value - 10);
            else if (value == 20) num2Text = "VEINTE";
            else if (value < 30) num2Text = "VEINTI" + NumeroALetras(value - 20);
            else if (value == 30) num2Text = "TREINTA";
            else if (value == 40) num2Text = "CUARENTA";
            else if (value == 50) num2Text = "CINCUENTA";
            else if (value == 60) num2Text = "SESENTA";
            else if (value == 70) num2Text = "SETENTA";
            else if (value == 80) num2Text = "OCHENTA";
            else if (value == 90) num2Text = "NOVENTA";
            else if (value < 100) num2Text = NumeroALetras(Math.Truncate(value / 10) * 10) + " Y " + NumeroALetras(value % 10);
            else if (value == 100) num2Text = "CIEN";
            else if (value < 200) num2Text = "CIENTO " + NumeroALetras(value - 100);
            else if (value == 200 || value == 300 || value == 400 || value == 600 || value == 800) num2Text = NumeroALetras(Math.Truncate(value / 100)) + "CIENTOS";
            else if (value == 500) num2Text = "QUINIENTOS";
            else if (value == 700) num2Text = "SETECIENTOS";
            else if (value == 900) num2Text = "NOVECIENTOS";
            else if (value < 1000) num2Text = NumeroALetras(Math.Truncate(value / 100) * 100) + " " + NumeroALetras(value % 100);
            else if (value == 1000) num2Text = "MIL";
            else if (value < 2000) num2Text = "MIL " + NumeroALetras(value % 1000);
            else if (value < 1000000)
            {
                num2Text = NumeroALetras(Math.Truncate(value / 1000)) + " MIL";
                if (value % 1000 > 0)
                {
                    num2Text = num2Text + " " + NumeroALetras(value % 1000);
                }
            }
            else if (value == 1000000)
            {
                num2Text = "UN MILLON";
            }
            else if (value < 2000000)
            {
                num2Text = "UN MILLON " + NumeroALetras(value % 1000000);
            }
            else if (value < 1000000000000)
            {
                num2Text = NumeroALetras(Math.Truncate(value / 1000000)) + " MILLONES ";
                if (value - Math.Truncate(value / 1000000) * 1000000 > 0)
                {
                    num2Text = num2Text + " " + NumeroALetras(value - Math.Truncate(value / 1000000) * 1000000);
                }
            }
            else if (value == 1000000000000) num2Text = "UN BILLON";
            else if (value < 2000000000000) num2Text = "UN BILLON " + NumeroALetras(value - Math.Truncate(value / 1000000000000) * 1000000000000);
            else
            {
                num2Text = NumeroALetras(Math.Truncate(value / 1000000000000)) + " BILLONES";
                if (value - Math.Truncate(value / 1000000000000) * 1000000000000 > 0)
                {
                    num2Text = num2Text + " " + NumeroALetras(value - Math.Truncate(value / 1000000000000) * 1000000000000);
                }
            }
            return num2Text;
        }
#if NETSTANDARD1_0 || NETSTANDARD2_0 || NET462
        public static DataTable ToTable<T>(this List<T> lista)
        {
            DataTable data = null;
            //if (lista?.Count <= 0)
            //{
            //    return data;
            //}
            Type tipo = typeof(T);
            data = new DataTable(tipo.Name);
            foreach (PropertyInfo p in tipo.GetProperties())
            {
                data.Columns.Add(p.Name, p.PropertyType);
            }

            lista.ForEach(v =>
            {
                List<object> valores = new List<object>();
                foreach (DataColumn column in data.Columns)
                {
                    valores.Add(tipo.GetProperty(column.ColumnName).GetValue(v));
                }
                data.Rows.Add(valores.ToArray());
            });
            return data;
        }
        public static DataTable ToTable<T>(this IEnumerable<T> lista)
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
            foreach (var item in input)
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
        public static void AddRange<T>(this ObservableCollection<T> ts, IEnumerable<T> elementos)
        {
            foreach (T elemento in elementos)
            {
                ts.Add(elemento);
            }
        }
        public static string Dia(DayOfWeek day)
        {
            switch (day)
            {
                case DayOfWeek.Sunday:
                    return "Domingo";
                case DayOfWeek.Monday:
                    return "Lunes";
                case DayOfWeek.Tuesday:
                    return "Martes";
                case DayOfWeek.Wednesday:
                    return "Miércoles";
                case DayOfWeek.Thursday:
                    return "Jueves";
                case DayOfWeek.Friday:
                    return "Viernes";
                case DayOfWeek.Saturday:
                    return "Sábado";
                default:
                    throw new ArgumentOutOfRangeException();
            }

        }
        public static string Mes(int Mes)
        {
            switch (Mes)
            {
                case 1:
                    return "Enero";
                case 2:
                    return "Febrero";
                case 3:
                    return "Marzo";
                case 4:
                    return "Abril";
                case 5:
                    return "Mayo";
                case 6:
                    return "Junio";
                case 7:
                    return "Julio";
                case 8:
                    return "Agosto";
                case 9:
                    return "Septiembre";
                case 10:
                    return "Octubre";
                case 11:
                    return "Noviembre";
                case 12:
                    return "Diciembre";
                default:
                    throw new ArgumentException();
            }
        }

    }
}
