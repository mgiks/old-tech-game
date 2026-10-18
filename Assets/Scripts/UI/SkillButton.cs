using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class SkillButton : MonoBehaviour
{
    [SerializeField] private SkillManager skillManager;
    [SerializeField] private SkillType skill;
    [SerializeField] private TextMeshProUGUI label;

    private Button button;

    void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

    void OnEnable()
    {
        skillManager.StateChanged += Refresh;
        Refresh();
    }

    void OnDisable()
    {
        skillManager.StateChanged -= Refresh;
    }

    private void OnClick()
    {
        skillManager.TryUpgrade(skill);
    }

    private void Refresh()
    {
        string skillName = skillManager.GetDisplayName(skill);

        if (skillManager.IsComingSoon(skill))
        {
            label.text = $"{skillName}\n<size=70%>скоро</size>";
        }
        else if (skillManager.IsMaxed(skill))
        {
            label.text = $"{skillName}\n<size=70%>ур. {skillManager.GetLevel(skill)} (макс.)</size>";
        }
        else
        {
            label.text = $"{skillName}\n<size=70%>ур. {skillManager.GetLevel(skill)}/{skillManager.GetMaxLevel(skill)} — {skillManager.GetCost(skill)} MB</size>";
        }

        button.interactable = skillManager.CanUpgrade(skill);
    }
}
