using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using RSS_DB;
using RSS_DB.Entities;

namespace RSS_Services
{
    // CCPA right-to-delete. Open games the user hosts/joined are fully unwound and
    // refunded first — no one else has a stake in that history yet. Games already
    // started or completed can't be handled the same way: GameSquares/ChatMessage/
    // CoinTransaction are live state other players still see, and GamePlayerId is a
    // load-bearing FK (board rendering, Thief-mode steals, period-winner payouts all
    // join through it) rather than just a historical label, so removing those rows
    // would corrupt other players' games. Instead they're re-keyed onto a disposable
    // "tombstone" account that was never signed up or logged into, and only then is
    // the real account — and the direct identifiers that made it identifiable —
    // actually deleted. Same shape as Reddit/GitHub's "[deleted]"/ghost-user pattern.
    public class AccountDeletionService
    {
        private readonly AppDbContext _appDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AvailableGamesServices _availableGamesServices;
        private readonly GamePlayerServices _gamePlayerServices;
        private readonly IMemoryCache _cache;

        public AccountDeletionService(
            AppDbContext appDbContext,
            UserManager<ApplicationUser> userManager,
            AvailableGamesServices availableGamesServices,
            GamePlayerServices gamePlayerServices,
            IMemoryCache cache)
        {
            _appDbContext = appDbContext;
            _userManager = userManager;
            _availableGamesServices = availableGamesServices;
            _gamePlayerServices = gamePlayerServices;
            _cache = cache;
        }

        public async Task DeleteAccountAsync(ApplicationUser user)
        {
            var userId = user.Id;

            // Step 1: unwind every open (not-started) game via the same paths a normal
            // host-delete/player-leave would take, before touching anything else.
            var openMemberships = await _appDbContext.GamePlayers
                .Include(gp => gp.Game)
                .Where(gp => gp.ApplicationUserId == userId && gp.Game.isOpen)
                .ToListAsync();

            foreach (var membership in openMemberships)
            {
                if (membership.IsHost)
                    await _availableGamesServices.DeleteGame(membership.GameId.ToString());
                else
                    await _gamePlayerServices.LeaveGame(userId, membership.GameId.ToString());
            }

            // Step 2: re-key whatever's left (started/completed games) onto a fresh
            // tombstone, then hard-delete the real account.
            await using var transaction = await _appDbContext.Database.BeginTransactionAsync();
            try
            {
                var tombstone = new ApplicationUser
                {
                    DisplayName = "Deleted User",
                    GamerTag = null,
                    EmailConfirmed = false,
                    CoinBalance = 0,
                    HasSeenWelcome = true,
                    IsDeleted = true,
                    DeletedAt = DateTimeOffset.UtcNow,
                };
                tombstone.Email = $"deleted-{tombstone.Id}@deleted.internal";
                tombstone.UserName = tombstone.Email;

                // No password set — CreateAsync(user) with no password param leaves
                // PasswordHash null, so this account can never pass a login check.
                var createResult = await _userManager.CreateAsync(tombstone);
                if (!createResult.Succeeded)
                    throw new InvalidOperationException("Failed to create tombstone account: " +
                        string.Join("; ", createResult.Errors.Select(e => e.Description)));

                await _appDbContext.GamePlayers
                    .Where(gp => gp.ApplicationUserId == userId)
                    .ExecuteUpdateAsync(s => s.SetProperty(gp => gp.ApplicationUserId, tombstone.Id));

                await _appDbContext.ChatMessages
                    .Where(m => m.ApplicationUserId == userId)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(m => m.ApplicationUserId, tombstone.Id)
                        .SetProperty(m => m.Message, "[deleted]")
                        .SetProperty(m => m.IsDeleted, true));

                await _appDbContext.CoinTransactions
                    .Where(t => t.ApplicationUserId == userId)
                    .ExecuteUpdateAsync(s => s.SetProperty(t => t.ApplicationUserId, tombstone.Id));

                // Invalidate any outstanding JWT immediately, before the row disappears.
                await _userManager.UpdateSecurityStampAsync(user);
                _cache.Remove($"stamp_{userId}");

                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Count > 0)
                    await _userManager.RemoveFromRolesAsync(user, roles);

                var deleteResult = await _userManager.DeleteAsync(user);
                if (!deleteResult.Succeeded)
                    throw new InvalidOperationException("Failed to delete account: " +
                        string.Join("; ", deleteResult.Errors.Select(e => e.Description)));

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
