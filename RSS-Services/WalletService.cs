using Microsoft.EntityFrameworkCore;
using RSS_DB;
using RSS_DB.Entities;
using RSS_Services.Helpers;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RSS_Services
{
    // Player coin inventory. Coins have no monetary value and apply to public games
    // only — private games wager freely without touching the wallet. CoinBalance is
    // only ever changed via ExecuteUpdate arithmetic so concurrent requests cannot
    // clobber each other; every balance change is paired with a CoinTransactions
    // ledger row, which is the auditable source of truth.
    public class WalletService
    {
        public const decimal DailyGrantAmount = 15m;

        private readonly AppDbContext _appDbContext;
        private readonly TimeHelpers _timeHelpers;

        public WalletService(AppDbContext appDbContext, TimeHelpers timeHelpers)
        {
            _appDbContext = appDbContext;
            _timeHelpers = timeHelpers;
        }

        // Grants today's coins (PST day, active days only — missed days never accrue)
        // and returns the current balance. Safe to call on every request: the unique
        // (user, GrantDate) index makes the grant at-most-once even under concurrent
        // calls from multiple tabs/devices.
        public async Task<decimal> EnsureDailyGrantAsync(string userId)
        {
            var todayPst = _timeHelpers.GetTimeDateTimeTodayInPst();

            var alreadyGranted = await _appDbContext.CoinTransactions
                .AnyAsync(t => t.ApplicationUserId == userId && t.GrantDate == todayPst);

            if (!alreadyGranted)
            {
                await using var transaction = await _appDbContext.Database.BeginTransactionAsync();
                try
                {
                    _appDbContext.CoinTransactions.Add(new CoinTransaction
                    {
                        ApplicationUserId = userId,
                        Amount = DailyGrantAmount,
                        Type = CoinTransactionTypes.DailyGrant,
                        GrantDate = todayPst
                    });
                    await _appDbContext.SaveChangesAsync();

                    await _appDbContext.Users
                        .Where(u => u.Id == userId)
                        .ExecuteUpdateAsync(s => s.SetProperty(u => u.CoinBalance, u => u.CoinBalance + DailyGrantAmount));

                    await transaction.CommitAsync();
                }
                catch (DbUpdateException)
                {
                    // A concurrent request won the unique-index race and granted today already.
                    await transaction.RollbackAsync();
                }
            }

            return await GetBalanceAsync(userId);
        }

        public Task<decimal> GetBalanceAsync(string userId)
        {
            return _appDbContext.Users
                .Where(u => u.Id == userId)
                .Select(u => u.CoinBalance)
                .FirstAsync();
        }

        public async Task<bool> HasSufficientCoinsAsync(string userId, decimal amount)
        {
            if (amount <= 0) return true;
            return await GetBalanceAsync(userId) >= amount;
        }

        // Conditional atomic deduction + ledger row. The decrement only succeeds when
        // the balance covers the amount, so racing requests can't double-spend. The
        // ledger row is left for the caller's SaveChanges — callers MUST invoke this
        // inside an open transaction so the decrement rolls back with everything else.
        public async Task WagerAsync(string userId, Guid squareGameId, decimal amount)
        {
            if (amount <= 0) return;

            var affected = await _appDbContext.Users
                .Where(u => u.Id == userId && u.CoinBalance >= amount)
                .ExecuteUpdateAsync(s => s.SetProperty(u => u.CoinBalance, u => u.CoinBalance - amount));

            if (affected == 0)
                throw new InvalidOperationException("Not enough coins for this wager.");

            _appDbContext.CoinTransactions.Add(new CoinTransaction
            {
                ApplicationUserId = userId,
                SquareGameId = squareGameId,
                Amount = -amount,
                Type = CoinTransactionTypes.Wager
            });
        }

        // Returns each player's net game spend to their wallet — used when an open
        // public game is deleted. Netting against the ledger (rather than counting
        // squares) stays exact for partial selections and makes a repeated call a
        // no-op, since issued refunds bring each net back to zero. Ledger rows are
        // left for the caller's SaveChanges inside its transaction.
        public async Task RefundGameWagersAsync(Guid squareGameId)
        {
            var netByUser = await _appDbContext.CoinTransactions
                .Where(t => t.SquareGameId == squareGameId)
                .GroupBy(t => t.ApplicationUserId)
                .Select(g => new { UserId = g.Key, Net = g.Sum(t => t.Amount) })
                .Where(g => g.Net < 0)
                .ToListAsync();

            foreach (var entry in netByUser)
            {
                var refund = -entry.Net;
                _appDbContext.CoinTransactions.Add(new CoinTransaction
                {
                    ApplicationUserId = entry.UserId,
                    SquareGameId = squareGameId,
                    Amount = refund,
                    Type = CoinTransactionTypes.Refund
                });

                await _appDbContext.Users
                    .Where(u => u.Id == entry.UserId)
                    .ExecuteUpdateAsync(s => s.SetProperty(u => u.CoinBalance, u => u.CoinBalance + refund));
            }
        }
    }
}
