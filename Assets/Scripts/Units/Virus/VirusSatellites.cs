using System.Collections.Generic;
using UnityEngine;

public class VirusSatellites : MonoBehaviour
{
    [SerializeField] private Virus virus;
    [SerializeField] private SkillTreeDefinition skillTree;
    [SerializeField] private EatFeedback feedback;
    [SerializeField] private float sizeInVirusHeights = 0.3f;
    [SerializeField] private float orbitInVirusHeights = 0.95f;
    [SerializeField] private float orbitPadding = 0.5f;
    [SerializeField] private float angularSpeed = 2.2f;
    [SerializeField] private float catchRadiusInSatelliteSizes = 0.6f;
    [SerializeField] private Color tint = new(0.85f, 0.55f, 1f, 1f);

    private readonly List<SpriteRenderer> satellites = new();
    private readonly List<DesktopIcon> caught = new();
    private SpriteRenderer virusRenderer;
    private float angle;

    void Awake()
    {
        virusRenderer = virus.GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        int count = virusRenderer.enabled ? Mathf.RoundToInt(skillTree.GetStat(SkillStat.Satellites)) : 0;
        EnsureCount(count);
        if (count == 0 || GamePause.IsPaused)
        {
            return;
        }

        Bounds bounds = virus.GetVisualBounds();
        Vector2 center = virus.GetAbsorbPoint();
        float virusHeight = bounds.size.y;
        float size = virusHeight * sizeInVirusHeights;
        float orbit = virusHeight * orbitInVirusHeights + orbitPadding;
        float spriteSize = Mathf.Max(virusRenderer.sprite.bounds.size.y, 0.001f);
        float catchRadius = size * catchRadiusInSatelliteSizes;

        angle += angularSpeed * Time.deltaTime;

        for (int i = 0; i < count; i++)
        {
            SpriteRenderer satellite = satellites[i];
            float a = angle + i * Mathf.PI * 2f / count;
            Vector2 position = center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * orbit;
            float wobble = 1f + Mathf.Sin(Time.time * 9f + i) * 0.08f;

            satellite.transform.position = new Vector3(position.x, position.y, 0f);
            satellite.transform.localScale = Vector3.one * (size / spriteSize * wobble);
            satellite.transform.rotation = Quaternion.Euler(0f, 0f, -a * Mathf.Rad2Deg * 1.5f);

            CatchIcons(position, catchRadius);
        }
    }

    private void CatchIcons(Vector2 position, float radius)
    {
        caught.Clear();
        float sqrRadius = radius * radius;
        foreach (DesktopIcon icon in DesktopIcon.Active)
        {
            if (icon.IsFree && ((Vector2)icon.transform.position - position).sqrMagnitude <= sqrRadius)
            {
                caught.Add(icon);
            }
        }

        foreach (DesktopIcon icon in caught)
        {
            if (feedback != null)
            {
                feedback.Burst(position, tint, 6, 3f);
            }

            virus.Absorb(icon);
        }
    }

    private void EnsureCount(int count)
    {
        while (satellites.Count < count)
        {
            GameObject go = new("Virus Satellite", typeof(SpriteRenderer));
            go.transform.SetParent(transform, false);
            SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
            sr.sprite = virusRenderer.sprite;
            sr.color = tint;
            sr.sortingLayerID = virusRenderer.sortingLayerID;
            sr.sortingOrder = virusRenderer.sortingOrder + 1;
            satellites.Add(sr);
        }

        for (int i = 0; i < satellites.Count; i++)
        {
            satellites[i].gameObject.SetActive(i < count);
        }
    }
}
