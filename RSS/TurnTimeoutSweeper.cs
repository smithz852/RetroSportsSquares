using RSS_Services;

namespace RSS
{
    // Server-side backstop for turn timers: the client countdown is only a courtesy, so this
    // skips players who never submitted (closed tab, modified client). State lives in the DB
    // (TurnStartedAt), so restarts lose nothing, and the locked conditional AdvanceTurn makes
    // it safe alongside submits, host skips, and multiple API instances.
    public class TurnTimeoutSweeper : BackgroundService
    {
        private static readonly TimeSpan Interval = TimeSpan.FromSeconds(5);

        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TurnTimeoutSweeper> _logger;

        public TurnTimeoutSweeper(IServiceProvider serviceProvider, ILogger<TurnTimeoutSweeper> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var gamePlayerServices = scope.ServiceProvider.GetRequiredService<GamePlayerServices>();
                    var advanced = await gamePlayerServices.AdvanceExpiredTurnsAsync(DateTimeOffset.UtcNow);
                    if (advanced > 0)
                        _logger.LogInformation("Turn sweeper skipped {Count} timed-out turn(s)", advanced);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    // One bad pass must not kill the loop
                    _logger.LogError(ex, "Turn timeout sweep failed");
                }

                try { await Task.Delay(Interval, stoppingToken); }
                catch (OperationCanceledException) { break; }
            }
        }
    }
}
