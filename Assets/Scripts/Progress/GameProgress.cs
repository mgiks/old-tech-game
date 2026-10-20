using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class GameProgress
{
    private const string FileName = "progress_v2.json";

    private static readonly Dictionary<string, int> skillLevels = new();
    private static bool loading;

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

    public static string SavePath => Path.Combine(Application.persistentDataPath, FileName);

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
        Load();
        Application.quitting -= Save;
        Application.quitting += Save;
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
        if (loading)
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
