using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

public enum SpendStrategy
{
    Balanced,
    Economy,
    Automation
}

public static class BalanceSimulator
{
    private static readonly int[] Milestones = { 0, 12, 20, 30, 40, 50, 60, 70, 80, 88, 100, 102, 110, 119, 120 };

    public static string Run()
    {
        StringBuilder report = new();
        List<string> errors = UnlockEval.Validate();
        report.AppendLine(errors.Count == 0 ? "VALIDATION: PASS" : "VALIDATION: FAIL");
        foreach (string error in errors)
        {
            report.AppendLine("  " + error);
        }

        report.AppendLine("NODES " + SkillBalanceTable.PurchasableCount());
        report.AppendLine("LEVEL_SUM " + SkillBalanceTable.MaxLevelSum());
        report.AppendLine("TOTAL_COST " + EconomyMath.TotalTreeCost());
        foreach (int p in new[] { 0, 20, 40, 60, 80, 100, 119 })
        {
            report.AppendLine("PRICE P" + p + " " + EconomyMath.NextPurchaseCost(p));
            report.AppendLine("REWARD P" + p + " " + EconomyMath.BaseFileReward(p));
        }

        long sample = EconomyMath.FileReward(0, 8, 5, false);
        report.AppendLine("MAX_MULT_REWARD_P0 " + sample);
        report.AppendLine("COMPRESSION_8 " + EffectMath.Compression(8).ToString("0.0000", CultureInfo.InvariantCulture));
        report.AppendLine("OPTIMIZATION_5 " + EffectMath.Optimization(5).ToString("0.0000", CultureInfo.InvariantCulture));

        foreach (SpendStrategy strategy in new[] { SpendStrategy.Balanced, SpendStrategy.Economy, SpendStrategy.Automation })
        {
            report.AppendLine("STRATEGY " + strategy);
            report.Append(SimulateStrategy(strategy));
        }

        return report.ToString();
    }

    private static string SimulateStrategy(SpendStrategy strategy)
    {
        List<string> order = PurchaseOrder(strategy);
        StringBuilder text = new();
        if (order.Count != 120)
        {
            text.AppendLine("  ORDER " + order.Count + " (expected 120)");
            return text.ToString();
        }

        Dictionary<string, int> levels = FreshLevels();
        long mb = 0;
        int runs = 0;
        double seconds = 0;
        int milestoneIndex = 0;
        AppendMilestone(text, 0, levels, mb, runs, seconds, SimulateRun(levels, 11), 0, 0);
        milestoneIndex = 1;
        HashSet<string> seenUnlocks = new();
        int[] seeds = { 11, 29, 47 };
        while (Sum(levels) < 120 && runs < 600)
        {
            RunResult[] trials = new RunResult[seeds.Length];
            long incomeSum = 0;
            double secondSum = 0;
            for (int i = 0; i < seeds.Length; i++)
            {
                trials[i] = SimulateRun(levels, seeds[i] + runs * 17);
                incomeSum += trials[i].Earned;
                secondSum += trials[i].Seconds;
            }

            long income = incomeSum / seeds.Length;
            double runSeconds = secondSum / seeds.Length;
            if (income <= 0 && mb < EconomyMath.NextPurchaseCost(Sum(levels)))
            {
                text.AppendLine("  STUCK runs=" + runs + " P=" + Sum(levels));
                break;
            }

            mb = EconomyMath.AddClamped(mb, income);
            seconds += runSeconds;
            runs++;
            RunResult sample = trials[0];
            while (Sum(levels) < 120)
            {
                int p = Sum(levels);
                long cost = EconomyMath.NextPurchaseCost(p);
                if (cost <= 0 || mb < cost)
                {
                    break;
                }

                string id = order[p];
                mb -= cost;
                levels[id]++;
                NoteUnlock(id, levels, seenUnlocks, text, runs, seconds);
                while (milestoneIndex < Milestones.Length && Sum(levels) >= Milestones[milestoneIndex])
                {
                    AppendMilestone(text, Milestones[milestoneIndex], levels, mb, runs, seconds, sample, runSeconds, income);
                    milestoneIndex++;
                }
            }
        }

        text.AppendLine("  FINAL_P " + Sum(levels) + " RUNS " + runs + " MINUTES " + (seconds / 60d).ToString("0.00", CultureInfo.InvariantCulture));
        return text.ToString();
    }

