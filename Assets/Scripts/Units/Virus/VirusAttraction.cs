using System.Collections.Generic;
using UnityEngine;

public class VirusAttraction : MonoBehaviour
{
    [SerializeField] private Virus virus;
    [SerializeField] private SkillManager skillManager;

    [Header("Radius")]
    [SerializeField] private float baseRadius = 1.5f;
    [SerializeField] private float radiusPerLevel = 0.5f;
    [SerializeField] private float pullSpeed = 3f;

    [Header("Screen-wide eating")]
    [SerializeField] private int screenWideFromLevel = 5;
    [SerializeField] private float baseScreenWideIntervalInSeconds = 3f;
    [SerializeField] private float screenWideIntervalReductionPerLevel = 0.4f;
    [SerializeField] private float minScreenWideIntervalInSeconds = 0.5f;

    private readonly HashSet<DesktopIcon> targeted = new();
    private float screenWideTimer;

    void Update()
    {
        targeted.RemoveWhere(icon => icon == null);

        int level = skillManager.GetLevel(SkillType.Autonomy);
        if (level <= 0)
        {
            return;
        }

        UpdateScreenWideTargeting(level);
        PullIcons(level);
    }

    private void UpdateScreenWideTargeting(int level)
    {
        if (level < screenWideFromLevel)
        {
            return;
        }

        screenWideTimer += Time.deltaTime;
        float interval = Mathf.Max(
            minScreenWideIntervalInSeconds,
            baseScreenWideIntervalInSeconds - screenWideIntervalReductionPerLevel * (level - screenWideFromLevel));

        if (screenWideTimer < interval)
        {
            return;
        }

        screenWideTimer = 0f;

        List<DesktopIcon> candidates = new();
        foreach (DesktopIcon icon in DesktopIcon.Active)
        {
            if (!icon.IsDragged && !icon.IsConsumed && !targeted.Contains(icon))
            {
                candidates.Add(icon);
            }
        }

        if (candidates.Count > 0)
        {
            targeted.Add(candidates[Random.Range(0, candidates.Count)]);
        }
    }

    private void PullIcons(int level)
    {
        Vector2 target = virus.GetAbsorbPoint();
        float radius = baseRadius + radiusPerLevel * (level - 1);
        float sqrRadius = radius * radius;

        for (int i = DesktopIcon.Active.Count - 1; i >= 0; i--)
        {
            DesktopIcon icon = DesktopIcon.Active[i];
            if (icon.IsDragged || icon.IsConsumed)
            {
                continue;
            }

            Vector2 position = icon.transform.position;
            if ((position - target).sqrMagnitude > sqrRadius && !targeted.Contains(icon))
            {
                continue;
            }

            Vector2 next = Vector2.MoveTowards(position, target, pullSpeed * Time.deltaTime);
            icon.transform.position = new Vector3(next.x, next.y, icon.transform.position.z);

            if (virus.ContainsPoint(next))
            {
                targeted.Remove(icon);
                virus.Absorb(icon);
            }
        }
    }
}
