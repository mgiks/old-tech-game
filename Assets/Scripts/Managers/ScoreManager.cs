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

    public event Action<Vector3, uint, bool> FileScored;

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
        Combo = Time.time - lastFileTime <= comboWindowInSeconds ? Combo + 1 : 1;
        lastFileTime = Time.time;

        uint value = baseFileValue + (uint)Mathf.RoundToInt(skillTree.GetStat(SkillStat.FileValue));
        if (icon.IsGolden)
        {
            value *= goldenMultiplier;
        }

        float comboMultiplier = 1f + skillTree.GetStat(SkillStat.Combo) * Mathf.Min(Combo - 1, comboCap);
        value = (uint)Mathf.Max(value, Mathf.RoundToInt(value * comboMultiplier));

        AddScore(value);
        FileScored?.Invoke(icon.transform.position, value, icon.IsGolden);
    }

    public void AddScore(uint count)
    {
        RunStats.AddMB(count);
        GameProgress.AddMB(count);
        punch = 1f;
    }

    private void UpdateDisplay()
    {
        scoreDisplay.text = $"MB: {GameProgress.MB}";
    }
}
