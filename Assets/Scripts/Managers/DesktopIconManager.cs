using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DesktopIconManager : MonoBehaviour
{
    [SerializeField] private GameObject icon;
    [SerializeField] private Virus virus;
    [SerializeField] private SkillTreeDefinition skillTree;
    [SerializeField] private float padding = 0.01f;
    [SerializeField] private uint initialIconCount = 5;
    [SerializeField] private int maxIconsOnScreen = 12;
    [SerializeField] private float baseSpawnIntervalInSeconds = 0.9f;
    [SerializeField] private float haloDiameterInVirusHeights = 2f;
    [SerializeField] private int maxSpawnAttempts = 30;
    [SerializeField] private RectTransform[] spawnBlockers;

    private Camera cam;
    private readonly float distanceFromCamera = 10f;
    private readonly Vector3[] blockerCorners = new Vector3[4];

    void Awake()
    {
        cam = Camera.main;
    }

    public void StartSpawning()
    {
        SpawnStartingIcons();
        StartCoroutine(SpawnDesktopIconsRoutine());
    }

    public void StopSpawning()
    {
        StopAllCoroutines();
    }

    private IEnumerator SpawnDesktopIconsRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(GetSpawnInterval());

            int free = GetMaxIconsOnScreen() - DesktopIcon.Active.Count;
            int burst = 1 + Mathf.RoundToInt(skillTree.GetStat(SkillStat.SpawnBurst));
            if (free > 0)
            {
                SpawnDesktopIcons((uint)Mathf.Min(free, burst));
            }
        }
    }

    private float GetSpawnInterval()
    {
        return baseSpawnIntervalInSeconds / (1f + skillTree.GetStat(SkillStat.SpawnRate));
    }

    private int GetMaxIconsOnScreen()
    {
        return maxIconsOnScreen + Mathf.RoundToInt(skillTree.GetStat(SkillStat.MaxFiles));
    }

    public void ClearDesktop()
    {
        for (int i = DesktopIcon.Active.Count - 1; i >= 0; i--)
        {
            Destroy(DesktopIcon.Active[i].gameObject);
        }
    }

    public void SpawnStartingIcons()
    {
        SpawnDesktopIcons(initialIconCount + (uint)Mathf.RoundToInt(skillTree.GetStat(SkillStat.StartFiles)));
    }

    [ContextMenu("Spawn 6 Desktop Icons")]
    public void Spawn6DesktopIcons()
    {
        SpawnDesktopIcons(6);
    }

    public void SpawnDesktopIcons(uint count)
    {
        for (int i = 0; i < count; i++)
        {
            if (TryGetPositionOutsideHalo(out Vector2 coords))
            {
                GameObject spawned = Instantiate(icon, new Vector3(coords.x, coords.y, 0f), Quaternion.identity);
                if (Random.value < EffectMath.ValuableChance(GameProgress.GetSkillLevel(SkillBalanceTable.B3)))
                {
                    spawned.GetComponent<DesktopIcon>().MakeGolden();
                }
            }
        }
    }

    private bool TryGetPositionOutsideHalo(out Vector2 position)
    {
        Bounds virusBounds = virus.GetVisualBounds();
        Vector2 haloCenter = virusBounds.center;
        float haloRadius = virusBounds.size.y * haloDiameterInVirusHeights * 0.5f;

        Vector2 iconHalfExtents = GetIconHalfExtents();
        float minDistance = haloRadius + iconHalfExtents.magnitude;

        Vector3 bottomLeft = cam.ViewportToWorldPoint(new Vector3(0f, Taskbar.ScreenFraction, distanceFromCamera));
        Vector3 topRight = cam.ViewportToWorldPoint(new Vector3(1f, 1f, distanceFromCamera));
        float minX = bottomLeft.x + iconHalfExtents.x + padding;
        float maxX = topRight.x - iconHalfExtents.x - padding;
        float minY = bottomLeft.y + iconHalfExtents.y + padding;
        float maxY = topRight.y - iconHalfExtents.y - padding;

        List<Rect> blockedRects = GetBlockedWorldRects(iconHalfExtents);

        for (int attempt = 0; attempt < maxSpawnAttempts; attempt++)
        {
            Vector2 candidate = new(Random.Range(minX, maxX), Random.Range(minY, maxY));
            if ((candidate - haloCenter).sqrMagnitude >= minDistance * minDistance && !IsInsideAny(candidate, blockedRects))
            {
                position = candidate;
                return true;
            }
        }

        position = default;
        return false;
    }

    private List<Rect> GetBlockedWorldRects(Vector2 iconHalfExtents)
    {
        List<Rect> rects = new();

        foreach (RectTransform blocker in spawnBlockers)
        {
            if (blocker == null || !blocker.gameObject.activeInHierarchy)
            {
                continue;
            }

            Canvas canvas = blocker.GetComponentInParent<Canvas>();
            Camera uiCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;

            blocker.GetWorldCorners(blockerCorners);

            Vector2 min = new(float.MaxValue, float.MaxValue);
            Vector2 max = new(float.MinValue, float.MinValue);
            foreach (Vector3 corner in blockerCorners)
            {
                Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(uiCamera, corner);
                Vector2 worldPoint = cam.ScreenToWorldPoint(new Vector3(screenPoint.x, screenPoint.y, distanceFromCamera));
                min = Vector2.Min(min, worldPoint);
                max = Vector2.Max(max, worldPoint);
            }

            rects.Add(Rect.MinMaxRect(
                min.x - iconHalfExtents.x - padding,
                min.y - iconHalfExtents.y - padding,
                max.x + iconHalfExtents.x + padding,
                max.y + iconHalfExtents.y + padding));
        }

        return rects;
    }

    private static bool IsInsideAny(Vector2 point, List<Rect> rects)
    {
        foreach (Rect rect in rects)
        {
            if (rect.Contains(point))
            {
                return true;
            }
        }

        return false;
    }

    private Vector2 GetIconHalfExtents()
    {
        Vector3 size = icon.GetComponent<SpriteRenderer>().sprite.bounds.size;
        Vector3 scale = icon.transform.localScale;
        return new Vector2(size.x * Mathf.Abs(scale.x) * 0.5f, size.y * Mathf.Abs(scale.y) * 0.5f);
    }
}