using RSS_Services;
using RSS_Services.Helpers;

namespace RSS.SportsDataAutomation
{
    public class NbaAutomation : BaseSportsAutomation
    {
        protected override string SportName => "NBA";
        protected override int LoadHourPst => 1; //  AM PST

        private string SportsType = "basketball";
        private int LeagueId = 12;

        public NbaAutomation(IServiceProvider serviceProvider) : base(serviceProvider) 
        { 

        }

        protected override async Task<bool> HasTodaysDataBeenLoaded()
        {
            using var scope = _serviceProvider.CreateScope();
             var nbaGameServices = scope.ServiceProvider.GetRequiredService<SportsGameServices>();
             return nbaGameServices.AreGamesInDbForToday(SportsType, LeagueId);
        }

        protected override async Task TryToLoadAvailableGames()
        {
            using var scope = _serviceProvider.CreateScope();
             var nbaGameServices = scope.ServiceProvider.GetRequiredService<SportsGameServices>();
            var timeHelpers = scope.ServiceProvider.GetRequiredService<TimeHelpers>();

            var dateString = timeHelpers.GetTimeStringTodayInPst();

            var gameUrl = $"https://v1.{SportsType}.api-sports.io/games?date={dateString}&timezone=America%2FLos_Angeles";
            
            var availableGames = await nbaGameServices.GetGamesAvailableToday(SportsType, gameUrl);

            if (availableGames.Count > 0)
            {
                nbaGameServices.SaveSportsData(availableGames);
            }

        }
    }
}