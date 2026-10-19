using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Flags]
public enum XpCorners
{
    None = 0,
    TopLeft = 1,
    TopRight = 2,
    BottomLeft = 4,
    BottomRight = 8,
    Top = TopLeft | TopRight,
    Bottom = BottomLeft | BottomRight,
    Right = TopRight | BottomRight,
    All = Top | Bottom
}

public static class XpUi
{
    public static readonly Color TextDark = new(0.04f, 0.14f, 0.42f, 1f);
    public static readonly Color HoverBlue = Hex("#316AC5");
    public static readonly Color WindowBody = Hex("#ECE9D8");
    public static readonly Color WindowFrame = Hex("#0054E3");
    public static readonly Color ButtonBorder = Hex("#003C74");

    private static readonly Dictionary<string, Sprite> cache = new();

    public static TMP_FontAsset Font => TMP_Settings.defaultFontAsset;

    public static Color Hex(string hex)
    {
        return ColorUtility.TryParseHtmlString(hex, out Color color) ? color : Color.magenta;
    }

    public static Sprite Panel(string id, int width, int height, int radius, XpCorners corners, params Color[] gradient)
    {
        if (cache.TryGetValue(id, out Sprite cached) && cached != null)
        {
            return cached;
        }

        Texture2D texture = CreateTexture(width, height);
        Color[] pixels = new Color[width * height];
        for (int y = 0; y < height; y++)
        {
            Color row = Sample(gradient, 1f - (y + 0.5f) / height);
            for (int x = 0; x < width; x++)
            {
                Color c = row;
                c.a *= CornerAlpha(x, y, width, height, radius, corners);
                pixels[y * width + x] = c;
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();

        float l = (corners & XpCorners.TopLeft) != 0 || (corners & XpCorners.BottomLeft) != 0 ? radius + 1 : 1;
        float r = (corners & XpCorners.TopRight) != 0 || (corners & XpCorners.BottomRight) != 0 ? radius + 1 : 1;
        float t = (corners & XpCorners.Top) != 0 ? radius + 1 : 1;
        float b = (corners & XpCorners.Bottom) != 0 ? radius + 1 : 1;
        return Store(id, texture, new Vector4(l, b, r, t));
    }

    public static Sprite Rounded(int radius)
    {
        return Panel($"rounded{radius}", radius * 2 + 4, radius * 2 + 4, radius, XpCorners.All, Color.white);
    }

    public static Sprite Circle()
    {
        return Panel("circle", 64, 64, 32, XpCorners.All, Color.white);
    }

    public static Sprite Ring(float thicknessFraction)
    {
        string id = $"ring{thicknessFraction:0.00}";
        if (cache.TryGetValue(id, out Sprite cached) && cached != null)
        {
            return cached;
        }

        const int size = 64;
        Texture2D texture = CreateTexture(size, size);
        float outer = size * 0.5f - 1f;
        float half = outer * thicknessFraction * 0.5f;
        float middle = outer - half;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(size * 0.5f, size * 0.5f));
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Clamp01(half - Mathf.Abs(d - middle) + 0.5f)));
            }
        }

        texture.Apply();
        return Store(id, texture, Vector4.zero);
    }

    public static Sprite Triangle()
    {
        if (cache.TryGetValue("triangle", out Sprite cached) && cached != null)
        {
            return cached;
        }

        const int size = 64;
        Texture2D texture = CreateTexture(size, size);
        Vector2 a = new(10f, 6f);
        Vector2 b = new(10f, 58f);
        Vector2 c = new(58f, 32f);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                int inside = 0;
                for (int sy = 0; sy < 4; sy++)
                {
                    for (int sx = 0; sx < 4; sx++)
                    {
                        Vector2 p = new(x + (sx + 0.5f) / 4f, y + (sy + 0.5f) / 4f);
                        if (Side(a, b, p) <= 0f && Side(b, c, p) <= 0f && Side(c, a, p) <= 0f)
                        {
                            inside++;
                        }
                    }
                }

                texture.SetPixel(x, y, new Color(1f, 1f, 1f, inside / 16f));
            }
        }

        texture.Apply();
        return Store("triangle", texture, Vector4.zero);
    }

    public static RectTransform Rect(string name, Transform parent)
    {
        GameObject go = new(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return (RectTransform)go.transform;
    }

    public static Image Image(string name, Transform parent, Sprite sprite, Color color, bool raycast = false)
    {
        RectTransform rect = Rect(name, parent);
        Image image = rect.gameObject.AddComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        image.raycastTarget = raycast;
        if (sprite != null && sprite.border != Vector4.zero)
        {
            image.type = UnityEngine.UI.Image.Type.Sliced;
        }

        return image;
    }

    public static TextMeshProUGUI Text(string name, Transform parent, string value, float size, Color color,
        TextAlignmentOptions alignment = TextAlignmentOptions.MidlineLeft, FontStyles style = FontStyles.Normal)
    {
        RectTransform rect = Rect(name, parent);
        TextMeshProUGUI text = rect.gameObject.AddComponent<TextMeshProUGUI>();
        if (Font != null)
        {
            text.font = Font;
        }

        text.text = value;
        text.fontSize = size;
        text.color = color;
        text.alignment = alignment;
        text.fontStyle = style;
        text.raycastTarget = false;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.overflowMode = TextOverflowModes.Overflow;
        return text;
    }

    public static TextMeshProUGUI ShadowText(string name, Transform parent, string key, float size, Color color,
        TextAlignmentOptions alignment, FontStyles style, Vector2 shadowOffset)
    {
        TextMeshProUGUI shadow = Text($"{name} Shadow", parent, string.Empty, size, new Color(0f, 0f, 0f, 0.45f), alignment, style);
        Stretch(shadow.rectTransform);
        shadow.rectTransform.anchoredPosition = shadowOffset;
        TextMeshProUGUI text = Text(name, parent, string.Empty, size, color, alignment, style);
        Stretch(text.rectTransform);
        if (key != null)
        {
            Localize(shadow, key);
            Localize(text, key);
        }

        return text;
    }

    public static void Localize(TMP_Text text, string key)
    {
        text.gameObject.AddComponent<LocalizedText>().Key = key;
    }

    public static void Place(RectTransform rect, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size)
    {
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = pivot;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }

    public static void Stretch(RectTransform rect, float left = 0f, float bottom = 0f, float right = 0f, float top = 0f)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = new Vector2(left, bottom);
        rect.offsetMax = new Vector2(-right, -top);
    }

    public static Button XpButton(string name, Transform parent, string key, Vector2 size)
    {
        Image border = Image(name, parent, Rounded(4), ButtonBorder, true);
        border.rectTransform.sizeDelta = size;
        Image face = Image("Face", border.transform, Panel("xpButtonFace", 16, 32, 3, XpCorners.All, Hex("#FFFFFF"), Hex("#F4F3EE"), Hex("#E3E1D5")), Color.white);
        Stretch(face.rectTransform, 1f, 1f, 1f, 1f);
        TextMeshProUGUI label = Text("Label", border.transform, string.Empty, 20f, Color.black, TextAlignmentOptions.Center);
        Stretch(label.rectTransform);
        if (key != null)
        {
            Localize(label, key);
        }

        Button button = border.gameObject.AddComponent<Button>();
        button.targetGraphic = face;
        ColorBlock colors = button.colors;
        colors.highlightedColor = Hex("#FFF4D6");
        colors.pressedColor = Hex("#D8D5C6");
        colors.selectedColor = Color.white;
        colors.fadeDuration = 0.05f;
        button.colors = colors;
        return button;
    }

    private static float Side(Vector2 a, Vector2 b, Vector2 p)
    {
        return (b.x - a.x) * (p.y - a.y) - (b.y - a.y) * (p.x - a.x);
    }

    private static float CornerAlpha(int x, int y, int width, int height, int radius, XpCorners corners)
    {
        if (radius <= 0)
        {
            return 1f;
        }

        Vector2 p = new(x + 0.5f, y + 0.5f);
        bool left = p.x < radius;
        bool right = p.x > width - radius;
        bool bottom = p.y < radius;
        bool top = p.y > height - radius;

        Vector2? center = null;
        if (top && left && (corners & XpCorners.TopLeft) != 0) center = new Vector2(radius, height - radius);
        else if (top && right && (corners & XpCorners.TopRight) != 0) center = new Vector2(width - radius, height - radius);
        else if (bottom && left && (corners & XpCorners.BottomLeft) != 0) center = new Vector2(radius, radius);
        else if (bottom && right && (corners & XpCorners.BottomRight) != 0) center = new Vector2(width - radius, radius);

        return center.HasValue ? Mathf.Clamp01(radius - Vector2.Distance(p, center.Value) + 0.5f) : 1f;
    }

    private static Color Sample(Color[] gradient, float t)
    {
        if (gradient == null || gradient.Length == 0)
        {
            return Color.white;
        }

        if (gradient.Length == 1)
        {
            return gradient[0];
        }

        float scaled = Mathf.Clamp01(t) * (gradient.Length - 1);
        int index = Mathf.Min(Mathf.FloorToInt(scaled), gradient.Length - 2);
        return Color.Lerp(gradient[index], gradient[index + 1], scaled - index);
    }

    private static Texture2D CreateTexture(int width, int height)
    {
        return new Texture2D(width, height, TextureFormat.RGBA32, false)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear,
            hideFlags = HideFlags.HideAndDontSave
        };
    }

    private static Sprite Store(string id, Texture2D texture, Vector4 border)
    {
        Sprite sprite = Sprite.Create(texture, new UnityEngine.Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, border);
        sprite.hideFlags = HideFlags.HideAndDontSave;
        sprite.name = id;
        cache[id] = sprite;
        return sprite;
    }
}
