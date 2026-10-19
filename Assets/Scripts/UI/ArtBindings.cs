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
}
