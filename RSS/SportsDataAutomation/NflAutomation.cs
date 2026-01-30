using Microsoft.AspNetCore.Http;
using RSS_Services;

namespace RSS.SportsDataAutomation
{
    public class NflAutomation : BaseSportsAutomation
    {
        protected override string SportName => "NFL";
        protected override int LoadHourUtc => 9; // 1 AM PST
        private string SportsType = "american-football";
        private int LeagueId = 1;


        public NflAutomation(IServiceProvider serviceProvider) : base(serviceProvider) 
        {
           
        }

        protected override async Task<bool> HasTodaysDataBeenLoaded()
        {
            using var scope = _serviceProvider.CreateScope();
            var nflGameServices = scope.ServiceProvider.GetRequiredService<SportsGameServices>();
            return nflGameServices.AreGamesInDbForToday(SportsType, LeagueId);
        }

        protected override async Task TryToLoadAvailableGames()
        {
            using var scope = _serviceProvider.CreateScope();
            var nflGameServices = scope.ServiceProvider.GetRequiredService<SportsGameServices>();
            //var saveDataService = scope.ServiceProvider.GetRequiredService<GeneralServices>();

            //move to helper after
            var pstZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
            var todayPst = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, pstZone).Date;
            var dateString = todayPst.ToString("yyyy-MM-dd");

            //var gameUrl = $"https://v1.{SportsType}.api-sports.io/games?league={LeagueId}&date={dateString}&timezone=America/Los_Angeles";
            var gameUrl = $"https://v1.{SportsType}.api-sports.io/games?league=1&season=2022&team=1&timezone=America/Los_Angeles";


            var availableGames = await nflGameServices.GetGamesAvailableToday(SportsType, gameUrl);
            if (availableGames.Count > 0)
            {
                nflGameServices.SaveSportsData(availableGames);
            }
            
        }
    }
}