using System;
using UnityEngine;

public static class SkillTreeFocus
{
    public static string SelectedId { get; private set; }
    public static string HoveredId { get; private set; }

    public static event Action Changed;

    public static string DisplayedId => string.IsNullOrEmpty(HoveredId) ? SelectedId : HoveredId;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetSession()
    {
        SelectedId = null;
        HoveredId = null;
        Changed = null;
    }

    public static void Select(string skillId)
    {
        if (SelectedId == skillId)
        {
            return;
        }

        SelectedId = skillId;
        Changed?.Invoke();
    }

    public static void Hover(string skillId)
    {
        if (HoveredId == skillId)
        {
            return;
        }

        HoveredId = skillId;
        Changed?.Invoke();
    }

    public static void ClearHover(string skillId)
    {
        if (HoveredId != skillId)
        {
            return;
        }

        HoveredId = null;
        Changed?.Invoke();
    }
}
