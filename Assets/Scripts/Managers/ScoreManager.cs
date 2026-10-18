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

    public event Action<Vector3, uint, bool> FileScored;

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
        uint value = baseFileValue + (uint)Mathf.RoundToInt(skillTree.GetStat(SkillStat.FileValue));
        if (icon.IsGolden)
        {
            value *= goldenMultiplier;
        }

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
