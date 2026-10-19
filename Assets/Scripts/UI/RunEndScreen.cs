using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RunEndScreen : MonoBehaviour
{
    [SerializeField] private SkillTreeDefinition skillTree;
    [SerializeField] private CanvasGroup group;
    [SerializeField] private RectTransform monitor;
    [SerializeField] private RectTransform virusDecor;
    [SerializeField] private Image computersFill;
    [SerializeField] private TextMeshProUGUI computersCount;
    [SerializeField] private TextMeshProUGUI totalValue;
    [SerializeField] private TextMeshProUGUI comparison;
    [SerializeField] private RectTransform recordStamp;
    [SerializeField] private TextMeshProUGUI filesValue;
    [SerializeField] private TextMeshProUGUI clicksValue;
    [SerializeField] private TextMeshProUGUI gbValue;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private TextMeshProUGUI upgradeLabel;
    [SerializeField] private Button continueButton;

    [SerializeField] private float zoomOutDuration = 0.45f;
    [SerializeField] private float startMonitorScale = 1.3f;
    [SerializeField] private float countDuration = 1.2f;
    [SerializeField] private float virusSpinSpeed = 12f;
    [SerializeField] private Color betterColor = new(0.45f, 1f, 0.55f, 1f);
    [SerializeField] private Color worseColor = new(1f, 0.45f, 0.45f, 1f);

    private Action onUpgrade;
    private Action onContinue;
    private int affordableUpgrades;
    private bool revealed;

    void Awake()
    {
        ApplyXpChrome();
        upgradeButton.onClick.AddListener(() => Choose(onUpgrade));
        continueButton.onClick.AddListener(() => Choose(onContinue));
        gameObject.SetActive(false);
    }

    private void ApplyXpChrome()
    {
        ArtBindings art = ArtBindings.Load();
        Image backdrop = GetComponent<Image>();
        if (backdrop != null)
        {
            backdrop.sprite = null;
            backdrop.color = new Color(0.04f, 0.1f, 0.24f, 0.45f);
        }

        if (monitor == null)
        {
            return;
        }

        Image bezel = monitor.GetComponent<Image>();
        if (bezel != null && art != null && art.windowFrame != null)
        {
            ArtBindings.ApplySlice(bezel, art.windowFrame);
        }

        Transform screen = monitor.Find("Screen");
        if (screen == null)
        {
            return;
        }

        Image screenImage = screen.GetComponent<Image>();
        if (screenImage != null)
        {
            screenImage.sprite = null;
            screenImage.color = XpUi.WindowBody;
        }

        BuildTitleBar(screen, art);
        StyleWell(screen.Find("Computers Bar"), art);
        StyleWell(screen.Find("Total Panel"), art);
        StyleWell(screen.Find("Stats Panel"), art);
        StyleWell(screen.Find("Stats Panel/Files"), art);
        StyleWell(screen.Find("Stats Panel/Clicks"), art);
        StyleWell(screen.Find("Stats Panel/GB"), art);
        EnlargeStats(screen);

        if (computersFill != null)
        {
            computersFill.color = XpUi.Hex("#2F8F45");
        }

        ArtBindings.StyleXpButton(upgradeButton);
        ArtBindings.StyleXpButton(continueButton);
        StyleTmp(upgradeButton.transform, "Label", 32f, Color.white, FontStyles.Bold);
        StyleTmp(continueButton.transform, "Label", 32f, Color.white, FontStyles.Bold);

        StyleTmp(screen, "Computers Label", 22f, XpUi.TextDark, FontStyles.Bold);
        StyleTmp(screen, "Computers Count", 22f, XpUi.TextDark, FontStyles.Bold);
        StyleTmp(screen, "Total Panel/Title", 28f, XpUi.TextDark, FontStyles.Bold);
        StyleTmp(screen, "Total Panel/Value", 52f, XpUi.Hex("#0C6B32"), FontStyles.Bold);
        StyleTmp(screen, "Stats Panel/Files/Value", 44f, XpUi.TextDark, FontStyles.Bold);
        StyleTmp(screen, "Stats Panel/Clicks/Value", 44f, XpUi.TextDark, FontStyles.Bold);
        StyleTmp(screen, "Stats Panel/GB/Value", 44f, XpUi.TextDark, FontStyles.Bold);
        StyleTmp(screen, "Stats Panel/Files Label", 26f, XpUi.TextDark, FontStyles.Normal);
        StyleTmp(screen, "Stats Panel/Clicks Label", 26f, XpUi.TextDark, FontStyles.Normal);
        StyleTmp(screen, "Stats Panel/GB Label", 26f, XpUi.TextDark, FontStyles.Normal);

        if (virusDecor != null)
        {
            virusDecor.sizeDelta = new Vector2(210f, 210f);
            virusDecor.anchoredPosition = new Vector2(-460f, -8f);
        }

        Transform fakeTaskbar = screen.Find("Taskbar");
        if (fakeTaskbar != null)
        {
            fakeTaskbar.gameObject.SetActive(false);
        }
    }

    private static void EnlargeStats(Transform screen)
    {
        RectTransform panel = screen.Find("Stats Panel") as RectTransform;
        if (panel == null)
        {
            return;
        }

        panel.sizeDelta = new Vector2(700f, 220f);
        panel.anchoredPosition = new Vector2(0f, -4f);
        PlaceStat(panel, "Files", -220f);
        PlaceStat(panel, "Clicks", 0f);
        PlaceStat(panel, "GB", 220f);

        RectTransform upgrade = screen.Find("Upgrade Button") as RectTransform;
        if (upgrade != null)
        {
            upgrade.anchoredPosition = new Vector2(upgrade.anchoredPosition.x, -168f);
        }

        RectTransform next = screen.Find("Continue Button") as RectTransform;
        if (next != null)
        {
            next.anchoredPosition = new Vector2(next.anchoredPosition.x, -286f);
        }
    }

    private static void PlaceStat(RectTransform panel, string name, float x)
    {
        RectTransform well = panel.Find(name) as RectTransform;
        if (well != null)
        {
            well.sizeDelta = new Vector2(170f, 112f);
            well.anchoredPosition = new Vector2(x, 32f);
        }

        RectTransform label = panel.Find(name + " Label") as RectTransform;
        if (label != null)
        {
            label.sizeDelta = new Vector2(210f, 68f);
            label.anchoredPosition = new Vector2(x, -70f);
        }
    }

    private static void BuildTitleBar(Transform screen, ArtBindings art)
    {
        Image title = XpUi.Image("Xp Title", screen, null, Color.white);
        if (art != null && art.titlebarActive != null)
        {
            ArtBindings.ApplySlice(title, art.titlebarActive);
        }
        else
        {
            title.color = XpUi.WindowFrame;
        }

        RectTransform rect = title.rectTransform;
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(0f, 42f);
        XpUi.ShadowText("Title", title.transform, "end.window", 24f, Color.white, TextAlignmentOptions.MidlineLeft, FontStyles.Bold, new Vector2(1.5f, -1.5f));
        TextMeshProUGUI[] captions = title.GetComponentsInChildren<TextMeshProUGUI>();
        for (int i = 0; i < captions.Length; i++)
        {
            captions[i].rectTransform.offsetMin = new Vector2(16f, 0f);
            captions[i].rectTransform.offsetMax = new Vector2(-12f, 0f);
        }
    }

    private static void StyleWell(Transform well, ArtBindings art)
    {
        if (well == null)
        {
            return;
        }

        Image image = well.GetComponent<Image>();
        if (image == null)
        {
            return;
        }

        if (art != null && art.panelSunken != null)
        {
            ArtBindings.ApplySlice(image, art.panelSunken);
        }
        else
        {
            image.color = new Color(0.78f, 0.78f, 0.72f, 1f);
        }
    }

    private static void StyleTmp(Transform parent, string path, float size, Color color, FontStyles style)
    {
        if (parent == null)
        {
            return;
        }

        Transform child = parent.Find(path);
        if (child == null)
        {
            return;
        }

        TextMeshProUGUI text = child.GetComponent<TextMeshProUGUI>();
        if (text == null)
        {
            return;
        }

        text.fontSize = size;
        text.color = color;
        text.fontStyle = style;
    }

    void Update()
    {
        float now = Time.unscaledTime;

        if (virusDecor != null)
        {
            virusDecor.Rotate(0f, 0f, virusSpinSpeed * Time.unscaledDeltaTime);
        }

        if (revealed && RunStats.IsNewRecord)
        {
            recordStamp.localScale = Vector3.one * (1f + 0.08f * Mathf.Sin(now * 6f));
        }

        if (revealed && affordableUpgrades > 0)
        {
            upgradeButton.transform.localScale = Vector3.one * (1f + 0.04f * Mathf.Sin(now * 5f));
        }
    }

    public void Show(Action upgrade, Action next)
    {
        GameAudio.Play(GameSfx.Icq);
        onUpgrade = upgrade;
        onContinue = next;
        upgradeButton.interactable = true;
        continueButton.interactable = true;
        gameObject.SetActive(true);
        StartCoroutine(PlayIn());
    }

    private void Choose(Action action)
    {
        GameAudio.PlayMouseClick();
        upgradeButton.interactable = false;
        continueButton.interactable = false;
        action?.Invoke();
    }

    private IEnumerator PlayIn()
    {
        revealed = false;
        int total = Mathf.Max(RunStats.TotalComputers, 1);
        computersCount.text = $"{RunStats.ComputersInfected}/{total}";
        SetValues(0f);
        computersFill.fillAmount = 0f;
        comparison.text = string.Empty;
        recordStamp.gameObject.SetActive(false);
        upgradeButton.transform.localScale = Vector3.one;

        affordableUpgrades = CountAffordableUpgrades();
        UpdateUpgradeLabel();

        for (float t = 0f; t < zoomOutDuration; t += Time.unscaledDeltaTime)
        {
            float k = 1f - Mathf.Pow(1f - t / zoomOutDuration, 3f);
            group.alpha = k;
            monitor.localScale = Vector3.one * Mathf.Lerp(startMonitorScale, 1f, k);
            yield return null;
        }

        group.alpha = 1f;
        monitor.localScale = Vector3.one;

        float computersTarget = RunStats.ComputersInfected / (float)total;
        for (float t = 0f; t < countDuration; t += Time.unscaledDeltaTime)
        {
            float k = 1f - Mathf.Pow(1f - t / countDuration, 3f);
            SetValues(k);
            computersFill.fillAmount = computersTarget * k;
            yield return null;
        }

        SetValues(1f);
        computersFill.fillAmount = computersTarget;
        revealed = true;
        ShowComparison();
        recordStamp.gameObject.SetActive(RunStats.IsNewRecord);
    }

    void OnEnable()
    {
        GameSettings.Changed += OnSettingsChanged;
    }

    void OnDisable()
    {
        GameSettings.Changed -= OnSettingsChanged;
    }

    private void OnSettingsChanged()
    {
        UpdateUpgradeLabel();
        if (revealed)
        {
            ShowComparison();
        }
    }

    private void UpdateUpgradeLabel()
    {
        upgradeLabel.text = affordableUpgrades > 0 ? Loc.F("end.upgrade_available", affordableUpgrades) : Loc.T("end.upgrade");
    }

    private void ShowComparison()
    {
        if (!RunStats.HasPreviousRun)
        {
            return;
        }

        long delta = (long)RunStats.MBEarned - RunStats.PreviousRunMB;
        float percent = delta * 100f / Mathf.Max(RunStats.PreviousRunMB, 1u);
        bool better = delta >= 0;
        string arrow = better ? "▲" : "▼";
        comparison.color = better ? betterColor : worseColor;
        comparison.text = Loc.F("end.comparison", arrow, better ? "+" : "", percent, RunStats.PreviousRunMB);
    }

    private int CountAffordableUpgrades()
    {
        int count = 0;
        foreach (SkillTreeDefinition.Node node in skillTree.Nodes)
        {
            if (skillTree.CanBuy(node))
            {
                count++;
            }
        }

        return count;
    }

    private void SetValues(float k)
    {
        totalValue.text = $"{Mathf.RoundToInt(RunStats.MBEarned * k)} MB";
        filesValue.text = Mathf.RoundToInt(RunStats.FilesInfected * k).ToString();
        clicksValue.text = Mathf.RoundToInt(RunStats.Clicks * k).ToString();
        gbValue.text = (RunStats.GBEarned * k).ToString("0.00");
    }
}
