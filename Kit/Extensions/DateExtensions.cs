using System;

namespace Kit
{
    public static class DateExtensions
    {
        public static DateTime GetNearest(this DayOfWeek day)
        {
            DateTime date = DateTime.Today;
            while (date.DayOfWeek != day)
            {
                if (date.DayOfWeek > day)
                {
                    date = date.AddDays(-1);
                }
                else
                {
                    date = date.AddDays(+1);
                }
            }

            return date;
        }

        public static string Dia(this DayOfWeek day)
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

        public static string Mes(this int Mes)
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