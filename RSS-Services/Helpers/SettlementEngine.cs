using RSS_DB.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RSS_Services.Helpers
{
    // One credit owed to a player at settlement. Period is set for PeriodWin lines,
    // null for redistribution/refund-style lines.
    public record SettlementLine(string UserId, decimal Amount, string Type, int? Period);

    // End-of-game payout math for every payout mode. Pure — no DB, no clock — so
    // each mode's rules live in one testable place. Invariant across all modes
    // ("pool conservation"): the returned lines always sum to exactly the pool
    // (pricePerSquare × claimed squares); coins never leak and are never minted.
    // The zero-winner game therefore needs no special case — it falls out as a
    // full pro-rata refund.
    public static class SettlementEngine
    {
        public static List<SettlementLine> ComputeSettlement(
            string payoutMode,
            IReadOnlyDictionary<int, string?> periodWinners,
            int periodCount,
            decimal pricePerSquare,
            IReadOnlyDictionary<string, int> squareCountsByUser)
        {
            var lines = payoutMode switch
            {
                PayoutModes.Default => ComputeDefault(periodWinners, periodCount, pricePerSquare, squareCountsByUser),
                PayoutModes.Fair => ComputeFair(periodWinners, periodCount, pricePerSquare, squareCountsByUser),
                PayoutModes.Push => ComputePush(periodWinners, periodCount, pricePerSquare, squareCountsByUser),
                _ => throw new NotSupportedException($"Payout mode '{payoutMode}' has no settlement implementation."),
            };

            ApplyRoundingResidual(lines, GetPool(pricePerSquare, squareCountsByUser));
            return lines;
        }

        public static decimal GetPool(decimal pricePerSquare, IReadOnlyDictionary<string, int> squareCountsByUser)
            => pricePerSquare * squareCountsByUser.Values.Sum();

        // Dashboard convenience: one player's prize money (period wins + pushes).
        // Pro-rata refunds are returned stakes, not winnings, so Redistribution
        // lines are excluded. Only meaningful for completed games.
        public static decimal GetUserPrizeTotal(
            string payoutMode,
            IReadOnlyDictionary<int, string?> periodWinners,
            int periodCount,
            decimal pricePerSquare,
            IEnumerable<string> claimedSquareOwners,
            string userId)
        {
            var squareCounts = claimedSquareOwners
                .GroupBy(o => o)
                .ToDictionary(g => g.Key, g => g.Count());

            return ComputeSettlement(payoutMode, periodWinners, periodCount, pricePerSquare, squareCounts)
                .Where(l => l.UserId == userId && l.Type != CoinTransactionTypes.Redistribution)
                .Sum(l => l.Amount);
        }

        // Default: each claimed period pays pool ÷ periodCount to its winner; the
        // unclaimed fraction of the pool is returned to ALL players pro-rata to
        // what they wagered (winners included — they funded the dead periods too).
        private static List<SettlementLine> ComputeDefault(
            IReadOnlyDictionary<int, string?> periodWinners,
            int periodCount,
            decimal pricePerSquare,
            IReadOnlyDictionary<string, int> squareCountsByUser)
        {
            var lines = new List<SettlementLine>();
            var pool = GetPool(pricePerSquare, squareCountsByUser);
            if (pool <= 0 || periodCount <= 0) return lines;

            var perPeriod = pool / periodCount;
            var unclaimedPeriods = 0;

            for (int period = 1; period <= periodCount; period++)
            {
                var winnerId = periodWinners.GetValueOrDefault(period);
                if (winnerId != null)
                    lines.Add(new SettlementLine(winnerId, Round2(perPeriod), CoinTransactionTypes.PeriodWin, period));
                else
                    unclaimedPeriods++;
            }

            if (unclaimedPeriods > 0)
                AddProRataRefund(lines, pricePerSquare, squareCountsByUser, (decimal)unclaimedPeriods / periodCount);

            return lines;
        }

        // Fair: the pool splits across the periods that were actually won, so every
        // unclaimed period raises the payout of every winning period. A player who
        // won two periods holds two shares. Zero winners → full pro-rata refund.
        private static List<SettlementLine> ComputeFair(
            IReadOnlyDictionary<int, string?> periodWinners,
            int periodCount,
            decimal pricePerSquare,
            IReadOnlyDictionary<string, int> squareCountsByUser)
        {
            var lines = new List<SettlementLine>();
            var pool = GetPool(pricePerSquare, squareCountsByUser);
            if (pool <= 0 || periodCount <= 0) return lines;

            var claimedPeriods = new List<(int Period, string WinnerId)>();
            for (int period = 1; period <= periodCount; period++)
            {
                var winnerId = periodWinners.GetValueOrDefault(period);
                if (winnerId != null) claimedPeriods.Add((period, winnerId));
            }

            if (claimedPeriods.Count == 0)
            {
                AddProRataRefund(lines, pricePerSquare, squareCountsByUser, 1m);
                return lines;
            }

            var perWinningPeriod = pool / claimedPeriods.Count;
            foreach (var (period, winnerId) in claimedPeriods)
                lines.Add(new SettlementLine(winnerId, Round2(perWinningPeriod), CoinTransactionTypes.PeriodWin, period));

            return lines;
        }

        // Push: every period pays the even share, but an unclaimed period's share
        // carries forward and lands on top of the next winner's payout (stacking
        // across consecutive unclaimed periods). A carry still riding after the
        // final period goes to the earliest winner. Zero winners → full refund.
        private static List<SettlementLine> ComputePush(
            IReadOnlyDictionary<int, string?> periodWinners,
            int periodCount,
            decimal pricePerSquare,
            IReadOnlyDictionary<string, int> squareCountsByUser)
        {
            var lines = new List<SettlementLine>();
            var pool = GetPool(pricePerSquare, squareCountsByUser);
            if (pool <= 0 || periodCount <= 0) return lines;

            string? earliestWinner = null;
            for (int period = 1; period <= periodCount && earliestWinner == null; period++)
                earliestWinner = periodWinners.GetValueOrDefault(period);

            if (earliestWinner == null)
            {
                AddProRataRefund(lines, pricePerSquare, squareCountsByUser, 1m);
                return lines;
            }

            var perPeriod = pool / periodCount;
            var carry = 0m;
            for (int period = 1; period <= periodCount; period++)
            {
                var winnerId = periodWinners.GetValueOrDefault(period);
                if (winnerId == null)
                {
                    carry += perPeriod;
                    continue;
                }

                lines.Add(new SettlementLine(winnerId, Round2(perPeriod), CoinTransactionTypes.PeriodWin, period));
                if (carry > 0)
                    lines.Add(new SettlementLine(winnerId, Round2(carry), CoinTransactionTypes.Push, period));
                carry = 0m;
            }

            // Unclaimed final period(s): the riding pot rewards whoever won first.
            if (carry > 0)
                lines.Add(new SettlementLine(earliestWinner, Round2(carry), CoinTransactionTypes.Push, null));

            return lines;
        }

        // The zero-winner game in every mode, and Default's consolation, are the
        // same operation: return `fraction` of each player's wager (winners
        // included — they funded the unclaimed periods too).
        private static void AddProRataRefund(
            List<SettlementLine> lines,
            decimal pricePerSquare,
            IReadOnlyDictionary<string, int> squareCountsByUser,
            decimal fraction)
        {
            foreach (var (userId, squares) in squareCountsByUser)
            {
                if (squares <= 0) continue;
                var refund = Round2(pricePerSquare * squares * fraction);
                if (refund > 0)
                    lines.Add(new SettlementLine(userId, refund, CoinTransactionTypes.Redistribution, null));
            }
        }

        private static decimal Round2(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);

        // Rounding each line to 2dp can leave a few hundredths of the pool
        // unallocated (or over-allocated). Pin the difference on the largest
        // line — it can absorb it without going negative — so the pool-conservation
        // invariant holds to the cent.
        private static void ApplyRoundingResidual(List<SettlementLine> lines, decimal pool)
        {
            if (lines.Count == 0) return;

            var residual = pool - lines.Sum(l => l.Amount);
            if (residual == 0) return;

            var largestIndex = 0;
            for (int i = 1; i < lines.Count; i++)
                if (lines[i].Amount > lines[largestIndex].Amount) largestIndex = i;

            lines[largestIndex] = lines[largestIndex] with { Amount = lines[largestIndex].Amount + residual };
        }
    }
}
