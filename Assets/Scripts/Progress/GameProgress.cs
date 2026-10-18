using System;
using System.Collections.Generic;
using UnityEngine;

public static class GameProgress
{
    private static readonly Dictionary<string, int> skillLevels = new();

    public static uint MB { get; private set; }

    public static event Action Changed;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetSession()
    {
        skillLevels.Clear();
        MB = 0;
        Changed = null;
    }

    public static int GetSkillLevel(string skillId)
    {
        return skillLevels.TryGetValue(skillId, out int level) ? level : 0;
    }

    public static void SetSkillLevel(string skillId, int level)
    {
        skillLevels[skillId] = level;
        Changed?.Invoke();
    }

    public static void AddMB(uint amount)
    {
        MB += amount;
        Changed?.Invoke();
    }

    public static bool TrySpendMB(uint amount)
    {
        if (amount > MB)
        {
            return false;
        }

        MB -= amount;
        Changed?.Invoke();
        return true;
    }
}