    private static void NoteUnlock(string id, Dictionary<string, int> levels, HashSet<string> seen, StringBuilder text, int runs, double seconds)
    {
        SkillDef skill = SkillBalanceTable.Find(id);
        if (skill == null || !skill.Major || levels[id] != 1 || !seen.Add(id))
        {
            return;
        }

        if (skill.Effect is SkillEffectId.MiniUnlock or SkillEffectId.ChainUnlock or SkillEffectId.OverloadUnlock
            or SkillEffectId.InfectUnlock or SkillEffectId.FragmentUnlock or SkillEffectId.SynergyManualMini
            or SkillEffectId.SynergyMiniChain or SkillEffectId.SynergyCascade)
        {
            text.AppendLine("  UNLOCK " + id + " P=" + Sum(levels) + " runs=" + runs + " min=" + (seconds / 60d).ToString("0.00", CultureInfo.InvariantCulture));
        }
    }

    private static void AppendMilestone(StringBuilder text, int milestone, Dictionary<string, int> levels, long mb, int runs, double seconds, RunResult sample, double runSeconds, long income)
    {
        int p = Sum(levels);
        long cost = EconomyMath.NextPurchaseCost(p >= 120 ? 119 : p);
        text.AppendLine(
            "  P" + milestone
            + " actual=" + p
            + " runs=" + runs
            + " min=" + (seconds / 60d).ToString("0.00", CultureInfo.InvariantCulture)
            + " mb=" + mb
            + " next=" + (p >= 120 ? 0 : EconomyMath.NextPurchaseCost(p))
            + " runIncome=" + income
            + " runSec=" + runSeconds.ToString("0.00", CultureInfo.InvariantCulture)
            + " files=" + sample.Files
            + " pcs=" + sample.Computers
            + " reward=" + EconomyMath.BaseFileReward(Math.Min(p, 119))
            + " systems=" + Systems(levels));
        if (milestone == 120)
        {
            text.AppendLine("  costAt119=" + cost);
        }
    }

    private static string Systems(Dictionary<string, int> levels)
    {
        string[] ids = { SkillBalanceTable.C1, SkillBalanceTable.D1, SkillBalanceTable.E1, SkillBalanceTable.F1, SkillBalanceTable.G1, SkillBalanceTable.H1 };
        StringBuilder text = new();
        foreach (string id in ids)
        {
            if (levels[id] > 0)
            {
                if (text.Length > 0)
                {
                    text.Append(',');
                }

                text.Append(id);
            }
        }

        return text.Length == 0 ? "-" : text.ToString();
    }

    private static List<string> PurchaseOrder(SpendStrategy strategy)
    {
        Dictionary<string, int> levels = FreshLevels();
        List<string> order = new();
        while (order.Count < 200)
        {
            SkillDef next = Pick(strategy, levels, long.MaxValue);
            if (next == null)
            {
                break;
            }

            levels[next.Id]++;
            order.Add(next.Id);
        }

        return order;
    }

    private static SkillDef Pick(SpendStrategy strategy, Dictionary<string, int> levels, long mb)
    {
        int total = Sum(levels);
        long cost = EconomyMath.NextPurchaseCost(total);
        if (total >= 120 || (cost > 0 && mb < cost))
        {
            return null;
        }

        SkillDef best = null;
        double bestScore = double.NegativeInfinity;
        foreach (SkillDef skill in SkillBalanceTable.Skills)
        {
            if (skill.Anchor || levels[skill.Id] >= skill.MaxLevel)
            {
                continue;
            }

            if (!UnlockEval.Meets(skill, id => levels[id], total))
            {
                continue;
            }

            double score = Score(strategy, skill, levels);
            if (score > bestScore || (Math.Abs(score - bestScore) < 0.0000001d && (best == null || string.CompareOrdinal(skill.Id, best.Id) < 0)))
            {
                best = skill;
                bestScore = score;
            }
        }

        return best;
    }

