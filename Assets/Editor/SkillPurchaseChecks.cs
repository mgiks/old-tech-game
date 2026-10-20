using UnityEditor;
using UnityEngine;

public static class SkillPurchaseCheckMenu
{
    [MenuItem("Tools/Old Tech/Run Skill Purchase Checks", false, 40)]
    public static void RunFromMenu()
    {
        Debug.Log(SkillPurchaseChecks.Run());
    }
}
