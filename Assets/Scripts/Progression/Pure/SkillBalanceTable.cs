using System;
using System.Collections.Generic;

public enum SkillCategory
{
    Core,
    Manual,
    Processing,
    MiniVirus,
    Chain,
    Overload,
    Infected,
    Fragment,
    Synergy
}

public enum SkillEffectId
{
    None,
    DownloadSpeed,
    Multithreading,
    DownloadBuffer,
    DownloadBurst,
    Compression,
    Optimization,
    ValuableFiles,
    MiniUnlock,
    Population,
    MoveSpeed,
    FeedSpeed,
    Targeting,
    ChainUnlock,
    ChainChance,
    ChainRange,
    ChainDepth,
    ChainBranch,
    OverloadUnlock,
    OverloadCharge,
    OverloadDuration,
    OverloadPower,
    InfectUnlock,
    PassiveProduction,
    InfectCapacity,
    InfectSpread,
    FragmentUnlock,
    FragmentChance,
    FragmentCount,
    FragmentValue,
    SynergyManualMini,
    SynergyMiniChain,
    SynergyCascade
}

public enum GameSystemId
{
    None,
    MiniVirus,
    Chain,
    Overload,
    Infected,
    Fragment
}

public enum SkillVisibility
{
    Hidden,
    Silhouette,
    Locked,
    Available,
    Purchasable,
    Partial,
    Maxed
}

public sealed class SkillDef
{
    public string Id;
    public SkillCategory Category;
    public SkillEffectId Effect;
    public int MaxLevel;
    public int MinTotal;
    public string[] UnlockIds = Array.Empty<string>();
    public int[] UnlockLevels = Array.Empty<int>();
    public GameSystemId[] Systems = Array.Empty<GameSystemId>();
    public string[] Links = Array.Empty<string>();
    public bool Major;
    public bool Anchor;
    public float X;
    public float Y;
}

public static class SkillBalanceTable
{
    public const string Core = "core";
    public const string A1 = "a1_download_speed";
    public const string A2 = "a2_multithreading";
    public const string A3 = "a3_download_buffer";
    public const string A4 = "a4_download_burst";
    public const string B1 = "b1_data_compression";
    public const string B2 = "b2_data_optimization";
    public const string B3 = "b3_valuable_files";
    public const string C1 = "c1_mini_virus_unlock";
    public const string C2 = "c2_virus_population";
    public const string C3 = "c3_movement_speed";
    public const string C4 = "c4_feeding_speed";
    public const string C5 = "c5_smart_targeting";
    public const string D1 = "d1_chain_unlock";
    public const string D2 = "d2_chain_probability";
    public const string D3 = "d3_chain_range";
    public const string D4 = "d4_chain_depth";
    public const string D5 = "d5_chain_branching";
    public const string E1 = "e1_overload_unlock";
    public const string E2 = "e2_overload_charge";
    public const string E3 = "e3_overload_duration";
    public const string E4 = "e4_overload_power";
    public const string F1 = "f1_infected_unlock";
    public const string F2 = "f2_passive_production";
    public const string F3 = "f3_infected_capacity";
    public const string F4 = "f4_infected_spread";
    public const string G1 = "g1_fragment_unlock";
    public const string G2 = "g2_fragment_chance";
    public const string G3 = "g3_fragment_count";
    public const string G4 = "g4_fragment_value";
    public const string H1 = "h1_manual_mini";
    public const string H2 = "h2_mini_chain";
    public const string H3 = "h3_cascade_overload";

