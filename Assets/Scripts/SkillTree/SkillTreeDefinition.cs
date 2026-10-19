using System;
using System.Collections.Generic;
using UnityEngine;

public enum SkillStat
{
    None = 0,
    SpawnRate = 1,
    MaxFiles = 2,
    StartFiles = 3,
    RunTime = 7,
    FileValue = 8,
    SpawnBurst = 9,
    GoldenChance = 10,
    CursorRadius = 11,
    HoverFeed = 12,
    CrawlerCount = 18,
    CrawlerSpeed = 19,
    CrawlerDownload = 20,
    ConfidentialChance = 21,
    CrawlerInstinct = 22,
    MagneticRadius = 23,
    CursorCapacity = 24,
    Vacuum = 25,
    SmokeBreak = 26,
    Overtime = 27,
    Sabotage = 28
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
        public bool requiresAllMaxed;
        public Vector2 position;
        public string[] requires = Array.Empty<string>();
        public Sprite icon;
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
            description = "Файлы появляются на рабочем столе чаще.",
            stat = SkillStat.SpawnRate, valuePerLevel = 0.4f, maxLevel = 5, baseCost = 4, costMultiplier = 1.55f,
            position = new Vector2(-300f, 0f), requires = new[] { "core" }
        },
        new()
        {
            id = "max_files", displayName = "Захламлённый стол",
            description = "На экране помещается больше файлов.",
            stat = SkillStat.MaxFiles, valuePerLevel = 5f, maxLevel = 4, baseCost = 10, costMultiplier = 1.6f,
            position = new Vector2(-560f, 140f), requires = new[] { "spawn_rate_1" }
        },
        new()
        {
            id = "start_files", displayName = "Автозагрузка",
            description = "Забег начинается с кучей файлов на столе.",
            stat = SkillStat.StartFiles, valuePerLevel = 3f, maxLevel = 4, baseCost = 8, costMultiplier = 1.6f,
            position = new Vector2(-560f, -140f), requires = new[] { "spawn_rate_1" }
        },
        new()
        {
            id = "spawn_burst", displayName = "Разгон",
            description = "Файлы появляются пачками: +1 файл в каждой пачке.",
            stat = SkillStat.SpawnBurst, valuePerLevel = 1f, maxLevel = 3, baseCost = 35, costMultiplier = 1.9f,
            position = new Vector2(-820f, 140f), requires = new[] { "max_files" }
        },

        new()
        {
            id = "file_value", displayName = "Сжатие",
            description = "Каждый съеденный файл приносит больше MB.",
            stat = SkillStat.FileValue, valuePerLevel = 1f, maxLevel = 6, baseCost = 6, costMultiplier = 1.7f,
            position = new Vector2(-300f, -300f), requires = new[] { "core" }
        },
        new()
        {
            id = "golden", displayName = "Ценные файлы",
            description = "Иногда появляются золотые файлы: они стоят в 5 раз больше.",
            stat = SkillStat.GoldenChance, valuePerLevel = 0.06f, maxLevel = 5, baseCost = 25, costMultiplier = 1.7f,
            position = new Vector2(-560f, -400f), requires = new[] { "file_value" }
        },
        new()
        {
            id = "confidential", displayName = "Секретные файлы",
            description = "Редко появляются секретные документы. Они стоят в 25 раз больше обычного файла.",
            stat = SkillStat.ConfidentialChance, valuePerLevel = SkillTuning.ConfidentialChancePerLevel, maxLevel = 5, baseCost = 36, costMultiplier = 1.7f,
            position = new Vector2(-820f, -480f), requires = new[] { "golden" }
        },

        new()
        {
            id = "crawler", displayName = "Ползун",
            description = "Маленький вирус сам ползёт к свободному файлу, садится на него и скачивает. Файл приносит MB, как будто его съел глич.",
            stat = SkillStat.CrawlerCount, valuePerLevel = 1f, maxLevel = 1, baseCost = 30,
            position = new Vector2(300f, 0f), requires = new[] { "core" }
        },
        new()
        {
            id = "crawler_speed", displayName = "Быстрые лапки",
            description = "Ползун быстрее добирается до файла.",
            stat = SkillStat.CrawlerSpeed, valuePerLevel = 0.7f, maxLevel = 5, baseCost = 18, costMultiplier = 1.6f,
            position = new Vector2(560f, 140f), requires = new[] { "crawler" }
        },
        new()
        {
            id = "crawler_download", displayName = "Быстрая закачка",
            description = "Ползун меньше сидит на файле. Сначала закачка занимает 1,7 с, на максимуме около 0,5 с.",
            stat = SkillStat.CrawlerDownload, valuePerLevel = 0.24f, maxLevel = 5, baseCost = 18, costMultiplier = 1.6f,
            position = new Vector2(560f, -140f), requires = new[] { "crawler" }
        },
        new()
        {
            id = "colony", displayName = "Колония",
            description = "Ещё один ползун. Вместе с первым их не больше трёх, и они не бегут на один и тот же файл.",
            stat = SkillStat.CrawlerCount, valuePerLevel = 1f, maxLevel = 2, baseCost = 70, costMultiplier = 1.9f,
            position = new Vector2(820f, 0f), requires = new[] { "crawler" }
        },
        new()
        {
            id = "instinct", displayName = "Острый нюх",
            description = "Ползуны сначала идут за золотыми и секретными файлами, но не бросают близкий обычный файл ради далёкого.",
            stat = SkillStat.CrawlerInstinct, valuePerLevel = 1f, maxLevel = 1, baseCost = 100,
            position = new Vector2(1040f, 140f), requires = new[] { "crawler" }
        },

        new()
        {
            id = "run_time", displayName = "Отсрочка увольнения",
            description = "Забег длится дольше.",
            stat = SkillStat.RunTime, valuePerLevel = 2f, maxLevel = 6, baseCost = 5, costMultiplier = 1.5f,
            position = new Vector2(0f, -220f), requires = new[] { "core" }
        },
        new()
        {
            id = "run_time_2", displayName = "Больничный",
            description = "Забег длится намного дольше.",
            stat = SkillStat.RunTime, valuePerLevel = 4f, maxLevel = 3, baseCost = 60, costMultiplier = 1.8f,
            position = new Vector2(0f, -440f), requires = new[] { "run_time" }
        },
        new()
        {
            id = "smoke_break", displayName = "Перекур",
            description = "При заражении следующего компьютера к таймеру добавляется секунда. Один раз за переход.",
            stat = SkillStat.SmokeBreak, valuePerLevel = 1f, maxLevel = 3, baseCost = 40, costMultiplier = 1.7f,
            position = new Vector2(260f, -320f), requires = new[] { "run_time" }
        },
        new()
        {
            id = "overtime", displayName = "Сверхурочные",
            description = "Каждые 100 MB, заработанные за этот забег, добавляют немного времени. Один и тот же порог не срабатывает дважды.",
            stat = SkillStat.Overtime, valuePerLevel = SkillTuning.OvertimeSecondsPerLevel, maxLevel = 3, baseCost = 90, costMultiplier = 1.85f,
            position = new Vector2(260f, -500f), requires = new[] { "smoke_break" }
        },
        new()
        {
            id = "sabotage", displayName = "Саботаж",
            description = "На 3, 6 и 9 заражённом компьютере таймер получает добавку. Каждый порог срабатывает один раз.",
            stat = SkillStat.Sabotage, valuePerLevel = SkillTuning.SabotageSecondsPerLevel, maxLevel = 3, baseCost = 110, costMultiplier = 1.85f,
            position = new Vector2(260f, -700f), requires = new[] { "overtime" }
        },

        new()
        {
            id = "cursor_radius", displayName = "Цепкий курсор",
            description = "Тащишь один файл — курсор прихватывает и все файлы рядом.",
            stat = SkillStat.CursorRadius, valuePerLevel = 0.45f, maxLevel = 5, baseCost = 8, costMultiplier = 1.6f,
            position = new Vector2(-300f, 260f), requires = new[] { "core" }
        },
        new()
        {
            id = "magnetic", displayName = "Магнитный курсор",
            description = "Файлы в небольшом радиусе плавно тянутся к курсору.",
            stat = SkillStat.MagneticRadius, valuePerLevel = SkillTuning.MagneticRadiusPerLevel, maxLevel = 4, baseCost = 22, costMultiplier = 1.65f,
            position = new Vector2(-520f, 340f), requires = new[] { "cursor_radius" }
        },
        new()
        {
            id = "wide_grip", displayName = "Широкий хват",
            description = "Курсор держит больше файлов сразу. Каждый уровень добавляет два места.",
            stat = SkillStat.CursorCapacity, valuePerLevel = SkillTuning.WideGripPerLevel, maxLevel = 3, baseCost = 30, costMultiplier = 1.65f,
            position = new Vector2(-740f, 420f), requires = new[] { "magnetic" }
        },
        new()
        {
            id = "hover_feed", displayName = "Без рук",
            description = "Кнопку зажимать больше не нужно: просто води мышкой, файлы липнут к курсору. Заведи их в глича.",
            stat = SkillStat.HoverFeed, valuePerLevel = 1f, maxLevel = 1, baseCost = 250,
            position = new Vector2(-960f, 500f), requires = new[] { "wide_grip" }
        },
        new()
        {
            id = "vacuum", displayName = "Пылесос",
            description = "Задержи курсор на месте — вокруг него открывается зона, и файлы сами слетаются и съедаются.",
            stat = SkillStat.Vacuum, valuePerLevel = 1f, maxLevel = 1, baseCost = 480,
            position = new Vector2(-1180f, 580f), requires = new[] { "hover_feed" }
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

    private static readonly string[] branchOrder =
    {
        "spawn_rate_1",
        "file_value",
        "run_time",
        "cursor_radius",
        "crawler"
    };

    public int PurchasedLevels
    {
        get
        {
            int total = 0;
            foreach (Node node in nodes)
            {
                if (!node.startsOwned)
                {
                    total += GetLevel(node);
                }
            }

            return total;
        }
    }

    public int OpenBranchCount => Mathf.Clamp(1 + PurchasedLevels / SkillTuning.LevelsPerBranch, 1, branchOrder.Length);

    public bool IsBranchOpen(Node node)
    {
        int index = BranchIndex(node);
        return index >= 0 && index < OpenBranchCount;
    }

    public bool IsPreview(Node node)
    {
        return !node.startsOwned && IsRevealed(node) && !IsBranchOpen(node);
    }

    public bool IsRevealed(Node node)
    {
        if (node.startsOwned || GameProgress.HasSeen(node.id))
        {
            return true;
        }

        return IsEligible(node);
    }

    public bool CanBuy(Node node)
    {
        return IsBranchOpen(node)
            && HasOwnedParent(node)
            && !node.comingSoon
            && !node.startsOwned
            && !IsMaxed(node)
            && (!node.requiresAllMaxed || IsEverythingElseMaxed())
            && GameProgress.MB >= GetCost(node);
    }

    private bool IsEligible(Node node)
    {
        if (node.startsOwned)
        {
            return true;
        }

        int index = BranchIndex(node);
        if (index < 0)
        {
            return false;
        }

        bool root = node.id == branchOrder[index];
        if (root)
        {
            return index <= OpenBranchCount;
        }

        if (index >= OpenBranchCount)
        {
            return false;
        }

        return HasOwnedParent(node);
    }

    private bool HasOwnedParent(Node node)
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

    private int BranchIndex(Node node)
    {
        string root = BranchRootId(node);
        for (int i = 0; i < branchOrder.Length; i++)
        {
            if (branchOrder[i] == root)
            {
                return i;
            }
        }

        return -1;
    }

    private string BranchRootId(Node node)
    {
        Node current = node;
        while (current.requires != null && current.requires.Length > 0)
        {
            Node parent = Find(current.requires[0]);
            if (parent == null || parent.startsOwned)
            {
                return current.id;
            }

            current = parent;
        }

        return current.id;
    }

    public bool IsEverythingElseMaxed()
    {
        GetMasteryProgress(out int done, out int total);
        return done >= total;
    }

    public void GetMasteryProgress(out int done, out int total)
    {
        done = 0;
        total = 0;
        foreach (Node node in nodes)
        {
            if (node.startsOwned || node.comingSoon || node.requiresAllMaxed)
            {
                continue;
            }

            done += Mathf.Min(GetLevel(node), node.maxLevel);
            total += node.maxLevel;
        }
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

    public static string GetName(Node node)
    {
        return Loc.TryGet($"skill.{node.id}.name", out string value) ? value : node.displayName;
    }

    public static string GetDescription(Node node)
    {
        return Loc.TryGet($"skill.{node.id}.desc", out string value) ? value : node.description;
    }

    public static string FormatStat(SkillStat stat, float total)
    {
        return stat switch
        {
            SkillStat.SpawnRate => Loc.F("stat.spawn_rate", 1f + total),
            SkillStat.MaxFiles => Loc.F("stat.max_files", total),
            SkillStat.StartFiles => Loc.F("stat.start_files", total),
            SkillStat.RunTime => Loc.F("stat.run_time", total),
            SkillStat.FileValue => Loc.F("stat.file_value", 1f + total),
            SkillStat.SpawnBurst => Loc.F("stat.spawn_burst", 1f + total),
            SkillStat.GoldenChance => Loc.F("stat.golden", total * 100f),
            SkillStat.CursorRadius => Loc.F("stat.cursor_radius", total),
            SkillStat.HoverFeed => Loc.T(total > 0f ? "stat.hover_on" : "stat.hover_off"),
            SkillStat.CrawlerCount => Loc.F("stat.crawlers", total),
            SkillStat.CrawlerSpeed => Loc.F("stat.crawler_speed", VirusCrawlers.MoveSpeed(total)),
            SkillStat.CrawlerDownload => Loc.F("stat.crawler_download", VirusCrawlers.DownloadDuration(total)),
            SkillStat.ConfidentialChance => Loc.F("stat.confidential", total * 100f),
            SkillStat.CrawlerInstinct => Loc.T(total > 0f ? "stat.instinct_on" : "stat.instinct_off"),
            SkillStat.MagneticRadius => Loc.F("stat.magnetic", total <= 0f ? 0f : SkillTuning.MagneticRadius(total)),
            SkillStat.CursorCapacity => Loc.F("stat.grip", SkillTuning.CursorHold(total)),
            SkillStat.Vacuum => Loc.T(total > 0f ? "stat.vacuum_on" : "stat.vacuum_off"),
            SkillStat.SmokeBreak => Loc.F("stat.smoke", total),
            SkillStat.Overtime => Loc.F("stat.overtime", total),
            SkillStat.Sabotage => Loc.F("stat.sabotage", total),
            _ => string.Empty
        };
    }
}
