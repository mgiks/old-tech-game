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
    RunTime
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
        public float valuePerLevel;
        [Min(1)] public int maxLevel = 1;
        public uint baseCost = 5;
        public float costMultiplier = 1.5f;
        public bool startsOwned;
        public bool comingSoon;
        public Vector2 position;
        public string[] requires = Array.Empty<string>();
    }

    [SerializeField] private Node[] nodes =
    {
        new()
        {
            id = "core", displayName = "Вирус",
            description = "Точка заражения. Отсюда растут все ветки.",
            startsOwned = true, position = new Vector2(0f, 0f)
        },

        new()
        {
            id = "spawn_rate_1", displayName = "Скорость",
            description = "Файлы появляются на рабочем столе на 25% чаще за уровень.",
            stat = SkillStat.SpawnRate, valuePerLevel = 0.25f, maxLevel = 5, baseCost = 5, costMultiplier = 1.5f,
            position = new Vector2(-300f, 0f), requires = new[] { "core" }
        },
        new()
        {
            id = "max_files", displayName = "Захламлённый стол",
            description = "+3 к лимиту файлов на экране за уровень.",
            stat = SkillStat.MaxFiles, valuePerLevel = 3f, maxLevel = 5, baseCost = 12, costMultiplier = 1.5f,
            position = new Vector2(-560f, 140f), requires = new[] { "spawn_rate_1" }
        },
        new()
        {
            id = "start_files", displayName = "Автозагрузка",
            description = "+2 файла на старте забега за уровень.",
            stat = SkillStat.StartFiles, valuePerLevel = 2f, maxLevel = 3, baseCost = 15, costMultiplier = 1.7f,
            position = new Vector2(-560f, -140f), requires = new[] { "spawn_rate_1" }
        },
        new()
        {
            id = "spawn_rate_2", displayName = "Разгон",
            description = "Ещё +25% к частоте появления файлов за уровень.",
            stat = SkillStat.SpawnRate, valuePerLevel = 0.25f, maxLevel = 5, baseCost = 40, costMultiplier = 1.6f,
            position = new Vector2(-820f, 140f), requires = new[] { "max_files" }
        },

        new()
        {
            id = "attraction", displayName = "Автономность",
            description = "Вирус сам притягивает и съедает файлы в радиусе 1.5 вокруг себя.",
            stat = SkillStat.AttractionRadius, valuePerLevel = 1.5f, maxLevel = 1, baseCost = 10,
            position = new Vector2(300f, 0f), requires = new[] { "core" }
        },
        new()
        {
            id = "attraction_radius", displayName = "Широкий охват",
            description = "+0.5 к радиусу притяжения за уровень.",
            stat = SkillStat.AttractionRadius, valuePerLevel = 0.5f, maxLevel = 5, baseCost = 15, costMultiplier = 1.5f,
            position = new Vector2(560f, 140f), requires = new[] { "attraction" }
        },
        new()
        {
            id = "pull_speed", displayName = "Сильная гравитация",
            description = "Файлы притягиваются на 1 ед./с быстрее за уровень.",
            stat = SkillStat.PullSpeed, valuePerLevel = 1f, maxLevel = 5, baseCost = 15, costMultiplier = 1.5f,
            position = new Vector2(560f, -140f), requires = new[] { "attraction" }
        },
        new()
        {
            id = "screen_wide", displayName = "Охота",
            description = "Вирус сам выбирает файлы по всему экрану и затягивает их. Каждый уровень сокращает паузу между охотами.",
            stat = SkillStat.ScreenWideEating, valuePerLevel = 1f, maxLevel = 5, baseCost = 60, costMultiplier = 1.7f,
            position = new Vector2(820f, 140f), requires = new[] { "attraction_radius" }
        },

        new()
        {
            id = "run_time", displayName = "Отсрочка увольнения",
            description = "+5 секунд к длительности забега за уровень.",
            stat = SkillStat.RunTime, valuePerLevel = 5f, maxLevel = 5, baseCost = 8, costMultiplier = 1.6f,
            position = new Vector2(0f, -220f), requires = new[] { "core" }
        },

        new()
        {
            id = "mutation", displayName = "Мутирование",
            description = "Открывает новые формы и улучшения вируса. В разработке.",
            comingSoon = true, position = new Vector2(0f, 220f), requires = new[] { "core" }
        },
    };

    public IReadOnlyList<Node> Nodes => nodes;

    public Node Find(string id)
    {
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
        int level = GameProgress.GetSkillLevel(node.id);
        return node.startsOwned ? Mathf.Max(1, level) : level;
    }

    public float GetStat(SkillStat stat)
    {
        float total = 0f;
        foreach (Node node in nodes)
        {
            if (node.stat == stat)
            {
                total += GetLevel(node) * node.valuePerLevel;
            }
        }

        return total;
    }

    public bool IsMaxed(Node node)
    {
        return GetLevel(node) >= node.maxLevel;
    }

    public uint GetCost(Node node)
    {
        return (uint)Mathf.CeilToInt(node.baseCost * Mathf.Pow(node.costMultiplier, GetLevel(node)));
    }

    public bool IsRevealed(Node node)
    {
        if (node.requires == null || node.requires.Length == 0)
        {
            return true;
        }

        foreach (string requiredId in node.requires)
        {
            Node required = Find(requiredId);
            if (required != null && GetLevel(required) > 0)
            {
                return true;
            }
        }

        return false;
    }

    public bool CanBuy(Node node)
    {
        return IsRevealed(node)
            && !node.comingSoon
            && !node.startsOwned
            && !IsMaxed(node)
            && GameProgress.MB >= GetCost(node);
    }

    public bool TryBuy(Node node)
    {
        if (!CanBuy(node) || !GameProgress.TrySpendMB(GetCost(node)))
        {
            return false;
        }

        GameProgress.SetSkillLevel(node.id, GetLevel(node) + 1);
        return true;
    }
}