    private static readonly SkillDef[] skills =
    {
        Node(Core, SkillCategory.Core, SkillEffectId.None, 0, 0, true, true, 0f, 0f, null, null, L()),

        Node(A1, SkillCategory.Manual, SkillEffectId.DownloadSpeed, 8, 0, true, false, -380f, 210f, null, null, L(Core)),
        Node(A2, SkillCategory.Manual, SkillEffectId.Multithreading, 3, 0, false, false, -640f, 370f, U(A1, 1), null, L(A1)),
        Node(A3, SkillCategory.Manual, SkillEffectId.DownloadBuffer, 4, 0, false, false, -640f, 170f, U(A1, 2), null, L(A1)),
        Node(A4, SkillCategory.Manual, SkillEffectId.DownloadBurst, 3, 0, false, false, -900f, 270f, U(A2, 1), null, L(A2)),

        Node(B1, SkillCategory.Processing, SkillEffectId.Compression, 8, 0, true, false, -380f, -190f, null, null, L(Core)),
        Node(B2, SkillCategory.Processing, SkillEffectId.Optimization, 5, 0, false, false, -640f, -50f, U(B1, 1), null, L(B1)),
        Node(B3, SkillCategory.Processing, SkillEffectId.ValuableFiles, 4, 0, false, false, -640f, -310f, U(B1, 2), null, L(B1)),

        Node(C1, SkillCategory.MiniVirus, SkillEffectId.MiniUnlock, 1, 12, true, false, 380f, 250f, null, null, L(Core)),
        Node(C2, SkillCategory.MiniVirus, SkillEffectId.Population, 5, 0, false, false, 660f, 470f, U(C1, 1), S(GameSystemId.MiniVirus), L(C1)),
        Node(C3, SkillCategory.MiniVirus, SkillEffectId.MoveSpeed, 6, 0, false, false, 660f, 270f, U(C1, 1), S(GameSystemId.MiniVirus), L(C1)),
        Node(C4, SkillCategory.MiniVirus, SkillEffectId.FeedSpeed, 6, 0, false, false, 660f, 70f, U(C1, 1), S(GameSystemId.MiniVirus), L(C1)),
        Node(C5, SkillCategory.MiniVirus, SkillEffectId.Targeting, 4, 0, false, false, 940f, 250f, U(C1, 1), S(GameSystemId.MiniVirus), L(C1)),

        Node(D1, SkillCategory.Chain, SkillEffectId.ChainUnlock, 1, 30, true, false, 380f, -210f, U(C1, 1), S(GameSystemId.MiniVirus), L(C1)),
        Node(D2, SkillCategory.Chain, SkillEffectId.ChainChance, 5, 0, false, false, 660f, -120f, U(D1, 1), S(GameSystemId.Chain), L(D1)),
        Node(D3, SkillCategory.Chain, SkillEffectId.ChainRange, 4, 0, false, false, 660f, -330f, U(D1, 1), S(GameSystemId.Chain), L(D1)),
        Node(D4, SkillCategory.Chain, SkillEffectId.ChainDepth, 4, 0, false, false, 940f, -90f, U(D1, 1), S(GameSystemId.Chain), L(D1)),
        Node(D5, SkillCategory.Chain, SkillEffectId.ChainBranch, 4, 0, false, false, 940f, -330f, U(D1, 1), S(GameSystemId.Chain), L(D1)),

        Node(E1, SkillCategory.Overload, SkillEffectId.OverloadUnlock, 1, 50, true, false, 40f, -530f, U(D1, 1), S(GameSystemId.Chain), L(D1)),
        Node(E2, SkillCategory.Overload, SkillEffectId.OverloadCharge, 4, 0, false, false, 300f, -530f, U(E1, 1), S(GameSystemId.Overload), L(E1)),
        Node(E3, SkillCategory.Overload, SkillEffectId.OverloadDuration, 3, 0, false, false, 40f, -750f, U(E1, 1), S(GameSystemId.Overload), L(E1)),
        Node(E4, SkillCategory.Overload, SkillEffectId.OverloadPower, 4, 0, false, false, 300f, -750f, U(E1, 1), S(GameSystemId.Overload), L(E1)),

        Node(F1, SkillCategory.Infected, SkillEffectId.InfectUnlock, 1, 70, true, false, -280f, -470f, U(E1, 1), S(GameSystemId.Overload), L(E1)),
        Node(F2, SkillCategory.Infected, SkillEffectId.PassiveProduction, 5, 0, false, false, -540f, -470f, U(F1, 1), S(GameSystemId.Infected), L(F1)),
        Node(F3, SkillCategory.Infected, SkillEffectId.InfectCapacity, 3, 0, false, false, -280f, -690f, U(F1, 1), S(GameSystemId.Infected), L(F1)),
        Node(F4, SkillCategory.Infected, SkillEffectId.InfectSpread, 3, 0, false, false, -540f, -690f, U(F1, 1), S(GameSystemId.Infected), L(F1)),

        Node(G1, SkillCategory.Fragment, SkillEffectId.FragmentUnlock, 1, 88, true, false, 620f, -560f, U(F1, 1), S(GameSystemId.Infected), L(F1)),
        Node(G2, SkillCategory.Fragment, SkillEffectId.FragmentChance, 3, 0, false, false, 880f, -560f, U(G1, 1), S(GameSystemId.Fragment), L(G1)),
        Node(G3, SkillCategory.Fragment, SkillEffectId.FragmentCount, 3, 0, false, false, 620f, -780f, U(G1, 1), S(GameSystemId.Fragment), L(G1)),
        Node(G4, SkillCategory.Fragment, SkillEffectId.FragmentValue, 3, 0, false, false, 880f, -780f, U(G1, 1), S(GameSystemId.Fragment), L(G1)),

        Node(H1, SkillCategory.Synergy, SkillEffectId.SynergyManualMini, 4, 102, true, false, 0f, 450f, U(C1, 1), S(GameSystemId.MiniVirus), L(A1, C1)),
        Node(H2, SkillCategory.Synergy, SkillEffectId.SynergyMiniChain, 4, 102, true, false, 380f, 40f, U(C1, 1, D1, 1), S(GameSystemId.MiniVirus, GameSystemId.Chain), L(C1, D1)),
        Node(H3, SkillCategory.Synergy, SkillEffectId.SynergyCascade, 3, 102, true, false, 160f, -370f, U(D1, 1, E1, 1), S(GameSystemId.Chain, GameSystemId.Overload), L(D1, E1))
    };

