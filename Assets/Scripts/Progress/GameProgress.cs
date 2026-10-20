using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class GameProgress
{
    public const string ProductionSaveName = "progress_v2.json";
    public const string DevelopmentSaveName = "progress_v2.dev.json";
    private const string ResetOnPlayKey = "OldTech.ResetProgressOnPlay";

    private static readonly Dictionary<string, int> skillLevels = new();
    private static bool loading;
    private static string sandboxPath;

    public static long MB { get; private set; }
    public static long TotalMB { get; private set; }
    public static bool IntroDone { get; private set; }
    public static bool GameCompleted { get; private set; }

    public static int TotalPurchases
    {
        get
        {
            int total = 0;
            foreach (SkillDef skill in SkillBalanceTable.Skills)
            {
                if (!skill.Anchor && skillLevels.TryGetValue(skill.Id, out int level))
                {
                    total += Mathf.Clamp(level, 0, skill.MaxLevel);
                }
            }

            return Mathf.Min(ProgressionConfig.TargetPurchases, total);
        }
    }

    public static event Action Changed;

    public static bool UsesDevelopmentSave
    {
        get
        {
#if UNITY_EDITOR
            return string.IsNullOrEmpty(sandboxPath);
#else
            return false;
#endif
        }
    }

    public static string SavePath
    {
        get
        {
            if (!string.IsNullOrEmpty(sandboxPath))
            {
                return sandboxPath;
            }

#if UNITY_EDITOR
            return Path.Combine(Application.persistentDataPath, DevelopmentSaveName);
#else
            return Path.Combine(Application.persistentDataPath, ProductionSaveName);
#endif
        }
    }

    public static string ProductionSavePath => Path.Combine(Application.persistentDataPath, ProductionSaveName);

    public static string DevelopmentSavePath => Path.Combine(Application.persistentDataPath, DevelopmentSaveName);

    internal static string SandboxPath => sandboxPath;

    internal static Dictionary<string, int> CopyLevels()
    {
        return new Dictionary<string, int>(skillLevels);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetSession()
    {
        Changed = null;
        GameEvents.Clear();
        ClearMemory();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Boot()
    {
        sandboxPath = null;
#if UNITY_EDITOR
        PrepareEditorSave();
#endif
        Load();
        Application.quitting -= Save;
        Application.quitting += Save;
    }

#if UNITY_EDITOR
    private static void PrepareEditorSave()
    {
        bool resetOnPlay = UnityEditor.EditorPrefs.GetBool(ResetOnPlayKey, false);
        if (resetOnPlay)
        {
            TryDelete(DevelopmentSavePath);
            return;
        }

        if (!File.Exists(DevelopmentSavePath) && File.Exists(ProductionSavePath))
        {
            try
            {
                File.Copy(ProductionSavePath, DevelopmentSavePath);
            }
            catch (Exception exception)
            {
                Debug.LogWarning("Development save could not be seeded: " + exception.Message);
            }
        }
    }
#endif

    public static ProgressSandbox BeginSandbox(string absoluteSavePath)
    {
        ProgressSandbox sandbox = ProgressSandbox.Capture();
        sandboxPath = absoluteSavePath;
        loading = true;
        ClearMemory();
        loading = false;
        return sandbox;
    }

    public static void ClearSandboxProgress()
    {
        if (string.IsNullOrEmpty(sandboxPath))
        {
            return;
        }

        loading = true;
        ClearMemory();
        loading = false;
    }

    public static void ResetDevelopmentSave()
    {
#if UNITY_EDITOR
        loading = true;
        ClearMemory();
        loading = false;
        TryDelete(DevelopmentSavePath);
        Changed?.Invoke();
#else
        Debug.LogWarning("Development reset is available in the editor only.");
#endif
    }

    private static bool WouldEraseProgress()
    {
        if (MB != 0 || TotalMB != 0 || IntroDone || GameCompleted || skillLevels.Count > 0 || !File.Exists(SavePath))
        {
            return false;
        }

        try
        {
            SaveFile existing = JsonUtility.FromJson<SaveFile>(File.ReadAllText(SavePath));
            return existing != null && (existing.mb > 0 || existing.totalMb > 0 || existing.introDone || existing.gameCompleted || (existing.ids != null && existing.ids.Length > 0));
        }
        catch (Exception)
        {
            return true;
        }
    }

    private static void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch (Exception exception)
        {
            Debug.LogWarning("Save could not be deleted: " + exception.Message);
        }
    }

    public static int GetSkillLevel(string skillId)
    {
        return skillId != null && skillLevels.TryGetValue(skillId, out int level) ? level : 0;
    }

    public static void SetIntroDone()
    {
        if (IntroDone)
        {
            return;
        }

        IntroDone = true;
        Save();
        Changed?.Invoke();
    }

    public static void SetGameCompleted()
    {
        GameCompleted = true;
        Save();
        Changed?.Invoke();
    }

    public static void AddMB(long amount)
    {
        if (amount <= 0)
        {
            return;
        }

        MB = EconomyMath.AddClamped(MB, amount);
        TotalMB = EconomyMath.AddClamped(TotalMB, amount);
        GameEvents.RaiseCurrency(amount);
        Changed?.Invoke();
        if (!loading)
        {
            Save();
        }
    }

    public static bool TrySpendMB(long amount)
    {
        if (amount <= 0 || amount > MB)
        {
            return false;
        }

        MB -= amount;
        Changed?.Invoke();
        return true;
    }

    public static void RefundMB(long amount)
    {
        if (amount <= 0)
        {
            return;
        }

        MB = EconomyMath.AddClamped(MB, amount);
        Changed?.Invoke();
    }

    public static bool CommitSkillLevel(string skillId, int level)
    {
        SkillDef skill = SkillBalanceTable.Find(skillId);
        if (skill == null || skill.Anchor || level < 1 || level > skill.MaxLevel)
        {
            return false;
        }

        if (TotalPurchases >= ProgressionConfig.TargetPurchases && GetSkillLevel(skillId) < level)
        {
            return false;
        }

        skillLevels[skillId] = level;
        Changed?.Invoke();
        GameEvents.RaisePurchased(skillId, level);
        if (skill.Major && level == 1 && skill.MaxLevel == 1)
        {
            GameEvents.RaiseMilestone(skillId);
        }

        Save();
        return true;
    }

    public static void StartOver()
    {
        ClearMemory();
        try
        {
            if (File.Exists(SavePath))
            {
                File.Delete(SavePath);
            }
        }
        catch (Exception exception)
        {
            Debug.LogWarning("Progress reset could not delete the save: " + exception.Message);
        }

        Changed?.Invoke();
    }

    private static void ClearMemory()
    {
        skillLevels.Clear();
        MB = 0;
        TotalMB = 0;
        IntroDone = false;
        GameCompleted = false;
    }

    public static void Save()
    {
        if (loading || WouldEraseProgress())
        {
            return;
        }

        try
        {
            SaveFile data = new()
            {
                version = ProgressionConfig.SaveVersion,
                mb = MB,
                totalMb = TotalMB,
                introDone = IntroDone,
                gameCompleted = GameCompleted
            };

            List<string> ids = new();
            List<int> levels = new();
            foreach (KeyValuePair<string, int> pair in skillLevels)
            {
                SkillDef skill = SkillBalanceTable.Find(pair.Key);
                if (skill == null || skill.Anchor || pair.Value <= 0)
                {
                    continue;
                }

                ids.Add(pair.Key);
                levels.Add(Mathf.Clamp(pair.Value, 0, skill.MaxLevel));
            }

            data.ids = ids.ToArray();
            data.levels = levels.ToArray();
            File.WriteAllText(SavePath, JsonUtility.ToJson(data));
        }
        catch (Exception exception)
        {
            Debug.LogWarning("Progress save failed: " + exception.Message);
        }
    }

    private static void Load()
    {
        loading = true;
        ClearMemory();
        try
        {
            if (!File.Exists(SavePath))
            {
                return;
            }

            SaveFile data = JsonUtility.FromJson<SaveFile>(File.ReadAllText(SavePath));
            if (data == null || data.version != ProgressionConfig.SaveVersion)
            {
                Reject("progress_v2.rejected.json");
                return;
            }

            if (data.ids == null || data.levels == null || data.ids.Length != data.levels.Length || data.mb < 0 || data.totalMb < 0)
            {
                Reject("progress_v2.corrupt.json");
                return;
            }

            MB = data.mb;
            TotalMB = Math.Max(data.totalMb, data.mb);
            IntroDone = data.introDone;
            GameCompleted = data.gameCompleted;
            for (int i = 0; i < data.ids.Length; i++)
            {
                SkillDef skill = SkillBalanceTable.Find(data.ids[i]);
                if (skill == null || skill.Anchor)
                {
                    continue;
                }

                int level = Mathf.Clamp(data.levels[i], 0, skill.MaxLevel);
                if (level > 0)
                {
                    skillLevels[skill.Id] = level;
                }
            }
        }
        catch (Exception exception)
        {
            Debug.LogWarning("Progress load failed: " + exception.Message);
            ClearMemory();
            Reject("progress_v2.corrupt.json");
        }
        finally
        {
            loading = false;
        }
    }

    internal static void LeaveSandbox(ProgressSandbox sandbox)
    {
        loading = true;
        skillLevels.Clear();
        if (sandbox.Levels != null)
        {
            foreach (KeyValuePair<string, int> pair in sandbox.Levels)
            {
                skillLevels[pair.Key] = pair.Value;
            }
        }

        MB = sandbox.Mb;
        TotalMB = sandbox.TotalMb;
        IntroDone = sandbox.Intro;
        GameCompleted = sandbox.Completed;
        sandboxPath = sandbox.PreviousPath;
        loading = false;
        Changed?.Invoke();
    }

    private static void Reject(string rejectedName)
    {
        try
        {
            string rejected = Path.Combine(Application.persistentDataPath, rejectedName);
            if (File.Exists(SavePath))
            {
                if (File.Exists(rejected))
                {
                    File.Delete(rejected);
                }

                File.Move(SavePath, rejected);
            }
        }
        catch (Exception exception)
        {
            Debug.LogWarning("Progress file could not be moved aside: " + exception.Message);
        }
    }

    [Serializable]
    private class SaveFile
    {
        public int version;
        public long mb;
        public long totalMb;
        public bool introDone;
        public bool gameCompleted;
        public string[] ids;
        public int[] levels;
    }
}

public sealed class ProgressSandbox : IDisposable
{
    internal Dictionary<string, int> Levels;
    internal long Mb;
    internal long TotalMb;
    internal bool Intro;
    internal bool Completed;
    internal string PreviousPath;
    private bool disposed;

    internal static ProgressSandbox Capture()
    {
        return new ProgressSandbox
        {
            Levels = GameProgress.CopyLevels(),
            Mb = GameProgress.MB,
            TotalMb = GameProgress.TotalMB,
            Intro = GameProgress.IntroDone,
            Completed = GameProgress.GameCompleted,
            PreviousPath = GameProgress.SandboxPath
        };
    }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        disposed = true;
        GameProgress.LeaveSandbox(this);
    }
}
