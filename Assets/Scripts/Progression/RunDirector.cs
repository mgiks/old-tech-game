using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class RunDirector : MonoBehaviour
{
    private sealed class Mini
    {
        public GameObject Body;
        public SpriteRenderer Renderer;
        public DesktopIcon Target;
        public int State;
        public float Idle;
        public Vector2 Home;
    }

    private sealed class ChainJob
    {
        public int Id;
        public int Depth;
        public int Generation;
        public DesktopIcon Icon;
        public DesktopIcon From;
        public float LineLife;
        public Transform Line;
    }

    private sealed class Shard
    {
        public Transform Body;
        public SpriteRenderer Renderer;
        public Vector2 Velocity;
        public float Life;
        public double Value;
        public bool Active;
    }

    private const int MiniIdle = 0;
    private const int MiniSearch = 1;
    private const int MiniMove = 2;
    private const int MiniDownload = 3;

    public static RunDirector Instance { get; private set; }

    public float BurstMultiplier { get; private set; } = 1f;
    public float OverloadMultiplier { get; private set; } = 1f;
    public float SynergyMultiplier { get; private set; } = 1f;

    private Virus virus;
    private EatFeedback feedback;
    private HoverDownloadFeel feel;
    private readonly List<Mini> minis = new();
    private readonly List<ChainJob> jobs = new();
    private readonly List<ChainJob> pending = new();
    private readonly Dictionary<int, int> chainBudget = new();
    private readonly Dictionary<int, double> cascadePaid = new();
    private readonly HashSet<long> cascadeGenerations = new();
    private readonly List<Shard> shards = new();
    private readonly List<DesktopIcon> scratch = new();

    private Sprite pixel;
    private Sprite virusSprite;
    private int nextChain = 1;
    private int manualStreak;
    private float burstLeft;
    private float synergyLeft;
    private float charge;
    private float overloadLeft;
    private bool overloadOn;
    private float spreadTimer;
    private double passiveBank;
    private double shardBank;
    private int handling;
    private Image meterBack;
    private Image meterFill;
    private TextMeshProUGUI meterLabel;
    private bool stopped;

    public void Init(Virus owner, EatFeedback eatFeedback, Image timer)
    {
        virus = owner;
        feedback = eatFeedback;
        virusSprite = owner != null ? owner.GetComponent<SpriteRenderer>()?.sprite : null;
        FileHoverDownload hover = FindFirstObjectByType<FileHoverDownload>();
        if (hover != null)
        {
            feel = hover.Feel;
        }

        BuildMeter(timer);
        EnsureMinis();
    }

    void OnEnable()
    {
        Instance = this;
    }

    void OnDisable()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    void OnDestroy()
    {
        Shutdown();
    }

    public void Shutdown()
    {
        stopped = true;
        jobs.Clear();
        pending.Clear();
        for (int i = 0; i < minis.Count; i++)
        {
            if (minis[i].Body != null)
            {
                Destroy(minis[i].Body);
            }
        }

        minis.Clear();
        for (int i = 0; i < shards.Count; i++)
        {
            if (shards[i].Body != null)
            {
                Destroy(shards[i].Body.gameObject);
            }
        }

        shards.Clear();
    }

    void Update()
    {
        if (stopped || GamePause.IsPaused)
        {
            return;
        }

        float dt = Time.deltaTime;
        RefreshBuffs(dt);
        LaunchReadyFiles();
        TickMinis(dt);
        TickChains(dt);
        FlushChains();
        TickInfection(dt);
        TickShards(dt);
        UpdateMeter();
    }

    public void HandleComplete(DesktopIcon icon, CompletionSource source)
    {
        if (icon == null || icon.DirectorHandled || stopped)
        {
            return;
        }

        icon.DirectorHandled = true;
        if (icon.ChainId > 0)
        {
            source = CompletionSource.Chain;
        }

        if (handling > 6)
        {
            return;
        }

        handling++;
        GameEvents.RaiseDownloadCompleted(icon, source);
        AddCharge(1f, 0, 0, false);
        if (source == CompletionSource.Manual)
        {
            RegisterManualBurst();
            if (GameplayQuery.Level(SkillBalanceTable.H1) > 0)
            {
                synergyLeft = 2f;
            }
        }

        if (source == CompletionSource.Chain)
        {
            GameEvents.RaiseChainTarget(icon);
            AddCharge((float)EffectMath.CascadeChargeBonus(GameplayQuery.Level(SkillBalanceTable.H3)), icon.ChainId, icon.ChainGeneration, true);
            ContinueChain(icon);
        }
        else if (source == CompletionSource.Manual || source == CompletionSource.MiniVirus)
        {
            TryStartChain(icon, source);
        }

        bool infected = source == CompletionSource.Manual && icon.WantsInfect && TryInfect(icon);
        if (!infected)
        {
            TryFragments(icon);
            icon.BeginDownloadFlash(feel != null ? feel.flashSeconds : 0.18f);
        }

        handling--;
    }

    private void RefreshBuffs(float dt)
    {
        if (burstLeft > 0f)
        {
            burstLeft -= dt;
        }

        if (synergyLeft > 0f)
        {
            synergyLeft -= dt;
        }

        if (overloadLeft > 0f)
        {
            overloadLeft -= dt;
            if (overloadLeft <= 0f)
            {
                overloadLeft = 0f;
                overloadOn = false;
                GameEvents.RaiseOverloadEnded();
            }
        }

        BurstMultiplier = burstLeft > 0f ? (float)EffectMath.BurstMultiplier(GameplayQuery.Level(SkillBalanceTable.A4)) : 1f;
        OverloadMultiplier = overloadOn ? (float)EffectMath.OverloadPower(GameplayQuery.Level(SkillBalanceTable.E4)) : 1f;
        SynergyMultiplier = synergyLeft > 0f ? (float)EffectMath.SynergySpeedMultiplier(GameplayQuery.Level(SkillBalanceTable.H1)) : 1f;
        VirusLook look = virus != null ? virus.GetComponent<VirusLook>() : null;
        if (look != null)
        {
            look.SetOverload(overloadOn ? 1f : 0f);
        }
    }

    private void RegisterManualBurst()
    {
        if (GameplayQuery.Level(SkillBalanceTable.A4) <= 0)
        {
            return;
        }

        manualStreak++;
        if (manualStreak < 3)
        {
            return;
        }

        manualStreak = 0;
        burstLeft = 3f;
        Popup(virus != null ? virus.GetAbsorbPoint() : Vector2.zero, Loc.T("popup.burst"), new Color(1f, 0.72f, 0.25f, 1f));
    }

    private void LaunchReadyFiles()
    {
        if (virus == null)
        {
            return;
        }

        scratch.Clear();
        for (int i = 0; i < DesktopIcon.Active.Count; i++)
        {
            DesktopIcon icon = DesktopIcon.Active[i];
            if (icon != null && icon.FlashReadyToFly)
            {
                scratch.Add(icon);
            }
        }

        foreach (DesktopIcon icon in scratch)
        {
            if (icon.WantsInfect && TryInfect(icon))
            {
                continue;
            }

            icon.BeginDelivery(virus.GetAbsorbPoint(), feel, () =>
            {
                if (virus != null)
                {
                    virus.Absorb(icon);
                }
            });
        }
    }

    private void EnsureMinis()
    {
        while (minis.Count < 6)
        {
            GameObject body = new("Mini Virus");
            body.transform.SetParent(transform, false);
            SpriteRenderer renderer = body.AddComponent<SpriteRenderer>();
            renderer.sprite = virusSprite;
            renderer.sortingOrder = 28;
            renderer.color = new Color(0.7f, 1f, 0.55f, 1f);
            body.SetActive(false);
            minis.Add(new Mini { Body = body, Renderer = renderer, State = MiniSearch });
        }
    }

    private void TickMinis(float dt)
    {
        EnsureMinis();
        int count = Mathf.Min(6, GameplayQuery.MiniCount());
        Vector2 origin = virus != null ? virus.GetAbsorbPoint() : Vector2.zero;
        float speed = GameplayQuery.MiniMoveSpeed();
        for (int i = 0; i < minis.Count; i++)
        {
            Mini mini = minis[i];
            bool alive = i < count && virusSprite != null;
            bool wasActive = mini.Body.activeSelf;
            mini.Body.SetActive(alive);
            if (!alive)
            {
                mini.Target = null;
                mini.State = MiniSearch;
                continue;
            }

            if (!wasActive)
            {
                float angle = i * Mathf.PI * 2f / 6f;
                mini.Body.transform.position = origin + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 1.15f;
            }

            float virusScale = virus != null ? Mathf.Max(0.2f, virus.transform.lossyScale.x) : 1f;
            float bob = 1f + Mathf.Sin(Time.time * (mini.State == MiniMove ? 14f : 6f) + i) * 0.08f;
            float size = virusScale * 0.28f * bob;
            mini.Body.transform.localScale = new Vector3(size, size * (mini.State == MiniDownload ? 0.85f : 1f), 1f);
            if (mini.State == MiniIdle)
            {
                mini.Idle -= dt;
                if (mini.Idle <= 0f)
                {
                    mini.State = MiniSearch;
                }
            }
            else if (mini.State == MiniSearch)
            {
                mini.Target = PickTarget(mini, origin, speed);
                mini.State = mini.Target != null ? MiniMove : MiniIdle;
                mini.Idle = 0.25f;
                if (mini.Target == null)
                {
                    mini.Home = origin + Random.insideUnitCircle * 0.8f;
                    mini.Body.transform.position = Vector2.MoveTowards(mini.Body.transform.position, mini.Home, speed * dt);
                }
            }
            else if (mini.Target == null || !mini.Target.IsFree)
            {
                mini.Target = null;
                mini.State = MiniSearch;
            }
            else if (mini.State == MiniMove)
            {
                Vector3 next = Vector2.MoveTowards(mini.Body.transform.position, mini.Target.transform.position, speed * dt);
                mini.Body.transform.position = next;
                if (Vector2.Distance(next, mini.Target.transform.position) <= 0.38f)
                {
                    mini.State = MiniDownload;
                }
            }
            else
            {
                mini.Body.transform.position = mini.Target.transform.position + new Vector3(0.12f, 0.12f, 0f);
                mini.Target.AddWork(GameplayQuery.MiniDownloadRate() * dt, new Color(0.55f, 1f, 0.45f, 0.9f), CompletionSource.MiniVirus);
                if (!mini.Target.IsFree)
                {
                    if (feedback != null)
                    {
                        feedback.Burst(mini.Target.transform.position, new Color(0.6f, 1f, 0.5f, 1f), 4, 2.5f);
                    }

                    mini.Target = null;
                    mini.State = MiniSearch;
                }
            }
        }

    }

    private DesktopIcon PickTarget(Mini mini, Vector2 origin, float speed)
    {
        int targeting = GameplayQuery.Targeting();
        DesktopIcon best = null;
        float bestScore = float.NegativeInfinity;
        long reward = GameplayQuery.StandardReward();
        float feed = Mathf.Max(0.05f, GameplayQuery.MiniDownloadRate());
        for (int i = 0; i < DesktopIcon.Active.Count; i++)
        {
            DesktopIcon icon = DesktopIcon.Active[i];
            if (icon == null || !icon.IsFree)
            {
                continue;
            }

            float distance = Vector2.Distance(mini.Body.transform.position, icon.transform.position);
            float travel = distance / Mathf.Max(0.2f, speed);
            float download = 1f / feed;
            int claims = ClaimCount(icon, mini);
            float value = icon.IsGolden ? reward * 5f : reward;
            float score = targeting switch
            {
                >= 4 => value / Mathf.Max(0.25f, travel + download) / (1f + claims),
                3 => value - travel * 8f - (claims > 0 ? value * 0.75f : 0f),
                2 => (icon.IsGolden && travel < 2.6f ? value : 0f) - distance,
                _ => -distance
            };

            if (score > bestScore)
            {
                bestScore = score;
                best = icon;
            }
        }

        return best;
    }

    private int ClaimCount(DesktopIcon icon, Mini except)
    {
        int count = 0;
        for (int i = 0; i < minis.Count; i++)
        {
            if (minis[i] != except && minis[i].Target == icon)
            {
                count++;
            }
        }

        return count;
    }

    private void TryStartChain(DesktopIcon origin, CompletionSource source)
    {
        if (GameplayQuery.Level(SkillBalanceTable.D1) <= 0 || origin == null)
        {
            return;
        }

        float chance = (float)EffectMath.ChainChance(1, GameplayQuery.Level(SkillBalanceTable.D2));
        if (source == CompletionSource.MiniVirus)
        {
            chance *= (float)EffectMath.MiniChainChanceScale(GameplayQuery.Level(SkillBalanceTable.H2));
        }

        if (chance <= 0f || Random.value >= chance)
        {
            return;
        }

        int id = nextChain++;
        chainBudget[id] = ProgressionConfig.ChainEventBudget;
        cascadePaid[id] = 0d;
        QueueChildren(id, origin, 1, 1);
        GameEvents.RaiseChainStarted(id);
        Popup(origin.transform.position, Loc.T("popup.chain"), new Color(0.75f, 0.45f, 1f, 1f));
    }

    private void ContinueChain(DesktopIcon origin)
    {
        if (origin == null || origin.ChainId <= 0)
        {
            return;
        }

        if (origin.ChainDepth >= EffectMath.ChainDepth(GameplayQuery.Level(SkillBalanceTable.D4)))
        {
            return;
        }

        QueueChildren(origin.ChainId, origin, origin.ChainDepth + 1, origin.ChainGeneration + 1);
    }

    private void QueueChildren(int chainId, DesktopIcon from, int depth, int generation)
    {
        if (from == null || !chainBudget.TryGetValue(chainId, out int budget) || budget <= 0)
        {
            return;
        }

        float radius = (float)EffectMath.ChainRadius(GameplayQuery.Level(SkillBalanceTable.D3));
        int branches = EffectMath.ChainBranching(GameplayQuery.Level(SkillBalanceTable.D5));
        scratch.Clear();
        for (int i = 0; i < DesktopIcon.Active.Count; i++)
        {
            DesktopIcon icon = DesktopIcon.Active[i];
            if (icon == null || icon == from || !icon.IsFree || icon.ChainId != 0)
            {
                continue;
            }

            if (SeenByChain(chainId, icon.RuntimeId))
            {
                continue;
            }

            if (Vector2.Distance(from.transform.position, icon.transform.position) <= radius)
            {
                scratch.Add(icon);
            }
        }

        scratch.Sort((a, b) =>
        {
            float da = Vector2.Distance(from.transform.position, a.transform.position);
            float db = Vector2.Distance(from.transform.position, b.transform.position);
            return da.CompareTo(db);
        });

        int spawned = 0;
        for (int i = 0; i < scratch.Count && spawned < branches && budget > 0; i++)
        {
            DesktopIcon icon = scratch[i];
            budget--;
            spawned++;
            icon.ChainId = chainId;
            icon.ChainDepth = depth;
            icon.ChainGeneration = generation;
            icon.MarkChained();
            pending.Add(new ChainJob
            {
                Id = chainId,
                Depth = depth,
                Generation = generation,
                Icon = icon,
                From = from,
                LineLife = 0.45f
            });
        }

        chainBudget[chainId] = budget;
    }

    private bool SeenByChain(int chainId, int runtimeId)
    {
        for (int i = 0; i < jobs.Count; i++)
        {
            if (jobs[i].Id == chainId && jobs[i].Icon != null && jobs[i].Icon.RuntimeId == runtimeId)
            {
                return true;
            }
        }

        for (int i = 0; i < pending.Count; i++)
        {
            if (pending[i].Id == chainId && pending[i].Icon != null && pending[i].Icon.RuntimeId == runtimeId)
            {
                return true;
            }
        }

        return false;
    }

    private void FlushChains()
    {
        for (int i = 0; i < pending.Count; i++)
        {
            ChainJob job = pending[i];
            job.Line = CreateLine(job.From, job.Icon);
            jobs.Add(job);
        }

        pending.Clear();
    }

    private void TickChains(float dt)
    {
        float rate = GameplayQuery.ChainDownloadRate();
        for (int i = jobs.Count - 1; i >= 0; i--)
        {
            ChainJob job = jobs[i];
            if (job.Line != null)
            {
                job.LineLife -= dt;
                StretchLine(job.Line, job.From, job.Icon);
                if (job.LineLife <= 0f)
                {
                    Destroy(job.Line.gameObject);
                    job.Line = null;
                }
            }

            if (job.Icon == null || job.Icon.IsConsumed || job.Icon.DirectorHandled || !job.Icon.IsFree && job.Icon.CompletionHandled)
            {
                if (job.Line != null)
                {
                    Destroy(job.Line.gameObject);
                }

                jobs.RemoveAt(i);
                continue;
            }

            job.Icon.AddWork(rate * dt, new Color(0.72f, 0.35f, 1f, 0.9f), CompletionSource.Chain);
            if (job.Icon.DirectorHandled)
            {
                if (job.Line != null)
                {
                    Destroy(job.Line.gameObject);
                }

                jobs.RemoveAt(i);
            }
        }
    }

    private bool TryInfect(DesktopIcon icon)
    {
        if (icon == null || icon.IsInfected || icon.IsConsumed)
        {
            return false;
        }

        int capacity = EffectMath.InfectedCapacity(GameplayQuery.Level(SkillBalanceTable.F1), GameplayQuery.Level(SkillBalanceTable.F3));
        if (capacity <= 0)
        {
            return false;
        }

        if (CountInfected() >= capacity)
        {
            Popup(icon.transform.position, Loc.T("popup.full"), new Color(1f, 0.45f, 0.35f, 1f));
            icon.WantsInfect = false;
            return false;
        }

        icon.BecomeInfected();
        GameEvents.RaiseInfected(icon);
        Popup(icon.transform.position, Loc.T("popup.infect"), new Color(0.45f, 1f, 0.4f, 1f));
        return true;
    }

    private void TickInfection(float dt)
    {
        int capacity = EffectMath.InfectedCapacity(GameplayQuery.Level(SkillBalanceTable.F1), GameplayQuery.Level(SkillBalanceTable.F3));
        int infected = CountInfected();
        if (infected > 0)
        {
            passiveBank += infected * GameplayQuery.StandardReward() * EffectMath.PassiveFractionPerSecond(GameplayQuery.Level(SkillBalanceTable.F2)) * dt;
            long pay = (long)System.Math.Floor(passiveBank);
            if (pay > 0)
            {
                passiveBank -= pay;
                Grant(pay, false);
            }
        }

        double interval = EffectMath.SpreadInterval(GameplayQuery.Level(SkillBalanceTable.F4));
        if (interval <= 0d || infected >= capacity || capacity <= 0)
        {
            return;
        }

        spreadTimer += dt;
        if (spreadTimer < interval)
        {
            return;
        }

        spreadTimer = 0f;
        DesktopIcon candidate = null;
        int seen = 0;
        for (int i = 0; i < DesktopIcon.Active.Count; i++)
        {
            DesktopIcon icon = DesktopIcon.Active[i];
            if (icon == null || !icon.IsFree || icon.DownloadProgress > 0.001f)
            {
                continue;
            }

            seen++;
            if (Random.Range(0, seen) == 0)
            {
                candidate = icon;
            }
        }

        if (candidate != null)
        {
            candidate.WantsInfect = false;
            candidate.BecomeInfected();
            GameEvents.RaiseInfected(candidate);
        }
    }

    private static int CountInfected()
    {
        int count = 0;
        for (int i = 0; i < DesktopIcon.Active.Count; i++)
        {
            if (DesktopIcon.Active[i] != null && DesktopIcon.Active[i].IsInfected)
            {
                count++;
            }
        }

        return count;
    }

    private void TryFragments(DesktopIcon icon)
    {
        if (icon == null || GameplayQuery.Level(SkillBalanceTable.G1) <= 0)
        {
            return;
        }

        float chance = (float)EffectMath.FragmentChance(1, GameplayQuery.Level(SkillBalanceTable.G2));
        if (Random.value >= chance)
        {
            return;
        }

        long source = EconomyMath.FileReward(
            GameProgress.TotalPurchases,
            GameplayQuery.Level(SkillBalanceTable.B1),
            GameplayQuery.Level(SkillBalanceTable.B2),
            icon.IsGolden);
        int count = EffectMath.FragmentCount(GameplayQuery.Level(SkillBalanceTable.G3));
        double each = source * EffectMath.FragmentFraction(GameplayQuery.Level(SkillBalanceTable.G4));
        for (int i = 0; i < count; i++)
        {
            SpawnShard(icon.transform.position, each);
        }
    }

    private void SpawnShard(Vector3 position, double value)
    {
        Shard shard = null;
        for (int i = 0; i < shards.Count; i++)
        {
            if (!shards[i].Active)
            {
                shard = shards[i];
                break;
            }
        }

        if (shard == null)
        {
            if (shards.Count >= 48)
            {
                shard = shards[0];
            }
            else
            {
                GameObject body = new("Data Shard");
                SpriteRenderer renderer = body.AddComponent<SpriteRenderer>();
                renderer.sprite = Pixel();
                renderer.sortingOrder = 36;
                shard = new Shard { Body = body.transform, Renderer = renderer };
                shards.Add(shard);
            }
        }

        float angle = Random.value * Mathf.PI * 2f;
        shard.Active = true;
        shard.Life = ProgressionConfig.FragmentLifetimeSeconds;
        shard.Value = value;
        shard.Velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * Random.Range(1.4f, 2.6f);
        shard.Body.gameObject.SetActive(true);
        shard.Body.position = position;
        shard.Renderer.color = new Color(0.55f, 0.95f, 1f, 1f);
    }

    private void TickShards(float dt)
    {
        Vector2 cursor = CursorWorld();
        for (int i = 0; i < shards.Count; i++)
        {
            Shard shard = shards[i];
            if (!shard.Active)
            {
                continue;
            }

            shard.Life -= dt;
            shard.Velocity *= Mathf.Exp(-2.2f * dt);
            shard.Body.position += (Vector3)(shard.Velocity * dt);
            float pulse = 0.12f + Mathf.Sin(Time.time * 10f + i) * 0.02f;
            shard.Body.localScale = new Vector3(pulse, pulse, 1f);
            bool collected = Vector2.Distance(shard.Body.position, cursor) <= 0.5f || MiniNear(shard.Body.position, 0.42f);
            if (shard.Life <= 0f || collected)
            {
                if (collected && shard.Value > 0d)
                {
                    shardBank += shard.Value;
                    long pay = (long)System.Math.Floor(shardBank);
                    if (pay > 0)
                    {
                        shardBank -= pay;
                        Grant(pay, true);
                        GameEvents.RaiseFragment(pay);
                        Popup(shard.Body.position, "+" + NumberFormat.Mb(pay), new Color(0.55f, 0.95f, 1f, 1f));
                    }
                }

                shard.Active = false;
                shard.Value = 0d;
                shard.Body.gameObject.SetActive(false);
            }
        }
    }

    private bool MiniNear(Vector3 point, float radius)
    {
        float sqr = radius * radius;
        for (int i = 0; i < minis.Count; i++)
        {
            if (!minis[i].Body.activeSelf)
            {
                continue;
            }

            if (((Vector2)minis[i].Body.transform.position - (Vector2)point).sqrMagnitude <= sqr)
            {
                return true;
            }
        }

        return false;
    }

    private void AddCharge(float amount, int chainId, int generation, bool cascade)
    {
        if (amount <= 0f || GameplayQuery.Level(SkillBalanceTable.E1) <= 0 || overloadOn)
        {
            return;
        }

        if (cascade)
        {
            long key = ((long)chainId << 8) ^ (uint)generation;
            if (!cascadeGenerations.Add(key))
            {
                return;
            }

            cascadePaid.TryGetValue(chainId, out double paid);
            double room = 3d - paid;
            if (room <= 0d)
            {
                return;
            }

            amount = Mathf.Min(amount, (float)room);
            cascadePaid[chainId] = paid + amount;
        }

        charge += amount;
        float required = (float)EffectMath.OverloadRequired(GameplayQuery.Level(SkillBalanceTable.E2));
        if (charge < required)
        {
            return;
        }

        charge = 0f;
        overloadOn = true;
        overloadLeft = (float)EffectMath.OverloadDuration(GameplayQuery.Level(SkillBalanceTable.E3));
        GameEvents.RaiseOverloadStarted();
        Popup(virus != null ? virus.GetAbsorbPoint() : Vector2.zero, Loc.T("popup.overload"), new Color(1f, 0.55f, 0.25f, 1f));
        if (feedback != null && virus != null)
        {
            feedback.Ring(virus.GetAbsorbPoint(), new Color(1f, 0.6f, 0.25f, 1f), 2.2f);
        }
    }

    private void Grant(long amount, bool popupAtVirus)
    {
        if (amount <= 0)
        {
            return;
        }

        RunStats.AddMB(amount);
        GameProgress.AddMB(amount);
        if (popupAtVirus && feedback != null && virus != null)
        {
            feedback.ShowPopup(virus.GetAbsorbPoint(), "+" + NumberFormat.Mb(amount), new Color(0.45f, 1f, 0.55f, 1f), 1f);
        }
    }

    private void Popup(Vector2 world, string message, Color color)
    {
        if (feedback != null && !string.IsNullOrEmpty(message))
        {
            feedback.ShowPopup(world, message, color, 1.1f);
        }
    }

    private void BuildMeter(Image timer)
    {
        if (timer == null)
        {
            return;
        }

        RectTransform parent = timer.rectTransform.parent as RectTransform;
        if (parent == null)
        {
            return;
        }

        GameObject back = new("Overload Meter", typeof(RectTransform), typeof(Image));
        back.transform.SetParent(parent, false);
        meterBack = back.GetComponent<Image>();
        meterBack.color = new Color(0.08f, 0.1f, 0.12f, 0.85f);
        RectTransform backRect = meterBack.rectTransform;
        backRect.anchorMin = timer.rectTransform.anchorMin;
        backRect.anchorMax = timer.rectTransform.anchorMax;
        backRect.pivot = timer.rectTransform.pivot;
        backRect.sizeDelta = new Vector2(180f, 16f);
        backRect.anchoredPosition = timer.rectTransform.anchoredPosition + new Vector2(0f, -30f);

        GameObject fill = new("Fill", typeof(RectTransform), typeof(Image));
        fill.transform.SetParent(back.transform, false);
        meterFill = fill.GetComponent<Image>();
        meterFill.color = new Color(1f, 0.62f, 0.22f, 1f);
        meterFill.type = Image.Type.Filled;
        meterFill.fillMethod = Image.FillMethod.Horizontal;
        RectTransform fillRect = meterFill.rectTransform;
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = new Vector2(2f, 2f);
        fillRect.offsetMax = new Vector2(-2f, -2f);

        GameObject label = new("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        label.transform.SetParent(back.transform, false);
        meterLabel = label.GetComponent<TextMeshProUGUI>();
        meterLabel.alignment = TextAlignmentOptions.Center;
        meterLabel.fontSize = 12f;
        meterLabel.color = Color.white;
        meterLabel.raycastTarget = false;
        RectTransform labelRect = meterLabel.rectTransform;
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;
        back.SetActive(false);
    }

    private void UpdateMeter()
    {
        if (meterBack == null)
        {
            return;
        }

        bool unlocked = GameplayQuery.Level(SkillBalanceTable.E1) > 0;
        meterBack.gameObject.SetActive(unlocked);
        if (!unlocked)
        {
            return;
        }

        float required = Mathf.Max(0.01f, (float)EffectMath.OverloadRequired(GameplayQuery.Level(SkillBalanceTable.E2)));
        meterFill.fillAmount = overloadOn ? Mathf.Clamp01(overloadLeft / Mathf.Max(0.01f, (float)EffectMath.OverloadDuration(GameplayQuery.Level(SkillBalanceTable.E3)))) : Mathf.Clamp01(charge / required);
        meterFill.color = overloadOn ? new Color(1f, 0.35f, 0.2f, 1f) : new Color(1f, 0.62f, 0.22f, 1f);
        meterLabel.text = overloadOn
            ? Loc.F("hud.overload_time", overloadLeft)
            : Loc.F("hud.overload_charge", charge, required);
    }

    private Transform CreateLine(DesktopIcon from, DesktopIcon to)
    {
        if (from == null || to == null)
        {
            return null;
        }

        GameObject body = new("Chain Link");
        SpriteRenderer renderer = body.AddComponent<SpriteRenderer>();
        renderer.sprite = Pixel();
        renderer.color = new Color(0.65f, 0.35f, 1f, 0.9f);
        renderer.sortingOrder = 12;
        StretchLine(body.transform, from, to);
        return body.transform;
    }

    private static void StretchLine(Transform line, DesktopIcon from, DesktopIcon to)
    {
        if (line == null || from == null || to == null)
        {
            return;
        }

        Vector3 a = from.transform.position;
        Vector3 b = to.transform.position;
        Vector3 delta = b - a;
        line.position = (a + b) * 0.5f;
        line.right = delta.sqrMagnitude > 0.0001f ? delta.normalized : Vector3.right;
        float width = PixelWidth();
        line.localScale = new Vector3(delta.magnitude / Mathf.Max(0.01f, width), 0.08f, 1f);
    }

    private Sprite Pixel()
    {
        if (pixel != null)
        {
            return pixel;
        }

        Texture2D texture = new(4, 4, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp,
            hideFlags = HideFlags.HideAndDontSave
        };
        Color[] pixels = new Color[16];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.white;
        }

        texture.SetPixels(pixels);
        texture.Apply(false, false);
        pixel = Sprite.Create(texture, new Rect(0f, 0f, 4f, 4f), new Vector2(0.5f, 0.5f), 4f);
        pixel.hideFlags = HideFlags.HideAndDontSave;
        return pixel;
    }

    private static float PixelWidth()
    {
        return 1f;
    }

    private static Vector2 CursorWorld()
    {
        Camera cam = Camera.main;
        if (cam == null || Mouse.current == null)
        {
            return new Vector2(9999f, 9999f);
        }

        return MonitorScreen.ToWorld(cam);
    }
}
