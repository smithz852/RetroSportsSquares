using RSS_DB.Entities;
using RSS_Services.Helpers;

namespace RSS_Tests
{
    public class SettlementEngineTests
    {
        private const string A = "user-a";
        private const string B = "user-b";
        private const string C = "user-c";

        private static Dictionary<int, string?> Periods(params string?[] winners)
        {
            var dict = new Dictionary<int, string?>();
            for (int i = 0; i < winners.Length; i++)
                dict[i + 1] = winners[i];
            return dict;
        }

        private static decimal Total(IEnumerable<SettlementLine> lines, string userId)
            => lines.Where(l => l.UserId == userId).Sum(l => l.Amount);

        [Fact]
        public void Default_AllPeriodsClaimed_PaysPerPeriodWithNoRedistribution()
        {
            var squares = new Dictionary<string, int> { [A] = 5, [B] = 5 };
            // pool = 2 * 10 = 20, per period = 5
            var lines = SettlementEngine.ComputeSettlement(
                PayoutModes.Default, Periods(A, B, A, B), 4, 2m, squares);

            Assert.Equal(4, lines.Count);
            Assert.All(lines, l => Assert.Equal(CoinTransactionTypes.PeriodWin, l.Type));
            Assert.Equal(10m, Total(lines, A));
            Assert.Equal(10m, Total(lines, B));
        }

        [Fact]
        public void Default_OneNullPeriod_RefundsProRataIncludingWinners()
        {
            var squares = new Dictionary<string, int> { [A] = 6, [B] = 2 };
            // pool = 8 * 1 = 8, per period = 2, one null period -> 2 coins refunded pro-rata
            var lines = SettlementEngine.ComputeSettlement(
                PayoutModes.Default, Periods(A, B, null, A), 4, 1m, squares);

            // A: two period wins (4) + refund 0.25 * 6 = 1.50
            Assert.Equal(5.50m, Total(lines, A));
            // B: one period win (2) + refund 0.25 * 2 = 0.50
            Assert.Equal(2.50m, Total(lines, B));
            Assert.Contains(lines, l => l.UserId == A && l.Type == CoinTransactionTypes.Redistribution);
            Assert.Contains(lines, l => l.UserId == B && l.Type == CoinTransactionTypes.Redistribution);
        }

        [Fact]
        public void Default_AllPeriodsNull_RefundsExactWagers()
        {
            var squares = new Dictionary<string, int> { [A] = 7, [B] = 1, [C] = 2 };
            var lines = SettlementEngine.ComputeSettlement(
                PayoutModes.Default, Periods(null, null, null, null), 4, 3m, squares);

            Assert.DoesNotContain(lines, l => l.Type == CoinTransactionTypes.PeriodWin);
            Assert.Equal(21m, Total(lines, A));
            Assert.Equal(3m, Total(lines, B));
            Assert.Equal(6m, Total(lines, C));
        }

        [Fact]
        public void Default_PeriodWinLinesCarryTheirPeriod()
        {
            var squares = new Dictionary<string, int> { [A] = 4 };
            var lines = SettlementEngine.ComputeSettlement(
                PayoutModes.Default, Periods(A, null, A, null), 4, 1m, squares);

            var winPeriods = lines.Where(l => l.Type == CoinTransactionTypes.PeriodWin)
                .Select(l => l.Period).ToList();
            Assert.Equal(new int?[] { 1, 3 }, winPeriods);
            Assert.All(lines.Where(l => l.Type == CoinTransactionTypes.Redistribution),
                l => Assert.Null(l.Period));
        }

        [Fact]
        public void Default_UnevenDivision_StillSumsExactlyToPool()
        {
            // pool = 10, 3 periods -> per period 3.333...
            var squares = new Dictionary<string, int> { [A] = 6, [B] = 4 };
            var lines = SettlementEngine.ComputeSettlement(
                PayoutModes.Default, Periods(A, B, null), 3, 1m, squares);

            Assert.Equal(10m, lines.Sum(l => l.Amount));
            Assert.All(lines, l => Assert.True(l.Amount > 0));
        }

        [Fact]
        public void Default_PlayerWithNoSquares_GetsNoRefund()
        {
            var squares = new Dictionary<string, int> { [A] = 4, [B] = 0 };
            var lines = SettlementEngine.ComputeSettlement(
                PayoutModes.Default, Periods(null, null), 2, 1m, squares);

            Assert.DoesNotContain(lines, l => l.UserId == B);
        }

        [Fact]
        public void ZeroPricePool_ProducesNoLines()
        {
            var squares = new Dictionary<string, int> { [A] = 5 };
            var lines = SettlementEngine.ComputeSettlement(
                PayoutModes.Default, Periods(A, null), 2, 0m, squares);

            Assert.Empty(lines);
        }

        [Fact]
        public void UnimplementedMode_Throws()
        {
            var squares = new Dictionary<string, int> { [A] = 1 };
            Assert.Throws<NotSupportedException>(() => SettlementEngine.ComputeSettlement(
                PayoutModes.Fair, Periods(A), 1, 1m, squares));
        }

        // Pool conservation is the core invariant of every mode: whatever the
        // period sequence, the settlement lines redistribute exactly the pool.
        [Fact]
        public void Default_RandomizedGames_AlwaysConserveThePool()
        {
            var rng = new Random(20260718);
            var users = new[] { A, B, C, "user-d", "user-e" };

            for (int i = 0; i < 500; i++)
            {
                var periodCount = rng.Next(1, 7);
                var price = Math.Round((decimal)rng.NextDouble() * 10m + 0.25m, 2);
                var squares = users
                    .Take(rng.Next(2, users.Length + 1))
                    .ToDictionary(u => u, _ => rng.Next(0, 12));
                if (squares.Values.Sum() == 0) squares[A] = 1;

                var winners = new string?[periodCount];
                var playersWithSquares = squares.Where(kv => kv.Value > 0).Select(kv => kv.Key).ToArray();
                for (int p = 0; p < periodCount; p++)
                    winners[p] = rng.Next(3) == 0 ? null : playersWithSquares[rng.Next(playersWithSquares.Length)];

                var lines = SettlementEngine.ComputeSettlement(
                    PayoutModes.Default, Periods(winners), periodCount, price, squares);

                var pool = SettlementEngine.GetPool(price, squares);
                Assert.Equal(pool, lines.Sum(l => l.Amount));
                Assert.All(lines, l => Assert.True(l.Amount > 0, $"non-positive line in iteration {i}"));
            }
        }
    }
}
