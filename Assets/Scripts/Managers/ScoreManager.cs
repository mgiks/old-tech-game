using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreDisplay;
    [SerializeField] private uint score = 0;

    [ContextMenu("Increment Score")]
    public void IncrementScore()
    {
        AddScore(1);
    }

    public void AddScore(uint count)
    {
        score += count;
        scoreDisplay.text = $"MBs: {score}";
    }
}