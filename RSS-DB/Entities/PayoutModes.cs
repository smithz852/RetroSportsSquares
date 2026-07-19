using System.Collections.Generic;

namespace RSS_DB.Entities
{
    // Payout mode names stored on SquareGames.PayoutMode. All modes split the pool
    // evenly per period; they differ in what an unclaimed ("null-square") period does.
    public static class PayoutModes
    {
        public const string Default = "Default";
        public const string Fair = "Fair";
        public const string Push = "Push";
        public const string Thief = "Thief";
        public const string Destruction = "Destruction";

        public static readonly IReadOnlyList<string> All = new[] { Default, Fair, Push, Thief, Destruction };

        // Modes with a working settlement implementation — game creation rejects the rest.
        public static readonly IReadOnlySet<string> Implemented = new HashSet<string> { Default, Fair, Push, Thief, Destruction };

        // Thief and Destruction's signature mechanics need winner → null → winner,
        // which is unreachable in 2-period games. Keyed on period count (not sport)
        // so a future 3-period sport qualifies automatically.
        public static int MinimumPeriods(string mode)
            => mode == Thief || mode == Destruction ? 3 : 1;
    }
}
