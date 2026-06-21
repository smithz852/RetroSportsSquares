using RSS_DB;
using RSS_DB.Entities;

namespace RSS
{
    public static class DevDataSeeder
    {
        public static void Seed(AppDbContext context)
        {
            var testGames = new List<DailySportsGames>
            {
                new DailySportsGames
                {
                    ApiGameId = 123490,
                    HomeTeam = "Lakers",
                    AwayTeam = "Warriors",
                    GameStartTime = DateTimeOffset.UtcNow,
                    SportType = "basketball",
                    League = "NBA",
                    LeagueId = 12,
                    Status = "NS",
                    InUse = false
                },
                // Add more test games here
            };

            foreach (var game in testGames)
            {
                if (!context.DailySportsGames.Any(g => g.ApiGameId == game.ApiGameId))
                    context.DailySportsGames.Add(game);
            }

            context.SaveChanges();
        }
    }
}
