using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class DesktopIcon : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    private static readonly List<DesktopIcon> active = new();

    public static IReadOnlyList<DesktopIcon> Active => active;

    public bool IsDragged { get; private set; }
    public bool IsConsumed { get; private set; }
    public bool IsGolden { get; private set; }
    public bool IsCarried { get; set; }
    public bool IsDelivering { get; private set; }
    public bool IsFlashing { get; private set; }
    public bool FlashReadyToFly { get; private set; }
    public bool IsFree => !IsDragged && !IsCarried && !IsConsumed && !IsDelivering && !IsFlashing && !FlashReadyToFly;

    [SerializeField] private float downloadProgress;
    public float DownloadProgress => downloadProgress;

    [SerializeField] private float popInDuration = 0.25f;
    [SerializeField] private Color goldenColor = new(1f, 0.82f, 0.25f, 1f);
    [SerializeField] private Color chainedColor = new(0.85f, 0.35f, 1f, 1f);
    [SerializeField] private Sprite[] iconVariants;

    private Vector3 offset;
    private Camera cam;
    private SpriteRenderer spriteRenderer;
    private Vector3 baseScale;
    private float age;

    private bool posing;
    private bool releasing;
    private Vector3 poseRest;
    private float poseCharge;
    private float poseTime;
    private HoverDownloadFeel poseFeel;

    private Vector3 deliveryStart;
    private Vector2 deliveryTarget;
    private float deliveryTime;
    private HoverDownloadFeel deliveryFeel;
    private Action onDelivered;

    private static Material sharedFillMaterial;
    private MaterialPropertyBlock fillBlock;
    private float fillBottom;
    private float fillHeight = 1f;
    private float fillPixelsPerUnit = 100f;
    private float flashLeft;
    private float flashDuration = 0.18f;
    private Color downloadHighlight = new(1f, 0.7f, 0.22f, 0.85f);
    private static readonly int FillId = Shader.PropertyToID("_Fill");
    private static readonly int FlashId = Shader.PropertyToID("_Flash");
    private static readonly int FillBottomId = Shader.PropertyToID("_FillBottom");
    private static readonly int FillHeightId = Shader.PropertyToID("_FillHeight");
    private static readonly int PixelsPerUnitId = Shader.PropertyToID("_PixelsPerUnit");
    private static readonly int HighlightId = Shader.PropertyToID("_Highlight");

    void Awake()
    {
        cam = Camera.main;
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (iconVariants != null && iconVariants.Length > 0)
        {
            spriteRenderer.sprite = iconVariants[UnityEngine.Random.Range(0, iconVariants.Length)];
        }

        baseScale = transform.localScale;
        transform.localScale = Vector3.zero;
        ApplyFillMaterial();
        CacheFillBounds();
        ApplyDownloadVisual(0f);
    }

    void Update()
    {
        age += Time.deltaTime;

        if (age < popInDuration)
        {
            float t = age / popInDuration;
            float overshoot = 1f + Mathf.Sin(t * Mathf.PI) * 0.35f;
            transform.localScale = baseScale * (t * overshoot);
        }
        else if (transform.localScale != baseScale)
        {
            transform.localScale = baseScale;
        }

        if (IsGolden)
        {
            float shimmer = 0.5f + 0.5f * Mathf.Sin(age * 8f);
            spriteRenderer.color = Color.Lerp(goldenColor, Color.white, shimmer * 0.45f);
        }
    }

    void LateUpdate()
    {
        if (IsFlashing)
        {
            TickFlash();
        }

        if (IsDelivering)
        {
            TickDelivery();
            return;
        }

        if (posing && poseFeel != null)
        {
            HoverDownloadFeel.Pose pose = poseFeel.Evaluate(poseCharge, poseTime);
            transform.position = poseRest + new Vector3(pose.slideUnits, 0f, 0f);
            transform.rotation = Quaternion.Euler(0f, 0f, pose.angleDegrees);
            return;
        }

        if (!releasing)
        {
            return;
        }

        float k = 1f - Mathf.Exp(-18f * Time.deltaTime);
        transform.position = Vector3.Lerp(transform.position, poseRest, k);
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.identity, k);
        if ((transform.position - poseRest).sqrMagnitude < 0.00001f && Quaternion.Angle(transform.rotation, Quaternion.identity) < 0.4f)
        {
            transform.position = poseRest;
            transform.rotation = Quaternion.identity;
            releasing = false;
        }
    }

    public void AddDownloadTime(float deltaSeconds, float secondsToComplete, Color highlight)
    {
        if (IsDelivering || IsConsumed || IsFlashing || FlashReadyToFly)
        {
            return;
        }

        downloadHighlight = highlight;
        float duration = Mathf.Max(0.05f, secondsToComplete);
        downloadProgress = Mathf.Clamp01(downloadProgress + deltaSeconds / duration);
        ApplyDownloadVisual(0f);
    }

    public void BeginDownloadFlash(float seconds)
    {
        if (IsFlashing || FlashReadyToFly || IsDelivering)
        {
            return;
        }

        downloadProgress = 1f;
        IsFlashing = true;
        flashDuration = Mathf.Max(0.05f, seconds);
        flashLeft = flashDuration;
        ApplyDownloadVisual(0f);
    }

    public void SetHoverPose(float charge, float time, HoverDownloadFeel feel)
    {
        if (IsDelivering || IsFlashing || FlashReadyToFly || feel == null)
        {
            return;
        }

        if (!posing)
        {
            poseRest = transform.position;
        }

        posing = true;
        releasing = false;
        poseCharge = charge;
        poseTime = time;
        poseFeel = feel;
    }

    public void ClearHoverPose()
    {
        if (IsDragged || IsDelivering)
        {
            posing = false;
            releasing = false;
            return;
        }

        if (!posing)
        {
            return;
        }

        posing = false;
        releasing = true;
    }

    public void BeginDelivery(Vector2 target, HoverDownloadFeel feel, Action delivered)
    {
        posing = false;
        releasing = false;
        IsFlashing = false;
        FlashReadyToFly = false;
        IsDragged = false;
        IsCarried = false;
        IsDelivering = true;
        deliveryStart = transform.position;
        deliveryTarget = target;
        transform.rotation = Quaternion.identity;
        deliveryTime = 0f;
        deliveryFeel = feel;
        onDelivered = delivered;
    }

    public void MakeGolden()
    {
        IsGolden = true;
        baseScale *= 1.15f;
    }

    public void MarkChained()
    {
        if (!IsGolden)
        {
            spriteRenderer.color = chainedColor;
        }
    }

    void OnEnable()
    {
        active.Add(this);
    }

    void OnDisable()
    {
        active.Remove(this);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        offset = transform.position - GetPointerWorldPos();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (IsDelivering || IsConsumed || IsFlashing || FlashReadyToFly)
        {
            return;
        }

        posing = false;
        releasing = false;
        IsDragged = true;
        ArtBindings art = ArtBindings.Load();
        if (art != null && art.cursorGrab != null)
        {
            Cursor.SetCursor(art.cursorGrab, art.cursorGrabHotspot, CursorMode.Auto);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        IsDragged = false;
        ArtBindings art = ArtBindings.Load();
        if (art != null && art.cursorDefault != null)
        {
            Cursor.SetCursor(art.cursorDefault, art.cursorDefaultHotspot, CursorMode.Auto);
        }

        if (!GamePause.IsPaused)
        {
            TryFeedVirus();
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (GamePause.IsPaused || IsDelivering || !IsDragged)
        {
            return;
        }

        transform.position = GetPointerWorldPos() + offset;
    }

    private void TickDelivery()
    {
        float duration = deliveryFeel != null ? Mathf.Max(0.05f, deliveryFeel.flySeconds) : 0.42f;
        float arc = deliveryFeel != null ? deliveryFeel.flyArc : 0.18f;

        deliveryTime += Time.deltaTime;
        float t = Mathf.Clamp01(deliveryTime / duration);
        float k = t * t * (3f - 2f * t);
        Vector2 from = deliveryStart;
        Vector2 to = deliveryTarget;
        Vector2 path = to - from;
        Vector2 side = path.sqrMagnitude > 0.0001f ? new Vector2(-path.y, path.x).normalized : Vector2.up;
        Vector2 next = Vector2.Lerp(from, to, k) + side * (Mathf.Sin(t * Mathf.PI) * arc);
        transform.position = new Vector3(next.x, next.y, deliveryStart.z);
        transform.rotation = Quaternion.identity;

        if (t < 1f)
        {
            return;
        }

        IsDelivering = false;
        Action delivered = onDelivered;
        onDelivered = null;
        delivered?.Invoke();
    }

    public bool TryConsume()
    {
        if (IsConsumed)
        {
            return false;
        }

        IsConsumed = true;
        RunStats.AddFile();
        return true;
    }

    private void TryFeedVirus()
    {
        Physics2D.SyncTransforms();

        Virus virus = FindVirusAt(GetPointerWorldPos());
        if (virus == null)
        {
            virus = FindVirusAt(transform.position);
        }

        if (virus != null)
        {
            virus.Absorb(this);
        }
    }

    private static Virus FindVirusAt(Vector2 point)
    {
        foreach (Collider2D hit in Physics2D.OverlapPointAll(point))
        {
            if (hit.TryGetComponent(out Virus virus))
            {
                return virus;
            }
        }

        return null;
    }

    private void TickFlash()
    {
        flashLeft -= Time.deltaTime;
        float k = 1f - Mathf.Clamp01(flashLeft / flashDuration);
        ApplyDownloadVisual(Mathf.Sin(k * Mathf.PI));
        if (flashLeft > 0f)
        {
            return;
        }

        IsFlashing = false;
        FlashReadyToFly = true;
        ApplyDownloadVisual(0f);
    }

    private void ApplyFillMaterial()
    {
        if (spriteRenderer == null)
        {
            return;
        }

        if (sharedFillMaterial == null)
        {
            Shader shader = Shader.Find("OldTech/FileDownloadFill");
            if (shader == null)
            {
                return;
            }

            sharedFillMaterial = new Material(shader)
            {
                name = "File Download Fill",
                hideFlags = HideFlags.HideAndDontSave
            };
        }

        spriteRenderer.sharedMaterial = sharedFillMaterial;
        fillBlock ??= new MaterialPropertyBlock();
    }

    private void CacheFillBounds()
    {
        Sprite sprite = spriteRenderer != null ? spriteRenderer.sprite : null;
        if (sprite == null)
        {
            return;
        }

        float pixelsPerUnit = Mathf.Max(1f, sprite.pixelsPerUnit);
        fillPixelsPerUnit = pixelsPerUnit;
        fillBottom = -sprite.pivot.y / pixelsPerUnit;
        fillHeight = Mathf.Max(0.0001f, sprite.rect.height / pixelsPerUnit);
    }

    private void ApplyDownloadVisual(float flash)
    {
        if (spriteRenderer == null || sharedFillMaterial == null)
        {
            return;
        }

        fillBlock ??= new MaterialPropertyBlock();
        spriteRenderer.GetPropertyBlock(fillBlock);
        if (spriteRenderer.sprite != null)
        {
            fillBlock.SetTexture("_MainTex", spriteRenderer.sprite.texture);
        }

        fillBlock.SetFloat(FillId, downloadProgress);
        fillBlock.SetFloat(FlashId, flash);
        fillBlock.SetFloat(FillBottomId, fillBottom);
        fillBlock.SetFloat(FillHeightId, fillHeight);
        fillBlock.SetFloat(PixelsPerUnitId, fillPixelsPerUnit);
        fillBlock.SetColor(HighlightId, downloadHighlight);
        spriteRenderer.SetPropertyBlock(fillBlock);
    }

    private Vector3 GetPointerWorldPos()
    {
        return MonitorScreen.ToWorld(cam);
    }
}
