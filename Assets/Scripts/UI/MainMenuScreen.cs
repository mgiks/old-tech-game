using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MainMenuScreen : MonoBehaviour
{
    private const string RunSceneName = "Main";
    private const float DesignWidth = 320f;
    private const float CanvasWidth = 1920f;
    private const float CanvasHeight = 1080f;

    [SerializeField] private Sprite wallpaper;
    [SerializeField] private Sprite logo;
    [SerializeField] private Sprite newGameNormal;
    [SerializeField] private Sprite newGameHover;
    [SerializeField] private Sprite newGamePressed;
    [SerializeField] private Sprite newGameDisabled;
    [SerializeField] private Sprite exitNormal;
    [SerializeField] private Sprite exitHover;
    [SerializeField] private Sprite exitPressed;
    [SerializeField] private Sprite exitDisabled;

    private bool starting;

    void Awake()
    {
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;

        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(CanvasWidth, CanvasHeight);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 1f;

        gameObject.AddComponent<GraphicRaycaster>();

        BuildWallpaper();
        BuildLogo();
        ArtButton("New Game", "mainmenu.new_game", newGameNormal, newGameHover, newGamePressed, newGameDisabled, 121, 51, 76, 13, StartNewGame);
        ArtButton("Exit", "mainmenu.exit", exitNormal, exitHover, exitPressed, exitDisabled, 121, 85, 76, 13, ExitGame);
    }

    private void BuildWallpaper()
    {
        Image image = XpUi.Image("Wallpaper", transform, wallpaper, Color.white);
        XpUi.Place(image.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(CanvasWidth, CanvasHeight));
        image.preserveAspect = true;
    }

    private void BuildLogo()
    {
        if (logo == null)
        {
            return;
        }

        Image image = XpUi.Image("Logo", transform, logo, Color.white);
        PlaceDesign(image.rectTransform, 109, 30, 100, 17);
        image.preserveAspect = true;
    }

    private void ArtButton(string name, string key, Sprite normal, Sprite hover, Sprite pressed, Sprite disabled, int x, int y, int width, int height, UnityAction action)
    {
        Image image = XpUi.Image(name, transform, normal, Color.white, true);
        PlaceDesign(image.rectTransform, x, y, width, height);

        Button button = image.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.transition = Selectable.Transition.SpriteSwap;
        SpriteState state = button.spriteState;
        state.highlightedSprite = hover;
        state.pressedSprite = pressed;
        state.selectedSprite = hover;
        state.disabledSprite = disabled;
        button.spriteState = state;

        TextMeshProUGUI label = XpUi.Text("Label", image.transform, string.Empty, 26f, Color.white, TextAlignmentOptions.Center, FontStyles.Bold);
        XpUi.Stretch(label.rectTransform, 8f, 2f, 8f, 2f);
        label.outlineWidth = 0.2f;
        label.outlineColor = new Color32(0, 24, 72, 220);
        XpUi.Localize(label, key);
        button.onClick.AddListener(() =>
        {
            GameAudio.PlayMouseClick();
            action();
        });
    }

    private static void PlaceDesign(RectTransform rect, int x, int y, int width, int height)
    {
        float scale = CanvasWidth / DesignWidth;
        XpUi.Place(rect, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(x * scale, -y * scale), new Vector2(width * scale, height * scale));
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
