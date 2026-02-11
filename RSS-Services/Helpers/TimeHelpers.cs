using RSS_DB.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSS_Services.Helpers
{
    public class TimeHelpers
    {
        public TimeHelpers()
        {
            
        }
        public string GetTimeStringTodayInPst()
        {
            var todayPst = GetTimeDateTimeTodayInPst();
            var dateString = todayPst.ToString("yyyy-MM-dd");
            return dateString;
        }

        public DateTime GetTimeDateTimeTodayInPst()
        {
            var pstZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
            var todayPst = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, pstZone).Date;
            return todayPst;
        }

        public DateTime GetCurrentTimeInPst()
        {
            var pstZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
            var currentTimePst = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, pstZone);
            return currentTimePst;
        }

        
    }
}
