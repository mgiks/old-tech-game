using System.Text;

public static class EffectText
{
    public static string Describe(SkillEffectId effect, int level)
    {
        level = level < 0 ? 0 : level;
        return effect switch
        {
            SkillEffectId.DownloadSpeed => Loc.F("effect.speed", EffectMath.ManualSpeed(level)),
            SkillEffectId.Multithreading => Loc.F("effect.slots", EffectMath.DownloadSlots(level)),
            SkillEffectId.DownloadBuffer => Loc.F("effect.buffer", EffectMath.BufferSeconds(level)),
            SkillEffectId.DownloadBurst => level <= 0 ? Loc.T("effect.burst_off") : Loc.F("effect.burst", EffectMath.BurstMultiplier(level)),
            SkillEffectId.Compression => Loc.F("effect.compression", EffectMath.Compression(level)),
            SkillEffectId.Optimization => Loc.F("effect.optimization", EffectMath.Optimization(level)),
            SkillEffectId.ValuableFiles => Loc.F("effect.valuable", EffectMath.ValuableChance(level) * 100d),
            SkillEffectId.MiniUnlock => Loc.F("effect.minis", EffectMath.MiniCount(level, 0)),
            SkillEffectId.Population => Loc.F("effect.minis", EffectMath.MiniCount(1, level)),
            SkillEffectId.MoveSpeed => Loc.F("effect.move", EffectMath.MiniMoveSpeed(level)),
            SkillEffectId.FeedSpeed => Loc.F("effect.feed", EffectMath.MiniFeedMultiplier(level)),
            SkillEffectId.Targeting => Loc.F("effect.target", level),
            SkillEffectId.ChainUnlock => Loc.F("effect.chain_chance", EffectMath.ChainChance(level, 0) * 100d),
            SkillEffectId.ChainChance => Loc.F("effect.chain_chance", EffectMath.ChainChance(1, level) * 100d),
            SkillEffectId.ChainRange => Loc.F("effect.chain_radius", EffectMath.ChainRadius(level)),
            SkillEffectId.ChainDepth => Loc.F("effect.chain_depth", EffectMath.ChainDepth(level)),
            SkillEffectId.ChainBranch => Loc.F("effect.chain_branch", EffectMath.ChainBranching(level)),
            SkillEffectId.OverloadUnlock => Loc.F("effect.overload_need", EffectMath.OverloadRequired(0)),
            SkillEffectId.OverloadCharge => Loc.F("effect.overload_need", EffectMath.OverloadRequired(level)),
            SkillEffectId.OverloadDuration => Loc.F("effect.overload_time", EffectMath.OverloadDuration(level)),
            SkillEffectId.OverloadPower => Loc.F("effect.overload_power", EffectMath.OverloadPower(level)),
            SkillEffectId.InfectUnlock => Loc.F("effect.capacity", EffectMath.InfectedCapacity(level, 0)),
            SkillEffectId.PassiveProduction => Loc.F("effect.passive", EffectMath.PassiveFractionPerSecond(level) * 100d),
            SkillEffectId.InfectCapacity => Loc.F("effect.capacity", EffectMath.InfectedCapacity(1, level)),
            SkillEffectId.InfectSpread => EffectMath.SpreadInterval(level) <= 0d ? Loc.T("effect.spread_off") : Loc.F("effect.spread", EffectMath.SpreadInterval(level)),
            SkillEffectId.FragmentUnlock => Loc.F("effect.frag_chance", EffectMath.FragmentChance(level, 0) * 100d),
            SkillEffectId.FragmentChance => Loc.F("effect.frag_chance", EffectMath.FragmentChance(1, level) * 100d),
            SkillEffectId.FragmentCount => Loc.F("effect.frag_count", EffectMath.FragmentCount(level)),
            SkillEffectId.FragmentValue => Loc.F("effect.frag_value", EffectMath.FragmentFraction(level) * 100d),
            SkillEffectId.SynergyManualMini => Loc.F("effect.synergy_speed", (EffectMath.SynergySpeedMultiplier(level) - 1d) * 100d),
            SkillEffectId.SynergyMiniChain => Loc.F("effect.synergy_chain", EffectMath.MiniChainChanceScale(level) * 100d),
            SkillEffectId.SynergyCascade => Loc.F("effect.synergy_cascade", EffectMath.CascadeChargeBonus(level)),
            _ => string.Empty
        };
    }

    public static string Requirement(SkillDef skill)
    {
        if (skill == null)
        {
            return string.Empty;
        }

        StringBuilder text = new();
        if (skill.MinTotal > 0)
        {
            text.Append(Loc.F("req.purchases", skill.MinTotal));
        }

        for (int i = 0; i < skill.UnlockIds.Length; i++)
        {
            SkillDef parent = SkillBalanceTable.Find(skill.UnlockIds[i]);
            if (parent == null || parent.Anchor)
            {
                continue;
            }

            if (text.Length > 0)
            {
                text.Append('\n');
            }

            string name = Loc.TryGet($"skill.{parent.Id}.name", out string value) ? value : parent.Id;
            text.Append(Loc.F("req.skill", name, skill.UnlockLevels[i]));
        }

        return text.ToString();
    }
}
