using System;
using System.Collections.Generic;
using UnityEngine;

public enum SkillStat
{
    None,
    SpawnRate,
    MaxFiles,
    StartFiles,
    AttractionRadius,
    PullSpeed,
    ScreenWideEating,
    RunTime,
    FileValue,
    SpawnBurst,
    GoldenChance,
    CursorRadius,
    HoverFeed,
    Combo,
    ChainChance,
    Satellites,
    TimePerComputer,
    ServerAccess
}

[CreateAssetMenu(menuName = "Old Tech/Skill Tree", fileName = "Skill Tree")]
public class SkillTreeDefinition : ScriptableObject
{
    [Serializable]
    public class Node
    {
        public string id;
        public string displayName;
        [TextArea] public string description;
        public SkillStat stat;
        public int maxLevel = 1;
        public bool startsOwned;
        public bool comingSoon;
        public bool requiresAllMaxed;
        public bool major;
        public bool anchor;
        public SkillCategory category;
        public SkillEffectId effect;
        public int minTotalPurchases;
        public Vector2 position;
        public string[] requires = Array.Empty<string>();
        public string[] unlockIds = Array.Empty<string>();
        public int[] unlockLevels = Array.Empty<int>();
        public Sprite icon;
    }

    [SerializeField] private Node[] nodes = Array.Empty<Node>();

    public IReadOnlyList<Node> Nodes => nodes;

    void OnEnable()
    {
        Dictionary<string, Sprite> icons = new();
        if (nodes != null)
        {
            foreach (Node node in nodes)
            {
                if (node != null && node.icon != null && !string.IsNullOrEmpty(node.id))
                {
                    icons[node.id] = node.icon;
                }
            }
        }

        nodes = SkillCatalog.BuildNodes();
        foreach (Node node in nodes)
        {
            if (icons.TryGetValue(node.id, out Sprite icon))
            {
                node.icon = icon;
            }
        }

        SkillCatalog.ApplyIcons(nodes);
    }

    public Node Find(string id)
    {
        if (nodes == null)
        {
            return null;
        }

        foreach (Node node in nodes)
        {
            if (node.id == id)
            {
                return node;
            }
        }

        return null;
    }

    public int GetLevel(Node node)
    {
        if (node == null)
        {
            return 0;
        }

        if (node.anchor)
        {
            return 1;
        }

        return Mathf.Clamp(GameProgress.GetSkillLevel(node.id), 0, node.maxLevel);
    }

    public float GetStat(SkillStat stat)
    {
        return 0f;
    }

    public bool IsMaxed(Node node)
    {
        return node != null && !node.anchor && GetLevel(node) >= node.maxLevel;
    }

    public long GetCost(Node node)
    {
        return SkillPurchase.NextCost();
    }

    public PurchaseDenial Evaluate(Node node)
    {
        return SkillPurchase.Evaluate(node == null ? null : SkillBalanceTable.Find(node.id));
    }

    public SkillVisibility GetVisibility(Node node)
    {
        SkillDef skill = node == null ? null : SkillBalanceTable.Find(node.id);
        if (skill == null)
        {
            return SkillVisibility.Hidden;
        }

        return UnlockEval.Visibility(
            skill,
            GameProgress.GetSkillLevel,
            GameProgress.TotalPurchases,
            GameProgress.MB,
            GetCost(node));
    }

    public bool IsRevealed(Node node)
    {
        return GetVisibility(node) != SkillVisibility.Hidden;
    }

    public int RevealTier(Node node)
    {
        return GetVisibility(node) switch
        {
            SkillVisibility.Hidden => 0,
            SkillVisibility.Silhouette => 3,
            SkillVisibility.Locked => 2,
            _ => 1
        };
    }

    public bool CanBuy(Node node)
    {
        return node != null && !node.comingSoon && Evaluate(node).CanBuy;
    }

    public bool TryBuy(Node node)
    {
        return node != null && !node.comingSoon && SkillPurchase.TryBuy(node.id);
    }

    public void GetMasteryProgress(out int done, out int total)
    {
        done = GameProgress.TotalPurchases;
        total = ProgressionConfig.TargetPurchases;
    }

    public bool IsEverythingElseMaxed()
    {
        GetMasteryProgress(out int done, out int total);
        return done >= total;
    }

    public static string GetName(Node node)
    {
        if (node == null)
        {
            return string.Empty;
        }

        return Loc.TryGet($"skill.{node.id}.name", out string value) ? value : node.displayName;
    }

    public static string GetDescription(Node node)
    {
        if (node == null)
        {
            return string.Empty;
        }

        return Loc.TryGet($"skill.{node.id}.desc", out string value) ? value : node.description;
    }

    public string RequirementText(Node node)
    {
        SkillDef skill = node == null ? null : SkillBalanceTable.Find(node.id);
        return skill == null ? string.Empty : EffectText.Requirement(skill);
    }

    public string CurrentEffectText(Node node)
    {
        return node == null || node.anchor ? string.Empty : EffectText.Describe(node.effect, GetLevel(node));
    }

    public string NextEffectText(Node node)
    {
        if (node == null || node.anchor || IsMaxed(node))
        {
            return string.Empty;
        }

        return EffectText.Describe(node.effect, GetLevel(node) + 1);
    }
}
