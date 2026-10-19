using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-50)]
public class CursorFeeder : MonoBehaviour
{
    [SerializeField] private Virus virus;
    [SerializeField] private SkillTreeDefinition skillTree;
    [SerializeField] private Sprite ringSprite;
    [SerializeField] private int sortingOrder = 40;
    [SerializeField] private float hoverBaseRadius = 0.45f;
    [SerializeField] private float clumpRadius = 0.35f;
    [SerializeField] private float followSharpness = 20f;
    [SerializeField] private float contactPadding = 0.3f;
    [SerializeField] private Color ringColor = new(0.45f, 1f, 0.55f, 0.18f);
    [SerializeField] private Color ringActiveColor = new(0.45f, 1f, 0.55f, 0.45f);

    private readonly Dictionary<DesktopIcon, Vector2> carried = new();
    private readonly List<DesktopIcon> buffer = new();
    private readonly List<DesktopIcon> nearby = new();
    private Camera cam;
    private SpriteRenderer ring;
    private SpriteRenderer pullRing;
    private SpriteRenderer virusRenderer;
    private float ringPulse;
    private float stillTime;
    private Vector2 lastCursor;

    void Awake()
    {
        cam = Camera.main;
        virusRenderer = virus.GetComponent<SpriteRenderer>();

        GameObject go = new("Cursor Ring", typeof(SpriteRenderer));
        go.transform.SetParent(transform, false);
        ring = go.GetComponent<SpriteRenderer>();
        ring.sprite = ringSprite;
        ring.sortingOrder = sortingOrder;
        ring.enabled = false;

        GameObject pull = new("Pull Ring", typeof(SpriteRenderer));
        pull.transform.SetParent(transform, false);
        pullRing = pull.GetComponent<SpriteRenderer>();
        pullRing.sprite = ringSprite;
        pullRing.sortingOrder = sortingOrder - 1;
        pullRing.enabled = false;
    }

    void OnDisable()
    {
        ReleaseAll();
        if (ring != null)
        {
            ring.enabled = false;
        }

        if (pullRing != null)
        {
            pullRing.enabled = false;
        }

        ClearInfluence();
    }

