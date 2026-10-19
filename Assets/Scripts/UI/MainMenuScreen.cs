using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MainMenuScreen : MonoBehaviour
{
    private const string RunSceneName = "Main";
    private const float WindowWidth = 460f;
    private const float WindowHeight = 300f;
    private const float TitleHeight = 36f;

    [SerializeField] private Sprite wallpaper;

    private bool starting;

    void Awake()
    {
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;

        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 1f;

        gameObject.AddComponent<GraphicRaycaster>();

        BuildWallpaper();
        BuildWindow();
    }

    private void BuildWallpaper()
    {
        Image image = XpUi.Image("Wallpaper", transform, wallpaper, Color.white);
        XpUi.Stretch(image.rectTransform);
        image.preserveAspect = false;
    }

    private void BuildWindow()
    {
        ArtBindings art = ArtBindings.Load();

        Image frame = XpUi.Image("Menu Window", transform, null, Color.white, true);
        if (art != null && art.windowFrame != null)
        {
            ArtBindings.ApplySlice(frame, art.windowFrame);
        }
        else
        {
            frame.sprite = XpUi.Panel("xpWindowFrame", 32, 32, 9, XpCorners.Top, XpUi.WindowFrame);
            frame.type = Image.Type.Sliced;
        }

        XpUi.Place(frame.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, Taskbar.Height * 0.5f), new Vector2(WindowWidth, WindowHeight));

        Image title = XpUi.Image("Title Bar", frame.transform, null, Color.white);
        if (art != null && art.titlebarActive != null)
        {
            ArtBindings.ApplySlice(title, art.titlebarActive);
        }
        else
        {
            title.sprite = XpUi.Panel("xpTitle", 32, 36, 8, XpCorners.Top, XpUi.Hex("#3D95FF"), XpUi.Hex("#0A5FEA"));
            title.type = Image.Type.Sliced;
        }

        title.rectTransform.anchorMin = new Vector2(0f, 1f);
        title.rectTransform.anchorMax = Vector2.one;
        title.rectTransform.pivot = new Vector2(0.5f, 1f);
        title.rectTransform.anchoredPosition = Vector2.zero;
        title.rectTransform.sizeDelta = new Vector2(0f, TitleHeight);

        RectTransform titleTextRoot = XpUi.Rect("Title Text", title.transform);
        XpUi.Stretch(titleTextRoot, 14f, 0f, 14f, 0f);
        XpUi.ShadowText("Title", titleTextRoot, "mainmenu.title", 22f, Color.white, TextAlignmentOptions.MidlineLeft, FontStyles.Bold, new Vector2(1.5f, -1.5f));

        Image body = XpUi.Image("Body", frame.transform, null, XpUi.WindowBody);
        if (art != null && art.settingsBody != null)
        {
            ArtBindings.ApplySlice(body, art.settingsBody);
        }

        XpUi.Stretch(body.rectTransform, 4f, 4f, 4f, TitleHeight);

        if (art != null && art.virus != null)
        {
            Image icon = XpUi.Image("Virus", body.transform, art.virus, Color.white);
            XpUi.Place(icon.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -18f), new Vector2(72f, 72f));
            icon.preserveAspect = true;
        }

        MenuButton(body.transform, "mainmenu.new_game", -118f, StartNewGame);
        MenuButton(body.transform, "mainmenu.exit", -186f, ExitGame);
    }

    private static void MenuButton(Transform parent, string key, float y, UnityAction action)
    {
        Button button = XpUi.XpButton(key, parent, key, new Vector2(280f, 48f));
        XpUi.Place((RectTransform)button.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, y), new Vector2(280f, 48f));
        ArtBindings.StyleXpButton(button);
        TextMeshProUGUI label = button.GetComponentInChildren<TextMeshProUGUI>();
        if (label != null)
        {
            label.fontSize = 22f;
        }

        button.onClick.AddListener(() =>
        {
            GameAudio.PlayMouseClick();
            action();
        });
    }

    private void StartNewGame()
    {
        if (starting)
        {
            return;
        }

        starting = true;
        GamePause.Resume();
        Time.timeScale = 1f;
        ViewManager.Switch(gameObject.scene.name, RunSceneName);
    }

    private static void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
