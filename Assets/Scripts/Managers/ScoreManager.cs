using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreDisplay;

    void OnEnable()
    {
        GameProgress.Changed += UpdateDisplay;
        UpdateDisplay();
    }

    void OnDisable()
    {
        GameProgress.Changed -= UpdateDisplay;
    }

    [ContextMenu("Increment Score")]
    public void IncrementScore()
    {
        AddScore(1);
    }

    public void AddScore(uint count)
    {
        RunStats.AddMB(count);
        GameProgress.AddMB(count);
    }

    private void UpdateDisplay()
    {
        scoreDisplay.text = $"MBs: {GameProgress.MB}";
    }
}
