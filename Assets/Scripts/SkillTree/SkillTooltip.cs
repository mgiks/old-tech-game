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
        string status;
        int level = skillTree.GetLevel(node);

        if (node.comingSoon)
        {
            status = $"<color=#C9A0FF>{Loc.T("tooltip.soon")}</color>";
        }
        else if (node.startsOwned)
        {
            status = string.Empty;
        }
        else if (skillTree.IsMaxed(node))
        {
            status = Loc.F("tooltip.level_max", level, node.maxLevel);
        }
        else
        {
            uint cost = skillTree.GetCost(node);
            string costColor = GameProgress.MB >= cost ? "#7CFF9A" : "#FF7C7C";
            status = Loc.F("tooltip.level_cost", level, node.maxLevel, costColor, cost);
        }

        if (node.requiresAllMaxed && !skillTree.IsMaxed(node))
        {
            skillTree.GetMasteryProgress(out int done, out int total);
            string progressColor = done >= total ? "#7CFF9A" : "#FF7C7C";
            status = $"{Loc.F("tooltip.mastery", progressColor, done, total)}\n{status}";
        }

        if (!node.comingSoon && node.stat != SkillStat.None)
        {
            float current = skillTree.GetStat(node.stat);
            string now = SkillTreeDefinition.FormatStat(node.stat, current);
            string preview = skillTree.IsMaxed(node)
                ? $"<color=#FFD24D>{now}</color>"
                : $"<color=#8A93B8>{now}</color>  →  <color=#7CFFE0><b>{SkillTreeDefinition.FormatStat(node.stat, current + node.valuePerLevel)}</b></color>";
            status = $"{preview}\n{status}";
        }

        string header = $"<b>{SkillTreeDefinition.GetName(node)}</b>\n{SkillTreeDefinition.GetDescription(node)}";
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

        Vector2 mouse = Mouse.current.position.ReadValue();
        bool flipX = mouse.x > Screen.width * 0.7f;
        bool flipY = mouse.y < Screen.height * 0.3f;

        rect.pivot = new Vector2(flipX ? 1f : 0f, flipY ? 0f : 1f);

        Camera uiCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)rect.parent, mouse, uiCamera, out Vector2 local);

        Vector2 offset = new(flipX ? -cursorOffset.x : cursorOffset.x, flipY ? -cursorOffset.y : cursorOffset.y);
        rect.anchoredPosition = local + offset;
    }
}
