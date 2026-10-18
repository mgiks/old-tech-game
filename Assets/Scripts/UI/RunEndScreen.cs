using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RunEndScreen : MonoBehaviour
{
    [SerializeField] private CanvasGroup group;
    [SerializeField] private RectTransform monitor;
    [SerializeField] private RectTransform virusDecor;
    [SerializeField] private Image computersFill;
    [SerializeField] private TextMeshProUGUI computersCount;
    [SerializeField] private TextMeshProUGUI totalValue;
    [SerializeField] private TextMeshProUGUI filesValue;
    [SerializeField] private TextMeshProUGUI clicksValue;
    [SerializeField] private TextMeshProUGUI gbValue;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private Button continueButton;

    [SerializeField] private float zoomOutDuration = 0.45f;
    [SerializeField] private float startMonitorScale = 1.3f;
    [SerializeField] private float countDuration = 1.2f;
    [SerializeField] private float virusSpinSpeed = 12f;

    private Action onUpgrade;
    private Action onContinue;

    void Awake()
    {
        upgradeButton.onClick.AddListener(() => Choose(onUpgrade));
        continueButton.onClick.AddListener(() => Choose(onContinue));
        gameObject.SetActive(false);
    }

    void Update()
    {
        if (virusDecor != null)
        {
            virusDecor.Rotate(0f, 0f, virusSpinSpeed * Time.unscaledDeltaTime);
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
        int total = Mathf.Max(RunStats.TotalComputers, 1);
        computersCount.text = $"{RunStats.ComputersInfected}/{total}";
        SetValues(0f);
        computersFill.fillAmount = 0f;

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
    }

    private void SetValues(float k)
    {
        totalValue.text = $"{Mathf.RoundToInt(RunStats.MBEarned * k)} MB";
        filesValue.text = Mathf.RoundToInt(RunStats.FilesInfected * k).ToString();
        clicksValue.text = Mathf.RoundToInt(RunStats.Clicks * k).ToString();
        gbValue.text = (RunStats.GBEarned * k).ToString("0.00");
    }
}