    private static readonly Dictionary<string, SkillDef> byId = BuildIndex();

    static SkillBalanceTable()
    {
        Find(A3).UnlockLevels = new[] { 2 };
        Find(B3).UnlockLevels = new[] { 2 };
    }

    public static IReadOnlyList<SkillDef> Skills => skills;

    public static SkillDef Find(string id)
    {
        if (id != null && byId.TryGetValue(id, out SkillDef skill))
        {
            return skill;
        }

        return null;
    }

    public static int PurchasableCount()
    {
        int count = 0;
        foreach (SkillDef skill in skills)
        {
            if (!skill.Anchor && skill.MaxLevel > 0)
            {
                count++;
            }
        }

        return count;
    }

    public static int MaxLevelSum()
    {
        int sum = 0;
        foreach (SkillDef skill in skills)
        {
            if (!skill.Anchor)
            {
                sum += skill.MaxLevel;
            }
        }

        return sum;
    }

    public static string SystemUnlockId(GameSystemId system)
    {
        return system switch
        {
            GameSystemId.MiniVirus => C1,
            GameSystemId.Chain => D1,
            GameSystemId.Overload => E1,
            GameSystemId.Infected => F1,
            GameSystemId.Fragment => G1,
            _ => null
        };
    }

    private static Dictionary<string, SkillDef> BuildIndex()
    {
        Dictionary<string, SkillDef> map = new();
        foreach (SkillDef skill in skills)
        {
            map.Add(skill.Id, skill);
        }

        return map;
    }

    private static string[] L(params string[] ids) => ids ?? Array.Empty<string>();

    private static string[] U(params object[] pairs)
    {
        string[] ids = new string[pairs.Length / 2];
        for (int i = 0; i < ids.Length; i++)
        {
            ids[i] = (string)pairs[i * 2];
        }

        return ids;
    }

    private static GameSystemId[] S(params GameSystemId[] systems) => systems;

    private static SkillDef Node(
        string id,
        SkillCategory category,
        SkillEffectId effect,
        int maxLevel,
        int minTotal,
        bool major,
        bool anchor,
        float x,
        float y,
        string[] unlockIds,
        GameSystemId[] systems,
        string[] links)
    {
        int[] levels = Array.Empty<int>();
        if (unlockIds != null && unlockIds.Length > 0)
        {
            levels = new int[unlockIds.Length];
            for (int i = 0; i < levels.Length; i++)
            {
                levels[i] = 1;
            }
        }

        return new SkillDef
        {
            Id = id,
            Category = category,
            Effect = effect,
            MaxLevel = maxLevel,
            MinTotal = minTotal,
            Major = major,
            Anchor = anchor,
            X = x,
            Y = y,
            UnlockIds = unlockIds ?? Array.Empty<string>(),
            UnlockLevels = levels,
            Systems = systems ?? Array.Empty<GameSystemId>(),
            Links = links ?? Array.Empty<string>()
        };
    }
}
