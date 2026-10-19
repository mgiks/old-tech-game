using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(50)]
public class VirusCrawlers : MonoBehaviour
{
    public const int MaxCount = 3;
    public const float BaseMoveSpeed = 1.6f;
    public const float BaseDownloadDuration = 1.7f;
    public const float MinDownloadDuration = 0.5f;

    [SerializeField] private Virus virus;
    [SerializeField] private SkillTreeDefinition skillTree;
    [SerializeField] private Material silhouette;
    [SerializeField] private float worldHeight = 0.42f;
    [SerializeField] private float arriveDistance = 0.28f;
    [SerializeField] private Color bodyColor = new(0.28f, 0.92f, 0.38f, 1f);
    [SerializeField] private Color downloadColor = Color.white;

    private readonly List<Crawler> crawlers = new();
    private SpriteRenderer virusRenderer;

    private static readonly Vector2[] homes =
    {
        new(0.85f, 0.35f),
        new(-0.7f, 0.55f),
        new(0.15f, -0.8f)
    };

    public static float MoveSpeed(float bonus)
    {
        return BaseMoveSpeed + bonus;
    }

    public static float DownloadDuration(float reduction)
    {
        return Mathf.Max(MinDownloadDuration, BaseDownloadDuration - reduction);
    }

    void Awake()
    {
        if (virus == null)
        {
            virus = GetComponent<Virus>();
        }

        virusRenderer = virus.GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        int count = 0;
        if (virusRenderer != null && virusRenderer.enabled && skillTree != null)
        {
            count = Mathf.Clamp(Mathf.RoundToInt(skillTree.GetStat(SkillStat.CrawlerCount)), 0, MaxCount);
        }

        EnsureCount(count);
        if (count == 0 || GamePause.IsPaused)
        {
            return;
        }

        float speed = MoveSpeed(skillTree.GetStat(SkillStat.CrawlerSpeed));
        float download = DownloadDuration(skillTree.GetStat(SkillStat.CrawlerDownload));
        float step = speed * Time.deltaTime;

        bool instinct = skillTree.GetStat(SkillStat.CrawlerInstinct) > 0f;
        for (int i = 0; i < count; i++)
        {
            ReleaseIfLost(crawlers[i]);
            if (!crawlers[i].downloading)
            {
                Retarget(crawlers[i], instinct);
            }
        }

        for (int i = 0; i < count; i++)
        {
            Tick(crawlers[i], i, step, download);
        }
    }

    private void Tick(Crawler crawler, int index, float step, float download)
    {
        Vector2 position = crawler.body.transform.position;
        Vector2 destination;
        bool working = false;

        if (crawler.target != null)
        {
            destination = crawler.target.transform.position;
            if (!crawler.downloading && (destination - position).sqrMagnitude <= arriveDistance * arriveDistance)
            {
                crawler.downloading = true;
                crawler.downloadLeft = download;
            }

            if (crawler.downloading)
            {
                working = true;
                crawler.downloadLeft -= Time.deltaTime;
                destination = crawler.target.transform.position;
                if (crawler.downloadLeft <= 0f)
                {
                    Finish(crawler);
                    return;
                }
            }
        }
        else
        {
            destination = virus.GetAbsorbPoint() + homes[index % homes.Length];
        }

        Vector2 next = working ? destination : Vector2.MoveTowards(position, destination, step);
        float angle = Mathf.Atan2(destination.y - position.y, destination.x - position.x) * Mathf.Rad2Deg;
        float wobble = Mathf.Sin(Time.time * 9f + index) * 8f;
        float pulse = working ? 1f + Mathf.Sin(Time.time * 14f + index) * 0.08f : 1f;

        Transform body = crawler.body.transform;
        body.position = new Vector3(next.x, next.y, 0f);
        body.rotation = Quaternion.Euler(0f, 0f, angle + wobble);
        ApplySize(body, pulse);
        crawler.body.color = working ? downloadColor : bodyColor;
    }

    private void Finish(Crawler crawler)
    {
        DesktopIcon icon = crawler.target;
        crawler.target = null;
        crawler.downloading = false;
        if (icon == null)
        {
            return;
        }

        icon.IsReserved = false;
        virus.Absorb(icon);
    }

    private void Retarget(Crawler crawler, bool instinct)
    {
        DesktopIcon best = FindBest(crawler, instinct);
        if (best == crawler.target)
        {
            return;
        }

        Release(crawler);
        if (best == null)
        {
            return;
        }

        best.IsReserved = true;
        crawler.target = best;
        crawler.downloading = false;
    }

    private DesktopIcon FindBest(Crawler crawler, bool instinct)
    {
        Vector2 from = crawler.body.transform.position;
        DesktopIcon best = null;
        float bestScore = float.MaxValue;

        foreach (DesktopIcon icon in DesktopIcon.Active)
        {
            bool mine = icon == crawler.target;
            if ((!icon.IsFree && !mine) || icon.IsCursorInfluenced || icon.IsConsumed)
            {
                continue;
            }

            float distance = Vector2.Distance(icon.transform.position, from);
            float score = distance;
            if (instinct && (icon.IsGolden || icon.IsConfidential))
            {
                score *= SkillTuning.InstinctDistanceWeight;
            }

            if (score < bestScore)
            {
                bestScore = score;
                best = icon;
            }
        }

        return best;
    }

    private void ReleaseIfLost(Crawler crawler)
    {
        DesktopIcon icon = crawler.target;
        if (icon == null)
        {
            crawler.target = null;
            crawler.downloading = false;
            return;
        }

        if (icon.IsConsumed || icon.IsDragged || icon.IsCarried || (!crawler.downloading && icon.IsCursorInfluenced))
        {
            Release(crawler);
        }
    }

    private static void Release(Crawler crawler)
    {
        if (crawler.target != null)
        {
            crawler.target.IsReserved = false;
        }

        crawler.target = null;
        crawler.downloading = false;
    }

    private void EnsureCount(int count)
    {
        while (crawlers.Count < count)
        {
            crawlers.Add(Create());
        }

        for (int i = 0; i < crawlers.Count; i++)
        {
            bool active = i < count;
            GameObject go = crawlers[i].body.gameObject;
            if (!active && go.activeSelf)
            {
                Release(crawlers[i]);
            }

            go.SetActive(active);
        }
    }

    private Crawler Create()
    {
        GameObject go = new("Crawler", typeof(SpriteRenderer));
        Transform root = virus.transform.parent != null ? virus.transform.parent : transform;
        go.transform.SetParent(root, false);
        SpriteRenderer renderer = go.GetComponent<SpriteRenderer>();
        renderer.sprite = virusRenderer.sprite;
        renderer.sharedMaterial = silhouette != null ? silhouette : virusRenderer.sharedMaterial;
        renderer.sortingLayerID = virusRenderer.sortingLayerID;
        renderer.sortingOrder = virusRenderer.sortingOrder + 3;
        renderer.color = bodyColor;
        go.transform.position = virus.GetAbsorbPoint();
        ApplySize(go.transform, 1f);
        return new Crawler { body = renderer };
    }

    private void ApplySize(Transform body, float pulse)
    {
        float spriteHeight = Mathf.Max(virusRenderer.sprite.bounds.size.y, 0.001f);
        float parentScale = body.parent != null ? Mathf.Max(body.parent.lossyScale.x, 0.001f) : 1f;
        body.localScale = Vector3.one * (worldHeight * pulse / spriteHeight / parentScale);
    }

    private class Crawler
    {
        public SpriteRenderer body;
        public DesktopIcon target;
        public bool downloading;
        public float downloadLeft;
    }
}
