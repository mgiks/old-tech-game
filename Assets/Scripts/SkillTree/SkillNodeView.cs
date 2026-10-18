using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SkillNodeView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image glow;
    [SerializeField] private Image frame;
    [SerializeField] private Image body;
    [SerializeField] private TextMeshProUGUI nameLabel;
    [SerializeField] private TextMeshProUGUI levelLabel;
    [SerializeField] private Button button;

    [Header("Motion")]
    [SerializeField] private float hoverScale = 1.12f;
    [SerializeField] private float springStiffness = 320f;
    [SerializeField] private float springDamping = 14f;
    [SerializeField] private float purchasePunch = 9f;
    [SerializeField] private float popVelocity = 14f;

    [Header("Colors")]
    [SerializeField] private Color affordableColor = new(0.15f, 0.95f, 0.55f, 1f);
    [SerializeField] private Color unaffordableColor = new(0.3f, 0.33f, 0.45f, 1f);
    [SerializeField] private Color ownedColor = new(0.25f, 0.55f, 1f, 1f);
    [SerializeField] private Color maxedColor = new(1f, 0.75f, 0.2f, 1f);
    [SerializeField] private Color comingSoonColor = new(0.65f, 0.3f, 1f, 1f);
    [SerializeField, Range(0f, 1f)] private float bodyBrightness = 0.22f;

    private SkillTreeDefinition skillTree;
    private SkillTreeDefinition.Node node;
    private SkillTooltip tooltip;
    private SkillTreeFx fx;
    private RectTransform rect;
    private Vector2 basePosition;
    private Color stateColor;
    private bool hovered;
    private bool wasRevealed;
    private bool popWithFx;
    private float scale = 1f;
    private float scaleVelocity;
    private float flash;
    private float popAt = -1f;
    private float denyTime = -10f;
    private float phase;

    public void Init(SkillTreeDefinition tree, SkillTreeDefinition.Node definition, SkillTooltip skillTooltip, SkillTreeFx skillFx)
    {
        skillTree = tree;
        node = definition;
        tooltip = skillTooltip;
        fx = skillFx;
        rect = (RectTransform)transform;
        basePosition = rect.anchoredPosition;
        phase = Random.value * 10f;
        button.onClick.AddListener(OnClick);
        Refresh(-1f);
    }

    public void PlayPop(float delay, bool withFx)
    {
        popAt = Time.unscaledTime + delay;
        popWithFx = withFx;
        scale = 0f;
        scaleVelocity = 0f;
        transform.localScale = Vector3.zero;
    }

    public void Refresh(float revealPopDelay)
    {
        bool revealed = skillTree.IsRevealed(node);
        bool newlyRevealed = revealed && !wasRevealed;
        wasRevealed = revealed;

        gameObject.SetActive(revealed);
        if (!revealed)
        {
            return;
        }

        if (newlyRevealed && revealPopDelay >= 0f)
        {
            PlayPop(revealPopDelay, true);
        }

        int level = skillTree.GetLevel(node);
        nameLabel.text = node.displayName;
        levelLabel.text = node.comingSoon ? "скоро" : node.startsOwned ? string.Empty : $"{level}/{node.maxLevel}";

        stateColor = GetStateColor(level);
        levelLabel.color = Color.Lerp(stateColor, Color.white, 0.35f);

        if (hovered)
        {
            tooltip.Show(skillTree, node);
        }
    }

    void Update()
    {
        float now = Time.unscaledTime;
        float dt = Time.unscaledDeltaTime;

        if (popAt >= 0f)
        {
            if (now < popAt)
            {
                transform.localScale = Vector3.zero;
                return;
            }

            popAt = -1f;
            scaleVelocity = popVelocity;
            flash = 1f;
            if (popWithFx)
            {
                fx.PlayReveal(rect, stateColor);
            }
            else
            {
                fx.PlayIntroPop(rect, stateColor);
            }
        }

        bool affordable = skillTree.CanBuy(node);

        float target = hovered ? hoverScale : 1f;
        if (affordable)
        {
            target += Mathf.Sin(now * 3f + phase) * 0.025f;
        }

        scaleVelocity += (target - scale) * springStiffness * dt;
        scaleVelocity *= Mathf.Exp(-springDamping * dt);
        scale += scaleVelocity * dt;
        transform.localScale = new Vector3(scale, scale, 1f);

        flash *= Mathf.Exp(-5f * dt);

        float pulse = affordable ? 0.5f + 0.5f * Mathf.Sin(now * 4f + phase) : 0f;
        float idleGlow = affordable ? 0.45f + 0.35f * pulse : skillTree.GetLevel(node) > 0 || node.comingSoon ? 0.3f : 0.12f;
        float glowAlpha = Mathf.Clamp01(idleGlow + (hovered ? 0.3f : 0f) + flash * 0.8f);

        glow.color = new Color(stateColor.r, stateColor.g, stateColor.b, glowAlpha);
        glow.rectTransform.localScale = Vector3.one * (1f + flash * 0.5f + pulse * 0.08f);
        frame.color = Color.Lerp(stateColor, Color.white, flash * 0.8f);

        Color dark = new(stateColor.r * bodyBrightness, stateColor.g * bodyBrightness, stateColor.b * bodyBrightness, 1f);
        body.color = Color.Lerp(dark, Color.white, flash * 0.5f);

        float denyAge = now - denyTime;
        float wobble = denyAge < 0.4f ? Mathf.Sin(denyAge * 70f) * 14f * (1f - denyAge / 0.4f) : 0f;
        rect.anchoredPosition = basePosition + new Vector2(wobble, 0f);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hovered = true;
        tooltip.Show(skillTree, node);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hovered = false;
        tooltip.Hide();
    }

    void OnDisable()
    {
        if (hovered && tooltip != null)
        {
            hovered = false;
            tooltip.Hide();
        }
    }

    private void OnClick()
    {
        if (node.startsOwned)
        {
            scaleVelocity += purchasePunch * 0.6f;
            flash = 0.6f;
            fx.PlayPoke(rect, stateColor);
            return;
        }

        if (node.comingSoon || skillTree.IsMaxed(node) || !skillTree.CanBuy(node))
        {
            denyTime = Time.unscaledTime;
            fx.PlayDenied(rect, node.comingSoon ? "Скоро"
                : skillTree.IsMaxed(node) ? "Максимум"
                : node.requiresAllMaxed && !skillTree.IsEverythingElseMaxed() ? "Нужно всё дерево"
                : "Мало MB");
            return;
        }

        uint cost = skillTree.GetCost(node);
        if (!skillTree.TryBuy(node))
        {
            return;
        }

        scaleVelocity += purchasePunch;
        flash = 1f;
        fx.PlayPurchase(rect, stateColor, skillTree.IsMaxed(node), cost);
    }

    private Color GetStateColor(int level)
    {
        if (node.comingSoon)
        {
            return comingSoonColor;
        }

        if (skillTree.IsMaxed(node))
        {
            return maxedColor;
        }

        if (skillTree.CanBuy(node))
        {
            return affordableColor;
        }

        return level > 0 ? ownedColor : unaffordableColor;
    }
}
