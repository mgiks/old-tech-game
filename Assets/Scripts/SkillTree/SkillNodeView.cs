using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SkillNodeView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image glow;
    [SerializeField] private Image frame;
    [SerializeField] private Image body;
    [SerializeField] private Image skillSlot;
    [SerializeField] private Image skillIcon;
    [SerializeField] private Sprite slotNormal;
    [SerializeField] private Sprite slotHover;
    [SerializeField] private Sprite slotSelected;
    [SerializeField] private Sprite slotLocked;
    [SerializeField] private TextMeshProUGUI nameLabel;
    [SerializeField] private TextMeshProUGUI levelLabel;
    [SerializeField] private Button button;

    private static readonly Color Paper = new(0.95f, 0.93f, 0.88f, 1f);
    private static readonly Color PaperOwned = new(0.86f, 0.91f, 0.84f, 1f);
    private static readonly Color PaperLocked = new(0.78f, 0.76f, 0.72f, 1f);
    private static readonly Color FrameOwned = new(0.38f, 0.55f, 0.40f, 1f);
    private static readonly Color FrameOpen = new(0.28f, 0.32f, 0.28f, 1f);
    private static readonly Color FrameLocked = new(0.42f, 0.40f, 0.37f, 1f);
    private static readonly Color FrameSelected = new(0.12f, 0.13f, 0.12f, 1f);
    private static readonly Color Ink = new(0.16f, 0.16f, 0.15f, 1f);
    private static readonly Color InkLocked = new(0.32f, 0.31f, 0.28f, 1f);

    private SkillTreeDefinition skillTree;
    private SkillTreeDefinition.Node node;
    private SkillTooltip tooltip;

    public SkillTreeDefinition.Node Node => node;

    public void Init(SkillTreeDefinition tree, SkillTreeDefinition.Node definition, SkillTooltip skillTooltip, SkillTreeFx skillFx)
    {
        skillTree = tree;
        node = definition;
        tooltip = skillTooltip;
        _ = skillFx;
        _ = slotNormal;
        _ = slotHover;
        _ = slotSelected;
        _ = slotLocked;
        button.onClick.AddListener(OnClick);
        ApplySquareLayout();
        if (skillIcon != null)
        {
            skillIcon.sprite = node.icon;
            skillIcon.enabled = node.icon != null;
            skillIcon.preserveAspect = true;
            skillIcon.raycastTarget = false;
            skillIcon.material = null;
        }

        Refresh();
    }

    public void Refresh()
    {
        bool revealed = skillTree.IsRevealed(node);
        if (!revealed)
        {
            if (SkillTreeFocus.HoveredId == node.id)
            {
                SkillTreeFocus.ClearHover(node.id);
            }

            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);
        ApplyVisual();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SkillTreeFocus.Hover(node.id);
        ApplyVisual();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SkillTreeFocus.ClearHover(node.id);
        ApplyVisual();
    }

    void OnEnable()
    {
        SkillTreeFocus.Changed -= ApplyVisual;
        SkillTreeFocus.Changed += ApplyVisual;
    }

    void OnDisable()
    {
        SkillTreeFocus.Changed -= ApplyVisual;
        if (node != null && SkillTreeFocus.HoveredId == node.id)
        {
            SkillTreeFocus.ClearHover(node.id);
        }
    }

    private void OnClick()
    {
        GameAudio.PlayMouseClick();
        SkillTreeFocus.Select(node.id);
        if (tooltip != null)
        {
            tooltip.Hide();
        }
    }

    private void ApplySquareLayout()
    {
        float size = node.major ? 112f : 80f;
        RectTransform nodeRect = (RectTransform)transform;
        nodeRect.sizeDelta = new Vector2(size, size);
        nodeRect.localScale = Vector3.one;

        if (glow != null)
        {
            glow.gameObject.SetActive(false);
            glow.raycastTarget = false;
        }

        if (nameLabel != null)
        {
            nameLabel.gameObject.SetActive(false);
        }

        if (skillSlot != null)
        {
            skillSlot.enabled = false;
            skillSlot.raycastTarget = false;
        }

        PreparePlate(frame, 0f, true);
        PreparePlate(body, 4f, false);
        if (skillIcon != null)
        {
            Stretch(skillIcon.rectTransform, node.major ? 18f : 14f, node.major ? 18f : 14f, 16f, 28f);
        }

        if (levelLabel != null)
        {
            RectTransform label = levelLabel.rectTransform;
            label.anchorMin = new Vector2(0f, 0f);
            label.anchorMax = new Vector2(1f, 0f);
            label.pivot = new Vector2(0.5f, 0f);
            label.anchoredPosition = new Vector2(0f, 4f);
            label.sizeDelta = new Vector2(-6f, 18f);
            levelLabel.fontSize = node.major ? 15f : 13f;
            levelLabel.alignment = TextAlignmentOptions.Center;
            levelLabel.textWrappingMode = TextWrappingModes.NoWrap;
            levelLabel.overflowMode = TextOverflowModes.Overflow;
            levelLabel.raycastTarget = false;
        }
    }

    private void ApplyVisual()
    {
        if (node == null || skillTree == null || !isActiveAndEnabled)
        {
            return;
        }

        bool selected = SkillTreeFocus.DisplayedId == node.id;
        SkillVisibility visibility = skillTree.GetVisibility(node);
        bool locked = visibility is SkillVisibility.Locked or SkillVisibility.Silhouette or SkillVisibility.Hidden;
        bool owned = skillTree.GetLevel(node) > 0 || node.anchor;
        Color frameColor = locked ? FrameLocked : owned ? FrameOwned : FrameOpen;
        Color fill = locked ? PaperLocked : owned ? PaperOwned : Paper;
        if (selected)
        {
            frameColor = FrameSelected;
        }

        if (frame != null)
        {
            frame.color = frameColor;
            frame.sprite = null;
        }

        if (body != null)
        {
            body.enabled = true;
            body.sprite = null;
            body.color = fill;
            float inset = selected ? 6f : 3f;
            body.rectTransform.offsetMin = new Vector2(inset, inset);
            body.rectTransform.offsetMax = new Vector2(-inset, -inset);
        }

        if (skillIcon != null)
        {
            skillIcon.color = locked ? new Color(0.45f, 0.44f, 0.42f, 1f) : Color.white;
        }

        if (levelLabel != null)
        {
            levelLabel.text = Mark(visibility, owned);
            levelLabel.color = locked ? InkLocked : Ink;
        }
    }

    private string Mark(SkillVisibility visibility, bool owned)
    {
        if (node.anchor)
        {
            return Loc.T("tree.core_state");
        }

        if (skillTree.IsMaxed(node))
        {
            return Loc.T("tree.mark_max");
        }

        if (visibility is SkillVisibility.Locked or SkillVisibility.Silhouette)
        {
            return Loc.T("tree.mark_lock");
        }

        if (visibility == SkillVisibility.Available)
        {
            return "MB";
        }

        int level = skillTree.GetLevel(node);
        if (owned || level > 0 || visibility == SkillVisibility.Purchasable)
        {
            return level + "/" + node.maxLevel;
        }

        return level + "/" + node.maxLevel;
    }

    private static void PreparePlate(Image image, float inset, bool raycast)
    {
        if (image == null)
        {
            return;
        }

        image.sprite = null;
        image.material = null;
        image.type = Image.Type.Simple;
        image.raycastTarget = raycast;
        Stretch(image.rectTransform, inset, inset, inset, inset);
    }

    private static void Stretch(RectTransform target, float left, float bottom, float right, float top)
    {
        target.anchorMin = Vector2.zero;
        target.anchorMax = Vector2.one;
        target.offsetMin = new Vector2(left, bottom);
        target.offsetMax = new Vector2(-right, -top);
        target.anchoredPosition = Vector2.zero;
        target.localScale = Vector3.one;
    }
}
