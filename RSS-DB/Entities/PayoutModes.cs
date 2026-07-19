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

        // Modes with a working settlement implementation — game creation rejects the
        // rest. Grows as each mode ships (Destruction next, then Thief).
        public static readonly IReadOnlySet<string> Implemented = new HashSet<string> { Default, Fair, Push };
    }
}
