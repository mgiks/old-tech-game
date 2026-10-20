using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GameEnding : MonoBehaviour
{
    [SerializeField] private Virus virus;
    [SerializeField] private DesktopIconManager iconManager;
    [SerializeField] private EatFeedback feedback;
    [SerializeField] private TMP_FontAsset font;
    [SerializeField] private string skillTreeSceneName = "Skill Tree";

    [Header("Overload")]
    [SerializeField] private float overloadDuration = 3.2f;
    [SerializeField] private int overloadIconCount = 40;
    [SerializeField] private float finalVirusScreenHeights = 2.5f;

    [Header("Blue screen")]
    [SerializeField] private float typeDuration = 3.5f;
    [SerializeField] private float blueScreenHoldMax = 4f;

    [Header("Colors")]
    [SerializeField] private Color infectColor = new(0.45f, 1f, 0.55f, 1f);
    [SerializeField] private Color dangerColor = new(1f, 0.2f, 0.25f, 1f);
    [SerializeField] private Color mutationColor = new(0.85f, 0.35f, 1f, 1f);
    [SerializeField] private Color blueScreenColor = new(0f, 0f, 0.67f, 1f);

    private const string GlitchChars = "#$%&@!?*01<>/\\|=+";

    private RectTransform root;
    private Image redOverlay;
    private Image whiteFlash;
    private Camera cam;

    public void Play()
    {
        cam = Camera.main;
        StartCoroutine(Sequence());
    }

    private IEnumerator Sequence()
    {
        BuildCanvas();
        yield return Overload();
        yield return BlueScreen();
        yield return FinalScreen();
    }

    private void BuildCanvas()
    {
        GameObject go = new("Ending Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        go.transform.SetParent(transform, false);
        Canvas canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 500;
        CanvasScaler scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        root = (RectTransform)go.transform;

        redOverlay = CreateImage("Red Overlay", root, new Color(dangerColor.r, dangerColor.g, dangerColor.b, 0f));
        Stretch(redOverlay.rectTransform);
        redOverlay.raycastTarget = true;
    }

    private IEnumerator Overload()
    {
        iconManager.StopSpawning();
        iconManager.SpawnDesktopIcons((uint)overloadIconCount);

        Vector3 startScale = virus.transform.localScale;
        float startHeight = Mathf.Max(virus.GetVisualBounds().size.y, 0.01f);
        float targetFactor = cam.orthographicSize * 2f * finalVirusScreenHeights / startHeight;

        float burstTimer = 0f;
        float wordTimer = 0f;

        for (float t = 0f; t < overloadDuration; t += Time.unscaledDeltaTime)
        {
            float k = t / overloadDuration;
            float dt = Time.unscaledDeltaTime;
            Vector2 center = virus.GetAbsorbPoint();

            float growth = Mathf.Lerp(1f, targetFactor, k * k * k);
            float throb = 1f + Mathf.Sin(t * Mathf.Lerp(8f, 40f, k)) * 0.06f * (1f + k);
            virus.transform.localScale = startScale * (growth * throb);

            feedback.Shake(0.05f + k * 0.7f);

            float pulse = 0.5f + 0.5f * Mathf.Sin(t * Mathf.Lerp(6f, 30f, k));
            redOverlay.color = new Color(dangerColor.r, dangerColor.g, dangerColor.b, pulse * Mathf.Lerp(0.08f, 0.45f, k));

            SuckIcons(center, Mathf.Lerp(4f, 30f, k), dt);

            burstTimer -= dt;
            if (burstTimer <= 0f)
            {
                burstTimer = Mathf.Lerp(0.15f, 0.03f, k);
                Vector2 point = cam.ViewportToWorldPoint(new Vector3(Random.value, Random.value, 10f));
                Color color = Random.value < 0.5f ? infectColor : Random.value < 0.5f ? mutationColor : Color.white;
                feedback.Burst(point, color, 12, 9f);
                feedback.Ring(point, color, Random.Range(1f, 4f));
            }

            wordTimer -= dt;
            if (wordTimer <= 0f)
            {
                wordTimer = Mathf.Lerp(0.2f, 0.04f, k);
                StartCoroutine(GlitchWord());
            }

            yield return null;
        }

        for (int i = DesktopIcon.Active.Count - 1; i >= 0; i--)
        {
            Destroy(DesktopIcon.Active[i].gameObject);
        }

        whiteFlash = CreateImage("White Flash", root, Color.white);
        Stretch(whiteFlash.rectTransform);
        feedback.Shake(0f);
        virus.GetComponent<SpriteRenderer>().enabled = false;
        virus.transform.localScale = startScale;
        redOverlay.color = Color.black;
    }

    private static void SuckIcons(Vector2 center, float speed, float dt)
    {
        for (int i = DesktopIcon.Active.Count - 1; i >= 0; i--)
        {
            DesktopIcon icon = DesktopIcon.Active[i];
            Vector2 next = Vector2.MoveTowards(icon.transform.position, center, speed * dt);
            icon.transform.position = new Vector3(next.x, next.y, icon.transform.position.z);
            icon.transform.Rotate(0f, 0f, 720f * dt);
            if ((next - center).sqrMagnitude < 0.3f)
            {
                Destroy(icon.gameObject);
            }
        }
    }

    private IEnumerator GlitchWord()
    {
        string[] words = Loc.T("ending.glitch_words").Split('|');
        TextMeshProUGUI word = CreateText("Glitch Word", root, words[Random.Range(0, words.Length)], Random.Range(28f, 70f));
        word.color = Random.value < 0.6f ? dangerColor : infectColor;
        word.fontStyle = FontStyles.Bold;
        word.rectTransform.sizeDelta = new Vector2(900f, 120f);
        word.rectTransform.anchoredPosition = new Vector2(Random.Range(-800f, 800f), Random.Range(-450f, 450f));
        word.rectTransform.localRotation = Quaternion.Euler(0f, 0f, Random.Range(-12f, 12f));

        float life = Random.Range(0.15f, 0.4f);
        for (float t = 0f; t < life; t += Time.unscaledDeltaTime)
        {
            word.rectTransform.anchoredPosition += new Vector2(Random.Range(-12f, 12f), 0f);
            word.alpha = Random.value < 0.2f ? 0f : 1f;
            yield return null;
        }

        Destroy(word.gameObject);
    }

    private IEnumerator BlueScreen()
    {
        GameAudio.Stop(GameSfx.VirusElectric);
        GameAudio.Play(GameSfx.CriticalError);
        Image panel = CreateImage("Blue Screen", root, blueScreenColor);
        Stretch(panel.rectTransform);
        whiteFlash.transform.SetAsLastSibling();

        TextMeshProUGUI text = CreateText("BSOD Text", panel.rectTransform, BuildBlueScreenText(), 34f);
        Stretch(text.rectTransform);
        text.rectTransform.offsetMin = new Vector2(180f, 120f);
        text.rectTransform.offsetMax = new Vector2(-180f, -120f);
        text.alignment = TextAlignmentOptions.TopLeft;
        text.color = Color.white;
        text.maxVisibleCharacters = 0;

        for (float t = 0f; t < 0.35f; t += Time.unscaledDeltaTime)
        {
            whiteFlash.color = new Color(1f, 1f, 1f, 1f - t / 0.35f);
            yield return null;
        }

        whiteFlash.color = new Color(1f, 1f, 1f, 0f);

        GameAudio.Play(GameSfx.Keyboard);
        text.ForceMeshUpdate();
        int total = text.textInfo.characterCount;
        for (float t = 0f; t < typeDuration; t += Time.unscaledDeltaTime)
        {
            text.maxVisibleCharacters = Mathf.FloorToInt(total * t / typeDuration);
            JitterPanel(panel.rectTransform);
            yield return null;
        }

        text.maxVisibleCharacters = total;
        panel.rectTransform.anchoredPosition = Vector2.zero;

        for (float t = 0f; t < blueScreenHoldMax; t += Time.unscaledDeltaTime)
        {
            if (t > 0.6f && AnyInputPressed())
            {
                break;
            }

            JitterPanel(panel.rectTransform);
            yield return null;
        }

        panel.rectTransform.anchoredPosition = Vector2.zero;
        yield return CrtShutdown(panel.rectTransform);
        Destroy(panel.gameObject);
    }

    private static void JitterPanel(RectTransform panel)
    {
        panel.anchoredPosition = Random.value < 0.06f ? new Vector2(Random.Range(-30f, 30f), 0f) : Vector2.zero;
    }

    private string BuildBlueScreenText()
    {
        return Loc.F("ending.bsod", RunStats.TotalComputers);
    }

    private IEnumerator CrtShutdown(RectTransform panel)
    {
        Image line = CreateImage("CRT Line", root, Color.white);
        line.rectTransform.sizeDelta = new Vector2(1920f, 1080f);
        line.color = new Color(1f, 1f, 1f, 0f);

        for (float t = 0f; t < 0.2f; t += Time.unscaledDeltaTime)
        {
            float k = t / 0.2f;
            float y = Mathf.Lerp(1f, 0.004f, k * k);
            panel.localScale = new Vector3(1f + k * 0.05f, y, 1f);
            line.rectTransform.localScale = new Vector3(1f, y, 1f);
            line.color = new Color(1f, 1f, 1f, k);
            yield return null;
        }

        panel.gameObject.SetActive(false);

        for (float t = 0f; t < 0.18f; t += Time.unscaledDeltaTime)
        {
            float k = t / 0.18f;
            line.rectTransform.localScale = new Vector3(Mathf.Lerp(1f, 0.002f, k * k), 0.004f, 1f);
            yield return null;
        }

        line.rectTransform.sizeDelta = new Vector2(16f, 16f);
        line.rectTransform.localScale = Vector3.one;
        for (float t = 0f; t < 0.4f; t += Time.unscaledDeltaTime)
        {
            line.color = new Color(1f, 1f, 1f, 1f - t / 0.4f);
            yield return null;
        }

        Destroy(line.gameObject);
        yield return new WaitForSecondsRealtime(0.6f);
    }

    private IEnumerator FinalScreen()
    {
        RectTransform screen = new GameObject("Final Screen", typeof(RectTransform)).GetComponent<RectTransform>();
        screen.SetParent(root, false);
        Stretch(screen);

        Image glow = CreateImage("Virus Glow", screen, new Color(infectColor.r, infectColor.g, infectColor.b, 0f));
        glow.rectTransform.sizeDelta = new Vector2(620f, 620f);
        glow.rectTransform.anchoredPosition = new Vector2(0f, 230f);
        Sprite glowSprite = feedback.ParticleSprite;
        if (glowSprite != null)
        {
            glow.sprite = glowSprite;
        }

        Image virusImage = CreateImage("Virus", screen, Color.white);
        virusImage.sprite = virus.GetComponent<SpriteRenderer>().sprite;
        virusImage.preserveAspect = true;
        virusImage.rectTransform.sizeDelta = new Vector2(340f, 340f);
        virusImage.rectTransform.anchoredPosition = new Vector2(0f, 230f);
        virusImage.rectTransform.localScale = Vector3.zero;

        BuildResultWindow(screen);
        virusImage.transform.SetAsLastSibling();

        TextMeshProUGUI title = CreateText("Title", screen, string.Empty, 64f);
        title.fontStyle = FontStyles.Bold;
        title.color = XpUi.TextDark;
        title.rectTransform.sizeDelta = new Vector2(1100f, 90f);
        title.rectTransform.anchoredPosition = new Vector2(0f, -40f);

        TextMeshProUGUI subtitle = CreateText("Subtitle", screen, Loc.T("ending.subtitle"), 32f);
        subtitle.color = new Color(0.15f, 0.2f, 0.32f, 0f);
        subtitle.rectTransform.sizeDelta = new Vector2(1000f, 60f);
        subtitle.rectTransform.anchoredPosition = new Vector2(0f, -120f);

        TextMeshProUGUI stats = CreateText("Stats", screen, BuildStatsText(), 26f);
        stats.color = new Color(0.2f, 0.24f, 0.32f, 0f);
        stats.rectTransform.sizeDelta = new Vector2(1700f, 60f);
        stats.rectTransform.anchoredPosition = new Vector2(0f, -190f);

        Button restart = CreateButton(screen, Loc.T("ending.restart"), new Vector2(-260f, -310f));
        Button keepEating = CreateButton(screen, Loc.T("ending.keep_eating"), new Vector2(260f, -310f));
        restart.gameObject.SetActive(false);
        keepEating.gameObject.SetActive(false);
        restart.onClick.AddListener(StartOver);
        keepEating.onClick.AddListener(KeepEating);

        for (float t = 0f; t < 0.8f; t += Time.unscaledDeltaTime)
        {
            float k = t / 0.8f;
            float elastic = 1f + Mathf.Sin(k * Mathf.PI * 3f) * (1f - k) * 0.5f;
            virusImage.rectTransform.localScale = Vector3.one * (k * elastic);
            glow.color = new Color(infectColor.r, infectColor.g, infectColor.b, k * 0.35f);
            yield return null;
        }

        StartCoroutine(AnimateVirus(virusImage.rectTransform, glow));

        string message = Loc.F("ending.title", RunStats.TotalComputers);
        yield return Decode(title, message, 1.4f);

        for (float t = 0f; t < 0.6f; t += Time.unscaledDeltaTime)
        {
            float a = t / 0.6f;
            subtitle.color = new Color(subtitle.color.r, subtitle.color.g, subtitle.color.b, a);
            stats.color = new Color(stats.color.r, stats.color.g, stats.color.b, a);
            yield return null;
        }

        restart.gameObject.SetActive(true);
        keepEating.gameObject.SetActive(true);
        yield return PopIn(restart.transform);
        yield return PopIn(keepEating.transform);
    }

    private static string BuildStatsText()
    {
        return Loc.F("ending.stats", RunStats.RunNumber, NumberFormat.Mb(GameProgress.TotalMB), RunStats.TotalFiles, RunStats.TotalClicks);
    }

    private IEnumerator AnimateVirus(RectTransform virusRect, Image glow)
    {
        while (true)
        {
            float t = Time.unscaledTime;
            virusRect.localScale = Vector3.one * (1f + Mathf.Sin(t * 3f) * 0.05f);
            virusRect.localRotation = Quaternion.Euler(0f, 0f, t * 25f);
            glow.rectTransform.localScale = Vector3.one * (1f + Mathf.Sin(t * 3f + 1f) * 0.1f);

            bool glitch = Random.value < 0.04f;
            virusRect.anchoredPosition = new Vector2(glitch ? Random.Range(-20f, 20f) : 0f, 230f);
            yield return null;
        }
    }

    private static IEnumerator Decode(TextMeshProUGUI text, string message, float duration)
    {
        char[] chars = new char[message.Length];
        for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
        {
            int revealed = Mathf.FloorToInt(message.Length * t / duration);
            for (int i = 0; i < message.Length; i++)
            {
                chars[i] = i < revealed || message[i] == ' ' ? message[i] : GlitchChars[Random.Range(0, GlitchChars.Length)];
            }

            text.text = new string(chars);
            yield return null;
        }

        text.text = message;
        text.rectTransform.localScale = Vector3.one * 1.15f;
        for (float t = 0f; t < 0.25f; t += Time.unscaledDeltaTime)
        {
            text.rectTransform.localScale = Vector3.one * Mathf.Lerp(1.15f, 1f, t / 0.25f);
            yield return null;
        }

        text.rectTransform.localScale = Vector3.one;
    }

    private static IEnumerator PopIn(Transform target)
    {
        for (float t = 0f; t < 0.25f; t += Time.unscaledDeltaTime)
        {
            float k = t / 0.25f;
            target.localScale = Vector3.one * (k * (1f + Mathf.Sin(k * Mathf.PI) * 0.3f));
            yield return null;
        }

        target.localScale = Vector3.one;
    }

    private bool AnyInputPressed()
    {
        bool key = Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame;
        bool click = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame && (!MonitorScreen.Active || MonitorScreen.OnGlass);
        if (click)
        {
            GameAudio.PlayMouseClick();
        }

        return key || click;
    }

    private void StartOver()
    {
        GameAudio.PlayMouseClick();
        GameProgress.StartOver();
        RunStats.ResetSession();
        Time.timeScale = 1f;
        ViewManager.Switch(gameObject.scene.name, gameObject.scene.name);
    }

    private void KeepEating()
    {
        GameAudio.PlayMouseClick();
        Time.timeScale = 1f;
        ViewManager.Switch(gameObject.scene.name, skillTreeSceneName);
    }

    private void BuildResultWindow(RectTransform screen)
    {
        ArtBindings art = ArtBindings.Load();
        Image frame = CreateImage("Result Window", screen, Color.white);
        frame.rectTransform.sizeDelta = new Vector2(1240f, 500f);
        frame.rectTransform.anchoredPosition = new Vector2(0f, -150f);
        if (art != null && art.windowFrame != null)
        {
            ArtBindings.ApplySlice(frame, art.windowFrame);
        }
        else
        {
            frame.color = XpUi.WindowFrame;
        }

        Image body = CreateImage("Body", frame.rectTransform, XpUi.WindowBody);
        Stretch(body.rectTransform);
        body.rectTransform.offsetMin = new Vector2(8f, 8f);
        body.rectTransform.offsetMax = new Vector2(-8f, -46f);

        Image title = CreateImage("Title Bar", frame.rectTransform, Color.white);
        title.rectTransform.anchorMin = new Vector2(0f, 1f);
        title.rectTransform.anchorMax = new Vector2(1f, 1f);
        title.rectTransform.pivot = new Vector2(0.5f, 1f);
        title.rectTransform.anchoredPosition = Vector2.zero;
        title.rectTransform.sizeDelta = new Vector2(0f, 42f);
        if (art != null && art.titlebarActive != null)
        {
            ArtBindings.ApplySlice(title, art.titlebarActive);
        }

        TextMeshProUGUI caption = CreateText("Caption", title.rectTransform, Loc.T("end.window"), 24f);
        Stretch(caption.rectTransform);
        caption.rectTransform.offsetMin = new Vector2(16f, 0f);
        caption.alignment = TextAlignmentOptions.MidlineLeft;
        caption.fontStyle = FontStyles.Bold;
        caption.color = Color.white;
    }

    private Button CreateButton(RectTransform parent, string label, Vector2 position)
    {
        Image image = CreateImage(label, parent, Color.white);
        image.raycastTarget = true;
        image.rectTransform.sizeDelta = new Vector2(440f, 78f);
        image.rectTransform.anchoredPosition = position;
        image.rectTransform.localScale = Vector3.zero;

        Button button = image.gameObject.AddComponent<Button>();
        ArtBindings.StyleXpButton(button);

        TextMeshProUGUI text = CreateText("Label", image.rectTransform, label, 30f);
        Stretch(text.rectTransform);
        text.fontStyle = FontStyles.Bold;
        text.color = Color.white;
        text.outlineWidth = 0.18f;
        text.outlineColor = new Color32(0, 24, 72, 220);

        return button;
    }

    private static Image CreateImage(string name, RectTransform parent, Color color)
    {
        GameObject go = new(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        Image image = go.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    private TextMeshProUGUI CreateText(string name, RectTransform parent, string value, float size)
    {
        GameObject go = new(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        TextMeshProUGUI text = go.GetComponent<TextMeshProUGUI>();
        if (font != null)
        {
            text.font = font;
        }

        text.text = value;
        text.fontSize = size;
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;
        text.textWrappingMode = TextWrappingModes.Normal;
        return text;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
