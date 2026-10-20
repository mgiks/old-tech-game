using UnityEngine;
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

    private Camera cam;
    private DesktopIcon charged;
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

        if (charged != null && (charged.IsFlashing || charged.FlashReadyToFly))
        {
            UpdateBusy(cursor, under == charged);
            downloadCharge = charged.DownloadProgress;
            if (charged.FlashReadyToFly && virus != null)
            {
                Launch(charged);
            }

            return;
        }

        UpdateBusy(cursor, under != null && !under.IsConsumed && under.IsFree);

        if (under == null || !under.IsFree)
        {
            PauseHover();
            return;
        }

        if (under != charged)
        {
            if (charged != null)
            {
                charged.ClearHoverPose();
            }

            charged = under;
        }

        charged.AddDownloadTime(Time.deltaTime, feel.downloadSeconds, feel.downloadHighlight);
        downloadCharge = charged.DownloadProgress;
        chargedFileName = charged.name;
        charged.SetHoverPose(downloadCharge, Time.time, feel);

        if (downloadCharge < 1f || virus == null)
        {
            return;
        }

        charged.ClearHoverPose();
        charged.BeginDownloadFlash(feel.flashSeconds);
    }

    private void Launch(DesktopIcon icon)
    {
        charged = null;
        downloadCharge = 0f;
        chargedFileName = string.Empty;
        icon.BeginDelivery(virus.GetAbsorbPoint(), feel, () => virus.Absorb(icon));
    }

    private void PauseHover()
    {
        if (charged != null)
        {
            charged.ClearHoverPose();
            charged = null;
        }

        downloadCharge = 0f;
        chargedFileName = string.Empty;
    }

    private void ReleaseCharge()
    {
        PauseHover();
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