    private static double Score(SpendStrategy strategy, SkillDef skill, Dictionary<string, int> levels)
    {
        int level = levels[skill.Id];
        double fill = skill.MaxLevel <= 0 ? 1d : (double)level / skill.MaxLevel;
        double categoryFill = CategoryFill(skill.Category, levels);
        double score = strategy switch
        {
            SpendStrategy.Economy => EconomyWeight(skill.Effect) - fill,
            SpendStrategy.Automation => AutomationWeight(skill.Effect) - fill,
            _ => 20d - categoryFill * 12d - fill
        };

        if (skill.Major && level == 0 && skill.MaxLevel == 1)
        {
            score += strategy == SpendStrategy.Balanced ? 3d : 1d;
        }

        return score;
    }

    private static double EconomyWeight(SkillEffectId effect)
    {
        return effect switch
        {
            SkillEffectId.Compression => 100d,
            SkillEffectId.Optimization => 92d,
            SkillEffectId.ValuableFiles => 84d,
            SkillEffectId.FragmentValue => 48d,
            SkillEffectId.FragmentChance => 44d,
            SkillEffectId.PassiveProduction => 40d,
            SkillEffectId.DownloadSpeed => 36d,
            SkillEffectId.MiniUnlock or SkillEffectId.ChainUnlock or SkillEffectId.OverloadUnlock or SkillEffectId.InfectUnlock or SkillEffectId.FragmentUnlock => 34d,
            _ => 12d
        };
    }

    private static double AutomationWeight(SkillEffectId effect)
    {
        return effect switch
        {
            SkillEffectId.MiniUnlock or SkillEffectId.Population or SkillEffectId.MoveSpeed or SkillEffectId.FeedSpeed or SkillEffectId.Targeting => 100d,
            SkillEffectId.ChainUnlock or SkillEffectId.ChainChance or SkillEffectId.ChainRange or SkillEffectId.ChainDepth or SkillEffectId.ChainBranch => 90d,
            SkillEffectId.SynergyMiniChain => 82d,
            SkillEffectId.SynergyManualMini => 70d,
            SkillEffectId.OverloadUnlock or SkillEffectId.OverloadCharge or SkillEffectId.OverloadDuration or SkillEffectId.OverloadPower => 64d,
            SkillEffectId.SynergyCascade => 60d,
            SkillEffectId.DownloadSpeed => 42d,
            SkillEffectId.Multithreading => 38d,
            SkillEffectId.DownloadBurst => 34d,
            _ => 14d
        };
    }

    private static double CategoryFill(SkillCategory category, Dictionary<string, int> levels)
    {
        int owned = 0;
        int max = 0;
        foreach (SkillDef skill in SkillBalanceTable.Skills)
        {
            if (skill.Category != category || skill.Anchor)
            {
                continue;
            }

            owned += levels[skill.Id];
            max += skill.MaxLevel;
        }

        return max <= 0 ? 0d : (double)owned / max;
    }

    private struct RunResult
    {
        public long Earned;
        public double Seconds;
        public int Files;
        public int Computers;
    }

    private struct ChainJob
    {
        public int Chain;
        public int Depth;
        public int Generation;
        public double Progress;
    }

