using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
#if UNITY_EDITOR
using UnityEditor;
#endif

[DefaultExecutionOrder(50)]
public class FileHoverDownload : MonoBehaviour
{
    private const string BusyFramesFolder = "Assets/Art/Cursors/busy_cusror";

    [SerializeField] private Virus virus;
    [SerializeField] private HoverDownloadFeel feel;

    [SerializeField] private float downloadCharge;
    [SerializeField] private string chargedFileName;

    public float DownloadCharge => downloadCharge;
    public HoverDownloadFeel Feel => feel;

    private readonly List<DesktopIcon> secondary = new();
    private readonly List<Linger> lingering = new();
    private DesktopIcon primary;
    private Camera cam;
    private SpriteRenderer busy;
    private SpriteRenderer busyShadow;
    private Sprite[] busyFrames;
    private float busyFrameTime;

    void Awake()
    {
        cam = Camera.main;
        if (virus == null)
        {
            virus = FindFirstObjectByType<Virus>();
        }

        if (feel == null)
        {
            feel = ScriptableObject.CreateInstance<HoverDownloadFeel>();
        }

        BuildBusyCursor();
        busyFrames = ResolveBusyFrames(feel);
    }

    void OnDisable()
    {
        ReleaseCharge();
        if (busy != null)
        {
            busy.enabled = false;
        }

        if (busyShadow != null)
        {
            busyShadow.enabled = false;
        }
    }

    void LateUpdate()
    {
        if (cam == null)
        {
            cam = Camera.main;
        }

        if (feel == null || cam == null || GamePause.IsPaused)
        {
            HideBusy();
            return;
        }

        if (MonitorScreen.Active && !MonitorScreen.OnGlass)
        {
            HideBusy();
            ReleaseCharge();
            return;
        }

        Vector2 cursor = MonitorScreen.ToWorld(cam);
        DesktopIcon under = FindUnderCursor(cursor);
        if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame && under != null)
        {
            under.WantsInfect = true;
        }

        bool downloading = under != null && under.IsFree;
        UpdateBusy(cursor, downloading || primary != null);
        if (RunDirector.Instance != null && RunDirector.Instance.BurstMultiplier > 1.01f && busy != null && busy.enabled)
        {
            busy.color = new Color(1f, 0.78f, 0.35f, 1f);
        }
        else if (busy != null)
        {
            busy.color = Color.white;
        }

        float dt = Time.deltaTime;
        if (under != primary)
        {
            Hold(primary);
            if (primary != null)
            {
                primary.ClearHoverPose();
            }

            primary = downloading ? under : null;
        }

        if (primary != null && primary.IsFree)
        {
            float rate = GameplayQuery.ManualPrimaryRate();
            primary.AddWork(rate * dt, Highlight(), CompletionSource.Manual);
            downloadCharge = primary.DownloadProgress;
            chargedFileName = primary.name;
            primary.SetHoverPose(downloadCharge, Time.time, feel);
            Remember(primary, rate);
        }
        else
        {
            Hold(primary);
            if (primary != null)
            {
                primary.ClearHoverPose();
            }

            primary = null;
            downloadCharge = 0f;
            chargedFileName = string.Empty;
        }

        int extra = Mathf.Max(0, GameplayQuery.DownloadSlots() - 1);
        for (int i = secondary.Count - 1; i >= 0; i--)
        {
            DesktopIcon icon = secondary[i];
            if (icon == null || !icon.IsFree || icon == primary)
            {
                Hold(icon);
                secondary.RemoveAt(i);
            }
        }

        if (primary == null)
        {
            for (int i = 0; i < secondary.Count; i++)
            {
                Hold(secondary[i]);
                secondary[i].ClearHoverPose();
            }

            secondary.Clear();
        }
        else
        {
            while (secondary.Count < extra)
            {
                DesktopIcon next = FindNearest(cursor, ProgressionConfig.MultithreadRadius);
                if (next == null)
                {
                    break;
                }

                secondary.Add(next);
            }

            float sideRate = GameplayQuery.ManualPrimaryRate() * ProgressionConfig.SecondarySpeedFactor;
            for (int i = 0; i < secondary.Count; i++)
            {
                secondary[i].AddWork(sideRate * dt, Highlight(), CompletionSource.Manual);
                Remember(secondary[i], sideRate);
            }
        }

