using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillTreeScreen : MonoBehaviour
{
    private static readonly Color Header = new(0.23f, 0.22f, 0.21f, 1f);
    private static readonly Color Workspace = new(0.905f, 0.886f, 0.839f, 1f);
    private static readonly Color Panel = new(0.965f, 0.953f, 0.925f, 1f);
    private static readonly Color Ink = new(0.16f, 0.16f, 0.15f, 1f);
    private static readonly Color Muted = new(0.38f, 0.36f, 0.32f, 1f);
    private static readonly Color HeaderInk = new(0.95f, 0.93f, 0.88f, 1f);
    private static readonly Color PriceInk = new(0.18f, 0.32f, 0.2f, 1f);

    [SerializeField] private TextMeshProUGUI mbDisplay;
    [SerializeField] private Button startRunButton;
    [SerializeField] private string runSceneName = "Main";
    [SerializeField] private CrtPowerOn crt;
    [SerializeField] private float mbCountSpeed = 12f;
    [SerializeField] private float mbPunchScale = 1.35f;

    private SkillTreeView treeView;
    private SkillTreeFx fx;
    private TextMeshProUGUI upgradesDisplay;
    private TextMeshProUGUI detailTitle;
    private TextMeshProUGUI detailBody;
    private TextMeshProUGUI detailEffect;
    private TextMeshProUGUI detailLevel;
    private TextMeshProUGUI detailStatus;
    private TextMeshProUGUI detailPrice;
    private TextMeshProUGUI detailPriceCaption;
    private TextMeshProUGUI detailReward;
    private TextMeshProUGUI buyLabel;
    private TextMeshProUGUI resetLabel;
    private Button buyButton;

    void Awake()
    {
        treeView = GetComponentInChildren<SkillTreeView>(true);
        fx = GetComponent<SkillTreeFx>();
        _ = mbCountSpeed;
        _ = mbPunchScale;
        startRunButton.onClick.AddListener(StartRun);
        ArtBindings.StyleXpButton(startRunButton);
        HideGlow(startRunButton.transform);
        BuildChrome();
        FlattenWorkspace();
    }

    void OnEnable()
    {
        GameProgress.Changed += OnProgressChanged;
        GameSettings.Changed += OnProgressChanged;
        SkillTreeFocus.Changed += OnProgressChanged;
        RefreshHud();
    }

    void Start()
    {
        FlattenWorkspace();
        if (string.IsNullOrEmpty(SkillTreeFocus.SelectedId))
        {
            SkillTreeFocus.Select(SkillBalanceTable.Core);
        }

        RefreshHud();
    }

    void OnDisable()
    {
        GameProgress.Changed -= OnProgressChanged;
        GameSettings.Changed -= OnProgressChanged;
        SkillTreeFocus.Changed -= OnProgressChanged;
    }

    private void OnProgressChanged()
    {
        RefreshHud();
    }

    private static void HideGlow(Transform button)
    {
        Transform parent = button.parent;
        if (parent == null)
        {
            return;
        }

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child.name.Contains("Glow"))
            {
                child.gameObject.SetActive(false);
            }
        }
    }

    private void StartRun()
    {
        GameAudio.PlayMouseClick();
        startRunButton.interactable = false;
        if (crt != null)
        {
            crt.PlayOff(SwitchToRun);
        }
        else
        {
            SwitchToRun();
        }
    }

    private void SwitchToRun()
    {
        ViewManager.Switch(gameObject.scene.name, runSceneName);
    }

    private void BuildChrome()
    {
        Transform top = FindDeep(transform, "Top Bar");
        if (top != null)
        {
            Image bar = top.GetComponent<Image>();
            if (bar != null)
            {
                bar.color = Header;
                bar.material = null;
            }

            RectTransform barRect = (RectTransform)top;
            barRect.sizeDelta = new Vector2(0f, 72f);
            RestyleTitle(top);
            RestyleCurrency(top);
            Transform hint = FindDeep(transform, "Hint");
            if (hint != null)
            {
                hint.gameObject.SetActive(false);
            }

            Transform neon = top.Find("Neon Line");
            if (neon != null)
            {
                Image line = neon.GetComponent<Image>();
                if (line != null)
                {
                    line.material = null;
                    line.color = new Color(0.12f, 0.12f, 0.11f, 1f);
                }
            }

            RectTransform run = startRunButton.GetComponent<RectTransform>();
            run.anchorMin = run.anchorMax = new Vector2(1f, 0.5f);
            run.pivot = new Vector2(1f, 0.5f);
            run.anchoredPosition = new Vector2(-18f, 0f);
            run.sizeDelta = new Vector2(230f, 46f);
        }

        Transform tree = FindDeep(transform, "Tree View");
        if (tree is RectTransform treeRect)
        {
            treeRect.offsetMin = new Vector2(8f, 264f);
            treeRect.offsetMax = new Vector2(-8f, -80f);
        }

        BuildDetailPanel();
        top?.SetAsLastSibling();
    }

    private void RestyleTitle(Transform top)
    {
        Transform title = top.Find("Title");
        if (title == null)
        {
            return;
        }

        GlitchText glitch = title.GetComponent<GlitchText>();
        if (glitch != null)
        {
            glitch.enabled = false;
        }

        TextMeshProUGUI text = title.GetComponent<TextMeshProUGUI>();
        if (text == null)
        {
            return;
        }

        text.color = HeaderInk;
        text.fontSize = 30f;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.overflowMode = TextOverflowModes.Overflow;
        text.alignment = TextAlignmentOptions.MidlineLeft;
        RectTransform rect = text.rectTransform;
        rect.anchorMin = rect.anchorMax = new Vector2(0f, 0.5f);
        rect.pivot = new Vector2(0f, 0.5f);
        rect.anchoredPosition = new Vector2(22f, 0f);
        rect.sizeDelta = new Vector2(480f, 48f);
    }

    private void RestyleCurrency(Transform top)
    {
        mbDisplay.textWrappingMode = TextWrappingModes.NoWrap;
        mbDisplay.overflowMode = TextOverflowModes.Overflow;
        mbDisplay.fontSize = 26f;
        mbDisplay.fontStyle = FontStyles.Bold;
        mbDisplay.alignment = TextAlignmentOptions.MidlineRight;
        mbDisplay.color = HeaderInk;
        mbDisplay.raycastTarget = false;
        RectTransform mbRect = mbDisplay.rectTransform;
        mbRect.anchorMin = mbRect.anchorMax = new Vector2(0.5f, 0.5f);
        mbRect.pivot = new Vector2(0.5f, 0.5f);
        mbRect.anchoredPosition = new Vector2(40f, 14f);
        mbRect.sizeDelta = new Vector2(280f, 32f);
        mbRect.localScale = Vector3.one;
        mbDisplay.alignment = TextAlignmentOptions.Center;

        upgradesDisplay = CreateText(top, "Upgrades Label", 22f, FontStyles.Normal, TextAlignmentOptions.Center);
        upgradesDisplay.color = HeaderInk;
        RectTransform upgradesRect = upgradesDisplay.rectTransform;
        upgradesRect.anchorMin = upgradesRect.anchorMax = new Vector2(0.5f, 0.5f);
        upgradesRect.pivot = new Vector2(0.5f, 0.5f);
        upgradesRect.anchoredPosition = new Vector2(40f, -16f);
        upgradesRect.sizeDelta = new Vector2(320f, 28f);
    }

    private void BuildDetailPanel()
    {
        GameObject panelGo = new("Skill Detail", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        panelGo.transform.SetParent(transform, false);
        RectTransform panel = (RectTransform)panelGo.transform;
        panel.anchorMin = new Vector2(0f, 0f);
        panel.anchorMax = new Vector2(1f, 0f);
        panel.pivot = new Vector2(0.5f, 0f);
        panel.anchoredPosition = new Vector2(0f, 52f);
        panel.sizeDelta = new Vector2(-16f, 200f);
        Image image = panelGo.GetComponent<Image>();
        image.color = Panel;
        image.raycastTarget = true;

        detailTitle = CreateText(panel, "Detail Title", 28f, FontStyles.Bold, TextAlignmentOptions.TopLeft);
        Place(detailTitle.rectTransform, 16f, -12f, 0.62f, 36f);
        detailBody = CreateText(panel, "Detail Body", 18f, FontStyles.Normal, TextAlignmentOptions.TopLeft);
        detailBody.color = Muted;
        detailBody.textWrappingMode = TextWrappingModes.Normal;
        detailBody.overflowMode = TextOverflowModes.Ellipsis;
        Place(detailBody.rectTransform, 16f, -50f, 0.62f, 48f);
        detailEffect = CreateText(panel, "Detail Effect", 18f, FontStyles.Normal, TextAlignmentOptions.TopLeft);
        detailEffect.color = Ink;
        detailEffect.textWrappingMode = TextWrappingModes.Normal;
        detailEffect.overflowMode = TextOverflowModes.Ellipsis;
        Place(detailEffect.rectTransform, 16f, -100f, 0.62f, 44f);
        detailLevel = CreateText(panel, "Detail Level", 18f, FontStyles.Bold, TextAlignmentOptions.BottomLeft);
        Place(detailLevel.rectTransform, 16f, 42f, 0.42f, 28f);
        detailLevel.rectTransform.anchorMin = new Vector2(0f, 0f);
        detailLevel.rectTransform.anchorMax = new Vector2(0.42f, 0f);
        detailLevel.rectTransform.pivot = new Vector2(0f, 0f);
        detailStatus = CreateText(panel, "Detail Status", 18f, FontStyles.Normal, TextAlignmentOptions.BottomLeft);
        detailStatus.color = Muted;
        detailStatus.textWrappingMode = TextWrappingModes.Normal;
        Place(detailStatus.rectTransform, 16f, 8f, 0.5f, 32f);
        detailStatus.rectTransform.anchorMin = new Vector2(0f, 0f);
        detailStatus.rectTransform.anchorMax = new Vector2(0.5f, 0f);
        detailStatus.rectTransform.pivot = new Vector2(0f, 0f);

        detailPriceCaption = CreateText(panel, "Price Caption", 16f, FontStyles.Normal, TextAlignmentOptions.TopRight);
        detailPriceCaption.color = Muted;
        AnchorRight(detailPriceCaption.rectTransform, -250f, -16f, 220f, 24f);
        detailPrice = CreateText(panel, "Price", 36f, FontStyles.Bold, TextAlignmentOptions.TopRight);
        detailPrice.color = PriceInk;
        AnchorRight(detailPrice.rectTransform, -250f, -40f, 220f, 44f);
        detailReward = CreateText(panel, "Reward", 16f, FontStyles.Normal, TextAlignmentOptions.TopRight);
        detailReward.color = Muted;
        AnchorRight(detailReward.rectTransform, -250f, -88f, 220f, 24f);

        buyButton = CreateButton(panel, "Buy Button", new Vector2(-16f, 16f), new Vector2(220f, 52f), out buyLabel);
        buyButton.onClick.AddListener(BuyDisplayed);
        Button reset = CreateButton(panel, "Reset View", new Vector2(-248f, 16f), new Vector2(180f, 40f), out resetLabel);
        reset.onClick.AddListener(() => treeView.ResetView());
    }

    private void FlattenWorkspace()
    {
        Transform background = FindDeep(transform, "Background");
        if (background != null)
        {
            SkillTreeBackground animated = background.GetComponent<SkillTreeBackground>();
            if (animated != null)
            {
                animated.enabled = false;
            }

            Image image = background.GetComponent<Image>();
            if (image != null)
            {
                image.material = null;
                image.color = Workspace;
            }
        }

        Transform scanlines = FindDeep(transform, "Scanlines");
        if (scanlines != null)
        {
            scanlines.gameObject.SetActive(false);
        }

        Transform post = FindDeep(transform, "Post FX");
        if (post != null)
        {
            Behaviour[] behaviours = post.GetComponents<Behaviour>();
            for (int i = 0; i < behaviours.Length; i++)
            {
                behaviours[i].enabled = false;
            }
        }

        SkillTooltip tooltip = GetComponentInChildren<SkillTooltip>(true);
        if (tooltip != null)
        {
            tooltip.Hide();
            tooltip.gameObject.SetActive(false);
        }
    }

    private void RefreshHud()
    {
        if (mbDisplay == null)
        {
            return;
        }

        mbDisplay.text = Loc.F("tree.mb", NumberFormat.Mb(GameProgress.MB));
        mbDisplay.rectTransform.localScale = Vector3.one;
        mbDisplay.color = HeaderInk;
        if (upgradesDisplay != null)
        {
            upgradesDisplay.text = Loc.F("tree.counter", GameProgress.TotalPurchases);
        }

        RefreshDetail();
    }

    private void RefreshDetail()
    {
        if (detailTitle == null || treeView == null || treeView.Definition == null)
        {
            return;
        }

        SkillTreeDefinition tree = treeView.Definition;
        SkillTreeDefinition.Node node = tree.Find(SkillTreeFocus.DisplayedId);
        if (resetLabel != null)
        {
            resetLabel.text = Loc.T("tree.reset_view");
        }

        detailReward.text = GameProgress.TotalPurchases >= ProgressionConfig.TargetPurchases
            ? Loc.T("tree.evolved")
            : Loc.F("tree.file_reward", NumberFormat.Mb(GameplayQuery.StandardReward()));
        buyLabel.text = Loc.T("tree.buy");

        if (node == null)
        {
            detailTitle.text = Loc.T("tree.pick");
            detailBody.text = Loc.T("tree.hint");
            detailEffect.text = string.Empty;
            detailLevel.text = string.Empty;
            detailStatus.text = string.Empty;
            detailPrice.text = string.Empty;
            detailPriceCaption.text = string.Empty;
            buyButton.interactable = false;
            return;
        }

        long price = tree.GetCost(node);
        PurchaseDenial denial = tree.Evaluate(node);
        detailTitle.text = SkillTreeDefinition.GetName(node);
        detailBody.text = SkillTreeDefinition.GetDescription(node);
        detailEffect.text = EffectLine(tree, node);
        detailLevel.text = node.anchor ? Loc.T("tree.core_state") : Loc.F("tree.level", tree.GetLevel(node), node.maxLevel);
        detailPrice.text = NumberFormat.Mb(price) + " MB";
        detailPriceCaption.text = Loc.T("tree.any_price");
        detailStatus.text = node.anchor ? Loc.T("tree.core_state") : denial.CanBuy ? Loc.T("tree.afford_yes") : SkillPurchase.DenialText(denial);
        detailStatus.color = denial.CanBuy ? PriceInk : Muted;
        buyButton.interactable = denial.CanBuy;
    }

    private static string EffectLine(SkillTreeDefinition tree, SkillTreeDefinition.Node node)
    {
        string current = tree.CurrentEffectText(node);
        string next = tree.NextEffectText(node);
        if (!string.IsNullOrEmpty(current) && !string.IsNullOrEmpty(next))
        {
            return current + "  →  " + next;
        }

        return string.IsNullOrEmpty(next) ? current : next;
    }

    private void BuyDisplayed()
    {
        if (treeView == null || treeView.Definition == null)
        {
            return;
        }

        SkillTreeDefinition tree = treeView.Definition;
        SkillTreeDefinition.Node node = tree.Find(SkillTreeFocus.DisplayedId);
        if (node == null)
        {
            return;
        }

        PurchaseDenial denial = tree.Evaluate(node);
        long price = tree.GetCost(node);
        if (!denial.CanBuy)
        {
            Deny(node, SkillPurchase.DenialText(denial));
            return;
        }

        if (!tree.TryBuy(node))
        {
            Deny(node, SkillPurchase.DenialText(tree.Evaluate(node)));
            return;
        }

        if (fx != null && treeView.TryGetNodeRect(node.id, out RectTransform rect))
        {
            fx.PlayPurchase(rect, PriceInk, tree.IsMaxed(node), price);
        }

        GameAudio.Play(GameSfx.LevelUp);
        RefreshHud();
    }

    private void Deny(SkillTreeDefinition.Node node, string reason)
    {
        GameAudio.PlayMouseClick();
        if (fx != null && treeView.TryGetNodeRect(node.id, out RectTransform rect))
        {
            fx.PlayDenied(rect, reason);
        }

        if (detailStatus != null)
        {
            detailStatus.text = reason;
            detailStatus.color = Muted;
        }
    }

    private TextMeshProUGUI CreateText(Transform parent, string name, float size, FontStyles style, TextAlignmentOptions alignment)
    {
        GameObject go = new(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        TextMeshProUGUI text = go.AddComponent<TextMeshProUGUI>();
        text.font = mbDisplay.font;
        text.fontSharedMaterial = mbDisplay.fontSharedMaterial;
        text.fontSize = size;
        text.fontStyle = style;
        text.alignment = alignment;
        text.color = Ink;
        text.raycastTarget = false;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.overflowMode = TextOverflowModes.Overflow;
        return text;
    }

    private Button CreateButton(Transform parent, string name, Vector2 corner, Vector2 size, out TextMeshProUGUI label)
    {
        GameObject go = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        RectTransform rect = (RectTransform)go.transform;
        rect.anchorMin = rect.anchorMax = new Vector2(1f, 0f);
        rect.pivot = new Vector2(1f, 0f);
        rect.anchoredPosition = corner;
        rect.sizeDelta = size;
        Image image = go.GetComponent<Image>();
        Button button = go.GetComponent<Button>();
        button.targetGraphic = image;
        ArtBindings.StyleXpButton(button);
        label = CreateText(rect, "Label", 20f, FontStyles.Bold, TextAlignmentOptions.Center);
        Stretch(label.rectTransform);
        label.color = Ink;
        return button;
    }

    private static void Place(RectTransform rect, float x, float y, float anchorMaxX, float height)
    {
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(anchorMaxX, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(x, y);
        rect.sizeDelta = new Vector2(-x, height);
    }

    private static void AnchorRight(RectTransform rect, float x, float y, float width, float height)
    {
        rect.anchorMin = rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(1f, 1f);
        rect.anchoredPosition = new Vector2(x, y);
        rect.sizeDelta = new Vector2(width, height);
    }

    private static Transform FindDeep(Transform root, string name)
    {
        if (root.name == name)
        {
            return root;
        }

        for (int i = 0; i < root.childCount; i++)
        {
            Transform found = FindDeep(root.GetChild(i), name);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