    void Update()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null || GamePause.IsPaused)
        {
            return;
        }

        RemoveDestroyed();

        Vector2 cursor = MonitorScreen.ToWorld(cam);
        bool hoverFeed = skillTree.GetStat(SkillStat.HoverFeed) > 0f;
        float grabRadius = skillTree.GetStat(SkillStat.CursorRadius);
        float radius = hoverFeed ? hoverBaseRadius + grabRadius : grabRadius;
        bool vacuumActive = UpdateVacuum(cursor, hoverFeed);
        float pullRadius = vacuumActive
            ? SkillTuning.VacuumRadius
            : SkillTuning.MagneticRadius(skillTree.GetStat(SkillStat.MagneticRadius));
        bool magnetOwned = skillTree.GetStat(SkillStat.MagneticRadius) > 0f;
        if (!vacuumActive && !magnetOwned)
        {
            pullRadius = 0f;
        }

        bool virusActive = virusRenderer.enabled;
        bool dragging = IsAnyIconDragged();
        bool collecting = virusActive && radius > 0f && (hoverFeed || dragging);

        ClearInfluence();
        if (virusActive && pullRadius > 0f)
        {
            PullFiles(cursor, pullRadius, vacuumActive ? SkillTuning.VacuumPullSpeed : SkillTuning.MagneticPullSpeed);
        }

        UpdateRing(cursor, radius, virusActive && radius > 0f, collecting);
        UpdatePullRing(cursor, pullRadius, virusActive && pullRadius > radius);

        if (!hoverFeed && carried.Count > 0 && mouse.leftButton.wasReleasedThisFrame)
        {
            if (virus.ContainsPoint(cursor))
            {
                AbsorbAll();
            }

            ReleaseAll();
            return;
        }

        if (!collecting)
        {
            ReleaseAll();
            return;
        }

        Vector2 virusCenter = virus.GetAbsorbPoint();

        Collect(cursor, radius);
        MoveCarried(cursor);

        if (hoverFeed)
        {
            Bounds bounds = virus.GetVisualBounds();
            float reach = Mathf.Max(bounds.extents.x, bounds.extents.y) + contactPadding;
            bool cursorInVirus = (cursor - virusCenter).sqrMagnitude <= reach * reach;
            buffer.Clear();
            foreach (DesktopIcon icon in carried.Keys)
            {
                if (cursorInVirus || ((Vector2)icon.transform.position - virusCenter).sqrMagnitude <= reach * reach)
                {
                    buffer.Add(icon);
                }
            }

            foreach (DesktopIcon icon in buffer)
            {
                Absorb(icon);
            }
        }
    }

    private static bool IsAnyIconDragged()
    {
        foreach (DesktopIcon icon in DesktopIcon.Active)
        {
            if (icon.IsDragged)
            {
                return true;
            }
        }

        return false;
    }

    private bool UpdateVacuum(Vector2 cursor, bool hoverFeed)
    {
        if (skillTree.GetStat(SkillStat.Vacuum) <= 0f || !hoverFeed)
        {
            stillTime = 0f;
            lastCursor = cursor;
            return false;
        }

        if ((cursor - lastCursor).sqrMagnitude > SkillTuning.VacuumStillSlack * SkillTuning.VacuumStillSlack)
        {
            stillTime = 0f;
        }
        else
        {
            stillTime += Time.deltaTime;
        }

        lastCursor = cursor;
        return stillTime >= SkillTuning.VacuumStillDelay;
    }

    private void PullFiles(Vector2 cursor, float radius, float speed)
    {
        float sqrRadius = radius * radius;
        float step = speed * Time.deltaTime;
        foreach (DesktopIcon icon in DesktopIcon.Active)
        {
            if (!icon.IsFree)
            {
                continue;
            }

            Vector2 position = icon.transform.position;
            Vector2 delta = cursor - position;
            if (delta.sqrMagnitude > sqrRadius)
            {
                continue;
            }

            icon.IsCursorInfluenced = true;
            if (delta.sqrMagnitude < 0.0004f)
            {
                continue;
            }

            Vector2 next = Vector2.MoveTowards(position, cursor, step);
            icon.transform.position = new Vector3(next.x, next.y, icon.transform.position.z);
        }
    }

    private void ClearInfluence()
    {
        foreach (DesktopIcon icon in DesktopIcon.Active)
        {
            icon.IsCursorInfluenced = false;
        }
    }

    private void Collect(Vector2 cursor, float radius)
    {
        int room = SkillTuning.CursorHold(skillTree.GetStat(SkillStat.CursorCapacity)) - carried.Count;
        if (room <= 0)
        {
            return;
        }

        float sqrRadius = radius * radius;
        nearby.Clear();
        foreach (DesktopIcon icon in DesktopIcon.Active)
        {
            if (!icon.IsFree)
            {
                continue;
            }

            if (((Vector2)icon.transform.position - cursor).sqrMagnitude <= sqrRadius)
            {
                nearby.Add(icon);
            }
        }

        nearby.Sort((a, b) =>
        {
            float da = ((Vector2)a.transform.position - cursor).sqrMagnitude;
            float db = ((Vector2)b.transform.position - cursor).sqrMagnitude;
            return da.CompareTo(db);
        });

        int take = Mathf.Min(room, nearby.Count);
        for (int i = 0; i < take; i++)
        {
            DesktopIcon icon = nearby[i];
            Vector2 offset = (Vector2)icon.transform.position - cursor;
            icon.IsCarried = true;
            icon.IsCursorInfluenced = false;
            carried[icon] = Vector2.ClampMagnitude(offset, clumpRadius);
            ringPulse = 1f;
        }
    }

    private void MoveCarried(Vector2 cursor)
    {
        float k = 1f - Mathf.Exp(-followSharpness * Time.deltaTime);
        foreach (KeyValuePair<DesktopIcon, Vector2> pair in carried)
        {
            Transform t = pair.Key.transform;
            Vector2 next = Vector2.Lerp(t.position, cursor + pair.Value, k);
            t.position = new Vector3(next.x, next.y, t.position.z);
        }
    }

    private void AbsorbAll()
    {
        buffer.Clear();
        buffer.AddRange(carried.Keys);
        foreach (DesktopIcon icon in buffer)
        {
            Absorb(icon);
        }
    }

    private void Absorb(DesktopIcon icon)
    {
        carried.Remove(icon);
        icon.IsCarried = false;
        virus.Absorb(icon);
    }

    private void ReleaseAll()
    {
        foreach (DesktopIcon icon in carried.Keys)
        {
            if (icon != null)
            {
                icon.IsCarried = false;
            }
        }

        carried.Clear();
    }

    private void RemoveDestroyed()
    {
        buffer.Clear();
        foreach (DesktopIcon icon in carried.Keys)
        {
            if (icon == null || icon.IsConsumed)
            {
                buffer.Add(icon);
            }
        }

        foreach (DesktopIcon icon in buffer)
        {
            carried.Remove(icon);
        }
    }

    private void UpdateRing(Vector2 cursor, float radius, bool visible, bool collecting)
    {
        ring.enabled = visible;
        if (!visible)
        {
            return;
        }

        ringPulse = Mathf.Max(0f, ringPulse - Time.deltaTime * 4f);
        float spriteSize = Mathf.Max(ringSprite.bounds.size.x, 0.001f);
        float diameter = radius * 2f * (1f + ringPulse * 0.15f);

        ring.transform.position = new Vector3(cursor.x, cursor.y, 0f);
        ring.transform.localScale = Vector3.one * (diameter / spriteSize);
        ring.color = Color.Lerp(collecting ? ringActiveColor : ringColor, Color.white, ringPulse * 0.5f);
    }

    private void UpdatePullRing(Vector2 cursor, float radius, bool visible)
    {
        pullRing.enabled = visible && ringSprite != null;
        if (!pullRing.enabled)
        {
            return;
        }

        float spriteSize = Mathf.Max(ringSprite.bounds.size.x, 0.001f);
        pullRing.transform.position = new Vector3(cursor.x, cursor.y, 0f);
        pullRing.transform.localScale = Vector3.one * (radius * 2f / spriteSize);
        Color color = ringColor;
        color.a *= 0.55f;
        pullRing.color = color;
    }
}