    private static RunResult SimulateRun(Dictionary<string, int> levels, int seed)
    {
        int Level(string id) => levels.TryGetValue(id, out int value) ? value : 0;
        Random rng = new(seed);
        int p = Sum(levels);
        int slots = EffectMath.DownloadSlots(Level(SkillBalanceTable.A2));
        int minis = EffectMath.MiniCount(Level(SkillBalanceTable.C1), Level(SkillBalanceTable.C2));
        double[] manual = new double[slots];
        bool[] manualOn = new bool[slots];
        double[] travel = new double[minis];
        double[] feed = new double[minis];
        bool[] feeding = new bool[minis];
        List<ChainJob> chains = new();
        Dictionary<int, int> budget = new();
        Dictionary<int, double> cascadePaid = new();
        HashSet<long> paidGenerations = new();
        int nextChain = 1;
        bool jumped = false;

        int free = ProgressionConfig.StartingFiles;
        int busy = 0;
        int infected = 0;
        double spawn = 0;
        double timeLeft = ProgressionConfig.BaseRunSeconds;
        double wall = 0;
        int computer = 0;
        int eatenHere = 0;
        int files = 0;
        long earned = 0;
        double passiveBank = 0;
        double fragmentBank = 0;
        double charge = 0;
        double overloadLeft = 0;
        int burstCount = 0;
        double burstLeft = 0;
        double synergyLeft = 0;
        double spread = 0;

        double Distance()
        {
            return Level(SkillBalanceTable.C5) switch
            {
                >= 4 => 2.8d,
                3 => 3.1d,
                2 => 3.3d,
                1 => 3.6d,
                _ => 4.4d
            };
        }

        void ClearDesktop()
        {
            free = ProgressionConfig.StartingFiles;
            busy = 0;
            infected = 0;
            Array.Clear(manual, 0, manual.Length);
            Array.Clear(manualOn, 0, manualOn.Length);
            Array.Clear(feed, 0, feed.Length);
            Array.Clear(feeding, 0, feeding.Length);
            chains.Clear();
            for (int i = 0; i < minis; i++)
            {
                travel[i] = Distance() / Math.Max(0.2d, MoveSpeed(synergyLeft));
            }
        }

        double MoveSpeed(double synergy)
        {
            double bonus = synergy > 0d ? EffectMath.SynergySpeedMultiplier(Level(SkillBalanceTable.H1)) : 1d;
            return EffectMath.MiniMoveSpeed(Level(SkillBalanceTable.C3)) * bonus;
        }

        double OverloadMult()
        {
            return overloadLeft > 0d && Level(SkillBalanceTable.E1) > 0
                ? EffectMath.OverloadPower(Level(SkillBalanceTable.E4))
                : 1d;
        }

        long StandardReward()
        {
            return EconomyMath.FileReward(p, Level(SkillBalanceTable.B1), Level(SkillBalanceTable.B2), false);
        }

        void Pay(long amount)
        {
            if (amount > 0)
            {
                earned = EconomyMath.AddClamped(earned, amount);
            }
        }

        void Fragments(long sourceReward)
        {
            if (Level(SkillBalanceTable.G1) <= 0)
            {
                return;
            }

            if (rng.NextDouble() >= EffectMath.FragmentChance(1, Level(SkillBalanceTable.G2)))
            {
                return;
            }

            double value = sourceReward * EffectMath.FragmentCount(Level(SkillBalanceTable.G3)) * EffectMath.FragmentFraction(Level(SkillBalanceTable.G4)) * 0.8d;
            fragmentBank += value;
        }

        void Charge(double amount, int chainId, int generation, bool cascade)
        {
            if (amount <= 0d || Level(SkillBalanceTable.E1) <= 0 || overloadLeft > 0d)
            {
                return;
            }

            if (cascade)
            {
                long key = ((long)chainId << 8) ^ (uint)generation;
                if (!paidGenerations.Add(key))
                {
                    return;
                }

                cascadePaid.TryGetValue(chainId, out double paid);
                double room = 3d - paid;
                if (room <= 0d)
                {
                    return;
                }

                amount = Math.Min(amount, room);
                cascadePaid[chainId] = paid + amount;
            }

            charge += amount;
            double required = EffectMath.OverloadRequired(Level(SkillBalanceTable.E2));
            if (charge >= required)
            {
                charge = 0d;
                overloadLeft = EffectMath.OverloadDuration(Level(SkillBalanceTable.E3));
            }
        }

        void FinishFile(bool manual, bool mini, int chainId, int depth, int generation)
        {
            busy = Math.Max(0, busy - 1);
            bool valuable = rng.NextDouble() < EffectMath.ValuableChance(Level(SkillBalanceTable.B3));
            long reward = EconomyMath.FileReward(p, Level(SkillBalanceTable.B1), Level(SkillBalanceTable.B2), valuable);
            Pay(reward);
            files++;
            eatenHere++;
            Fragments(reward);
            Charge(1d, chainId, generation, false);
            if (chainId > 0)
            {
                Charge(EffectMath.CascadeChargeBonus(Level(SkillBalanceTable.H3)), chainId, generation, true);
            }

            if (manual)
            {
                burstCount++;
                synergyLeft = Level(SkillBalanceTable.H1) > 0 ? 2d : synergyLeft;
                if (Level(SkillBalanceTable.A4) > 0 && burstCount >= 3)
                {
                    burstCount = 0;
                    burstLeft = 3d;
                }
            }

            if (manual || mini)
            {
                double chance = EffectMath.ChainChance(Level(SkillBalanceTable.D1), Level(SkillBalanceTable.D2));
                if (mini)
                {
                    chance *= EffectMath.MiniChainChanceScale(Level(SkillBalanceTable.H2));
                }

                if (chance > 0d && rng.NextDouble() < chance)
                {
                    int id = nextChain++;
                    budget[id] = ProgressionConfig.ChainEventBudget;
                    StartChain(id, 1, 1);
                }
            }
            else if (chainId > 0 && depth < EffectMath.ChainDepth(Level(SkillBalanceTable.D4)))
            {
                StartChain(chainId, depth + 1, generation + 1);
            }

            if (eatenHere >= Required(computer))
            {
                computer++;
                eatenHere = 0;
                wall += 0.8d;
                timeLeft = Math.Min(ProgressionConfig.MaxRemainingSeconds, timeLeft + ProgressionConfig.ComputerTimeBonus);
                jumped = true;
                ClearDesktop();
            }
        }

        void StartChain(int chainId, int depth, int generation)
        {
            if (!budget.TryGetValue(chainId, out int left))
            {
                return;
            }

            int branches = EffectMath.ChainBranching(Level(SkillBalanceTable.D5));
            for (int i = 0; i < branches && left > 0 && free > 0; i++)
            {
                free--;
                busy++;
                left--;
                chains.Add(new ChainJob { Chain = chainId, Depth = depth, Generation = generation, Progress = 0d });
            }

            budget[chainId] = left;
        }

        ClearDesktop();
        const double step = 0.05d;
        while (timeLeft > 0d && computer < ProgressionConfig.ComputerCount)
        {
            double dt = Math.Min(step, timeLeft);
            double overload = OverloadMult();
            double burst = burstLeft > 0d ? EffectMath.BurstMultiplier(Level(SkillBalanceTable.A4)) : 1d;
            double synergy = synergyLeft > 0d ? EffectMath.SynergySpeedMultiplier(Level(SkillBalanceTable.H1)) : 1d;
            spawn += dt;
            while (spawn >= ProgressionConfig.SpawnIntervalSeconds && free + busy + infected < ProgressionConfig.MaxFilesOnDesktop)
            {
                spawn -= ProgressionConfig.SpawnIntervalSeconds;
                free++;
            }

            double primary = EffectMath.ManualSpeed(Level(SkillBalanceTable.A1)) * burst * overload / ProgressionConfig.BaseDownloadSeconds;
            for (int i = 0; i < slots; i++)
            {
                if (!manualOn[i])
                {
                    bool allowed = i == 0 || manualOn[0];
                    if (allowed && free > 0)
                    {
                        free--;
                        busy++;
                        manualOn[i] = true;
                        manual[i] = 0d;
                    }
                }

                if (!manualOn[i])
                {
                    continue;
                }

                double rate = i == 0 ? primary : primary * ProgressionConfig.SecondarySpeedFactor * 0.75d;
                manual[i] += rate * dt;
                if (manual[i] >= 1d)
                {
                    manualOn[i] = false;
                    manual[i] = 0d;
                    FinishFile(true, false, 0, 0, 0);
                    if (jumped || computer >= ProgressionConfig.ComputerCount)
                    {
                        break;
                    }
                }
            }

            if (jumped)
            {
                jumped = false;
            }

            double feedRate = EffectMath.MiniFeedMultiplier(Level(SkillBalanceTable.C4)) * overload * synergy / ProgressionConfig.BaseDownloadSeconds;
            for (int i = 0; i < minis; i++)
            {
                if (!feeding[i])
                {
                    travel[i] -= dt;
                    if (travel[i] > 0d)
                    {
                        continue;
                    }

                    if (free <= 0)
                    {
                        travel[i] = 0.15d;
                        continue;
                    }

                    free--;
                    busy++;
                    feeding[i] = true;
                    feed[i] = 0d;
                }

                feed[i] += feedRate * dt;
                if (feed[i] >= 1d)
                {
                    feeding[i] = false;
                    travel[i] = Distance() / Math.Max(0.2d, MoveSpeed(synergyLeft));
                    FinishFile(false, true, 0, 0, 0);
                    if (jumped)
                    {
                        break;
                    }
                }
            }

            if (jumped)
            {
                jumped = false;
            }

            double chainRate = ProgressionConfig.ChainSpeedFactor * overload / ProgressionConfig.BaseDownloadSeconds;
            for (int i = chains.Count - 1; i >= 0; i--)
            {
                ChainJob job = chains[i];
                job.Progress += chainRate * dt;
                if (job.Progress >= 1d)
                {
                    chains.RemoveAt(i);
                    FinishFile(false, false, job.Chain, job.Depth, job.Generation);
                    if (jumped)
                    {
                        break;
                    }
                }
                else
                {
                    chains[i] = job;
                }
            }

            if (jumped)
            {
                jumped = false;
            }

            int capacity = EffectMath.InfectedCapacity(Level(SkillBalanceTable.F1), Level(SkillBalanceTable.F3));
            double interval = EffectMath.SpreadInterval(Level(SkillBalanceTable.F4));
            if (capacity > 0 && interval > 0d && infected < capacity && free > 0)
            {
                spread += dt;
                if (spread >= interval)
                {
                    spread = 0d;
                    free--;
                    infected++;
                }
            }

            if (free == 0 && infected > 0 && busy == 0)
            {
                infected--;
                Pay(StandardReward());
                files++;
                eatenHere++;
                if (eatenHere >= Required(computer))
                {
                    computer++;
                    eatenHere = 0;
                    wall += 0.8d;
                    timeLeft = Math.Min(ProgressionConfig.MaxRemainingSeconds, timeLeft + ProgressionConfig.ComputerTimeBonus);
                    ClearDesktop();
                }
            }

            if (infected > 0)
            {
                passiveBank += infected * StandardReward() * EffectMath.PassiveFractionPerSecond(Level(SkillBalanceTable.F2)) * dt;
            }

            long passivePay = (long)Math.Floor(passiveBank);
            if (passivePay > 0)
            {
                passiveBank -= passivePay;
                Pay(passivePay);
            }

            long fragmentPay = (long)Math.Floor(fragmentBank);
            if (fragmentPay > 0)
            {
                fragmentBank -= fragmentPay;
                Pay(fragmentPay);
            }

            timeLeft -= dt;
            wall += dt;
            if (burstLeft > 0d)
            {
                burstLeft -= dt;
            }

            if (synergyLeft > 0d)
            {
                synergyLeft -= dt;
            }

            if (overloadLeft > 0d)
            {
                overloadLeft -= dt;
            }
        }

        return new RunResult
        {
            Earned = earned,
            Seconds = wall,
            Files = files,
            Computers = computer
        };
    }

    private static int Required(int computerIndex)
    {
        return (int)Math.Ceiling(ProgressionConfig.BaseFilesPerComputer * Math.Pow(ProgressionConfig.FilesPerComputerGrowth, computerIndex));
    }

    private static Dictionary<string, int> FreshLevels()
    {
        Dictionary<string, int> levels = new();
        foreach (SkillDef skill in SkillBalanceTable.Skills)
        {
            levels[skill.Id] = 0;
        }

        return levels;
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
