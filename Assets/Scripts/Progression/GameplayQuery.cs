using UnityEngine;

public static class GameplayQuery
{
    public static int Level(string id) => GameProgress.GetSkillLevel(id);

    public static float ManualPrimaryRate()
    {
        float rate = (float)EffectMath.ManualSpeed(Level(SkillBalanceTable.A1)) / ProgressionConfig.BaseDownloadSeconds;
        RunDirector run = RunDirector.Instance;
        if (run != null)
        {
            rate *= run.BurstMultiplier;
            rate *= run.OverloadMultiplier;
        }

        return Mathf.Max(0.01f, rate);
    }

    public static int DownloadSlots() => EffectMath.DownloadSlots(Level(SkillBalanceTable.A2));

    public static float BufferSeconds() => (float)EffectMath.BufferSeconds(Level(SkillBalanceTable.A3));

    public static float MiniMoveSpeed()
    {
        float speed = (float)EffectMath.MiniMoveSpeed(Level(SkillBalanceTable.C3));
        RunDirector run = RunDirector.Instance;
        if (run != null)
        {
            speed *= run.SynergyMultiplier;
        }

        return speed;
    }

    public static float MiniDownloadRate()
    {
        float rate = (float)EffectMath.MiniFeedMultiplier(Level(SkillBalanceTable.C4)) / ProgressionConfig.BaseDownloadSeconds;
        RunDirector run = RunDirector.Instance;
        if (run != null)
        {
            rate *= run.OverloadMultiplier;
            rate *= run.SynergyMultiplier;
        }

        return Mathf.Max(0.01f, rate);
    }

    public static float ChainDownloadRate()
    {
        float rate = ProgressionConfig.ChainSpeedFactor / ProgressionConfig.BaseDownloadSeconds;
        RunDirector run = RunDirector.Instance;
        if (run != null)
        {
            rate *= run.OverloadMultiplier;
        }

        return rate;
    }

    public static int MiniCount() => EffectMath.MiniCount(Level(SkillBalanceTable.C1), Level(SkillBalanceTable.C2));

    public static int Targeting() => Level(SkillBalanceTable.C5);

    public static long StandardReward()
    {
        return EconomyMath.FileReward(
            GameProgress.TotalPurchases,
            Level(SkillBalanceTable.B1),
            Level(SkillBalanceTable.B2),
            false);
    }
}
