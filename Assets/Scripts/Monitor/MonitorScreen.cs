using UnityEngine;
using UnityEngine.InputSystem;

public static class MonitorScreen
{
    public static bool Active { get; private set; }
    public static bool OnGlass { get; private set; }
    public static Vector2 Pointer { get; private set; }
    public static Vector2 Viewport { get; private set; }
    public static float DragScale { get; private set; } = 1f;
    public static Vector2Int PixelSize { get; private set; } = new(1920, 1080);

    public static Vector2 GamePointer
    {
        get
        {
            if (Active)
            {
                return Pointer;
            }

            return Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
        }
    }

    public static void SetActive(RenderTexture texture, float dragScale, bool onGlass, Vector2 pointer)
    {
        Active = texture != null;
        PixelSize = texture != null ? new Vector2Int(texture.width, texture.height) : new Vector2Int(Screen.width, Screen.height);
        DragScale = dragScale;
        OnGlass = onGlass;
        Pointer = pointer;
        Viewport = PixelSize.x > 0 && PixelSize.y > 0
            ? new Vector2(pointer.x / PixelSize.x, pointer.y / PixelSize.y)
            : Vector2.zero;
    }

    public static Vector3 ToWorld(Camera cam)
    {
        if (cam == null)
        {
            return Vector3.zero;
        }

        Vector2 viewport = Viewport;
        if (!Active)
        {
            Vector2 mouse = Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
            float width = Mathf.Max(1, Screen.width);
            float height = Mathf.Max(1, Screen.height);
            viewport = new Vector2(mouse.x / width, mouse.y / height);
        }

        float forward = cam.transform.forward.z;
        float distance = Mathf.Abs(forward) > 0.001f ? -cam.transform.position.z / forward : 0f;
        Vector3 world = cam.ViewportToWorldPoint(new Vector3(viewport.x, viewport.y, distance));
        world.z = 0f;
        return world;
    }

    public static void ResetSession()
    {
        Active = false;
        OnGlass = false;
        Pointer = Vector2.zero;
        Viewport = Vector2.zero;
        DragScale = 1f;
    }
}
