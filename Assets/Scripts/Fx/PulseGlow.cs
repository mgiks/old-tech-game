using UnityEngine;
using UnityEngine.UI;

public class PulseGlow : MonoBehaviour
{
    [SerializeField] private Graphic target;
    [SerializeField] private float speed = 1.2f;
    [SerializeField, Range(0f, 1f)] private float minAlpha = 0.35f;
    [SerializeField, Range(0f, 1f)] private float maxAlpha = 1f;
    [SerializeField] private float scaleAmount = 0.08f;

    void Update()
    {
        float k = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * speed * Mathf.PI * 2f);

        Color color = target.color;
        color.a = Mathf.Lerp(minAlpha, maxAlpha, k);
        target.color = color;

        target.rectTransform.localScale = Vector3.one * (1f + k * scaleAmount);
    }
}
