using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class DesktopIcon : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    private static readonly List<DesktopIcon> active = new();

    public static IReadOnlyList<DesktopIcon> Active => active;

    public bool IsDragged { get; private set; }
    public bool IsConsumed { get; private set; }
    public bool IsGolden { get; private set; }
    public bool IsConfidential { get; private set; }
    public bool IsCarried { get; set; }
    public bool IsReserved { get; set; }
    public bool IsCursorInfluenced { get; set; }
    public bool IsFree => !IsDragged && !IsCarried && !IsConsumed && !IsReserved;

    [SerializeField] private float popInDuration = 0.25f;
    [SerializeField] private Color goldenColor = new(1f, 0.82f, 0.25f, 1f);
    [SerializeField] private Color confidentialColor = new(0.62f, 0.18f, 0.95f, 1f);
    [SerializeField] private Color stampColor = new(0.95f, 0.12f, 0.22f, 0.95f);
    [SerializeField] private Sprite[] iconVariants;

    private Vector3 offset;
    private Camera cam;
    private SpriteRenderer spriteRenderer;
    private Vector3 baseScale;
    private float age;

    void Awake()
    {
        cam = Camera.main;
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (iconVariants != null && iconVariants.Length > 0)
        {
            spriteRenderer.sprite = iconVariants[Random.Range(0, iconVariants.Length)];
        }

        baseScale = transform.localScale;
        transform.localScale = Vector3.zero;
    }

    void Update()
    {
        age += Time.deltaTime;

        if (age < popInDuration)
        {
            float t = age / popInDuration;
            float overshoot = 1f + Mathf.Sin(t * Mathf.PI) * 0.35f;
            transform.localScale = baseScale * (t * overshoot);
        }
        else if (transform.localScale != baseScale)
        {
            transform.localScale = baseScale;
        }

        if (IsConfidential)
        {
            float pulse = 0.5f + 0.5f * Mathf.Sin(age * 6f);
            spriteRenderer.color = Color.Lerp(confidentialColor, Color.white, pulse * 0.35f);
        }
        else if (IsGolden)
        {
            float shimmer = 0.5f + 0.5f * Mathf.Sin(age * 8f);
            spriteRenderer.color = Color.Lerp(goldenColor, Color.white, shimmer * 0.45f);
        }
    }

    public void MakeGolden()
    {
        IsGolden = true;
        baseScale *= 1.15f;
    }

    public void MakeConfidential()
    {
        IsConfidential = true;
        baseScale *= 1.3f;
        GameObject stamp = new("Classified Stamp", typeof(SpriteRenderer));
        stamp.transform.SetParent(transform, false);
        stamp.transform.localPosition = new Vector3(0f, -0.12f, 0f);
        stamp.transform.localScale = new Vector3(0.55f, 0.18f, 1f);
        SpriteRenderer mark = stamp.GetComponent<SpriteRenderer>();
        mark.sprite = spriteRenderer.sprite;
        mark.color = stampColor;
        mark.sortingLayerID = spriteRenderer.sortingLayerID;
        mark.sortingOrder = spriteRenderer.sortingOrder + 1;
        mark.sharedMaterial = spriteRenderer.sharedMaterial;
    }

    void OnEnable()
    {
        active.Add(this);
    }

    void OnDisable()
    {
        active.Remove(this);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        offset = transform.position - GetPointerWorldPos();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        IsDragged = true;
        ArtBindings art = ArtBindings.Load();
        if (art != null && art.cursorGrab != null)
        {
            Cursor.SetCursor(art.cursorGrab, art.cursorGrabHotspot, CursorMode.Auto);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        IsDragged = false;
        ArtBindings art = ArtBindings.Load();
        if (art != null && art.cursorDefault != null)
        {
            Cursor.SetCursor(art.cursorDefault, art.cursorDefaultHotspot, CursorMode.Auto);
        }

        if (!GamePause.IsPaused)
        {
            TryFeedVirus();
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (GamePause.IsPaused)
        {
            return;
        }

        transform.position = GetPointerWorldPos() + offset;
    }

    public bool TryConsume()
    {
        if (IsConsumed)
        {
            return false;
        }

        IsConsumed = true;
        RunStats.AddFile();
        return true;
    }

    private void TryFeedVirus()
    {
        Physics2D.SyncTransforms();

        Virus virus = FindVirusAt(GetPointerWorldPos());
        if (virus == null)
        {
            virus = FindVirusAt(transform.position);
        }

        if (virus != null)
        {
            virus.Absorb(this);
        }
    }

    private static Virus FindVirusAt(Vector2 point)
    {
        foreach (Collider2D hit in Physics2D.OverlapPointAll(point))
        {
            if (hit.TryGetComponent(out Virus virus))
            {
                return virus;
            }
        }

        return null;
    }

    private Vector3 GetPointerWorldPos()
    {
        return MonitorScreen.ToWorld(cam);
    }
}
