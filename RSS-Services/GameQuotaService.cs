using Microsoft.EntityFrameworkCore;
using RSS_DB;
using RSS_DB.Entities;
using RSS_Services.Helpers;
using System;
using System.Threading.Tasks;

namespace RSS_Services
{
    // Daily per-user caps on game starts/joins (Admins exempt) — a resource-exhaustion
    // guard, not a security control, so a race that occasionally lets someone sneak in
    // one extra is an accepted low-stakes edge case. Day boundary matches the rest of
    // the app's "daily" semantics (PST), see TimeHelpers/CoinWalletService.
    public class GameQuotaService
    {
        public const int DailyStartLimit = 2;
        public const int DailyJoinLimit = 5;

        private readonly AppDbContext _appDbContext;
        private readonly TimeHelpers _timeHelpers;

        public GameQuotaService(AppDbContext appDbContext, TimeHelpers timeHelpers)
        {
            _appDbContext = appDbContext;
            _timeHelpers = timeHelpers;
        }

        public async Task<int> GetCountTodayAsync(string userId, string type)
        {
            var todayPst = _timeHelpers.GetTimeDateTimeTodayInPst();
            return await _appDbContext.GameActivityLogs
                .CountAsync(l => l.ApplicationUserId == userId && l.Type == type && l.ActivityDatePst == todayPst);
        }

        // Adds the tracked row only — caller SaveChanges as part of its own unit of work.
        public void LogActivity(string userId, Guid gameId, string type)
        {
            var todayPst = _timeHelpers.GetTimeDateTimeTodayInPst();
            _appDbContext.GameActivityLogs.Add(new GameActivityLog
            {
                ApplicationUserId = userId,
                GameId = gameId,
                Type = type,
                ActivityDatePst = todayPst
            });
        }
    }
}
