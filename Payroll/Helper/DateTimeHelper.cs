using System.Globalization;

namespace Payroll.Helper
{
    public class DateTimeHelper
    {
        public static DateTime ConverPersianDateToMiladi(string date)
        {

            PersianCalendar persianCalendar = new PersianCalendar();
            int year = int.Parse(date.Substring(0, 4));
            int month = int.Parse(date.Substring(4, 2));
            int day = int.Parse(date.Substring(6, 2));

            return persianCalendar.ToDateTime(year, month, day, 0, 0, 0, 0);
        }
    }
}
