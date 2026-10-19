using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class XpHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Image background;
    private TMP_Text label;
    private Color idleText;

    public static void Attach(Image background, TMP_Text label)
    {
        XpHover hover = background.gameObject.AddComponent<XpHover>();
        hover.background = background;
        hover.label = label;
        hover.idleText = label.color;
        hover.Set(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Set(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Set(false);
    }

    void OnDisable()
    {
        Set(false);
    }

    private void Set(bool hovered)
    {
        if (background == null)
        {
            return;
        }

        background.color = hovered ? XpUi.HoverBlue : new Color(1f, 1f, 1f, 0f);
        label.color = hovered ? Color.white : idleText;
    }
}
