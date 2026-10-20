using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public static class SkillCatalog
{
    private static readonly Dictionary<string, string> iconPaths = new()
    {
        [SkillBalanceTable.Core] = "Assets/Art/Skills/icon_core.png",
        [SkillBalanceTable.A1] = "Assets/Art/Skills/icon_spawn_rate_1.png",
        [SkillBalanceTable.A2] = "Assets/Art/Skills/icon_cursor_radius.png",
        [SkillBalanceTable.A3] = "Assets/Art/Skills/icon_hover_feed.png",
        [SkillBalanceTable.A4] = "Assets/Art/Skills/icon_spawn_burst.png",
        [SkillBalanceTable.B1] = "Assets/Art/Skills/icon_file_value.png",
        [SkillBalanceTable.B2] = "Assets/Art/Skills/icon_gluttony.png",
        [SkillBalanceTable.B3] = "Assets/Art/Skills/icon_golden.png",
        [SkillBalanceTable.C1] = "Assets/Art/Skills/icon_satellites.png",
        [SkillBalanceTable.C2] = "Assets/Art/Skills/icon_attraction.png",
        [SkillBalanceTable.C3] = "Assets/Art/Skills/icon_pull_speed.png",
        [SkillBalanceTable.C4] = "Assets/Art/Skills/icon_screen_wide.png",
        [SkillBalanceTable.C5] = "Assets/Art/Skills/icon_attraction_radius.png",
        [SkillBalanceTable.D1] = "Assets/Art/Skills/icon_chain.png",
        [SkillBalanceTable.D2] = "Assets/Art/Skills/icon_mutation.png",
        [SkillBalanceTable.D3] = "Assets/Art/Skills/icon_expansion.png",
        [SkillBalanceTable.D4] = "Assets/Art/Skills/icon_run_time.png",
        [SkillBalanceTable.D5] = "Assets/Art/Skills/icon_max_files.png",
        [SkillBalanceTable.E1] = "Assets/Art/Skills/icon_spawn_burst.png",
        [SkillBalanceTable.E2] = "Assets/Art/Skills/icon_run_time.png",
        [SkillBalanceTable.E3] = "Assets/Art/Skills/icon_run_time_2.png",
        [SkillBalanceTable.E4] = "Assets/Art/Skills/icon_screen_wide.png",
        [SkillBalanceTable.F1] = "Assets/Art/Skills/icon_chain.png",
        [SkillBalanceTable.F2] = "Assets/Art/Skills/icon_file_value.png",
        [SkillBalanceTable.F3] = "Assets/Art/Skills/icon_max_files.png",
        [SkillBalanceTable.F4] = "Assets/Art/Skills/icon_expansion.png",
        [SkillBalanceTable.G1] = "Assets/Art/Skills/icon_golden.png",
        [SkillBalanceTable.G2] = "Assets/Art/Skills/icon_golden.png",
        [SkillBalanceTable.G3] = "Assets/Art/Skills/icon_spawn_burst.png",
        [SkillBalanceTable.G4] = "Assets/Art/Skills/icon_file_value.png",
        [SkillBalanceTable.H1] = "Assets/Art/Skills/icon_mutation.png",
        [SkillBalanceTable.H2] = "Assets/Art/Skills/icon_chain.png",
        [SkillBalanceTable.H3] = "Assets/Art/Skills/icon_gluttony.png"
    };

    public static SkillTreeDefinition.Node[] BuildNodes()
    {
        SkillTreeDefinition.Node[] built = new SkillTreeDefinition.Node[SkillBalanceTable.Skills.Count];
        for (int i = 0; i < built.Length; i++)
        {
            SkillDef skill = SkillBalanceTable.Skills[i];
            built[i] = new SkillTreeDefinition.Node
            {
                id = skill.Id,
                displayName = skill.Id,
                description = skill.Id,
                maxLevel = Mathf.Max(skill.Anchor ? 1 : skill.MaxLevel, skill.Anchor ? 1 : 1),
                startsOwned = skill.Anchor,
                major = skill.Major,
                anchor = skill.Anchor,
                category = skill.Category,
                effect = skill.Effect,
                minTotalPurchases = skill.MinTotal,
                position = new Vector2(skill.X, skill.Y),
                requires = skill.Links,
                unlockIds = skill.UnlockIds,
                unlockLevels = skill.UnlockLevels
            };

            if (skill.Anchor)
            {
                built[i].maxLevel = 1;
            }
        }

        return built;
    }

    public static void ApplyIcons(SkillTreeDefinition.Node[] built)
    {
#if UNITY_EDITOR
        if (built == null)
        {
            return;
        }

        foreach (SkillTreeDefinition.Node node in built)
        {
            if (node.icon != null || !iconPaths.TryGetValue(node.id, out string path))
            {
                continue;
            }

            node.icon = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
#endif
    }
}
