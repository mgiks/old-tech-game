using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Editor play uses progress_v2.dev.json. Player builds keep progress_v2.json.
/// Reset Test Progress deletes only the development file. Reset Progress On Play is off
/// by default and, when enabled, clears the development file as play mode starts.
/// </summary>
public static class SkillTreeDevTools
{
    private const string ResetOnPlayKey = "OldTech.ResetProgressOnPlay";
    private const string MenuRoot = "Tools/Old Tech/";

    [MenuItem(MenuRoot + "Reset Test Progress", false, 1)]
    public static void ResetTestProgress()
    {
        string production = GameProgress.ProductionSavePath;
        string development = GameProgress.DevelopmentSavePath;
        GameProgress.ResetDevelopmentSave();
        RunStats.ResetSession();
        SkillTreeFocus.Select(null);
        if (EditorApplication.isPlaying)
        {
            Scene active = SceneManager.GetActiveScene();
            if (!string.IsNullOrEmpty(active.name))
            {
                SceneManager.LoadScene(active.name);
            }
        }

        Debug.Log("Тестовый прогресс сброшен. Файл разработки: " + development + ". Файл игрока не изменён: " + production);
    }

    [MenuItem(MenuRoot + "Reset Progress On Play", false, 2)]
    private static void ToggleResetOnPlay()
    {
        EditorPrefs.SetBool(ResetOnPlayKey, !EditorPrefs.GetBool(ResetOnPlayKey, false));
    }

    [MenuItem(MenuRoot + "Reset Progress On Play", true)]
    private static bool ValidateResetOnPlay()
    {
        Menu.SetChecked(MenuRoot + "Reset Progress On Play", EditorPrefs.GetBool(ResetOnPlayKey, false));
        return true;
    }

    [MenuItem(MenuRoot + "Grant 100 MB", false, 20)]
    private static void Grant100() => Grant(100L);

    [MenuItem(MenuRoot + "Grant 10K MB", false, 21)]
    private static void Grant10K() => Grant(10_000L);

    [MenuItem(MenuRoot + "Grant 1M MB", false, 22)]
    private static void Grant1M() => Grant(1_000_000L);

    private static void Grant(long amount)
    {
        if (!EditorApplication.isPlaying)
        {
            Debug.LogWarning("Начисление MB работает только в Play Mode и пишется в файл разработки.");
            return;
        }

        GameProgress.AddMB(amount);
        Debug.Log("Начислено " + amount + " MB. Сейчас " + GameProgress.MB + " MB. Файл: " + GameProgress.SavePath);
    }
}
