using System;
using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreDisplay;
    [SerializeField] private uint score = 0;

    public event Action<uint> ScoreChanged;

    public uint Score => score;

    void Start()
    {
        UpdateDisplay();
    }

    [ContextMenu("Increment Score")]
    public void IncrementScore()
    {
        AddScore(1);
    }

    public void AddScore(uint count)
    {
        score += count;
        UpdateDisplay();
    }

    public bool TrySpend(uint amount)
    {
        if (amount > score)
        {
            return false;
        }

        score -= amount;
        UpdateDisplay();
        return true;
    }

    private void UpdateDisplay()
    {
        scoreDisplay.text = $"MBs: {score}";
        ScoreChanged?.Invoke(score);
    }
}
