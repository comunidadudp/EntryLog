namespace EntryLog.Business.Utils
{
    public static class TimeFunctions
    {
        private const string SA_PACIFIC_STANDARD_TIME = "SA Pacific Standard Time";

        public static DateTime GetSAPacificStandardTime()
            => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById(SA_PACIFIC_STANDARD_TIME));

        public static DateTime GetSAPacificStandardTime(DateTime utcDate)
            => TimeZoneInfo.ConvertTimeFromUtc(utcDate, TimeZoneInfo.FindSystemTimeZoneById(SA_PACIFIC_STANDARD_TIME));
    }
}
