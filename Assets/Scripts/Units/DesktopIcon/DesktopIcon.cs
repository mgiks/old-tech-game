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
    public bool IsCarried { get; set; }
    public bool IsFree => !IsDragged && !IsCarried && !IsConsumed;

    [SerializeField] private float popInDuration = 0.25f;
    [SerializeField] private Color goldenColor = new(1f, 0.82f, 0.25f, 1f);
    [SerializeField] private Color chainedColor = new(0.85f, 0.35f, 1f, 1f);

    private Vector3 offset;
    private Camera cam;
    private SpriteRenderer spriteRenderer;
    private Vector3 baseScale;
    private float age;

    void Awake()
    {
        cam = Camera.main;
        spriteRenderer = GetComponent<SpriteRenderer>();
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

        if (IsGolden)
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

    public void MarkChained()
    {
        if (!IsGolden)
        {
            spriteRenderer.color = chainedColor;
        }
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
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        IsDragged = false;
        TryFeedVirus();
    }

    public void OnDrag(PointerEventData eventData)
    {
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
        Vector3 mousePoint = Mouse.current.position.ReadValue();
        Vector3 worldPoint = cam.ScreenToWorldPoint(mousePoint);

        return worldPoint;
    }
}
