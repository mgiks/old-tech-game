using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillTreeFx : MonoBehaviour
{
    private class Particle
    {
        public RectTransform rect;
        public Image image;
        public Vector2 position;
        public Vector2 velocity;
        public Color color;
        public float age;
        public float life;
        public float startSize;
        public float endSize;
        public float drag;
        public float stretch;
        public bool active;
    }

    private class Popup
    {
        public TextMeshProUGUI text;
        public Vector2 position;
        public float age;
        public float life;
        public float baseScale;
        public bool active;
    }

    [SerializeField] private RectTransform fxRoot;
    [SerializeField] private Sprite particleSprite;
    [SerializeField] private Sprite ringSprite;
    [SerializeField] private Material additiveMaterial;
    [SerializeField] private Material popupFontMaterial;
    [SerializeField] private ScreenShake shake;
    [SerializeField] private PostFxPunch postFx;
    [SerializeField] private SkillTreeBackground background;
    [SerializeField] private Image flash;
    [SerializeField] private int particlePoolSize = 320;
    [SerializeField] private int popupPoolSize = 12;
    [SerializeField] private Color costColor = new(1f, 0.45f, 0.5f, 1f);
    [SerializeField] private Color deniedColor = new(1f, 0.25f, 0.3f, 1f);

    private readonly List<Particle> particles = new();
    private readonly List<Popup> popups = new();
    private float flashAlpha;

    void Awake()
    {
        _ = shake;
        _ = postFx;
        _ = background;
        for (int i = 0; i < particlePoolSize; i++)
        {
            GameObject go = new("Particle", typeof(RectTransform), typeof(Image));
            RectTransform rect = (RectTransform)go.transform;
            rect.SetParent(fxRoot, false);
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);

            Image image = go.GetComponent<Image>();
            image.material = additiveMaterial;
            image.raycastTarget = false;
            go.SetActive(false);

            particles.Add(new Particle { rect = rect, image = image });
        }

        for (int i = 0; i < popupPoolSize; i++)
        {
            GameObject go = new("Popup", typeof(RectTransform), typeof(TextMeshProUGUI));
            RectTransform rect = (RectTransform)go.transform;
            rect.SetParent(fxRoot, false);
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(700f, 90f);

            TextMeshProUGUI text = go.GetComponent<TextMeshProUGUI>();
            text.alignment = TextAlignmentOptions.Center;
            text.fontStyle = FontStyles.Bold;
            text.raycastTarget = false;
            if (popupFontMaterial != null)
            {
                text.fontSharedMaterial = popupFontMaterial;
            }

            go.SetActive(false);
            popups.Add(new Popup { text = text });
        }

        if (flash != null)
        {
            flash.color = new Color(1f, 1f, 1f, 0f);
            flash.enabled = false;
        }
    }

    public void PlayPurchase(RectTransform node, Color color, bool maxed, long cost)
    {
        Vector2 p = ToFx(node);
        ShowPopup(p + Vector2.up * 64f, "-" + NumberFormat.Mb(cost) + " MB", costColor, 0.8f);
        _ = color;
        _ = maxed;
    }

    public void PlayReveal(RectTransform node, Color color)
    {
        _ = node;
        _ = color;
    }

    public void PlayIntroPop(RectTransform node, Color color)
    {
        _ = node;
        _ = color;
    }

    public void PlayPoke(RectTransform node, Color color)
    {
        _ = node;
        _ = color;
    }

    public void PlayDenied(RectTransform node, string message)
    {
        if (node == null)
        {
            return;
        }

        Vector2 p = ToFx(node);
        ShowPopup(p + Vector2.up * 64f, message, deniedColor, 1.1f);
    }

    void Update()
    {
        float dt = Time.unscaledDeltaTime;

        foreach (Particle particle in particles)
        {
            if (!particle.active)
            {
                continue;
            }

            particle.age += dt;
            if (particle.age >= particle.life)
            {
                particle.active = false;
                particle.rect.gameObject.SetActive(false);
                continue;
            }

            float t = particle.age / particle.life;
            particle.velocity *= Mathf.Exp(-particle.drag * dt);
            particle.position += particle.velocity * dt;

            float size = Mathf.Lerp(particle.startSize, particle.endSize, 1f - (1f - t) * (1f - t));
            float speed = particle.velocity.magnitude;

            particle.rect.anchoredPosition = particle.position;
            particle.rect.sizeDelta = new Vector2(size * (1f + speed * particle.stretch), size);
            if (particle.stretch > 0f && speed > 1f)
            {
                particle.rect.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(particle.velocity.y, particle.velocity.x) * Mathf.Rad2Deg);
            }

            Color color = particle.color;
            color.a *= Mathf.Pow(1f - t, 1.5f);
            particle.image.color = color;
        }

        foreach (Popup popup in popups)
        {
            if (!popup.active)
            {
                continue;
            }

            popup.age += dt;
            if (popup.age >= popup.life)
            {
                popup.active = false;
                popup.text.gameObject.SetActive(false);
                continue;
            }

            float t = popup.age / popup.life;
            popup.position += Vector2.up * (90f * dt);
            popup.text.rectTransform.anchoredPosition = popup.position;

            float pop = t < 0.15f ? Mathf.Lerp(1.6f, 1f, t / 0.15f) : 1f;
            popup.text.rectTransform.localScale = Vector3.one * (popup.baseScale * pop);
            popup.text.alpha = t < 0.6f ? 1f : 1f - (t - 0.6f) / 0.4f;
        }

        if (flash != null)
        {
            flashAlpha *= Mathf.Exp(-7f * dt);
            flash.enabled = flashAlpha > 0.003f;
            flash.color = new Color(1f, 1f, 1f, flashAlpha);
        }
    }

    private void EmitSpark(Vector2 position, Vector2 velocity, float life, float size, Color color)
    {
        Emit(particleSprite, position, velocity, life, size, 0f, 4.5f, 0.035f, color);
    }

    private void EmitRing(Vector2 position, float startSize, float endSize, float life, Color color)
    {
        Emit(ringSprite, position, Vector2.zero, life, startSize, endSize, 0f, 0f, color);
    }

    private void Emit(Sprite sprite, Vector2 position, Vector2 velocity, float life, float startSize, float endSize, float drag, float stretch, Color color)
    {
        Particle particle = GetFreeParticle();
        particle.active = true;
        particle.age = 0f;
        particle.life = life;
        particle.position = position;
        particle.velocity = velocity;
        particle.startSize = startSize;
        particle.endSize = endSize;
        particle.drag = drag;
        particle.stretch = stretch;
        particle.color = color;

        particle.image.sprite = sprite;
        particle.rect.localRotation = Quaternion.identity;
        particle.rect.anchoredPosition = position;
        particle.rect.sizeDelta = new Vector2(startSize, startSize);
        particle.image.color = color;
        particle.rect.gameObject.SetActive(true);
    }

    private Particle GetFreeParticle()
    {
        Particle oldest = particles[0];
        foreach (Particle particle in particles)
        {
            if (!particle.active)
            {
                return particle;
            }

            if (particle.age / particle.life > oldest.age / oldest.life)
            {
                oldest = particle;
            }
        }

        return oldest;
    }

    private void ShowPopup(Vector2 position, string message, Color color, float scale)
    {
        Popup popup = popups[0];
        foreach (Popup candidate in popups)
        {
            if (!candidate.active)
            {
                popup = candidate;
                break;
            }

            if (candidate.age > popup.age)
            {
                popup = candidate;
            }
        }

        popup.active = true;
        popup.age = 0f;
        popup.life = 1f;
        popup.position = position;
        popup.baseScale = scale;
        popup.text.text = message;
        popup.text.color = color;
        popup.text.fontSize = 30f;
        popup.text.rectTransform.anchoredPosition = position;
        popup.text.gameObject.SetActive(true);
        popup.text.transform.SetAsLastSibling();
    }

    private Vector2 ToFx(RectTransform target)
    {
        return fxRoot.InverseTransformPoint(target.position);
    }

    private static Vector2 RandomDirection()
    {
        float angle = Random.value * Mathf.PI * 2f;
        return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
    }
}
