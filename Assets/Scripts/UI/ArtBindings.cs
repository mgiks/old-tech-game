using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ArtBindings : MonoBehaviour
{
    public Sprite taskbar;
    public Sprite startButton;
    public Sprite windowFrame;
    public Sprite titlebarActive;
    public Sprite closeNormal;
    public Sprite closeHover;
    public Sprite closePressed;
    public Sprite closeDisabled;
    public Sprite panelRaised;
    public Sprite panelSunken;
    public Sprite settingsBody;
    public Sprite volumeTrack;
    public Sprite volumeFill;
    public Sprite volumeHandle;
    public Sprite settingsIcon;
    public Sprite virus;
    public Sprite buttonNormal;
    public Sprite buttonHover;
    public Sprite buttonPressed;
    public Sprite buttonDisabled;
    public Texture2D cursorDefault;
    public Vector2 cursorDefaultHotspot = new(1f, 1f);
    public Texture2D cursorGrab;
    public Vector2 cursorGrabHotspot = new(10f, 8f);

    private static ArtBindings cached;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void ApplyCursor()
    {
        ArtBindings art = Load();
        if (art != null && art.cursorDefault != null)
        {
            Cursor.SetCursor(art.cursorDefault, art.cursorDefaultHotspot, CursorMode.Auto);
        }
    }

    public static ArtBindings Load()
    {
        if (cached != null)
        {
            return cached;
        }

        cached = Resources.Load<ArtBindings>("ArtBindings");
        return cached;
    }

    public static void ApplySlice(Image image, Sprite sprite)
    {
        if (image == null || sprite == null)
        {
            return;
        }

        image.sprite = sprite;
        image.type = Image.Type.Sliced;
        image.color = Color.white;
        if (sprite.pixelsPerUnit > 0f)
        {
            image.pixelsPerUnitMultiplier = 100f / sprite.pixelsPerUnit;
        }
    }

    public static void StyleXpButton(Button button)
    {
        if (button == null)
        {
            return;
        }

        ArtBindings art = Load();
        Image image = button.targetGraphic as Image;
        if (image == null)
        {
            image = button.GetComponent<Image>();
        }

        if (image != null && art != null && art.buttonNormal != null)
        {
            image.material = null;
            ApplySlice(image, art.buttonNormal);
            button.targetGraphic = image;
            button.transition = Selectable.Transition.SpriteSwap;
            SpriteState state = button.spriteState;
            state.highlightedSprite = art.buttonHover;
            state.pressedSprite = art.buttonPressed;
            state.selectedSprite = art.buttonHover;
            state.disabledSprite = art.buttonDisabled;
            button.spriteState = state;
        }

        TextMeshProUGUI label = button.GetComponentInChildren<TextMeshProUGUI>();
        if (label != null)
        {
            label.color = Color.white;
            label.fontStyle = FontStyles.Bold;
            label.outlineWidth = 0.18f;
            label.outlineColor = new Color32(0, 24, 72, 220);
        }
    }
}
