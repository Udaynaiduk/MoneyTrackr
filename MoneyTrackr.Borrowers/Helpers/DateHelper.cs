namespace MoneyTrackr.Borrowers.Helpers
{
    public static class DateHelper
    {
        /// <summary>
        /// Calculates the number of full months and remaining days between two dates.
        /// </summary>
        /// <param name="start">Start date.</param>
        /// <param name="end">End date.</param>
        /// <returns>Tuple of (months, days).</returns>
        public static (int months, int days) CalculateFullMonths(DateTime start, DateTime end)
        {
            if (end < start)
                throw new ArgumentException("End date cannot be earlier than start date.");

            int totalMonths = (end.Year - start.Year) * 12 + end.Month - start.Month;
            int remainingDays = end.Day - start.Day;

            if (remainingDays < 0)
            {
                totalMonths--;
                DateTime previousMonth = end.AddMonths(-1);
                remainingDays += DateTime.DaysInMonth(previousMonth.Year, previousMonth.Month);
            }

            return (totalMonths, remainingDays);
        }
    }
}
