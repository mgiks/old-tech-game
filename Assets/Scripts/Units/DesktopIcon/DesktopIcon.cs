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

    private Vector3 offset;
    private Camera cam;

    void Awake()
    {
        cam = Camera.main;
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
