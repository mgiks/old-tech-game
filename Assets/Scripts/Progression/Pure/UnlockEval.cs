using System;
using System.Collections.Generic;

public static class UnlockEval
{
    public static bool Meets(SkillDef skill, Func<string, int> level, int totalPurchases)
    {
        if (skill == null || skill.Anchor)
        {
            return false;
        }

        if (totalPurchases < skill.MinTotal)
        {
            return false;
        }

        for (int i = 0; i < skill.UnlockIds.Length; i++)
        {
            int need = i < skill.UnlockLevels.Length ? skill.UnlockLevels[i] : 1;
            if (level(skill.UnlockIds[i]) < need)
            {
                return false;
            }
        }

        for (int i = 0; i < skill.Systems.Length; i++)
        {
            if (!SystemUnlocked(skill.Systems[i], level))
            {
                return false;
            }
        }

        return true;
    }

    public static bool SystemUnlocked(GameSystemId system, Func<string, int> level)
    {
        string id = SkillBalanceTable.SystemUnlockId(system);
        return string.IsNullOrEmpty(id) || level(id) > 0;
    }

    public static SkillVisibility Visibility(SkillDef skill, Func<string, int> level, int totalPurchases, long mb, long nextCost)
    {
        if (skill == null)
        {
            return SkillVisibility.Hidden;
        }

        if (skill.Anchor)
        {
            return SkillVisibility.Maxed;
        }

        int owned = level(skill.Id);
        if (skill.MaxLevel > 0 && owned >= skill.MaxLevel)
        {
            return SkillVisibility.Maxed;
        }

        if (owned > 0)
        {
            return SkillVisibility.Partial;
        }

        if (Meets(skill, level, totalPurchases))
        {
            return mb >= nextCost && nextCost > 0 ? SkillVisibility.Purchasable : SkillVisibility.Available;
        }

        if (!SkillPrereqsMet(skill, level, out int unmet, out bool oneShort, out string unmetId, out int unmetHave))
        {
            if (unmet == 1 && oneShort && unmetId != null)
            {
                if (unmetHave > 0)
                {
                    return SkillVisibility.Locked;
                }

                SkillDef parent = SkillBalanceTable.Find(unmetId);
                if (parent != null && (parent.Anchor || Meets(parent, level, totalPurchases) || level(parent.Id) > 0))
                {
                    return SkillVisibility.Silhouette;
                }
            }

            return SkillVisibility.Hidden;
        }

        if (totalPurchases < skill.MinTotal)
        {
            return totalPurchases + ProgressionConfig.PreviewLead >= skill.MinTotal
                ? SkillVisibility.Silhouette
                : SkillVisibility.Hidden;
        }

        return SkillVisibility.Locked;
    }

    public static List<string> Validate()
    {
        List<string> errors = new();
        HashSet<string> ids = new();
        foreach (SkillDef skill in SkillBalanceTable.Skills)
        {
            if (string.IsNullOrEmpty(skill.Id) || !ids.Add(skill.Id))
            {
                errors.Add("duplicate or empty id: " + skill.Id);
            }

            if (skill.UnlockIds.Length != skill.UnlockLevels.Length)
            {
                errors.Add(skill.Id + " unlock array mismatch");
            }

            foreach (string link in skill.Links)
            {
                if (SkillBalanceTable.Find(link) == null)
                {
                    errors.Add(skill.Id + " missing link " + link);
                }
            }

            foreach (string unlock in skill.UnlockIds)
            {
                if (SkillBalanceTable.Find(unlock) == null)
                {
                    errors.Add(skill.Id + " missing prereq " + unlock);
                }
            }
        }

        if (SkillBalanceTable.PurchasableCount() != 32)
        {
            errors.Add("purchasable count " + SkillBalanceTable.PurchasableCount());
        }

        if (SkillBalanceTable.MaxLevelSum() != 120)
        {
            errors.Add("max level sum " + SkillBalanceTable.MaxLevelSum());
        }

        errors.AddRange(FindCycles());
        errors.AddRange(FindLayoutOverlaps(170f));
        errors.AddRange(SimulateReachability());
        return errors;
    }

