using UnityEngine;
using UnityEngine.InputSystem;

public class VirusLook : MonoBehaviour
{
    private const float Pixel = 1f / 32f;

    private Virus virus;
    private SpriteRenderer body;
    private SpriteRenderer shadow;
    private SpriteRenderer glow;
    private SpriteRenderer[] outline;
    private float eat;
    private float deny;
    private float click;

    void Awake()
    {
        virus = GetComponent<Virus>();
        body = GetComponent<SpriteRenderer>();
        if (body == null || body.sprite == null)
        {
            return;
        }

        int order = Mathf.Max(body.sortingOrder, 4);
        body.sortingOrder = order + 2;
        Sprite sprite = body.sprite;
        shadow = Make("Shadow", sprite, order - 1, new Color(0.02f, 0.06f, 0.12f, 0.55f), new Vector3(Pixel, -Pixel, 0f), 1f);
        glow = Make("Glow", sprite, order - 2, new Color(0.25f, 0.85f, 0.4f, 0.22f), Vector3.zero, 1.4f);

        Vector3[] offsets =
        {
            new(Pixel, 0f, 0f),
            new(-Pixel, 0f, 0f),
            new(0f, Pixel, 0f),
            new(0f, -Pixel, 0f)
        };
        outline = new SpriteRenderer[offsets.Length];
        for (int i = 0; i < offsets.Length; i++)
        {
            outline[i] = Make("Outline", sprite, order + 1, new Color(0.02f, 0.1f, 0.04f, 0.95f), offsets[i], 1f);
        }
    }

        float overload;

    public void SetOverload(float amount)
    {
        overload = Mathf.Clamp01(amount);
    }

    public void PlayEat()
    {
        eat = 1f;
    }

    public void PlayDeny()
    {
        deny = 1f;
    }

    void Update()
    {
        if (body == null)
        {
            return;
        }

        bool visible = body.enabled;
        SetExtras(visible);
        if (!visible)
        {
            return;
        }

        float dt = Time.deltaTime;
        eat = Mathf.Max(0f, eat - dt * 2.2f);
        deny = Mathf.Max(0f, deny - dt * 1.5f);
        click = Mathf.Max(0f, click - dt * 3.5f);
        WatchClick();

        float pulse = 0.65f + 0.35f * Mathf.Sin(Time.time * (5f + overload * 8f));
        if (glow != null)
        {
            Color glowColor = deny > 0.01f
                ? new Color(0.95f, 0.25f, 0.22f, 0.2f + deny * 0.45f)
                : new Color(0.3f + overload * 0.6f, 0.9f, 0.45f, 0.14f + pulse * 0.12f + eat * 0.4f + click * 0.25f + overload * 0.35f);
            glow.color = glowColor;
            float grow = 1.4f + eat * 0.25f + click * 0.12f;
            glow.transform.localScale = new Vector3(grow, grow, 1f);
        }

        Color tint = Color.Lerp(Color.white, new Color(0.75f, 1f, 0.82f), Mathf.Max(eat, click));
        if (deny > 0.01f)
        {
            tint = Color.Lerp(tint, new Color(1f, 0.4f, 0.38f), deny);
        }

        body.color = tint;
    }

    private void SetExtras(bool visible)
    {
        if (shadow != null)
        {
            shadow.enabled = visible;
        }

        if (glow != null)
        {
            glow.enabled = visible;
        }

        if (outline == null)
        {
            return;
        }

        for (int i = 0; i < outline.Length; i++)
        {
            if (outline[i] != null)
            {
                outline[i].enabled = visible;
            }
        }
    }

    private void WatchClick()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null || virus == null || !mouse.leftButton.wasPressedThisFrame)
        {
            return;
        }

        if (MonitorScreen.Active && !MonitorScreen.OnGlass)
        {
            return;
        }

        Camera cam = Camera.main;
        if (cam == null || !virus.ContainsPoint(MonitorScreen.ToWorld(cam)))
        {
            return;
        }

        click = 1f;
    }

    private SpriteRenderer Make(string name, Sprite sprite, int order, Color color, Vector3 offset, float scale)
    {
        GameObject go = new(name, typeof(SpriteRenderer));
        go.transform.SetParent(transform, false);
        go.transform.localPosition = offset;
        go.transform.localScale = new Vector3(scale, scale, 1f);
        SpriteRenderer renderer = go.GetComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = color;
        renderer.sortingOrder = order;
        renderer.sharedMaterial = body.sharedMaterial;
        return renderer;
    }
}
