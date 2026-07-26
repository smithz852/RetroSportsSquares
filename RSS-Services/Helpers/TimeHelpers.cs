using TimeZoneConverter;

namespace RSS_Services.Helpers
{
    public class TimeHelpers
    {
        public static readonly TimeZoneInfo PacificZone = TZConvert.GetTimeZoneInfo("America/Los_Angeles");

        public string GetTimeStringTodayInPst()
        {
            var todayPst = GetTimeDateTimeTodayInPst();
            var dateString = todayPst.ToString("yyyy-MM-dd");
            return dateString;
        }

        public DateTime GetTimeDateTimeTodayInPst()
        {
            return TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, PacificZone).Date;
        }

        public DateTime GetCurrentTimeInPst()
        {
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, PacificZone);
        }
    }
}
