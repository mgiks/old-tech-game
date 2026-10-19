using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Taskbar : MonoBehaviour
{
    public const float Height = 44f;
    public const string MainMenuScene = "Main Menu Test";

    private const float MenuWidth = 460f;
    private const float MenuHeight = 540f;

    private enum State
    {
        Closed,
        Menu,
        Settings
    }

    private static Taskbar instance;

    private Canvas canvas;
    private GameObject blocker;
    private GameObject menu;
    private SettingsWindow settings;
    private Image startFace;
    private TextMeshProUGUI clock;
    private TextMeshProUGUI mbLabel;
    private TextMeshProUGUI runLabel;
    private State state;
    private float nextClockUpdate;

    public static string ReturnScene { get; private set; }

    public static bool IsOpen => instance != null && instance.state != State.Closed;

    public static float ScreenFraction => instance != null && instance.canvas != null && Screen.height > 0
        ? Height * instance.canvas.scaleFactor / Screen.height
        : 0f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetSession()
    {
        instance = null;
        ReturnScene = null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Create()
    {
        if (instance != null)
        {
            return;
        }

        GameObject go = new("Taskbar");
        DontDestroyOnLoad(go);
        instance = go.AddComponent<Taskbar>();
    }

    void Awake()
    {
        canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;

        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 1f;

        gameObject.AddComponent<GraphicRaycaster>();

        BuildBlocker();
        BuildBar();
        BuildMenu();
        settings = SettingsWindow.Create(transform, CloseSettings);
        settings.gameObject.SetActive(false);
        SetState(State.Closed);
    }

    void OnEnable()
    {
        GameSettings.Changed += RefreshMenuInfo;
    }

    void OnDisable()
    {
        GameSettings.Changed -= RefreshMenuInfo;
    }

    void Update()
    {
        if (Time.unscaledTime >= nextClockUpdate)
        {
            nextClockUpdate = Time.unscaledTime + 1f;
            clock.text = DateTime.Now.ToString("HH:mm");
        }

        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
        {
            HandleEscape();
        }
    }

    private void HandleEscape()
    {
        switch (state)
        {
            case State.Settings:
                CloseSettings();
                break;
            case State.Menu:
                SetState(State.Closed);
                break;
            default:
                SetState(State.Menu);
                break;
        }
    }

    private void ToggleMenu()
    {
        SetState(state == State.Closed ? State.Menu : State.Closed);
    }

    private void CloseSettings()
    {
        SetState(State.Menu);
    }

    private void SetState(State next)
    {
        state = next;
        bool open = next != State.Closed;
        blocker.SetActive(open);
        menu.SetActive(next == State.Menu);
        settings.gameObject.SetActive(next == State.Settings);
        startFace.color = open ? new Color(0.78f, 0.85f, 0.78f, 1f) : Color.white;

        if (open)
        {
            GamePause.Pause();
            RefreshMenuInfo();
        }
        else
        {
            GamePause.Resume();
        }
    }

    private void OpenMainMenu()
    {
        string current = SceneManager.GetActiveScene().name;
        SetState(State.Closed);
        if (current == MainMenuScene)
        {
            return;
        }

        ReturnScene = current;
        Time.timeScale = 1f;
        ViewManager.Switch(current, MainMenuScene);
    }

    public static void OpenMenu()
    {
        if (instance != null && instance.state == State.Closed)
        {
            instance.SetState(State.Menu);
        }
    }

    public static void ReturnFromMainMenu()
    {
        string target = string.IsNullOrEmpty(ReturnScene) ? "Main" : ReturnScene;
        Time.timeScale = 1f;
        ViewManager.Switch(MainMenuScene, target);
    }

    private static void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void RefreshMenuInfo()
    {
        if (mbLabel == null)
        {
            return;
        }

        mbLabel.text = Loc.F("menu.mb", GameProgress.MB);
        runLabel.text = Loc.F("menu.run", Mathf.Max(1, RunStats.RunNumber));
    }

    private void BuildBlocker()
    {
        Image image = XpUi.Image("Blocker", transform, null, new Color(0f, 0f, 0f, 0.25f), true);
        XpUi.Stretch(image.rectTransform);
        Button button = image.gameObject.AddComponent<Button>();
        button.transition = Selectable.Transition.None;
        button.onClick.AddListener(() =>
        {
            if (state == State.Menu)
            {
                SetState(State.Closed);
            }
        });
        blocker = image.gameObject;
    }

    private void BuildBar()
    {
        Image bar = XpUi.Image("Bar", transform, XpUi.Panel("xpTaskbar", 4, 44, 0, XpCorners.None,
            XpUi.Hex("#3A81F3"), XpUi.Hex("#2663E0"), XpUi.Hex("#245EDB"), XpUi.Hex("#245CD6"), XpUi.Hex("#1F4FC1"), XpUi.Hex("#1941A5")), Color.white, true);
        bar.rectTransform.anchorMin = Vector2.zero;
        bar.rectTransform.anchorMax = new Vector2(1f, 0f);
        bar.rectTransform.pivot = new Vector2(0.5f, 0f);
        bar.rectTransform.anchoredPosition = Vector2.zero;
        bar.rectTransform.sizeDelta = new Vector2(0f, Height);

        BuildStartButton(bar.transform);
        BuildTaskButton(bar.transform);
        BuildTray(bar.transform);
    }

    private void BuildStartButton(Transform bar)
    {
        startFace = XpUi.Image("Start Button", bar, XpUi.Panel("xpStart", 64, 64, 24, XpCorners.Right,
            XpUi.Hex("#79C879"), XpUi.Hex("#3EA63E"), XpUi.Hex("#349A34"), XpUi.Hex("#2E8B2E"), XpUi.Hex("#277727")), Color.white, true);
        startFace.pixelsPerUnitMultiplier = 1.4f;
        XpUi.Place(startFace.rectTransform, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(150f, Height));

        RectTransform flag = XpUi.Rect("Flag", startFace.transform);
        XpUi.Place(flag, new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(30f, 0f), new Vector2(26f, 26f));
        flag.localRotation = Quaternion.Euler(0f, 0f, -6f);
        Image shadow = XpUi.Image("Shadow", flag, XpUi.Rounded(4), new Color(0f, 0f, 0f, 0.3f));
        XpUi.Place(shadow.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(1.5f, -1.5f), new Vector2(26f, 26f));
        FlagPiece(flag, new Vector2(-6.5f, 6.5f), XpUi.Hex("#F25022"));
        FlagPiece(flag, new Vector2(6.5f, 6.5f), XpUi.Hex("#7FBA00"));
        FlagPiece(flag, new Vector2(-6.5f, -6.5f), XpUi.Hex("#00A4EF"));
        FlagPiece(flag, new Vector2(6.5f, -6.5f), XpUi.Hex("#FFB900"));

        RectTransform label = XpUi.Rect("Label", startFace.transform);
        XpUi.Stretch(label, 50f, 0f, 8f, 2f);
        XpUi.ShadowText("Text", label, "start.button", 28f, Color.white, TextAlignmentOptions.MidlineLeft, FontStyles.Bold | FontStyles.Italic, new Vector2(1.5f, -1.5f));

        Button button = startFace.gameObject.AddComponent<Button>();
        button.transition = Selectable.Transition.None;
        button.onClick.AddListener(ToggleMenu);
    }

    private static void FlagPiece(Transform flag, Vector2 position, Color color)
    {
        Image piece = XpUi.Image("Piece", flag, XpUi.Rounded(3), color);
        XpUi.Place(piece.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), position, new Vector2(11.5f, 11.5f));
    }

    private static void BuildTaskButton(Transform bar)
    {
        Image border = XpUi.Image("Task Button", bar, XpUi.Rounded(4), XpUi.Hex("#1C4FB8"));
        XpUi.Place(border.rectTransform, Vector2.zero, new Vector2(0f, 0.5f), new Vector2(162f, Height * 0.5f), new Vector2(230f, 34f));
        Image face = XpUi.Image("Face", border.transform, XpUi.Panel("xpTask", 16, 34, 3, XpCorners.All,
            XpUi.Hex("#1E4FB5"), XpUi.Hex("#2C63D3"), XpUi.Hex("#2A5FCC")), Color.white);
        XpUi.Stretch(face.rectTransform, 1f, 1f, 1f, 1f);

        Image icon = XpUi.Image("Icon", border.transform, XpUi.Circle(), XpUi.Hex("#59E35A"));
        XpUi.Place(icon.rectTransform, new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(20f, 0f), new Vector2(20f, 20f));
        GlitchEyes(icon.transform, 20f);

        TextMeshProUGUI label = XpUi.Text("Label", border.transform, string.Empty, 19f, Color.white);
        XpUi.Stretch(label.rectTransform, 38f, 0f, 8f, 0f);
        XpUi.Localize(label, "start.task");
    }

    private void BuildTray(Transform bar)
    {
        Image tray = XpUi.Image("Tray", bar, XpUi.Panel("xpTray", 4, 44, 0, XpCorners.None,
            XpUi.Hex("#1C9BF0"), XpUi.Hex("#0F8AE6"), XpUi.Hex("#0D7FDD"), XpUi.Hex("#0C6CC9")), Color.white);
        XpUi.Place(tray.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 0f), Vector2.zero, new Vector2(120f, Height));

        Image edge = XpUi.Image("Edge", tray.transform, null, XpUi.Hex("#0A3C9C"));
        XpUi.Place(edge.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), Vector2.zero, new Vector2(1.5f, Height));
        Image highlight = XpUi.Image("Highlight", tray.transform, null, new Color(1f, 1f, 1f, 0.35f));
        XpUi.Place(highlight.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(1.5f, 0f), new Vector2(1f, Height));

        clock = XpUi.Text("Clock", tray.transform, "00:00", 20f, Color.white, TextAlignmentOptions.Center);
        XpUi.Stretch(clock.rectTransform, 6f, 0f, 0f, 0f);
    }

    private void BuildMenu()
    {
        RectTransform root = XpUi.Rect("Start Menu", transform);
        XpUi.Place(root, Vector2.zero, Vector2.zero, new Vector2(0f, Height), new Vector2(MenuWidth, MenuHeight));
        menu = root.gameObject;

        Image frame = XpUi.Image("Frame", root, XpUi.Panel("xpMenuFrame", 32, 32, 9, XpCorners.Top, XpUi.Hex("#1C55C7")), Color.white, true);
        XpUi.Stretch(frame.rectTransform);

        const float headerHeight = 76f;
        const float footerHeight = 50f;

        Image header = XpUi.Image("Header", root, XpUi.Panel("xpMenuHeader", 32, 76, 8, XpCorners.Top,
            XpUi.Hex("#4A8FF2"), XpUi.Hex("#2A6AE0"), XpUi.Hex("#2462D6"), XpUi.Hex("#1C55C7")), Color.white);
        header.rectTransform.anchorMin = new Vector2(0f, 1f);
        header.rectTransform.anchorMax = Vector2.one;
        header.rectTransform.pivot = new Vector2(0.5f, 1f);
        header.rectTransform.anchoredPosition = Vector2.zero;
        header.rectTransform.sizeDelta = new Vector2(0f, headerHeight);

        Image avatarFrame = XpUi.Image("Avatar", header.transform, XpUi.Rounded(6), Color.white);
        XpUi.Place(avatarFrame.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(12f, 0f), new Vector2(56f, 56f));
        Image avatarBack = XpUi.Image("Back", avatarFrame.transform, XpUi.Panel("xpAvatar", 16, 32, 4, XpCorners.All, XpUi.Hex("#1B1B2E"), XpUi.Hex("#0D3B1E")), Color.white);
        XpUi.Stretch(avatarBack.rectTransform, 3f, 3f, 3f, 3f);
        Image virus = XpUi.Image("Virus", avatarFrame.transform, XpUi.Circle(), XpUi.Hex("#59E35A"));
        XpUi.Place(virus.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(36f, 36f));
        GlitchEyes(virus.transform, 36f);

        RectTransform name = XpUi.Rect("Name", header.transform);
        XpUi.Stretch(name, 82f, 0f, 12f, 0f);
        XpUi.ShadowText("Text", name, "start.user", 28f, Color.white, TextAlignmentOptions.MidlineLeft, FontStyles.Bold, new Vector2(1.5f, -1.5f));

        Image orange = XpUi.Image("Orange Line", root, null, XpUi.Hex("#E68B2C"));
        orange.rectTransform.anchorMin = new Vector2(0f, 1f);
        orange.rectTransform.anchorMax = Vector2.one;
        orange.rectTransform.pivot = new Vector2(0.5f, 1f);
        orange.rectTransform.anchoredPosition = new Vector2(0f, -headerHeight);
        orange.rectTransform.sizeDelta = new Vector2(-4f, 3f);

        float bodyTop = headerHeight + 3f;
        const float leftWidth = 270f;

        Image left = XpUi.Image("Left Column", root, null, Color.white);
        left.rectTransform.anchorMin = Vector2.zero;
        left.rectTransform.anchorMax = new Vector2(0f, 1f);
        left.rectTransform.offsetMin = new Vector2(2f, footerHeight);
        left.rectTransform.offsetMax = new Vector2(2f + leftWidth, -bodyTop);

        Image right = XpUi.Image("Right Column", root, null, XpUi.Hex("#D3E5FA"));
        XpUi.Stretch(right.rectTransform, 2f + leftWidth, footerHeight, 2f, bodyTop);
        Image divider = XpUi.Image("Divider", right.transform, null, XpUi.Hex("#95BDEE"));
        XpUi.Place(divider.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), Vector2.zero, Vector2.zero);
        divider.rectTransform.anchorMin = Vector2.zero;
        divider.rectTransform.anchorMax = new Vector2(0f, 1f);
        divider.rectTransform.sizeDelta = new Vector2(1.5f, 0f);

        float y = -10f;
        MenuItem(left.transform, "menu.continue", Icon.Continue, ref y, () => SetState(State.Closed));
        MenuItem(left.transform, "menu.settings", Icon.Settings, ref y, () => SetState(State.Settings));
        MenuItem(left.transform, "menu.main_menu", Icon.MainMenu, ref y, OpenMainMenu);
        Image separator = XpUi.Image("Separator", left.transform, null, XpUi.Hex("#D5D2CA"));
        XpUi.Place(separator.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f), new Vector2(0f, y - 8f), new Vector2(leftWidth - 24f, 1.5f));
        y -= 16f;
        MenuItem(left.transform, "menu.quit", Icon.Quit, ref y, QuitGame);

        BuildInfoColumn(right.transform);

        Image footer = XpUi.Image("Footer", root, XpUi.Panel("xpMenuFooter", 4, 50, 0, XpCorners.None,
            XpUi.Hex("#3D7FE8"), XpUi.Hex("#2462D6"), XpUi.Hex("#1C55C7")), Color.white);
        footer.rectTransform.anchorMin = Vector2.zero;
        footer.rectTransform.anchorMax = new Vector2(1f, 0f);
        footer.rectTransform.pivot = new Vector2(0.5f, 0f);
        footer.rectTransform.anchoredPosition = Vector2.zero;
        footer.rectTransform.sizeDelta = new Vector2(0f, footerHeight);

        TextMeshProUGUI hint = XpUi.Text("Hint", footer.transform, string.Empty, 19f, Color.white, TextAlignmentOptions.MidlineRight);
        XpUi.Stretch(hint.rectTransform, 12f, 0f, 16f, 0f);
        XpUi.Localize(hint, "menu.hint");
    }

    private void BuildInfoColumn(Transform column)
    {
        Image monitor = XpUi.Image("Monitor", column, XpUi.Rounded(4), XpUi.Hex("#5B7FBF"));
        XpUi.Place(monitor.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(14f, -18f), new Vector2(34f, 28f));
        Image screen = XpUi.Image("Screen", monitor.transform, null, XpUi.Hex("#3A9BE8"));
        XpUi.Stretch(screen.rectTransform, 4f, 6f, 4f, 3f);
        Image stand = XpUi.Image("Stand", monitor.transform, null, XpUi.Hex("#5B7FBF"));
        XpUi.Place(stand.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 1f), Vector2.zero, new Vector2(16f, 5f));

        TextMeshProUGUI title = XpUi.Text("Title", column, string.Empty, 20f, XpUi.TextDark, TextAlignmentOptions.MidlineLeft, FontStyles.Bold);
        XpUi.Place(title.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(54f, -14f), new Vector2(130f, 36f));
        title.textWrappingMode = TextWrappingModes.Normal;
        title.fontSizeMin = 14f;
        title.enableAutoSizing = true;
        title.fontSizeMax = 20f;
        XpUi.Localize(title, "menu.my_computer");

        mbLabel = InfoLine(column, -70f);
        runLabel = InfoLine(column, -104f);

        TextMeshProUGUI paused = InfoLine(column, -150f);
        paused.fontStyle = FontStyles.Italic;
        paused.color = XpUi.Hex("#C0392B");
        XpUi.Localize(paused, "menu.paused");
    }

    private static TextMeshProUGUI InfoLine(Transform column, float y)
    {
        TextMeshProUGUI text = XpUi.Text("Info", column, string.Empty, 18f, XpUi.TextDark);
        XpUi.Place(text.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(14f, y), new Vector2(160f, 30f));
        return text;
    }

    private enum Icon
    {
        Continue,
        Settings,
        MainMenu,
        Quit
    }

    private static void MenuItem(Transform column, string key, Icon icon, ref float y, Action onClick)
    {
        const float height = 66f;
        Image background = XpUi.Image("Item " + key, column, XpUi.Rounded(3), Color.clear, true);
        background.rectTransform.anchorMin = new Vector2(0f, 1f);
        background.rectTransform.anchorMax = Vector2.one;
        background.rectTransform.pivot = new Vector2(0.5f, 1f);
        background.rectTransform.anchoredPosition = new Vector2(0f, y);
        background.rectTransform.sizeDelta = new Vector2(-12f, height);
        y -= height + 2f;

        RectTransform iconRoot = XpUi.Rect("Icon", background.transform);
        XpUi.Place(iconRoot, new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(32f, 0f), new Vector2(42f, 42f));
        DrawIcon(iconRoot, icon);

        TextMeshProUGUI label = XpUi.Text("Label", background.transform, string.Empty, 22f, Color.black,
            TextAlignmentOptions.MidlineLeft, icon == Icon.Quit ? FontStyles.Normal : FontStyles.Bold);
        XpUi.Stretch(label.rectTransform, 64f, 0f, 6f, 0f);
        XpUi.Localize(label, key);

        XpHover.Attach(background, label);
        Button button = background.gameObject.AddComponent<Button>();
        button.transition = Selectable.Transition.None;
        button.onClick.AddListener(() => onClick());
    }

    private static void DrawIcon(RectTransform root, Icon icon)
    {
        switch (icon)
        {
            case Icon.Continue:
            {
                Image disc = XpUi.Image("Disc", root, XpUi.Circle(), XpUi.Hex("#2FA84F"));
                XpUi.Stretch(disc.rectTransform);
                Image play = XpUi.Image("Play", root, XpUi.Triangle(), Color.white);
                XpUi.Place(play.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(2f, 0f), new Vector2(24f, 24f));
                break;
            }
            case Icon.Settings:
            {
                Image tile = XpUi.Image("Tile", root, XpUi.Rounded(6), XpUi.Hex("#5D7FC0"));
                XpUi.Stretch(tile.rectTransform);
                for (int i = 0; i < 4; i++)
                {
                    Image tooth = XpUi.Image("Tooth", root, null, Color.white);
                    XpUi.Place(tooth.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(30f, 7f));
                    tooth.rectTransform.localRotation = Quaternion.Euler(0f, 0f, i * 45f);
                }

                Image gear = XpUi.Image("Gear", root, XpUi.Circle(), Color.white);
                XpUi.Place(gear.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(22f, 22f));
                Image hole = XpUi.Image("Hole", root, XpUi.Circle(), XpUi.Hex("#5D7FC0"));
                XpUi.Place(hole.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(9f, 9f));
                break;
            }
            case Icon.MainMenu:
            {
                Image tile = XpUi.Image("Tile", root, XpUi.Rounded(6), XpUi.Hex("#E39A2D"));
                XpUi.Stretch(tile.rectTransform);
                Image roof = XpUi.Image("Roof", root, XpUi.Triangle(), Color.white);
                XpUi.Place(roof.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 6f), new Vector2(26f, 30f));
                roof.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                Image house = XpUi.Image("House", root, null, Color.white);
                XpUi.Place(house.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -7f), new Vector2(18f, 14f));
                Image door = XpUi.Image("Door", root, null, XpUi.Hex("#E39A2D"));
                XpUi.Place(door.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -10f), new Vector2(6f, 8f));
                break;
            }
            case Icon.Quit:
            {
                Image tile = XpUi.Image("Tile", root, XpUi.Rounded(6), XpUi.Hex("#D9452B"));
                XpUi.Stretch(tile.rectTransform);
                Image ring = XpUi.Image("Ring", root, XpUi.Ring(0.22f), Color.white);
                XpUi.Place(ring.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -2f), new Vector2(26f, 26f));
                Image gap = XpUi.Image("Gap", root, null, XpUi.Hex("#D9452B"));
                XpUi.Place(gap.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 9f), new Vector2(10f, 12f));
                Image bar = XpUi.Image("Bar", root, XpUi.Rounded(2), Color.white);
                XpUi.Place(bar.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 6f), new Vector2(4.5f, 15f));
                break;
            }
        }
    }

    private static void GlitchEyes(Transform virus, float size)
    {
        float eye = size * 0.18f;
        for (int side = -1; side <= 1; side += 2)
        {
            Image image = XpUi.Image("Eye", virus, XpUi.Circle(), Color.black);
            XpUi.Place(image.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(side * size * 0.17f, size * 0.08f), new Vector2(eye, eye));
        }
    }
}