        for (int i = lingering.Count - 1; i >= 0; i--)
        {
            Linger linger = lingering[i];
            if (linger.Icon == null || !linger.Icon.IsFree || linger.Left <= 0f)
            {
                lingering.RemoveAt(i);
                continue;
            }

            linger.Left -= dt;
            linger.Icon.AddWork(linger.Rate * dt, Highlight(), CompletionSource.Manual);
        }
    }

    private Color Highlight()
    {
        Color color = feel.downloadHighlight;
        if (RunDirector.Instance != null && RunDirector.Instance.BurstMultiplier > 1.01f)
        {
            color = Color.Lerp(color, new Color(1f, 0.85f, 0.3f, 1f), 0.65f);
        }

        return color;
    }

    private void Hold(DesktopIcon icon)
    {
        if (icon == null || !icon.IsFree)
        {
            return;
        }

        float seconds = GameplayQuery.BufferSeconds();
        if (seconds <= 0f)
        {
            return;
        }

        for (int i = 0; i < lingering.Count; i++)
        {
            if (lingering[i].Icon == icon)
            {
                lingering[i].Left = seconds;
                return;
            }
        }

        lingering.Add(new Linger { Icon = icon, Left = seconds, Rate = RateOf(icon) });
    }

    private void Remember(DesktopIcon icon, float rate)
    {
        for (int i = 0; i < lingering.Count; i++)
        {
            if (lingering[i].Icon == icon)
            {
                lingering[i].Rate = rate;
                lingering.RemoveAt(i);
                break;
            }
        }

        for (int i = 0; i < lingering.Count; i++)
        {
            if (lingering[i].Icon == icon)
            {
                lingering[i].Rate = rate;
            }
        }
    }

    private float RateOf(DesktopIcon icon)
    {
        return icon == primary
            ? GameplayQuery.ManualPrimaryRate()
            : GameplayQuery.ManualPrimaryRate() * ProgressionConfig.SecondarySpeedFactor;
    }

    private DesktopIcon FindNearest(Vector2 cursor, float radius)
    {
        float best = radius * radius;
        DesktopIcon found = null;
        for (int i = 0; i < DesktopIcon.Active.Count; i++)
        {
            DesktopIcon icon = DesktopIcon.Active[i];
            if (icon == null || !icon.IsFree || icon == primary || secondary.Contains(icon))
            {
                continue;
            }

            float distance = ((Vector2)icon.transform.position - cursor).sqrMagnitude;
            if (distance < best)
            {
                best = distance;
                found = icon;
            }
        }

        return found;
    }

    private void PauseHover()
    {
        Hold(primary);
        if (primary != null)
        {
            primary.ClearHoverPose();
            primary = null;
        }

        for (int i = 0; i < secondary.Count; i++)
        {
            Hold(secondary[i]);
            if (secondary[i] != null)
            {
                secondary[i].ClearHoverPose();
            }
        }

        secondary.Clear();
        downloadCharge = 0f;
        chargedFileName = string.Empty;
    }

    private void ReleaseCharge()
    {
        PauseHover();
        lingering.Clear();
    }

    private sealed class Linger
    {
        public DesktopIcon Icon;
        public float Left;
        public float Rate;
    }

    private static DesktopIcon FindUnderCursor(Vector2 cursor)
    {
        Collider2D[] hits = Physics2D.OverlapPointAll(cursor);
        DesktopIcon best = null;
        float bestDistance = float.MaxValue;
        foreach (Collider2D hit in hits)
        {
            if (hit == null || !hit.TryGetComponent(out DesktopIcon icon) || icon.IsConsumed)
            {
                continue;
            }

            float distance = ((Vector2)icon.transform.position - cursor).sqrMagnitude;
            if (distance < bestDistance)
            {
                best = icon;
                bestDistance = distance;
            }
        }

        return best;
    }

    private void UpdateBusy(Vector2 cursor, bool visible)
    {
        if (busy == null)
        {
            return;
        }

        busy.enabled = visible && busyFrames != null && busyFrames.Length > 0;
        if (!busy.enabled)
        {
            if (busyShadow != null)
            {
                busyShadow.enabled = false;
            }

            return;
        }

        busyFrameTime += Time.deltaTime;
        float frameDuration = Mathf.Max(0.001f, feel.busyFrameSeconds);
        int frameIndex = Mathf.FloorToInt(busyFrameTime / frameDuration) % busyFrames.Length;
        Sprite frame = busyFrames[frameIndex];
        if (frame == null)
        {
            busy.enabled = false;
            if (busyShadow != null)
            {
                busyShadow.enabled = false;
            }

            return;
        }

        busy.sprite = frame;
        if (busyShadow != null)
        {
            busyShadow.sprite = frame;
            busyShadow.enabled = true;
            float height = Mathf.Max(1f, frame.rect.height);
            busyShadow.transform.localScale = Vector3.one * (1f + 2f / height);
        }

        float worldPerPixel = cam.orthographicSize * 2f / Mathf.Max(1, cam.pixelHeight);
        Vector2 offset = feel.busyOffsetPixels * worldPerPixel;
        busy.transform.position = new Vector3(cursor.x + offset.x, cursor.y + offset.y, 0f);
        busy.transform.rotation = Quaternion.identity;

        float spriteSize = Mathf.Max(frame.bounds.size.x, 0.001f);
        busy.transform.localScale = Vector3.one * (feel.busyDiameterPixels * worldPerPixel / spriteSize);
    }

    private void HideBusy()
    {
        if (busy != null)
        {
            busy.enabled = false;
        }

        if (busyShadow != null)
        {
            busyShadow.enabled = false;
        }

        busyFrameTime = 0f;
    }

    private void BuildBusyCursor()
    {
        GameObject go = new("Busy Cursor");
        go.transform.SetParent(transform, false);
        busy = go.AddComponent<SpriteRenderer>();
        busy.sortingOrder = 80;
        busy.enabled = false;

        GameObject shadow = new("Busy Shadow");
        shadow.transform.SetParent(go.transform, false);
        busyShadow = shadow.AddComponent<SpriteRenderer>();
        busyShadow.color = new Color(0.02f, 0.03f, 0.06f, 0.85f);
        busyShadow.sortingOrder = 79;
        busyShadow.enabled = false;
    }

    private static Sprite[] ResolveBusyFrames(HoverDownloadFeel feelAsset)
    {
        if (feelAsset != null && feelAsset.busyFrames != null && feelAsset.busyFrames.Length > 0)
        {
            return feelAsset.busyFrames;
        }

#if UNITY_EDITOR
        Sprite[] loaded = new Sprite[18];
        int count = 0;
        for (int i = 1; i <= 18; i++)
        {
            string path = $"{BusyFramesFolder}/frame_{i:000}.png";
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (Object asset in assets)
            {
                if (asset is Sprite sprite)
                {
                    loaded[count++] = sprite;
                    break;
                }
            }
        }

        if (count == 18)
        {
            return loaded;
        }
#endif

        return null;
    }
}
