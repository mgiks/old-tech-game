#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class SkillPurchaseChecks
{
    private static readonly int[] Checkpoints = { 0, 5, 12, 30, 50, 70, 88, 102, 119, 120 };

    public static string Run()
    {
        List<string> errors = new();
        StringBuilder report = new();
        string productionHash = Hash(GameProgress.ProductionSavePath);
        string developmentHash = Hash(GameProgress.DevelopmentSavePath);
        string folder = Path.Combine(Application.temporaryCachePath, "skill-purchase-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);

        using (GameProgress.BeginSandbox(Path.Combine(folder, "sandbox.json")))
        {
            foreach (string error in UnlockEval.Validate())
            {
                errors.Add("validate: " + error);
            }

            CheckDenials(errors, report);
            CheckRepeatAndEffect(errors, report);
            string table = CheckOrder(PurchaseOrder.Table, errors, report);
            string spread = CheckOrder(PurchaseOrder.Spread, errors, report);
            string depth = CheckOrder(PurchaseOrder.Depth, errors, report);
            if (table == spread || table == depth || spread == depth)
            {
                errors.Add("orders did not diverge\n" + table + "\n" + spread + "\n" + depth);
            }
            else
            {
                report.AppendLine("orders diverge");
                report.AppendLine("table " + table);
                report.AppendLine("spread " + spread);
                report.AppendLine("depth " + depth);
            }
        }

        if (Hash(GameProgress.ProductionSavePath) != productionHash)
        {
            errors.Add("production save changed");
        }

        if (Hash(GameProgress.DevelopmentSavePath) != developmentHash)
        {
            errors.Add("development save changed");
        }

        report.Insert(0, (errors.Count == 0 ? "RESULT: PASS" : "RESULT: FAIL " + errors.Count) + "\n" + string.Join("\n", errors) + "\n");
        return report.ToString();
    }

    private static void CheckDenials(List<string> errors, StringBuilder report)
    {
        GameProgress.ClearSandboxProgress();
        long untouched = GameProgress.MB;
        PurchaseDenial broke = SkillPurchase.Evaluate(SkillBalanceTable.A1);
        if (broke.Block != PurchaseBlock.MissingCurrency)
        {
            errors.Add("A1 with 0 MB block " + broke.Block);
        }

        if (SkillPurchase.TryBuy(SkillBalanceTable.A1) || GameProgress.MB != untouched || GameProgress.GetSkillLevel(SkillBalanceTable.A1) != 0)
        {
            errors.Add("failed A1 purchase changed progress");
        }

        GameProgress.AddMB(EconomyMath.TotalTreeCost());
        long funded = GameProgress.MB;
        PurchaseDenial buffer = SkillPurchase.Evaluate(SkillBalanceTable.A3);
        if (buffer.Block != PurchaseBlock.RequiredSkill || buffer.SkillId != SkillBalanceTable.A1 || buffer.RequiredLevel != 2)
        {
            errors.Add("A3 denial " + buffer.Block + " " + buffer.SkillId + " " + buffer.RequiredLevel);
        }

        if (SkillPurchase.TryBuy(SkillBalanceTable.A3) || GameProgress.MB != funded)
        {
            errors.Add("locked A3 deducted MB");
        }

        PurchaseDenial mini = SkillPurchase.Evaluate(SkillBalanceTable.C1);
        if (mini.Block != PurchaseBlock.TotalThreshold || mini.RequiredPurchases != 12)
        {
            errors.Add("C1 denial " + mini.Block + " " + mini.RequiredPurchases);
        }

        if (SkillPurchase.TryBuy(SkillBalanceTable.C1) || GameProgress.MB != funded)
        {
            errors.Add("threshold purchase deducted MB");
        }

        string denial = SkillPurchase.DenialText(buffer);
        if (string.IsNullOrEmpty(denial) || denial.StartsWith("deny.", StringComparison.Ordinal))
        {
            errors.Add("denial text missing: " + denial);
        }

        report.AppendLine("denials: no-mb, skill A1 lv2, purchases 12; MB unchanged");
    }

    private static void CheckRepeatAndEffect(List<string> errors, StringBuilder report)
    {
        GameProgress.ClearSandboxProgress();
        GameProgress.AddMB(EconomyMath.TotalTreeCost());
        SkillTreeDefinition asset = AssetDatabase.LoadAssetAtPath<SkillTreeDefinition>("Assets/Data/Skill Tree.asset");
        if (asset == null || asset.Find(SkillBalanceTable.A1) == null)
        {
            errors.Add("skill tree asset missing");
            return;
        }

        long shared = asset.GetCost(asset.Find(SkillBalanceTable.A1));
        if (shared != asset.GetCost(asset.Find(SkillBalanceTable.B1)) || shared != SkillPurchase.NextCost())
        {
            errors.Add("shown price diverges from charged price");
        }

        float speed0 = GameplayQuery.ManualPrimaryRate();
        long reward0 = GameplayQuery.StandardReward();
        int minis0 = GameplayQuery.MiniCount();
        if (minis0 != 0)
        {
            errors.Add("minis before unlock " + minis0);
        }

        if (!asset.TryBuy(asset.Find(SkillBalanceTable.A1)))
        {
            errors.Add("asset TryBuy A1 failed");
            return;
        }

        float speed1 = GameplayQuery.ManualPrimaryRate();
        if (GameProgress.GetSkillLevel(SkillBalanceTable.A1) != 1 || speed1 <= speed0)
        {
            errors.Add("A1 purchase did not change download speed " + speed0 + " -> " + speed1);
        }

        if (!asset.CanBuy(asset.Find(SkillBalanceTable.A1)))
        {
            errors.Add("second A1 level is not purchasable");
        }

        string[] opening = { SkillBalanceTable.B1, SkillBalanceTable.A2, SkillBalanceTable.B2, SkillBalanceTable.A4 };
        for (int i = 0; i < opening.Length; i++)
        {
            long cost = SkillPurchase.NextCost();
            long before = GameProgress.MB;
            int level = GameProgress.GetSkillLevel(opening[i]);
            if (!SkillPurchase.TryBuy(opening[i]) || GameProgress.MB != before - cost || GameProgress.GetSkillLevel(opening[i]) != level + 1)
            {
                errors.Add("opening buy failed " + opening[i]);
                return;
            }
        }

        if (GameProgress.TotalPurchases != 5)
        {
            errors.Add("opening count " + GameProgress.TotalPurchases);
        }

        if (!SkillPurchase.Evaluate(SkillBalanceTable.A1).CanBuy)
        {
            errors.Add("repeat purchase blocked at 5");
        }

        long beforeSecond = GameProgress.MB;
        long secondCost = SkillPurchase.NextCost();
        if (!SkillPurchase.TryBuy(SkillBalanceTable.A1) || GameProgress.GetSkillLevel(SkillBalanceTable.A1) != 2 || GameProgress.MB != beforeSecond - secondCost)
        {
            errors.Add("A1 level 2 transaction failed");
            return;
        }

        float speed2 = GameplayQuery.ManualPrimaryRate();
        if (speed2 <= speed1)
        {
            errors.Add("A1 level 2 did not change speed");
        }

        if (!SkillPurchase.Evaluate(SkillBalanceTable.A3).CanBuy || !SkillPurchase.TryBuy(SkillBalanceTable.A3))
        {
            errors.Add("A3 did not unlock at A1 level 2");
            return;
        }

        int slots = GameplayQuery.DownloadSlots();
        if (!SkillPurchase.TryBuy(SkillBalanceTable.A2))
        {
            errors.Add("A2 second level failed");
            return;
        }

        if (GameplayQuery.DownloadSlots() <= slots)
        {
            errors.Add("multithreading did not add a download slot");
        }

        long plain = EconomyMath.FileReward(GameProgress.TotalPurchases, 0, 0, false);
        if (!SkillPurchase.TryBuy(SkillBalanceTable.B1))
        {
            errors.Add("B1 buy failed");
            return;
        }

        long compressed = GameplayQuery.StandardReward();
        long expected = EconomyMath.FileReward(
            GameProgress.TotalPurchases,
            GameProgress.GetSkillLevel(SkillBalanceTable.B1),
            GameProgress.GetSkillLevel(SkillBalanceTable.B2),
            false);
        if (compressed != expected || compressed <= plain)
        {
            errors.Add("compression reward " + compressed + " expected " + expected + " plain " + plain);
        }

        if (reward0 <= 0)
        {
            errors.Add("base reward was not positive");
        }

        report.AppendLine("effect: speed " + speed0.ToString("0.000") + " -> " + speed1.ToString("0.000") + " -> " + speed2.ToString("0.000")
            + "; slots " + slots + " -> " + GameplayQuery.DownloadSlots()
            + "; reward " + compressed);
    }

    private static string CheckOrder(PurchaseOrder order, List<string> errors, StringBuilder report)
    {
        GameProgress.ClearSandboxProgress();
        long budget = EconomyMath.TotalTreeCost();
        GameProgress.AddMB(budget);
        long start = GameProgress.MB;
        StringBuilder signature = new();
        int guard = 0;
        while (GameProgress.TotalPurchases < ProgressionConfig.TargetPurchases && guard++ < 130)
        {
            NoteCheckpoint(order, errors, report);
            string id = Pick(order);
            if (id == null)
            {
                errors.Add(order + " deadlock at " + GameProgress.TotalPurchases);
                break;
            }

            long cost = SkillPurchase.NextCost();
            if (cost != EconomyMath.NextPurchaseCost(GameProgress.TotalPurchases))
            {
                errors.Add(order + " price mismatch at " + GameProgress.TotalPurchases);
            }

            long before = GameProgress.MB;
            int level = GameProgress.GetSkillLevel(id);
            int minis = GameplayQuery.MiniCount();
            if (!SkillPurchase.TryBuy(id))
            {
                errors.Add(order + " TryBuy failed " + id + " at " + GameProgress.TotalPurchases);
                break;
            }

            if (GameProgress.MB != before - cost || GameProgress.GetSkillLevel(id) != level + 1)
            {
                errors.Add(order + " transaction mismatch " + id);
                break;
            }

            if (id == SkillBalanceTable.C1 && (minis != 0 || GameplayQuery.MiniCount() != 1))
            {
                errors.Add(order + " mini unlock did not spawn a mini virus");
            }

            if (signature.Length < 180)
            {
                if (signature.Length > 0)
                {
                    signature.Append(',');
                }

                signature.Append(id);
            }
        }

        NoteCheckpoint(order, errors, report);
        if (GameProgress.TotalPurchases != 120)
        {
            errors.Add(order + " finished at " + GameProgress.TotalPurchases);
        }

        long spent = 0;
        for (int i = 0; i < GameProgress.TotalPurchases; i++)
        {
            long cost = EconomyMath.NextPurchaseCost(i);
            if (spent > long.MaxValue - cost)
            {
                spent = long.MaxValue;
                break;
            }

            spent += cost;
        }

        if (GameProgress.MB != start - spent)
        {
            errors.Add(order + " balance " + GameProgress.MB + " expected " + (start - spent));
        }

        long stuck = GameProgress.MB;
        if (SkillPurchase.TryBuy(SkillBalanceTable.A1) || GameProgress.MB != stuck)
        {
            errors.Add(order + " purchase past 120");
        }

        foreach (SkillDef skill in SkillBalanceTable.Skills)
        {
            if (skill.Anchor)
            {
                continue;
            }

            if (GameProgress.GetSkillLevel(skill.Id) != skill.MaxLevel)
            {
                errors.Add(order + " short " + skill.Id + " " + GameProgress.GetSkillLevel(skill.Id) + "/" + skill.MaxLevel);
            }

            if (SkillPurchase.Evaluate(skill).CanBuy)
            {
                errors.Add(order + " still buyable " + skill.Id);
            }
        }

        return signature.ToString();
    }

    private static void NoteCheckpoint(PurchaseOrder order, List<string> errors, StringBuilder report)
    {
        int count = GameProgress.TotalPurchases;
        bool wanted = false;
        for (int i = 0; i < Checkpoints.Length; i++)
        {
            if (Checkpoints[i] == count)
            {
                wanted = true;
                break;
            }
        }

        if (!wanted)
        {
            return;
        }

        int buyable = 0;
        int sum = 0;
        bool repeat = false;
        foreach (SkillDef skill in SkillBalanceTable.Skills)
        {
            if (skill.Anchor)
            {
                continue;
            }

            int level = GameProgress.GetSkillLevel(skill.Id);
            sum += level;
            if (level > skill.MaxLevel)
            {
                errors.Add(order + " over max " + skill.Id);
            }

            if (SkillPurchase.Evaluate(skill).CanBuy)
            {
                buyable++;
                if (level > 0 && level < skill.MaxLevel)
                {
                    repeat = true;
                }
            }
        }

        if (sum != count)
        {
            errors.Add(order + " sum " + sum + " at " + count);
        }

        if (count < 120 && buyable == 0)
        {
            errors.Add(order + " nothing to buy at " + count);
        }

        if (count == 120 && buyable != 0)
        {
            errors.Add(order + " buyable at 120");
        }

        if (count == 5 && !repeat)
        {
            errors.Add(order + " no repeat purchase at 5");
        }

        if (count == 12 && (GameProgress.GetSkillLevel(SkillBalanceTable.C1) != 0 || !SkillPurchase.Evaluate(SkillBalanceTable.C1).CanBuy))
        {
            errors.Add(order + " C1 gate at 12");
        }

        ExpectGate(order, count, 30, SkillBalanceTable.C1, SkillBalanceTable.D1, errors);
        ExpectGate(order, count, 50, SkillBalanceTable.D1, SkillBalanceTable.E1, errors);
        ExpectGate(order, count, 70, SkillBalanceTable.E1, SkillBalanceTable.F1, errors);
        ExpectGate(order, count, 88, SkillBalanceTable.F1, SkillBalanceTable.G1, errors);
        if (count == 102
            && GameProgress.GetSkillLevel(SkillBalanceTable.C1) > 0
            && GameProgress.GetSkillLevel(SkillBalanceTable.D1) > 0
            && GameProgress.GetSkillLevel(SkillBalanceTable.E1) > 0
            && GameProgress.GetSkillLevel(SkillBalanceTable.H1) == 0
            && GameProgress.GetSkillLevel(SkillBalanceTable.H2) == 0
            && GameProgress.GetSkillLevel(SkillBalanceTable.H3) == 0
            && !SkillPurchase.Evaluate(SkillBalanceTable.H1).CanBuy
            && !SkillPurchase.Evaluate(SkillBalanceTable.H2).CanBuy
            && !SkillPurchase.Evaluate(SkillBalanceTable.H3).CanBuy)
        {
            errors.Add(order + " synergies still closed at 102");
        }

        long reward = GameplayQuery.StandardReward();
        long expected = EconomyMath.FileReward(count, GameProgress.GetSkillLevel(SkillBalanceTable.B1), GameProgress.GetSkillLevel(SkillBalanceTable.B2), false);
        if (reward != expected)
        {
            errors.Add(order + " reward mismatch at " + count);
        }

        report.AppendLine(order + " @" + count + ": buyable=" + buyable + " mb=" + GameProgress.MB + " next=" + SkillPurchase.NextCost() + " reward=" + reward + " c1=" + GameProgress.GetSkillLevel(SkillBalanceTable.C1));
    }

    private static void ExpectGate(PurchaseOrder order, int count, int at, string parent, string child, List<string> errors)
    {
        if (count != at || GameProgress.GetSkillLevel(parent) <= 0 || GameProgress.GetSkillLevel(child) > 0)
        {
            return;
        }

        if (!SkillPurchase.Evaluate(child).CanBuy)
        {
            errors.Add(order + " " + child + " stayed locked at " + at);
        }
    }

    private static string Pick(PurchaseOrder order)
    {
        SkillDef best = null;
        foreach (SkillDef skill in SkillBalanceTable.Skills)
        {
            if (!SkillPurchase.Evaluate(skill).CanBuy)
            {
                continue;
            }

            if (best == null || Prefer(order, skill, best))
            {
                best = skill;
            }
        }

        return best == null ? null : best.Id;
    }

    private static bool Prefer(PurchaseOrder order, SkillDef candidate, SkillDef current)
    {
        int candidateLevel = GameProgress.GetSkillLevel(candidate.Id);
        int currentLevel = GameProgress.GetSkillLevel(current.Id);
        if (order == PurchaseOrder.Spread)
        {
            if (candidateLevel != currentLevel)
            {
                return candidateLevel < currentLevel;
            }

            return string.CompareOrdinal(candidate.Id, current.Id) < 0;
        }

        if (order == PurchaseOrder.Depth)
        {
            if (candidate.Category != current.Category)
            {
                return candidate.Category > current.Category;
            }

            if (candidateLevel != currentLevel)
            {
                return candidateLevel > currentLevel;
            }

            return string.CompareOrdinal(candidate.Id, current.Id) > 0;
        }

        return false;
    }

    private static string Hash(string path)
    {
        if (!File.Exists(path))
        {
            return "missing";
        }

        using SHA256 sha = SHA256.Create();
        byte[] hash = sha.ComputeHash(File.ReadAllBytes(path));
        StringBuilder text = new(hash.Length * 2);
        for (int i = 0; i < hash.Length; i++)
        {
            text.Append(hash[i].ToString("x2"));
        }

        return text.ToString();
    }

    private enum PurchaseOrder
    {
        Table,
        Spread,
        Depth
    }
}
#endif
