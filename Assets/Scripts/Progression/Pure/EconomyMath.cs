using System;

public static class EconomyMath
{
    public static long NextPurchaseCost(int purchasedLevels)
    {
        if (purchasedLevels < 0)
        {
            purchasedLevels = 0;
        }

        if (purchasedLevels >= ProgressionConfig.TargetPurchases)
        {
            return 0;
        }

        return CeilPositive(ProgressionConfig.PriceBase * Math.Pow(ProgressionConfig.Growth, purchasedLevels));
    }

    public static long BaseFileReward(int purchasedLevels)
    {
        if (purchasedLevels < 0)
        {
            purchasedLevels = 0;
        }

        long value = CeilPositive(Math.Pow(ProgressionConfig.Growth, purchasedLevels));
        return value < 1 ? 1 : value;
    }

    public static long FileReward(int purchasedLevels, int compressionLevel, int optimizationLevel, bool valuable)
    {
        double reward = BaseFileReward(purchasedLevels);
        reward *= EffectMath.Compression(compressionLevel);
        reward *= EffectMath.Optimization(optimizationLevel);
        reward *= valuable ? 5d : 1d;
        if (double.IsNaN(reward) || double.IsInfinity(reward) || reward <= 0d)
        {
            return 0;
        }

        if (reward >= long.MaxValue)
        {
            return long.MaxValue;
        }

        return (long)Math.Round(reward, MidpointRounding.AwayFromZero);
    }

    public static long TotalTreeCost()
    {
        long total = 0;
        for (int p = 0; p < ProgressionConfig.TargetPurchases; p++)
        {
            long cost = NextPurchaseCost(p);
            if (total > long.MaxValue - cost)
            {
                return long.MaxValue;
            }

            total += cost;
        }

        return total;
    }

    public static long AddClamped(long current, long amount)
    {
        if (amount <= 0 || current < 0)
        {
            return current < 0 ? 0 : current;
        }

        if (current > long.MaxValue - amount)
        {
            return long.MaxValue;
        }

        return current + amount;
    }

    private static long CeilPositive(double raw)
    {
        if (double.IsNaN(raw) || raw <= 0d)
        {
            return 0;
        }

        if (double.IsInfinity(raw) || raw >= long.MaxValue)
        {
            return long.MaxValue;
        }

        double nudged = raw - 1e-6d;
        if (nudged < 0d)
        {
            nudged = 0d;
        }

        return (long)Math.Ceiling(nudged);
    }
}
