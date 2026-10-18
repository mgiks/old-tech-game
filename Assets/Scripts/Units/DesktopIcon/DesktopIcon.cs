using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class DesktopIcon : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    private Vector3 offset;
    private Camera cam;

    void Awake()
    {
        cam = Camera.main;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        offset = transform.position - GetPointerWorldPos();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
    }

    public void OnEndDrag(PointerEventData eventData)
    {
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = GetPointerWorldPos() + offset;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.gameObject.CompareTag("Virus"))
        {
            return;
        }

        Destroy(gameObject);
    }

    private Vector3 GetPointerWorldPos()
    {
        Vector3 mousePoint = Mouse.current.position.ReadValue();
        Vector3 worldPoint = cam.ScreenToWorldPoint(mousePoint);

        return worldPoint;
    }
}