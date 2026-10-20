using System;

public static class EffectMath
{
    public static int ClampLevel(int level, int max)
    {
        if (level < 0)
        {
            return 0;
        }

        return level > max ? max : level;
    }

    public static double ManualSpeed(int level) => 1d + 0.25d * ClampLevel(level, 8);

    public static int DownloadSlots(int level) => 1 + ClampLevel(level, 3);

    public static double BufferSeconds(int level) => 0.25d * ClampLevel(level, 4);

    public static double BurstMultiplier(int level)
    {
        return ClampLevel(level, 3) switch
        {
            1 => 1.4d,
            2 => 1.7d,
            3 => 2.0d,
            _ => 1d
        };
    }

    public static double Compression(int level) => Math.Pow(1.14d, ClampLevel(level, 8));

    public static double Optimization(int level) => Math.Pow(1.10d, ClampLevel(level, 5));

    public static double ValuableChance(int level) => 0.03d * ClampLevel(level, 4);

    public static int MiniCount(int unlockLevel, int populationLevel)
    {
        return unlockLevel > 0 ? 1 + ClampLevel(populationLevel, 5) : 0;
    }

    public static double MiniMoveSpeed(int level)
    {
        return ProgressionConfig.MiniBaseMoveSpeed * (1d + 0.30d * ClampLevel(level, 6));
    }

    public static double MiniFeedMultiplier(int level) => 1d + 0.30d * ClampLevel(level, 6);

    public static double ChainChance(int unlockLevel, int probabilityLevel)
    {
        if (unlockLevel <= 0)
        {
            return 0d;
        }

        return 0.10d + 0.08d * ClampLevel(probabilityLevel, 5);
    }

    public static double ChainRadius(int rangeLevel)
    {
        return ProgressionConfig.ChainBaseRadius + ProgressionConfig.ChainRadiusPerLevel * ClampLevel(rangeLevel, 4);
    }

    public static int ChainDepth(int depthLevel) => 1 + ClampLevel(depthLevel, 4);

    public static int ChainBranching(int branchingLevel) => 1 + ClampLevel(branchingLevel, 4);

    public static double OverloadRequired(int chargeLevel) => 12d * Math.Pow(0.92d, ClampLevel(chargeLevel, 4));

    public static double OverloadDuration(int durationLevel) => 4d + ClampLevel(durationLevel, 3);

    public static double OverloadPower(int powerLevel) => 1.5d + 0.25d * ClampLevel(powerLevel, 4);

    public static double PassiveFractionPerSecond(int productionLevel) => 0.05d * (1d + 0.20d * ClampLevel(productionLevel, 5));

    public static int InfectedCapacity(int unlockLevel, int capacityLevel)
    {
        return unlockLevel > 0 ? 2 + 2 * ClampLevel(capacityLevel, 3) : 0;
    }

    public static double SpreadInterval(int spreadLevel)
    {
        return ClampLevel(spreadLevel, 3) switch
        {
            1 => 12d,
            2 => 9d,
            3 => 6d,
            _ => 0d
        };
    }

    public static double FragmentChance(int unlockLevel, int chanceLevel)
    {
        if (unlockLevel <= 0)
        {
            return 0d;
        }

        double chance = 0.10d + 0.10d * ClampLevel(chanceLevel, 3);
        return chance > 0.40d ? 0.40d : chance;
    }

    public static int FragmentCount(int countLevel) => 2 + ClampLevel(countLevel, 3);

    public static double FragmentFraction(int valueLevel) => 0.05d + 0.02d * ClampLevel(valueLevel, 3);

    public static double SynergySpeedMultiplier(int level) => 1d + 0.10d * ClampLevel(level, 4);

    public static double MiniChainChanceScale(int level) => 0.25d * ClampLevel(level, 4);

    public static double CascadeChargeBonus(int level) => 0.10d * ClampLevel(level, 3);
}
