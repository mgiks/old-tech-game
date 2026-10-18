using System.Collections.Generic;
using UnityEngine;

public class VirusAttraction : MonoBehaviour
{
    [SerializeField] private Virus virus;
    [SerializeField] private SkillTreeDefinition skillTree;
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private float basePullSpeed = 3f;

    [Header("Screen-wide eating")]
    [SerializeField] private float baseScreenWideIntervalInSeconds = 3f;
    [SerializeField] private float screenWideIntervalReductionPerLevel = 0.4f;
    [SerializeField] private float minScreenWideIntervalInSeconds = 0.5f;

    [Header("Chain reaction")]
    [SerializeField] private float chainPullSpeed = 14f;

    private readonly HashSet<DesktopIcon> targeted = new();
    private readonly HashSet<DesktopIcon> chained = new();
    private float screenWideTimer;

    void OnEnable()
    {
        if (scoreManager != null)
        {
            scoreManager.FileScored += OnFileScored;
        }
    }

    void OnDisable()
    {
        if (scoreManager != null)
        {
            scoreManager.FileScored -= OnFileScored;
        }
    }

    void Update()
    {
        targeted.RemoveWhere(icon => icon == null);
        chained.RemoveWhere(icon => icon == null);

        UpdateScreenWideTargeting(Mathf.RoundToInt(skillTree.GetStat(SkillStat.ScreenWideEating)));
        PullIcons(skillTree.GetStat(SkillStat.AttractionRadius));
    }

    private void OnFileScored(Vector3 position, uint value, bool golden)
    {
        if (Random.value >= skillTree.GetStat(SkillStat.ChainChance))
        {
            return;
        }

        Vector2 center = virus.GetAbsorbPoint();
        DesktopIcon nearest = null;
        float best = float.MaxValue;
        foreach (DesktopIcon icon in DesktopIcon.Active)
        {
            if (!icon.IsFree || chained.Contains(icon))
            {
                continue;
            }

            float distance = ((Vector2)icon.transform.position - center).sqrMagnitude;
            if (distance < best)
            {
                best = distance;
                nearest = icon;
            }
        }

        if (nearest != null)
        {
            nearest.MarkChained();
            chained.Add(nearest);
        }
    }

    private void UpdateScreenWideTargeting(int level)
    {
        if (level <= 0)
        {
            return;
        }

        screenWideTimer += Time.deltaTime;
        float interval = Mathf.Max(
            minScreenWideIntervalInSeconds,
            baseScreenWideIntervalInSeconds - screenWideIntervalReductionPerLevel * (level - 1));

        if (screenWideTimer < interval)
        {
            return;
        }

        screenWideTimer = 0f;

        List<DesktopIcon> candidates = new();
        foreach (DesktopIcon icon in DesktopIcon.Active)
        {
            if (icon.IsFree && !targeted.Contains(icon))
            {
                candidates.Add(icon);
            }
        }

        int targets = 1 + (level - 1) / 2;
        for (int i = 0; i < targets && candidates.Count > 0; i++)
        {
            int index = Random.Range(0, candidates.Count);
            targeted.Add(candidates[index]);
            candidates.RemoveAt(index);
        }
    }

    private void PullIcons(float radius)
    {
        if (radius <= 0f && targeted.Count == 0 && chained.Count == 0)
        {
            return;
        }

        Vector2 target = virus.GetAbsorbPoint();
        float sqrRadius = radius * radius;
        float pullSpeed = basePullSpeed + skillTree.GetStat(SkillStat.PullSpeed);

        for (int i = DesktopIcon.Active.Count - 1; i >= 0; i--)
        {
            DesktopIcon icon = DesktopIcon.Active[i];
            if (!icon.IsFree)
            {
                continue;
            }

            bool isChained = chained.Contains(icon);
            Vector2 position = icon.transform.position;
            if ((position - target).sqrMagnitude > sqrRadius && !targeted.Contains(icon) && !isChained)
            {
                continue;
            }

            float speed = isChained ? Mathf.Max(chainPullSpeed, pullSpeed) : pullSpeed;
            Vector2 next = Vector2.MoveTowards(position, target, speed * Time.deltaTime);
            icon.transform.position = new Vector3(next.x, next.y, icon.transform.position.z);

            if (virus.ContainsPoint(next))
            {
                targeted.Remove(icon);
                chained.Remove(icon);
                virus.Absorb(icon);
            }
        }
    }
}
