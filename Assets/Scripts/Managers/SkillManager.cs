using System;
using UnityEngine;

public enum SkillType
{
    Speed,
    Autonomy,
    Mutation
}

public class SkillManager : MonoBehaviour
{
    [Serializable]
    private class Skill
    {
        public SkillType type;
        public string displayName;
        public int maxLevel = 10;
        public uint baseCost = 5;
        public float costMultiplier = 1.5f;
        public bool comingSoon;
        [NonSerialized] public int level;
    }

    [SerializeField] private ScoreManager scoreManager;
    [SerializeField]
    private Skill[] skills =
    {
        new() { type = SkillType.Speed, displayName = "Speed", maxLevel = 10, baseCost = 5, costMultiplier = 1.5f },
        new() { type = SkillType.Autonomy, displayName = "Autonomy", maxLevel = 10, baseCost = 10, costMultiplier = 1.6f },
        new() { type = SkillType.Mutation, displayName = "Mutations", comingSoon = true },
    };

    public event Action StateChanged;

    void OnEnable()
    {
        scoreManager.ScoreChanged += OnScoreChanged;
    }

    void OnDisable()
    {
        scoreManager.ScoreChanged -= OnScoreChanged;
    }

    public int GetLevel(SkillType type) => Find(type).level;

    public int GetMaxLevel(SkillType type) => Find(type).maxLevel;

    public string GetDisplayName(SkillType type) => Find(type).displayName;

    public bool IsComingSoon(SkillType type) => Find(type).comingSoon;

    public bool IsMaxed(SkillType type)
    {
        Skill skill = Find(type);
        return skill.level >= skill.maxLevel;
    }

    public uint GetCost(SkillType type)
    {
        Skill skill = Find(type);
        return (uint)Mathf.CeilToInt(skill.baseCost * Mathf.Pow(skill.costMultiplier, skill.level));
    }

    public bool CanUpgrade(SkillType type)
    {
        return !IsComingSoon(type) && !IsMaxed(type) && scoreManager.Score >= GetCost(type);
    }

    public bool TryUpgrade(SkillType type)
    {
        if (!CanUpgrade(type) || !scoreManager.TrySpend(GetCost(type)))
        {
            return false;
        }

        Find(type).level++;
        StateChanged?.Invoke();
        return true;
    }

    private void OnScoreChanged(uint score)
    {
        StateChanged?.Invoke();
    }

    private Skill Find(SkillType type)
    {
        foreach (Skill skill in skills)
        {
            if (skill.type == type)
            {
                return skill;
            }
        }

        throw new ArgumentException($"Skill {type} is not configured on {name}");
    }
}
