using System.Text;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class GlitchText : MonoBehaviour
{
    [SerializeField] private Vector2 intervalRange = new(1.5f, 4f);
    [SerializeField] private float glitchDuration = 0.18f;
    [SerializeField] private float maxOffset = 8f;
    [SerializeField, Range(0f, 1f)] private float scrambleChance = 0.25f;
    [SerializeField] private string glyphs = "#$%&@!?/\\<>01";
    [SerializeField] private Color[] glitchColors =
    {
        new(0.3f, 1f, 1f, 1f),
        new(1f, 0.3f, 0.9f, 1f),
        new(0.4f, 1f, 0.5f, 1f),
    };

    private TMP_Text text;
    private RectTransform rect;
    private string original;
    private Color originalColor;
    private Vector2 basePosition;
    private float nextGlitch;
    private float glitchEnd;
    private bool glitching;
    private readonly StringBuilder builder = new();

    void Awake()
    {
        text = GetComponent<TMP_Text>();
        rect = (RectTransform)transform;
        original = text.text;
        originalColor = text.color;
        basePosition = rect.anchoredPosition;
        nextGlitch = Time.unscaledTime + Random.Range(intervalRange.x, intervalRange.y);
    }

    void Update()
    {
        float now = Time.unscaledTime;

        if (now >= nextGlitch)
        {
            glitchEnd = now + glitchDuration;
            nextGlitch = now + Random.Range(intervalRange.x, intervalRange.y);
            glitching = true;
        }

        if (glitching && now < glitchEnd)
        {
            text.text = Scramble(original);
            text.color = glitchColors[Random.Range(0, glitchColors.Length)];
            rect.anchoredPosition = basePosition + Random.insideUnitCircle * maxOffset;
        }
        else if (glitching)
        {
            glitching = false;
            text.text = original;
            text.color = originalColor;
            rect.anchoredPosition = basePosition;
        }
    }

    private string Scramble(string source)
    {
        builder.Clear();
        foreach (char c in source)
        {
            builder.Append(!char.IsWhiteSpace(c) && Random.value < scrambleChance ? glyphs[Random.Range(0, glyphs.Length)] : c);
        }

        return builder.ToString();
    }
}
