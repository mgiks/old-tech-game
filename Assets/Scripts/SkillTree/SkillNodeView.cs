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

    [Header("Motion")]
    [SerializeField] private float hoverScale = 1.12f;
    [SerializeField] private float springStiffness = 320f;
    [SerializeField] private float springDamping = 14f;
    [SerializeField] private float purchasePunch = 9f;
    [SerializeField] private float popVelocity = 14f;

    [Header("Colors")]
    [SerializeField] private Color affordableColor = new(0.22f, 0.78f, 0.28f, 1f);
    [SerializeField] private Color unaffordableColor = new(0.82f, 0.18f, 0.16f, 1f);
    [SerializeField] private Color ownedColor = new(0.22f, 0.48f, 0.9f, 1f);
    [SerializeField] private Color maxedColor = new(0.93f, 0.72f, 0.16f, 1f);
    [SerializeField] private Color comingSoonColor = new(0.55f, 0.56f, 0.58f, 1f);
    [SerializeField] private Color lockedColor = new(0.42f, 0.44f, 0.47f, 1f);
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
        if (skillIcon != null)
        {
            skillIcon.sprite = node.icon;
            skillIcon.enabled = node.icon != null;
            skillIcon.color = Color.white;
            skillIcon.material = null;
            skillIcon.preserveAspect = true;
            skillIcon.raycastTarget = false;
        }

        if (frame != null)
        {
            frame.material = null;
            frame.color = Color.white;
        }

        if (glow != null)
        {
            glow.material = null;
            glow.rectTransform.sizeDelta = new Vector2(148f, 148f);
        }

        if (body != null && skillSlot != null && slotNormal != null)
        {
            body.enabled = false;
        }

        if (nameLabel != null)
        {
            nameLabel.color = XpUi.TextDark;
        }

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

        gameObject.SetActive(true);
        if (!revealed)
        {
            nameLabel.text = SkillTreeDefinition.GetName(node);
            levelLabel.text = string.Empty;
            stateColor = lockedColor;
            nameLabel.color = new Color(0.35f, 0.38f, 0.42f, 1f);
            levelLabel.color = nameLabel.color;
            return;
        }

        if (newlyRevealed && revealPopDelay >= 0f)
        {
            PlayPop(revealPopDelay, true);
        }

        int level = skillTree.GetLevel(node);
        nameLabel.text = SkillTreeDefinition.GetName(node);
        levelLabel.text = node.comingSoon ? Loc.T("tree.soon") : node.startsOwned ? string.Empty : $"{level}/{node.maxLevel}";

        stateColor = GetStateColor();
        nameLabel.color = XpUi.TextDark;
        levelLabel.color = XpUi.TextDark;

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
        float glowAlpha = (affordable ? 0.16f : 0.07f) + pulse * 0.06f + (hovered ? 0.08f : 0f) + flash * 0.12f;
        glow.color = new Color(stateColor.r, stateColor.g, stateColor.b, Mathf.Clamp01(glowAlpha));
        glow.rectTransform.localScale = Vector3.one * (1f + flash * 0.12f);
        if (skillSlot != null && slotNormal != null)
        {
            skillSlot.sprite = hovered && slotHover != null ? slotHover : slotNormal;
            skillSlot.color = stateColor;
            frame.color = stateColor;
        }
        else
        {
            frame.color = stateColor;
            if (skillSlot != null)
            {
                skillSlot.color = stateColor;
            }
        }

        if (body != null && body.enabled)
        {
            Color paper = Color.Lerp(new Color(0.7f, 0.69f, 0.62f, 1f), XpUi.WindowBody, Mathf.Clamp01(bodyBrightness + 0.7f));
            body.color = paper;
        }

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
            GameAudio.PlayMouseClick();
            scaleVelocity += purchasePunch * 0.6f;
            flash = 0.6f;
            fx.PlayPoke(rect, stateColor);
            return;
        }

        if (node.comingSoon || skillTree.IsMaxed(node) || !skillTree.CanBuy(node))
        {
            GameAudio.PlayMouseClick();
            denyTime = Time.unscaledTime;
            string denyKey = "deny.no_mb";
            if (!skillTree.IsRevealed(node))
            {
                denyKey = "deny.locked";
            }
            else if (node.comingSoon)
            {
                denyKey = "deny.soon";
            }
            else if (skillTree.IsMaxed(node))
            {
                denyKey = "deny.max";
            }
            else if (node.requiresAllMaxed && !skillTree.IsEverythingElseMaxed())
            {
                denyKey = "deny.need_all";
            }

            fx.PlayDenied(rect, Loc.T(denyKey));
            return;
        }

        uint cost = skillTree.GetCost(node);
        if (!skillTree.TryBuy(node))
        {
            return;
        }

        scaleVelocity += purchasePunch;
        flash = 1f;
        GameAudio.Play(GameSfx.LevelUp);
        fx.PlayPurchase(rect, stateColor, skillTree.IsMaxed(node), cost);
    }

    private Color GetStateColor()
    {
        if (!skillTree.IsRevealed(node))
        {
            return lockedColor;
        }

        if (node.comingSoon)
        {
            return comingSoonColor;
        }

        if (node.startsOwned || skillTree.IsMaxed(node))
        {
            return skillTree.IsMaxed(node) ? maxedColor : ownedColor;
        }

        if (skillTree.CanBuy(node))
        {
            return affordableColor;
        }

        return unaffordableColor;
    }
}
