using System.Globalization;

namespace BalancedScorecard
{
    public static class Extensions
    {
        public static DateTime? ToDateTime(this string dateString, string format = "dd.MM.yyyy HH:mm:ss")
        {
            try
            {
                var dateTime = DateTime.ParseExact(dateString, format, CultureInfo.InvariantCulture);
                return dateTime;
            }
            catch (FormatException)
            {
                Console.WriteLine("The format of the date does not fit.");
                return null;
            }
        }

        public static DateTime ToStartOfDay(this DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 0, 0, 0, 0);
        }

        public static DateTime ToEndOfDay(this DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 23, 59, 59, 999).AddTicks(9999);
        }
    }
}
