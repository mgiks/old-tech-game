using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UiSwitch : MonoBehaviour, IPointerClickHandler
{
    private static readonly Color OnColor = new(0.22f, 0.66f, 0.27f, 1f);
    private static readonly Color OffColor = new(0.62f, 0.62f, 0.6f, 1f);

    private Image track;
    private RectTransform knob;
    private float position;

    public bool IsOn { get; private set; }

    public event Action<bool> Toggled;

    public static UiSwitch Create(Transform parent, Vector2 size)
    {
        Image track = XpUi.Image("Switch", parent, XpUi.Rounded(16), OffColor, true);
        track.rectTransform.sizeDelta = size;

        Image knob = XpUi.Image("Knob", track.transform, XpUi.Circle(), Color.white);
        float knobSize = size.y - 6f;
        XpUi.Place(knob.rectTransform, new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(knobSize, knobSize));

        UiSwitch control = track.gameObject.AddComponent<UiSwitch>();
        control.track = track;
        control.knob = knob.rectTransform;
        return control;
    }

    public void SetIsOn(bool value)
    {
        if (value == IsOn)
        {
            return;
        }

        IsOn = value;
        position = value ? 1f : 0f;
        Apply();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        GameAudio.PlayMouseClick();
        IsOn = !IsOn;
        Toggled?.Invoke(IsOn);
    }

    void Update()
    {
        float target = IsOn ? 1f : 0f;
        if (!Mathf.Approximately(position, target))
        {
            position = Mathf.MoveTowards(position, target, Time.unscaledDeltaTime * 8f);
            Apply();
        }
    }

    private void Apply()
    {
        float width = ((RectTransform)transform).rect.width;
        float inset = 3f + knob.sizeDelta.x * 0.5f;
        knob.anchoredPosition = new Vector2(Mathf.Lerp(inset, width - inset, position), 0f);
        track.color = Color.Lerp(OffColor, OnColor, position);
    }
}
