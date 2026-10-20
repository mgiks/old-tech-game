using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class SkillTooltip : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Vector2 cursorOffset = new(24f, -24f);

    private RectTransform rect;
    private Canvas canvas;

    void Awake()
    {
        rect = (RectTransform)transform;
        canvas = GetComponentInParent<Canvas>();
        Hide();
    }

    void Update()
    {
        FollowCursor();
    }

    public void Show(SkillTreeDefinition skillTree, SkillTreeDefinition.Node node)
    {
        SkillVisibility visibility = skillTree.GetVisibility(node);
        int level = skillTree.GetLevel(node);
        string status = string.Empty;
        if (!node.anchor && visibility != SkillVisibility.Silhouette)
        {
            string current = skillTree.CurrentEffectText(node);
            string next = skillTree.NextEffectText(node);
            if (!string.IsNullOrEmpty(current))
            {
                status = Loc.F("tooltip.now", current);
            }

            if (!string.IsNullOrEmpty(next))
            {
                status = string.IsNullOrEmpty(status) ? Loc.F("tooltip.next", next) : status + "\n" + Loc.F("tooltip.next", next);
            }
        }

        if (node.anchor)
        {
            status = string.Empty;
        }
        else if (skillTree.IsMaxed(node))
        {
            status += "\n" + Loc.F("tooltip.level_max", level, node.maxLevel);
        }
        else if (visibility is SkillVisibility.Purchasable or SkillVisibility.Available or SkillVisibility.Partial)
        {
            long cost = skillTree.GetCost(node);
            string costColor = GameProgress.MB >= cost ? "#7CFF9A" : "#FF7C7C";
            status += "\n" + Loc.F("tooltip.level_cost", level, node.maxLevel, costColor, NumberFormat.Mb(cost));
        }

        if (visibility is SkillVisibility.Locked or SkillVisibility.Silhouette or SkillVisibility.Hidden)
        {
            string requirement = skillTree.RequirementText(node);
            if (!string.IsNullOrEmpty(requirement))
            {
                status += "\n" + Loc.F("tooltip.req", requirement);
            }
        }

        string description = visibility == SkillVisibility.Silhouette ? Loc.T("tooltip.soon") : SkillTreeDefinition.GetDescription(node);
        string header = $"<b>{SkillTreeDefinition.GetName(node)}</b>\n{description}";
        text.text = string.IsNullOrEmpty(status) ? header : $"{header}\n\n{status}";
        gameObject.SetActive(true);
        FollowCursor();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void FollowCursor()
    {
        if (Mouse.current == null)
        {
            return;
        }

        Vector2 mouse = MonitorScreen.Active ? MonitorScreen.Pointer : Mouse.current.position.ReadValue();
        Vector2Int pixels = MonitorScreen.Active ? MonitorScreen.PixelSize : new Vector2Int(Screen.width, Screen.height);
        bool flipX = mouse.x > pixels.x * 0.7f;
        bool flipY = mouse.y < pixels.y * 0.3f;

        rect.pivot = new Vector2(flipX ? 1f : 0f, flipY ? 0f : 1f);

        Camera uiCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)rect.parent, mouse, uiCamera, out Vector2 local);

        Vector2 offset = new(flipX ? -cursorOffset.x : cursorOffset.x, flipY ? -cursorOffset.y : cursorOffset.y);
        rect.anchoredPosition = local + offset;
    }
}
