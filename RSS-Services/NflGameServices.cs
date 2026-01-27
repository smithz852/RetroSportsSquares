using RSS_DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RSS_Services
{
    public class NflGameServices
    {
        private readonly AppDbContext _appDbContext;
        private readonly HttpClient _httpClient;

        public NflGameServices(AppDbContext appDbContext, HttpClient httpClient)
        {
            _appDbContext = appDbContext;
            _httpClient = httpClient;
        }

        public bool AreGamesInDbForToday()
        {
            var pstZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
            var todayPst = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, pstZone).Date;
            return _appDbContext.DailyNflGames
                .Any(g => g.StartTime.Date == todayPst);
        }

        public async Task<bool> AreNflGamesAvailableToday()
        {
            try
            {
                var pstZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
                var todayPst = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, pstZone).Date;
                var dateString = todayPst.ToString("yyyy-MM-dd");
                
                var response = await _httpClient.GetAsync($"https://v1.american-football.api-sports.io/games?league=1&date={dateString}&timezone=America/Los_Angeles");
                response.EnsureSuccessStatusCode();
                
                var json = await response.Content.ReadAsStringAsync();
                
                using var document = JsonDocument.Parse(json);
                var results = document.RootElement.GetProperty("results").GetInt32();

                if (results == 0)
                {
                    return false;
                }
                
                return true;
            }
            catch (HttpRequestException)
            {
                return false; // Handle API failures gracefully
            }
        }
    }
}
