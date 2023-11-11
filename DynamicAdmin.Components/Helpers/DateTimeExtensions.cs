using System.Globalization;

namespace DynamicAdmin.Components.Helpers;

public static class DateTimeExtensions
{
    public static string DateToString(this DateTime date) => date.ToShortDateString();
    public static string WeekOfYear(this DateTime date) => CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstDay, DayOfWeek.Sunday).ToString();
    public static string MonthToString(this DateTime date) => date.ToString("MMMM");
    public static string YearToString(this DateTime date) => date.Year.ToString();
}
