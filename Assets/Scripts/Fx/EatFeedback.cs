using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EatFeedback : MonoBehaviour
{
    private class Particle
    {
        public SpriteRenderer renderer;
        public Vector2 velocity;
        public float age;
        public float life;
        public float startSize;
        public float endSize;
        public Color color;
        public float spriteSize;
        public bool active;
    }

    private class Popup
    {
        public TextMeshProUGUI text;
        public float age;
        public float scale;
        public bool active;
    }

    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private Virus virus;
    [SerializeField] private RectTransform popupRoot;
    [SerializeField] private Sprite particleSprite;
    [SerializeField] private Sprite ringSprite;
    [SerializeField] private int sortingOrder = 50;
    [SerializeField] private Color normalColor = new(0.45f, 1f, 0.55f, 1f);
    [SerializeField] private Color goldenColor = new(1f, 0.82f, 0.25f, 1f);
    [SerializeField] private Color confidentialColor = new(0.95f, 0.2f, 0.55f, 1f);
    [SerializeField] private float popupLife = 0.8f;
    [SerializeField] private float goldenShake = 0.12f;

    public Sprite ParticleSprite => particleSprite;

    private readonly List<Particle> particles = new();
    private readonly List<Popup> popups = new();
    private Camera cam;
    private Vector3 cameraBase;
    private float shake;

    void Awake()
    {
        cam = Camera.main;
        cameraBase = cam.transform.position;

        for (int i = 0; i < 80; i++)
        {
            GameObject go = new("Eat Particle", typeof(SpriteRenderer));
            go.transform.SetParent(transform, false);
            SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
            sr.sortingOrder = sortingOrder;
            go.SetActive(false);
            particles.Add(new Particle { renderer = sr });
        }

        for (int i = 0; i < 16; i++)
        {
            GameObject go = new("Eat Popup", typeof(RectTransform), typeof(TextMeshProUGUI));
            RectTransform rect = (RectTransform)go.transform;
            rect.SetParent(popupRoot, false);
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(900f, 80f);
            TextMeshProUGUI text = go.GetComponent<TextMeshProUGUI>();
            text.alignment = TextAlignmentOptions.Center;
            text.fontStyle = FontStyles.Bold;
            text.raycastTarget = false;
            text.outlineWidth = 0.2f;
            text.outlineColor = new Color32(0, 0, 0, 200);
            go.SetActive(false);
            popups.Add(new Popup { text = text });
        }
    }

    void OnEnable()
    {
        scoreManager.FileScored += OnFileScored;
    }

    void OnDisable()
    {
        scoreManager.FileScored -= OnFileScored;
    }

    public void Shake(float amount)
    {
        if (!GameSettings.ScreenShake)
        {
            return;
        }

        shake = Mathf.Max(shake, amount);
    }

    public void Burst(Vector2 position, Color color, int count, float speed)
    {
        for (int i = 0; i < count; i++)
        {
            float angle = Random.value * Mathf.PI * 2f;
            Vector2 direction = new(Mathf.Cos(angle), Mathf.Sin(angle));
            Emit(particleSprite, position, direction * Random.Range(speed * 0.4f, speed), Random.Range(0.3f, 0.6f), Random.Range(0.12f, 0.25f), 0f, color);
        }
    }

    public void Ring(Vector2 position, Color color, float size)
    {
        Emit(ringSprite, position, Vector2.zero, 0.4f, size * 0.3f, size, color);
    }

    private void OnFileScored(Vector3 iconPosition, uint value, bool golden, bool confidential)
    {
        Vector2 center = virus.GetAbsorbPoint();
        float virusSize = virus.GetVisualBounds().size.y;
        Color color = confidential ? confidentialColor : golden ? goldenColor : normalColor;
        bool special = golden || confidential;

        Burst(center, color, confidential ? 34 : golden ? 26 : 10, confidential ? 9f : golden ? 7f : 4f);
        Ring(center, color, virusSize * (confidential ? 2.8f : golden ? 2.2f : 1.5f));
        ShowPopup(center + Vector2.up * (virusSize * 0.5f), special ? $"+{value} MB!" : $"+{value}", color, confidential ? 1.7f : golden ? 1.5f : 1f);

        if (special)
        {
            Shake(confidential ? goldenShake * 1.4f : goldenShake);
        }
    }

    void LateUpdate()
    {
        float dt = Time.unscaledDeltaTime;

        shake = GameSettings.ScreenShake ? Mathf.Max(0f, shake - dt * 0.6f) : 0f;
        cam.transform.position = cameraBase + (Vector3)(Random.insideUnitCircle * shake);

        foreach (Particle p in particles)
        {
            if (!p.active)
            {
                continue;
            }

            p.age += dt;
            if (p.age >= p.life)
            {
                p.active = false;
                p.renderer.gameObject.SetActive(false);
                continue;
            }

            float t = p.age / p.life;
            p.velocity *= Mathf.Exp(-4f * dt);
            p.renderer.transform.position += (Vector3)(p.velocity * dt);
            float size = Mathf.Lerp(p.startSize, p.endSize > 0f ? p.endSize : p.startSize * 0.2f, t);
            p.renderer.transform.localScale = Vector3.one * (size / p.spriteSize);
            Color c = p.color;
            c.a *= 1f - t;
            p.renderer.color = c;
        }

        foreach (Popup popup in popups)
        {
            if (!popup.active)
            {
                continue;
            }

            popup.age += dt;
            if (popup.age >= popupLife)
            {
                popup.active = false;
                popup.text.gameObject.SetActive(false);
                continue;
            }

            float t = popup.age / popupLife;
            popup.text.rectTransform.anchoredPosition += Vector2.up * (140f * dt);
            float pop = t < 0.15f ? Mathf.Lerp(1.8f, 1f, t / 0.15f) : 1f;
            popup.text.rectTransform.localScale = Vector3.one * (popup.scale * pop);
            popup.text.alpha = t < 0.6f ? 1f : 1f - (t - 0.6f) / 0.4f;
        }
    }

    private void Emit(Sprite sprite, Vector2 position, Vector2 velocity, float life, float startSize, float endSize, Color color)
    {
        Particle p = particles.Find(x => !x.active) ?? particles[Random.Range(0, particles.Count)];
        p.active = true;
        p.age = 0f;
        p.life = life;
        p.velocity = velocity;
        p.startSize = startSize;
        p.endSize = endSize;
        p.color = color;
        p.spriteSize = Mathf.Max(sprite.bounds.size.x, 0.001f);
        p.renderer.sprite = sprite;
        p.renderer.color = color;
        p.renderer.transform.position = new Vector3(position.x, position.y, 0f);
        p.renderer.transform.localScale = Vector3.one * (startSize / p.spriteSize);
        p.renderer.gameObject.SetActive(true);
    }

    public void ShowPopup(Vector2 worldPosition, string message, Color color, float scale)
    {
        Popup popup = popups.Find(x => !x.active) ?? popups[0];
        RectTransformUtility.ScreenPointToLocalPointInRectangle(popupRoot, cam.WorldToScreenPoint(worldPosition), null, out Vector2 local);

        popup.active = true;
        popup.age = 0f;
        popup.scale = scale;
        popup.text.text = message;
        popup.text.color = color;
        popup.text.fontSize = 46f;
        popup.text.rectTransform.anchoredPosition = local + new Vector2(Random.Range(-40f, 40f), 0f);
        popup.text.gameObject.SetActive(true);
    }
}
