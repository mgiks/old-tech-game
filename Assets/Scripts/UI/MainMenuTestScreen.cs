using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuTestScreen : MonoBehaviour
{
    void Awake()
    {
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 1f;
        gameObject.AddComponent<GraphicRaycaster>();

        Image background = XpUi.Image("Background", transform, XpUi.Panel("mainMenuBack", 4, 64, 0, XpCorners.None,
            XpUi.Hex("#0B2A5C"), XpUi.Hex("#06142E")), Color.white);
        XpUi.Stretch(background.rectTransform);

        TextMeshProUGUI title = XpUi.Text("Title", transform, string.Empty, 72f, Color.white, TextAlignmentOptions.Center, FontStyles.Bold);
        XpUi.Place(title.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 120f), new Vector2(1400f, 100f));
        XpUi.Localize(title, "mainmenu.title");

        TextMeshProUGUI subtitle = XpUi.Text("Subtitle", transform, string.Empty, 30f, new Color(1f, 1f, 1f, 0.7f), TextAlignmentOptions.Center);
        XpUi.Place(subtitle.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 40f), new Vector2(1400f, 50f));
        XpUi.Localize(subtitle, "mainmenu.subtitle");

        Button back = XpUi.XpButton("Back", transform, "mainmenu.back", new Vector2(280f, 52f));
        XpUi.Place((RectTransform)back.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -80f), new Vector2(280f, 52f));
        back.GetComponentInChildren<TMP_Text>().fontSize = 24f;
        back.onClick.AddListener(Taskbar.ReturnFromMainMenu);
    }
}
