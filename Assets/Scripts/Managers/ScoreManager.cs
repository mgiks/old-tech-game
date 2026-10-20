using System;
using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreDisplay;
    [SerializeField] private SkillTreeDefinition skillTree;
    [SerializeField] private uint baseFileValue = 1;
    [SerializeField] private uint goldenMultiplier = 5;
    [SerializeField] private float punchScale = 1.4f;
    [SerializeField] private Color punchColor = new(0.45f, 1f, 0.55f, 1f);

    [Header("Combo")]
    [SerializeField] private float comboWindowInSeconds = 1.2f;
    [SerializeField, Min(1)] private int comboCap = 10;

    public event Action<Vector3, long, bool> FileScored;

    public int Combo { get; private set; }
    public bool ComboUnlocked => skillTree.GetStat(SkillStat.Combo) > 0f;

    private float lastFileTime = -100f;
    private float punch;
    private Color baseColor;

    void Awake()
    {
        baseColor = scoreDisplay.color;
    }

    void OnEnable()
    {
        GameProgress.Changed += UpdateDisplay;
        UpdateDisplay();
    }

    void OnDisable()
    {
        GameProgress.Changed -= UpdateDisplay;
    }

    void Update()
    {
        if (punch <= 0f)
        {
            return;
        }

        punch = Mathf.Max(0f, punch - Time.unscaledDeltaTime * 4f);
        scoreDisplay.rectTransform.localScale = Vector3.one * Mathf.Lerp(1f, punchScale, punch);
        scoreDisplay.color = Color.Lerp(baseColor, punchColor, punch);
    }

    [ContextMenu("Increment Score")]
    public void IncrementScore()
    {
        AddScore(1);
    }

    public void AddFile(DesktopIcon icon)
    {
        if (icon == null || skillTree == null || !icon.TryMarkReward())
        {
            return;
        }

        long value = EconomyMath.FileReward(
            GameProgress.TotalPurchases,
            GameProgress.GetSkillLevel(SkillBalanceTable.B1),
            GameProgress.GetSkillLevel(SkillBalanceTable.B2),
            icon.IsGolden);
        AddScore(value);
        GameEvents.RaiseConsumed(icon);
        FileScored?.Invoke(icon.transform.position, value, icon.IsGolden);
    }

    public void AddScore(long count)
    {
        if (count <= 0)
        {
            return;
        }

        RunStats.AddMB(count);
        GameProgress.AddMB(count);
        punch = 1f;
    }

    private void UpdateDisplay()
    {
        scoreDisplay.text = "MB: " + NumberFormat.Mb(GameProgress.MB);
    }
}
