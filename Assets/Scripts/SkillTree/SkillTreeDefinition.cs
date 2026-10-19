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
        public float valuePerLevel;
        [Min(1)] public int maxLevel = 1;
        public uint baseCost = 5;
        public float costMultiplier = 1.5f;
        public bool startsOwned;
        public bool comingSoon;
        public bool requiresAllMaxed;
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
            id = "attraction", displayName = "Автономность",
            description = "Вирус сам притягивает и съедает файлы вокруг себя.",
            stat = SkillStat.AttractionRadius, valuePerLevel = 1.5f, maxLevel = 1, baseCost = 12,
            position = new Vector2(300f, 0f), requires = new[] { "core" }
        },
        new()
        {
            id = "attraction_radius", displayName = "Широкий охват",
            description = "Вирус притягивает файлы с большего расстояния.",
            stat = SkillStat.AttractionRadius, valuePerLevel = 0.6f, maxLevel = 5, baseCost = 15, costMultiplier = 1.55f,
            position = new Vector2(560f, 140f), requires = new[] { "attraction" }
        },
        new()
        {
            id = "pull_speed", displayName = "Сильная гравитация",
            description = "Файлы летят к вирусу быстрее.",
            stat = SkillStat.PullSpeed, valuePerLevel = 1.5f, maxLevel = 4, baseCost = 15, costMultiplier = 1.55f,
            position = new Vector2(560f, -140f), requires = new[] { "attraction" }
        },
        new()
        {
            id = "screen_wide", displayName = "Охота",
            description = "Вирус сам выбирает файлы по всему экрану и затягивает их. Каждый уровень охотится чаще, с 3-го — по несколько файлов сразу.",
            stat = SkillStat.ScreenWideEating, valuePerLevel = 1f, maxLevel = 5, baseCost = 50, costMultiplier = 1.8f,
            position = new Vector2(820f, 140f), requires = new[] { "attraction_radius" }
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
            id = "server_access", displayName = "Сетевой червь",
            description = "Последний, 11-й компьютер — сервер фирмы. Он защищён, пока вирус не освоит всё. Прокачай всё дерево до максимума.",
            stat = SkillStat.ServerAccess, valuePerLevel = 1f, maxLevel = 1, baseCost = 1500, requiresAllMaxed = true,
            position = new Vector2(0f, -680f), requires = new[] { "run_time_2" }
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
            id = "hover_feed", displayName = "Без рук",
            description = "Кнопку зажимать больше не нужно: просто води мышкой, файлы липнут к курсору. Заведи их в глича.",
            stat = SkillStat.HoverFeed, valuePerLevel = 1f, maxLevel = 1, baseCost = 40,
            position = new Vector2(-560f, 380f), requires = new[] { "cursor_radius" }
        },

        new()
        {
            id = "mutation", displayName = "Мутирование",
            description = "Вирус мутирует. Ешь файлы без пауз — растёт комбо, и каждый файл в серии стоит больше.",
            stat = SkillStat.Combo, valuePerLevel = 0.1f, maxLevel = 1, baseCost = 25,
            position = new Vector2(0f, 240f), requires = new[] { "core" }
        },
        new()
        {
            id = "gluttony", displayName = "Ненасытность",
            description = "Каждый шаг комбо даёт ещё больше MB.",
            stat = SkillStat.Combo, valuePerLevel = 0.05f, maxLevel = 4, baseCost = 40, costMultiplier = 1.7f,
            position = new Vector2(0f, 470f), requires = new[] { "mutation" }
        },
        new()
        {
            id = "chain", displayName = "Цепная реакция",
            description = "Съеденный файл может заразить другой файл — тот сам летит в глича.",
            stat = SkillStat.ChainChance, valuePerLevel = 0.1f, maxLevel = 5, baseCost = 35, costMultiplier = 1.6f,
            position = new Vector2(280f, 360f), requires = new[] { "mutation" }
        },
        new()
        {
            id = "expansion", displayName = "Экспансия",
            description = "Каждый заражённый компьютер добавляет время к забегу.",
            stat = SkillStat.TimePerComputer, valuePerLevel = 1f, maxLevel = 3, baseCost = 60, costMultiplier = 1.8f,
            position = new Vector2(-280f, 560f), requires = new[] { "gluttony" }
        },
        new()
        {
            id = "satellites", displayName = "Отпочкование",
            description = "Вокруг глича кружат его маленькие копии и съедают всё, к чему прикоснутся.",
            stat = SkillStat.Satellites, valuePerLevel = 1f, maxLevel = 3, baseCost = 70, costMultiplier = 1.9f,
            position = new Vector2(0f, 700f), requires = new[] { "gluttony" }
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
            && (!node.requiresAllMaxed || IsEverythingElseMaxed())
            && GameProgress.MB >= GetCost(node);
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
            SkillStat.AttractionRadius => Loc.F("stat.attraction_radius", total),
            SkillStat.PullSpeed => Loc.F("stat.pull_speed", total),
            SkillStat.ScreenWideEating => Loc.F("stat.screen_wide", total),
            SkillStat.RunTime => Loc.F("stat.run_time", total),
            SkillStat.FileValue => Loc.F("stat.file_value", 1f + total),
            SkillStat.SpawnBurst => Loc.F("stat.spawn_burst", 1f + total),
            SkillStat.GoldenChance => Loc.F("stat.golden", total * 100f),
            SkillStat.CursorRadius => Loc.F("stat.cursor_radius", total),
            SkillStat.HoverFeed => Loc.T(total > 0f ? "stat.hover_on" : "stat.hover_off"),
            SkillStat.Combo => Loc.F("stat.combo", total * 100f),
            SkillStat.ChainChance => Loc.F("stat.chain", total * 100f),
            SkillStat.Satellites => Loc.F("stat.satellites", total),
            SkillStat.TimePerComputer => Loc.F("stat.time_per_pc", total),
            SkillStat.ServerAccess => Loc.T(total > 0f ? "stat.server_open" : "stat.server_closed"),
            _ => string.Empty
        };
    }
}
