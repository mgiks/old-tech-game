using UnityEngine;

public class ScreenShake : MonoBehaviour
{
    [SerializeField] private float decay = 7f;
    [SerializeField] private float frequency = 28f;
    [SerializeField] private float maxStrength = 45f;
    [SerializeField] private float rotationPerPixel = 0.04f;

    private RectTransform rect;
    private Vector2 basePosition;
    private float strength;
    private float seed;

    void Awake()
    {
        rect = (RectTransform)transform;
        basePosition = rect.anchoredPosition;
        seed = Random.value * 100f;
    }

    public void Add(float amount)
    {
        if (!GameSettings.ScreenShake)
        {
            return;
        }

        strength = Mathf.Min(strength + amount, maxStrength);
    }

    void Update()
    {
        strength *= Mathf.Exp(-decay * Time.unscaledDeltaTime);

        float t = Time.unscaledTime * frequency;
        Vector2 offset = new Vector2(
            Mathf.PerlinNoise(seed, t) - 0.5f,
            Mathf.PerlinNoise(seed + 7.3f, t) - 0.5f) * (2f * strength);

        rect.anchoredPosition = basePosition + offset;
        rect.localRotation = Quaternion.Euler(0f, 0f, offset.x * rotationPerPixel);
    }
}
