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
        private readonly SportsGameServices _sportsGameServices;

        public TimeHelpers(SportsGameServices sportsGameServices)
        {
            _sportsGameServices = sportsGameServices;
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

        public bool HasGameStarted(string gameId)
        {
            var game = _sportsGameServices.GetDailySportGameById(gameId);
            var startTimeString = game.GameStartTime;

            var gameStartTime = TimeSpan.Parse(startTimeString);
            var currentTime = GetCurrentTimeInPst().TimeOfDay;

            if (currentTime >= gameStartTime)
            {
                return true;
            }
            return false;
        }
    }
}
