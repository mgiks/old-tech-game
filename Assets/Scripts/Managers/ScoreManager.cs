using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreDisplay;
    [SerializeField] private uint score = 0;

    [ContextMenu("Add Score")]
    public void AddScore(uint count)
    {
        score += count;
        scoreDisplay.text = $"MBs: {score}";
    }
}