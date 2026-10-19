using System;
using System.Collections.Generic;
using UnityEngine;

public static class GameProgress
{
    private const string SaveKey = "oldtech.progress.v1";

    private static readonly Dictionary<string, int> skillLevels = new();
    private static readonly HashSet<string> seenSkills = new();

    private static bool introDone;
    private static bool gameCompleted;

    public static uint MB { get; private set; }
    public static ulong TotalMB { get; private set; }
    public static bool IntroDone => introDone;
    public static bool GameCompleted => gameCompleted;

    public static event Action Changed;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetSession()
    {
        Changed = null;
        skillLevels.Clear();
        seenSkills.Clear();
        MB = 0;
        TotalMB = 0;
        introDone = false;
        gameCompleted = false;
        Load();
    }

    public static void StartOver()
    {
        skillLevels.Clear();
        seenSkills.Clear();
        MB = 0;
        TotalMB = 0;
        introDone = false;
        gameCompleted = false;
        PlayerPrefs.DeleteKey(SaveKey);
        PlayerPrefs.Save();
        Changed?.Invoke();
    }

    public static void CompleteIntro()
    {
        if (introDone)
        {
            return;
        }

        introDone = true;
        Save();
    }

    public static void MarkGameCompleted()
    {
        gameCompleted = true;
        Save();
    }

    public static bool HasSeen(string skillId)
    {
        return seenSkills.Contains(skillId);
    }

    public static void RememberSeen(List<string> ids)
    {
        bool added = false;
        for (int i = 0; i < ids.Count; i++)
        {
            if (seenSkills.Add(ids[i]))
            {
                added = true;
            }
        }

        if (added)
        {
            Save();
        }
    }

    public static int GetSkillLevel(string skillId)
    {
        return skillLevels.TryGetValue(skillId, out int level) ? level : 0;
    }

    public static void SetSkillLevel(string skillId, int level)
    {
        skillLevels[skillId] = level;
        Save();
        Changed?.Invoke();
    }

    public static void AddMB(uint amount)
    {
        MB += amount;
        TotalMB += amount;
        Save();
        Changed?.Invoke();
    }

    public static bool TrySpendMB(uint amount)
    {
        if (amount > MB)
        {
            return false;
        }

        MB -= amount;
        Save();
        Changed?.Invoke();
        return true;
    }

    private static void Save()
    {
        SaveFile file = new()
        {
            mb = MB,
            totalMb = (long)TotalMB,
            introDone = introDone ? 1 : 0,
            gameCompleted = gameCompleted ? 1 : 0,
            seen = new string[seenSkills.Count],
            skills = new SkillRecord[skillLevels.Count]
        };

        seenSkills.CopyTo(file.seen);
        int index = 0;
        foreach (KeyValuePair<string, int> pair in skillLevels)
        {
            file.skills[index++] = new SkillRecord { id = pair.Key, level = pair.Value };
        }

        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(file));
        PlayerPrefs.Save();
    }

    private static void Load()
    {
        if (!PlayerPrefs.HasKey(SaveKey))
        {
            return;
        }

        SaveFile file = JsonUtility.FromJson<SaveFile>(PlayerPrefs.GetString(SaveKey));
        if (file == null)
        {
            return;
        }

        MB = (uint)Mathf.Max(0, file.mb);
        TotalMB = (ulong)Mathf.Max(0, file.totalMb);
        introDone = file.introDone != 0;
        gameCompleted = file.gameCompleted != 0;

        if (file.seen != null)
        {
            foreach (string id in file.seen)
            {
                if (!string.IsNullOrEmpty(id))
                {
                    seenSkills.Add(id);
                }
            }
        }

        if (file.skills == null)
        {
            return;
        }

        foreach (SkillRecord record in file.skills)
        {
            if (record != null && !string.IsNullOrEmpty(record.id) && record.level > 0)
            {
                skillLevels[record.id] = record.level;
            }
        }
    }

    [Serializable]
    private class SaveFile
    {
        public long mb;
        public long totalMb;
        public int introDone;
        public int gameCompleted;
        public string[] seen = Array.Empty<string>();
        public SkillRecord[] skills = Array.Empty<SkillRecord>();
    }

    [Serializable]
    private class SkillRecord
    {
        public string id;
        public int level;
    }
}
