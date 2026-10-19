using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsWindow : MonoBehaviour
{
    private const float TitleHeight = 36f;
    private const float RowHeight = 72f;
    private const float ControlX = 320f;

    private Slider volume;
    private TextMeshProUGUI volumeLabel;
    private TMP_Dropdown resolution;
    private readonly List<Vector2Int> resolutions = new();
    private Button ruButton;
    private Button enButton;
    private UiSwitch effects;
    private TextMeshProUGUI effectsLabel;
    private UiSwitch shake;
    private TextMeshProUGUI shakeLabel;
    private Action onClose;

    public static SettingsWindow Create(Transform parent, Action onClose)
    {
        Image frame = XpUi.Image("Settings Window", parent, XpUi.Panel("xpWindowFrame", 32, 32, 9, XpCorners.Top, XpUi.WindowFrame), Color.white, true);
        XpUi.Place(frame.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 30f), new Vector2(660f, 36f + RowHeight * 5f + 110f));

        SettingsWindow window = frame.gameObject.AddComponent<SettingsWindow>();
        window.onClose = onClose;
        window.Build(frame.rectTransform);
        return window;
    }

    void OnEnable()
    {
        GameSettings.Changed += Refresh;
        Refresh();
    }

    void OnDisable()
    {
        GameSettings.Changed -= Refresh;
    }

    private void Build(RectTransform frame)
    {
        Image title = XpUi.Image("Title Bar", frame, XpUi.Panel("xpTitle", 32, 36, 8, XpCorners.Top,
            XpUi.Hex("#3D95FF"), XpUi.Hex("#0A5FEA"), XpUi.Hex("#0758E0"), XpUi.Hex("#0653D8"), XpUi.Hex("#0447C9")), Color.white);
        XpUi.Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), Vector2.zero, Vector2.zero);
        title.rectTransform.anchorMin = new Vector2(0f, 1f);
        title.rectTransform.anchorMax = new Vector2(1f, 1f);
        title.rectTransform.sizeDelta = new Vector2(0f, TitleHeight);

        RectTransform titleTextRoot = XpUi.Rect("Title Text", title.transform);
        XpUi.Stretch(titleTextRoot, 14f, 0f, 50f, 0f);
        XpUi.ShadowText("Title", titleTextRoot, "settings.title", 22f, Color.white, TextAlignmentOptions.MidlineLeft, FontStyles.Bold, new Vector2(1.5f, -1.5f));

        Image close = XpUi.Image("Close", title.transform, XpUi.Rounded(4), Color.white, true);
        XpUi.Place(close.rectTransform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-6f, 0f), new Vector2(28f, 28f));
        Image closeFace = XpUi.Image("Face", close.transform, XpUi.Panel("xpClose", 16, 32, 3, XpCorners.All, XpUi.Hex("#E9845F"), XpUi.Hex("#D9532A"), XpUi.Hex("#C63F17")), Color.white);
        XpUi.Stretch(closeFace.rectTransform, 1.5f, 1.5f, 1.5f, 1.5f);
        TextMeshProUGUI cross = XpUi.Text("X", close.transform, "X", 20f, Color.white, TextAlignmentOptions.Center, FontStyles.Bold);
        XpUi.Stretch(cross.rectTransform);
        Button closeButton = close.gameObject.AddComponent<Button>();
        closeButton.targetGraphic = closeFace;
        closeButton.onClick.AddListener(Close);

        Image body = XpUi.Image("Body", frame, null, XpUi.WindowBody);
        XpUi.Stretch(body.rectTransform, 4f, 4f, 4f, TitleHeight);

        float y = -24f;
        RectTransform row = Row(body.transform, "settings.volume", ref y);
        volume = CreateSlider(row);
        volumeLabel = XpUi.Text("Percent", row, "100%", 22f, Color.black, TextAlignmentOptions.MidlineRight);
        XpUi.Place(volumeLabel.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(ControlX + 240f, 0f), new Vector2(64f, 40f));
        volume.onValueChanged.AddListener(value => GameSettings.SetVolume(value / 100f));

        row = Row(body.transform, "settings.resolution", ref y);
        resolution = CreateDropdown(row);
        resolution.onValueChanged.AddListener(OnResolutionPicked);

        row = Row(body.transform, "settings.language", ref y);
        ruButton = XpUi.XpButton("RU", row, null, new Vector2(100f, 38f));
        XpUi.Place((RectTransform)ruButton.transform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(ControlX, 0f), new Vector2(100f, 38f));
        ruButton.GetComponentInChildren<TMP_Text>().text = "RU";
        ruButton.onClick.AddListener(() => GameSettings.SetLanguage(GameLanguage.Ru));
        enButton = XpUi.XpButton("EN", row, null, new Vector2(100f, 38f));
        XpUi.Place((RectTransform)enButton.transform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(ControlX + 110f, 0f), new Vector2(100f, 38f));
        enButton.GetComponentInChildren<TMP_Text>().text = "EN";
        enButton.onClick.AddListener(() => GameSettings.SetLanguage(GameLanguage.En));

        row = Row(body.transform, "settings.effects", ref y);
        effects = CreateSwitch(row, out effectsLabel);
        effects.Toggled += GameSettings.SetScreenEffects;

        row = Row(body.transform, "settings.shake", ref y);
        shake = CreateSwitch(row, out shakeLabel);
        shake.Toggled += GameSettings.SetScreenShake;

        Image line = XpUi.Image("Separator", body.transform, null, new Color(0.67f, 0.66f, 0.6f, 1f));
        XpUi.Place(line.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 76f), new Vector2(600f, 1f));

        Button ok = XpUi.XpButton("OK", body.transform, "settings.ok", new Vector2(120f, 38f));
        XpUi.Place((RectTransform)ok.transform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-24f, 22f), new Vector2(120f, 38f));
        ok.onClick.AddListener(Close);

        Refresh();
    }

    public void Close()
    {
        onClose?.Invoke();
    }

    private void Refresh()
    {
        if (volume == null)
        {
            return;
        }

        int percent = Mathf.RoundToInt(GameSettings.MasterVolume * 100f);
        volume.SetValueWithoutNotify(percent);
        volumeLabel.text = $"{percent}%";

        RefreshResolutions();

        bool ru = GameSettings.Language == GameLanguage.Ru;
        StyleChoice(ruButton, ru);
        StyleChoice(enButton, !ru);

        effects.SetIsOn(GameSettings.ScreenEffects);
        effectsLabel.text = Loc.T(GameSettings.ScreenEffects ? "settings.on" : "settings.off");
        shake.SetIsOn(GameSettings.ScreenShake);
        shakeLabel.text = Loc.T(GameSettings.ScreenShake ? "settings.on" : "settings.off");
    }

    private void RefreshResolutions()
    {
        resolutions.Clear();
        foreach (Resolution r in Screen.resolutions)
        {
            Vector2Int size = new(r.width, r.height);
            if (!resolutions.Contains(size))
            {
                resolutions.Add(size);
            }
        }

        Vector2Int current = GameSettings.Resolution;
        if (!resolutions.Contains(current))
        {
            resolutions.Add(current);
        }

        resolutions.Sort((a, b) => a.x != b.x ? b.x.CompareTo(a.x) : b.y.CompareTo(a.y));

        List<string> options = new();
        foreach (Vector2Int size in resolutions)
        {
            options.Add($"{size.x} × {size.y}");
        }

        resolution.ClearOptions();
        resolution.AddOptions(options);
        resolution.SetValueWithoutNotify(resolutions.IndexOf(current));
        resolution.RefreshShownValue();
    }

    private void OnResolutionPicked(int index)
    {
        if (index >= 0 && index < resolutions.Count)
        {
            GameSettings.SetResolution(resolutions[index].x, resolutions[index].y);
        }
    }

    private static void StyleChoice(Button button, bool selected)
    {
        Image face = (Image)button.targetGraphic;
        face.sprite = selected
            ? XpUi.Panel("xpButtonSelected", 16, 32, 3, XpCorners.All, XpUi.Hex("#5A8FE6"), XpUi.Hex("#316AC5"), XpUi.Hex("#2457AE"))
            : XpUi.Panel("xpButtonFace", 16, 32, 3, XpCorners.All, XpUi.Hex("#FFFFFF"), XpUi.Hex("#F4F3EE"), XpUi.Hex("#E3E1D5"));
        TMP_Text label = button.GetComponentInChildren<TMP_Text>();
        label.color = selected ? Color.white : Color.black;
        label.fontStyle = selected ? FontStyles.Bold : FontStyles.Normal;
    }

    private static RectTransform Row(Transform body, string key, ref float y)
    {
        RectTransform row = XpUi.Rect("Row " + key, body);
        row.anchorMin = new Vector2(0f, 1f);
        row.anchorMax = new Vector2(1f, 1f);
        row.pivot = new Vector2(0.5f, 1f);
        row.anchoredPosition = new Vector2(0f, y);
        row.sizeDelta = new Vector2(0f, RowHeight);
        y -= RowHeight;

        TextMeshProUGUI label = XpUi.Text("Label", row, string.Empty, 22f, Color.black);
        XpUi.Place(label.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(28f, 0f), new Vector2(280f, 40f));
        XpUi.Localize(label, key);
        return row;
    }

    private static Slider CreateSlider(RectTransform row)
    {
        GameObject go = DefaultControls.CreateSlider(new DefaultControls.Resources
        {
            standard = XpUi.Rounded(6),
            background = XpUi.Rounded(6),
            knob = XpUi.Circle()
        });
        go.transform.SetParent(row, false);
        RectTransform rect = (RectTransform)go.transform;
        XpUi.Place(rect, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(ControlX, 0f), new Vector2(230f, 18f));

        Slider slider = go.GetComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 100f;
        slider.wholeNumbers = true;

        Image background = go.transform.Find("Background").GetComponent<Image>();
        background.color = XpUi.Hex("#C9C6B8");
        slider.fillRect.GetComponent<Image>().color = XpUi.Hex("#3AA845");
        Image handle = slider.handleRect.GetComponent<Image>();
        handle.color = XpUi.Hex("#2F6FD8");
        slider.handleRect.sizeDelta = new Vector2(26f, 0f);
        ((RectTransform)slider.handleRect.parent).offsetMin = new Vector2(13f, -5f);
        ((RectTransform)slider.handleRect.parent).offsetMax = new Vector2(-13f, 5f);
        return slider;
    }

    private static TMP_Dropdown CreateDropdown(RectTransform row)
    {
        GameObject go = TMP_DefaultControls.CreateDropdown(new TMP_DefaultControls.Resources
        {
            standard = XpUi.Rounded(4),
            background = XpUi.Rounded(4),
            inputField = XpUi.Rounded(4),
            knob = XpUi.Rounded(4),
            checkmark = XpUi.Circle(),
            dropdown = XpUi.Triangle(),
            mask = XpUi.Rounded(4)
        });
        go.transform.SetParent(row, false);
        XpUi.Place((RectTransform)go.transform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(ControlX, 0f), new Vector2(300f, 38f));

        go.GetComponent<Image>().color = Color.white;
        Transform arrow = go.transform.Find("Arrow");
        if (arrow != null)
        {
            arrow.localRotation = Quaternion.Euler(0f, 0f, -90f);
            arrow.GetComponent<Image>().color = XpUi.HoverBlue;
            ((RectTransform)arrow).sizeDelta = new Vector2(16f, 16f);
        }

        foreach (TMP_Text text in go.GetComponentsInChildren<TMP_Text>(true))
        {
            if (XpUi.Font != null)
            {
                text.font = XpUi.Font;
            }

            text.fontSize = 20f;
            text.color = Color.black;
        }

        TMP_Dropdown dropdown = go.GetComponent<TMP_Dropdown>();
        if (dropdown.template != null)
        {
            dropdown.template.sizeDelta = new Vector2(0f, 260f);
            Transform item = dropdown.template.Find("Viewport/Content/Item");
            if (item != null)
            {
                ((RectTransform)item).sizeDelta = new Vector2(0f, 32f);
                Toggle toggle = item.GetComponent<Toggle>();
                ColorBlock colors = toggle.colors;
                colors.highlightedColor = XpUi.Hex("#C5D8F7");
                toggle.colors = colors;
            }

            Transform content = dropdown.template.Find("Viewport/Content");
            if (content != null)
            {
                ((RectTransform)content).sizeDelta = new Vector2(0f, 34f);
            }
        }

        return dropdown;
    }

    private static UiSwitch CreateSwitch(RectTransform row, out TextMeshProUGUI state)
    {
        UiSwitch control = UiSwitch.Create(row, new Vector2(74f, 34f));
        XpUi.Place((RectTransform)control.transform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(ControlX, 0f), new Vector2(74f, 34f));
        state = XpUi.Text("State", row, string.Empty, 22f, Color.black);
        XpUi.Place(state.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(ControlX + 90f, 0f), new Vector2(120f, 40f));
        return control;
    }
}
