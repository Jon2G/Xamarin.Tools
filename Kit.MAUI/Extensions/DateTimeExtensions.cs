namespace Kit.MAUI
{
    public static class DateTimeExtensions
    {
        public static int GetDifferenceInYears(this DateTime startDate, DateTime? endDate = null)
        {
            endDate ??= DateTime.Now;
            return (((DateTime)endDate).AddTicks(-startDate.Ticks).Year - 1);
        }
        public static int GetDifferenceInMonths(this DateTime startDate, DateTime? endDate = null)
        {
            endDate ??= DateTime.Now;
            int months = ((startDate.Year - endDate.Value.Year) * 12) + startDate.Month - endDate.Value.Month;
            return months;
        }
    }
}
