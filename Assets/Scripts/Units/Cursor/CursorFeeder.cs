using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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
    private Camera cam;
    private SpriteRenderer ring;
    private SpriteRenderer virusRenderer;
    private float ringPulse;

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
    }

    void OnDisable()
    {
        ReleaseAll();
        if (ring != null)
        {
            ring.enabled = false;
        }
    }

    void Update()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null || GamePause.IsPaused)
        {
            return;
        }

        RemoveDestroyed();

        Vector2 cursor = cam.ScreenToWorldPoint(mouse.position.ReadValue());
        bool hoverFeed = skillTree.GetStat(SkillStat.HoverFeed) > 0f;
        float grabRadius = skillTree.GetStat(SkillStat.CursorRadius);
        float radius = hoverFeed ? hoverBaseRadius + grabRadius : grabRadius;
        bool virusActive = virusRenderer.enabled;
        bool dragging = IsAnyIconDragged();
        bool collecting = virusActive && radius > 0f && (hoverFeed || dragging);

        UpdateRing(cursor, radius, virusActive && radius > 0f, collecting);

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
        float attractionRadius = skillTree.GetStat(SkillStat.AttractionRadius);

        Collect(cursor, radius, virusCenter, attractionRadius);
        MoveCarried(cursor);
        HandOverToAttraction(virusCenter, attractionRadius);

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

    private void HandOverToAttraction(Vector2 virusCenter, float attractionRadius)
    {
        if (attractionRadius <= 0f)
        {
            return;
        }

        buffer.Clear();
        foreach (DesktopIcon icon in carried.Keys)
        {
            if (((Vector2)icon.transform.position - virusCenter).sqrMagnitude <= attractionRadius * attractionRadius)
            {
                buffer.Add(icon);
            }
        }

        foreach (DesktopIcon icon in buffer)
        {
            carried.Remove(icon);
            icon.IsCarried = false;
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

    private void Collect(Vector2 cursor, float radius, Vector2 virusCenter, float attractionRadius)
    {
        float sqrRadius = radius * radius;
        float sqrAttraction = attractionRadius * attractionRadius;
        foreach (DesktopIcon icon in DesktopIcon.Active)
        {
            if (!icon.IsFree || ((Vector2)icon.transform.position - virusCenter).sqrMagnitude <= sqrAttraction)
            {
                continue;
            }

            Vector2 offset = (Vector2)icon.transform.position - cursor;
            if (offset.sqrMagnitude <= sqrRadius)
            {
                icon.IsCarried = true;
                carried[icon] = Vector2.ClampMagnitude(offset, clumpRadius);
                ringPulse = 1f;
            }
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
}
