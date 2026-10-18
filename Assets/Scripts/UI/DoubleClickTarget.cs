using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DoubleClickTarget : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Graphic selectionHighlight;
    [SerializeField] private float doubleClickTime = 0.4f;

    public event Action DoubleClicked;

    private float lastClickTime = -10f;

    void Awake()
    {
        SetSelected(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        float now = Time.unscaledTime;
        if (now - lastClickTime <= doubleClickTime)
        {
            lastClickTime = -10f;
            DoubleClicked?.Invoke();
            return;
        }

        lastClickTime = now;
        SetSelected(true);
    }

    public void SetSelected(bool selected)
    {
        if (selectionHighlight != null)
        {
            selectionHighlight.enabled = selected;
        }
    }
}
