using System;
using System.Collections.Generic;
using UnityEngine;

public static class GameProgress
{
    private static readonly Dictionary<string, int> skillLevels = new();

    public static uint MB { get; private set; }
    public static ulong TotalMB { get; private set; }
    public static bool IntroDone { get; set; }
    public static bool GameCompleted { get; set; }

    public static event Action Changed;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetSession()
    {
        Changed = null;
        StartOver();
    }

    public static void StartOver()
    {
        skillLevels.Clear();
        MB = 0;
        TotalMB = 0;
        IntroDone = false;
        GameCompleted = false;
        Changed?.Invoke();
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
        TotalMB += amount;
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
