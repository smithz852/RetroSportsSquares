using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RSS_DB;
using RSS_DB.Entities;
using RSS_Services.Helpers;
using ZlEmailProvider;

namespace RSS_Services
{
    // End-of-game recap emails — the single email a player receives for a finished
    // game. Every send is best-effort: failures are logged and swallowed so a Resend
    // outage or an undeliverable address can never break the score-refetch or
    // stale-close automation loops.
    public class GameNotificationService
    {
        private readonly AppDbContext _appDbContext;
        private readonly IEmailService _emailService;
        private readonly ResendOptions _resendOptions;
        private readonly ILogger<GameNotificationService> _logger;

        public GameNotificationService(AppDbContext appDbContext, IEmailService emailService, IOptions<ResendOptions> resendOptions, ILogger<GameNotificationService> logger)
        {
            _appDbContext = appDbContext;
            _emailService = emailService;
            _resendOptions = resendOptions.Value;
            _logger = logger;
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
                    game.SettlementMode, game.PeriodWinners, game.PeriodCount, game.PricePerSquare,
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

                    // CAN-SPAM opt-out — honored per recipient; the batch still runs
                    // for everyone else and RecapEmailSent above stays at-most-once.
                    if (player.User.RecapEmailsOptedOut) continue;

                    var squaresClaimed = game.GameSquares.Count(gs => (gs.OriginalGamePlayerId ?? gs.GamePlayerId) == player.Id);

                    // Lazily assigned, never expires — the opt-out link must keep
                    // working for at least 30 days (CAN-SPAM), so this isn't tied to
                    // Identity's short-lived DataProtectionTokenProvider.
                    if (player.User.UnsubscribeToken == null)
                    {
                        player.User.UnsubscribeToken = Guid.NewGuid();
                        await _appDbContext.SaveChangesAsync();
                    }
                    var unsubscribeUrl = $"{_resendOptions.AppBaseUrl}/email-preferences?u={player.User.Id}&t={player.User.UnsubscribeToken}";

                    var model = new RecapEmailModel
                    {
                        GameName = game.GameName,
                        MatchupLine = matchupLine,
                        PeriodRows = periodRows,
                        RecipientName = GetDisplayName(player.User),
                        CoinsWagered = game.PricePerSquare * squaresClaimed,
                        CoinsWon = settledByUser.GetValueOrDefault(player.ApplicationUserId, 0m),
                        UnsubscribeUrl = unsubscribeUrl
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
