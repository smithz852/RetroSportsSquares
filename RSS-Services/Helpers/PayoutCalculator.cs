namespace RSS_Services.Helpers
{
    public static class PayoutCalculator
    {
        // Canonical payout formula: the total pool is split evenly across all periods.
        // Periods with no claimed winning square simply pay out nothing — their share
        // is not redistributed to other winners.
        public static decimal GetPayoutPerPeriod(decimal pricePerSquare, int claimedSquareCount, int periodCount)
        {
            if (periodCount <= 0) return 0;
            return (pricePerSquare * claimedSquareCount) / periodCount;
        }
    }
}