    private static bool SkillPrereqsMet(SkillDef skill, Func<string, int> level, out int unmet, out bool oneShort, out string unmetId, out int unmetHave)
    {
        unmet = 0;
        oneShort = true;
        unmetId = null;
        unmetHave = 0;
        for (int i = 0; i < skill.UnlockIds.Length; i++)
        {
            int need = skill.UnlockLevels[i];
            int have = level(skill.UnlockIds[i]);
            if (have >= need)
            {
                continue;
            }

            unmet++;
            unmetId = skill.UnlockIds[i];
            unmetHave = have;
            if (have < need - 1)
            {
                oneShort = false;
            }
        }

        if (unmet != 1)
        {
            oneShort = false;
        }

        return unmet == 0;
    }

    private static List<string> FindCycles()
    {
        List<string> errors = new();
        Dictionary<string, int> color = new();
        foreach (SkillDef skill in SkillBalanceTable.Skills)
        {
            if (Visit(skill.Id, color, errors))
            {
                break;
            }
        }

        return errors;
    }

    private static bool Visit(string id, Dictionary<string, int> color, List<string> errors)
    {
        if (id == null)
        {
            return false;
        }

        color.TryGetValue(id, out int state);
        if (state == 1)
        {
            errors.Add("cycle at " + id);
            return true;
        }

        if (state == 2)
        {
            return false;
        }

        color[id] = 1;
        SkillDef skill = SkillBalanceTable.Find(id);
        if (skill != null)
        {
            foreach (string unlock in skill.UnlockIds)
            {
                if (Visit(unlock, color, errors))
                {
                    return true;
                }
            }
        }

        color[id] = 2;
        return false;
    }

    private static List<string> FindLayoutOverlaps(float minDistance)
    {
        List<string> errors = new();
        IReadOnlyList<SkillDef> skills = SkillBalanceTable.Skills;
        for (int i = 0; i < skills.Count; i++)
        {
            for (int j = i + 1; j < skills.Count; j++)
            {
                float dx = skills[i].X - skills[j].X;
                float dy = skills[i].Y - skills[j].Y;
                float distance = (float)Math.Sqrt(dx * dx + dy * dy);
                if (distance < minDistance)
                {
                    errors.Add("overlap " + skills[i].Id + " " + skills[j].Id + " d=" + distance.ToString("0.0"));
                }
            }
        }

        return errors;
    }

    private static List<string> SimulateReachability()
    {
        List<string> errors = new();
        Dictionary<string, int> levels = new();
        foreach (SkillDef skill in SkillBalanceTable.Skills)
        {
            levels[skill.Id] = 0;
        }

        int guard = 0;
        while (guard++ < 200)
        {
            SkillDef next = null;
            int total = Sum(levels);
            foreach (SkillDef skill in SkillBalanceTable.Skills)
            {
                if (skill.Anchor || skill.MaxLevel <= 0 || levels[skill.Id] >= skill.MaxLevel)
                {
                    continue;
                }

                if (Meets(skill, id => levels.TryGetValue(id, out int value) ? value : 0, total))
                {
                    next = skill;
                    break;
                }
            }

            if (next == null)
            {
                break;
            }

            levels[next.Id]++;
        }

        int reached = Sum(levels);
        if (reached != 120)
        {
            errors.Add("reachable purchases " + reached + "/120");
            foreach (SkillDef skill in SkillBalanceTable.Skills)
            {
                if (!skill.Anchor && levels[skill.Id] < skill.MaxLevel)
                {
                    errors.Add("short " + skill.Id + " " + levels[skill.Id] + "/" + skill.MaxLevel);
                }
            }
        }

        return errors;
    }

    private static int Sum(Dictionary<string, int> levels)
    {
        int total = 0;
        foreach (SkillDef skill in SkillBalanceTable.Skills)
        {
            if (!skill.Anchor)
            {
                total += levels[skill.Id];
            }
        }

        return total;
    }
}
