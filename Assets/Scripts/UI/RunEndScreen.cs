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
        upgradeButton.onClick.AddListener(() => Choose(onUpgrade));
        continueButton.onClick.AddListener(() => Choose(onContinue));
        gameObject.SetActive(false);
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
        onUpgrade = upgrade;
        onContinue = next;
        upgradeButton.interactable = true;
        continueButton.interactable = true;
        gameObject.SetActive(true);
        StartCoroutine(PlayIn());
    }

    private void Choose(Action action)
    {
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
