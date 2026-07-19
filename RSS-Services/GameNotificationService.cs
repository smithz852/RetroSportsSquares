using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RSS_DB;
using RSS_DB.Entities;
using RSS_Services.Helpers;
using ZlEmailProvider;

namespace RSS_Services
{
    // Game-event emails (period wins, end-of-game recaps). Every send is best-effort:
    // failures are logged and swallowed so a Resend outage or an undeliverable address
    // can never break the score-refetch or stale-close automation loops.
    public class GameNotificationService
    {
        private readonly AppDbContext _appDbContext;
        private readonly IEmailService _emailService;
        private readonly ILogger<GameNotificationService> _logger;

        public GameNotificationService(AppDbContext appDbContext, IEmailService emailService, ILogger<GameNotificationService> logger)
        {
            _appDbContext = appDbContext;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task SendPeriodWinEmailAsync(Guid squareGameId, int period, string winnerApplicationUserId)
        {
            try
            {
                var game = await _appDbContext.SquareGames
                    .Include(g => g.DailySportGame)
                    .FirstOrDefaultAsync(g => g.Id == squareGameId);
                if (game == null)
                {
                    _logger.LogWarning("Period-win email skipped: game {GameId} not found", squareGameId);
                    return;
                }

                var user = await _appDbContext.Users.FindAsync(winnerApplicationUserId);
                if (string.IsNullOrEmpty(user?.Email))
                {
                    _logger.LogWarning("Period-win email skipped: user {UserId} has no email (game {GameId})", winnerApplicationUserId, squareGameId);
                    return;
                }

                // Only Default's per-period amount is final mid-game; other modes
                // announce the win and leave exact coins to the settlement recap.
                decimal? payout = null;
                if (game.PayoutMode == PayoutModes.Default)
                {
                    var claimedSquares = await _appDbContext.GameSquares
                        .CountAsync(gs => gs.SquareGamesId == squareGameId && gs.GamePlayerId != null);
                    payout = PayoutCalculator.GetPayoutPerPeriod(game.PricePerSquare, claimedSquares, game.PeriodCount);
                }
                var periodLabel = GameEmailTemplates.GetPeriodLabel(period, game.PeriodCount);

                var (subject, text, html) = GameEmailTemplates.BuildPeriodWin(game.GameName, periodLabel, payout);
                await _emailService.SendAsync(user.Email, subject, text, html);
                _logger.LogInformation("Period-win email sent to {Email} for game {GameId}, period {Period}", user.Email, squareGameId, period);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send period-win email for game {GameId}, period {Period}", squareGameId, period);
            }
        }

        public async Task SendGameRecapEmailsAsync(Guid squareGameId)
        {
            SquareGames? game;
            try
            {
                game = await _appDbContext.SquareGames
                    .Include(g => g.DailySportGame)
                    .Include(g => g.GamePlayers).ThenInclude(gp => gp.User)
                    .Include(g => g.GameSquares)
                    .FirstOrDefaultAsync(g => g.Id == squareGameId);
                if (game == null || !game.IsCompleted || game.RecapEmailSent) return;

                // Flag-first: at-most-once semantics. A crash after this save loses the
                // recap, which is preferable to re-mailing every player on retry.
                game.RecapEmailSent = true;
                await _appDbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to prepare recap emails for game {GameId}", squareGameId);
                return;
            }

            try
            {
                // Recap amounts come from the settlement engine (same pure math that
                // credits wallets), so they're correct per mode — Fair's boosted
                // periods, Push carries, Default's pro-rata refund all included.
                var settlementLines = SettlementEngine.ComputeSettlement(
                    game.PayoutMode, game.PeriodWinners, game.PeriodCount, game.PricePerSquare,
                    GameSettlementService.GetSquareCountsByUser(game));
                var settledByUser = settlementLines
                    .GroupBy(l => l.UserId)
                    .ToDictionary(g => g.Key, g => g.Sum(l => l.Amount));
                var payoutByPeriod = settlementLines
                    .Where(l => l.Period != null)
                    .GroupBy(l => l.Period!.Value)
                    .ToDictionary(g => g.Key, g => g.Sum(l => l.Amount));

                var namesByUserId = game.GamePlayers
                    .Where(gp => gp.User != null)
                    .ToDictionary(gp => gp.ApplicationUserId, gp => GetDisplayName(gp.User));

                var periodRows = new List<RecapPeriodRow>();
                for (int period = 1; period <= game.PeriodCount; period++)
                {
                    var winnerId = game.PeriodWinners.GetValueOrDefault(period);
                    var hasWinner = winnerId != null;
                    periodRows.Add(new RecapPeriodRow
                    {
                        PeriodLabel = GameEmailTemplates.GetPeriodLabel(period, game.PeriodCount),
                        WinnerName = hasWinner ? namesByUserId.GetValueOrDefault(winnerId!, "Unknown player") : "No winner",
                        Payout = hasWinner ? payoutByPeriod.GetValueOrDefault(period, 0m) : 0
                    });
                }

                var sport = game.DailySportGame;
                var matchupLine = $"{sport.AwayTeam} @ {sport.HomeTeam} — {sport.CurrentAwayScore}–{sport.CurrentHomeScore}";

                foreach (var player in game.GamePlayers)
                {
                    if (string.IsNullOrEmpty(player.User?.Email))
                    {
                        _logger.LogWarning("Recap email skipped for player {GamePlayerId}: no email (game {GameId})", player.Id, squareGameId);
                        continue;
                    }

                    var squaresClaimed = game.GameSquares.Count(gs => (gs.OriginalGamePlayerId ?? gs.GamePlayerId) == player.Id);

                    var model = new RecapEmailModel
                    {
                        GameName = game.GameName,
                        MatchupLine = matchupLine,
                        PeriodRows = periodRows,
                        RecipientName = GetDisplayName(player.User),
                        CoinsWagered = game.PricePerSquare * squaresClaimed,
                        CoinsWon = settledByUser.GetValueOrDefault(player.ApplicationUserId, 0m)
                    };

                    try
                    {
                        var (subject, text, html) = GameEmailTemplates.BuildRecap(model);
                        await _emailService.SendAsync(player.User.Email, subject, text, html);
                        _logger.LogInformation("Recap email sent to {Email} for game {GameId}", player.User.Email, squareGameId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to send recap email to {Email} for game {GameId}", player.User.Email, squareGameId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send recap emails for game {GameId}", squareGameId);
            }
        }

        private static string GetDisplayName(ApplicationUser user)
            => user.DisplayName ?? user.GamerTag ?? user.Email ?? "A player";
    }
}
