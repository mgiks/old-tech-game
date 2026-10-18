using UnityEngine;

public static class RunStats
{
    public static uint MBEarned { get; private set; }
    public static int FilesInfected { get; private set; }
    public static int Clicks { get; private set; }
    public static int ComputersInfected { get; private set; }
    public static int TotalComputers { get; private set; }

    public static int RunNumber { get; private set; }
    public static uint PreviousRunMB { get; private set; }
    public static bool HasPreviousRun { get; private set; }
    public static bool IsNewRecord { get; private set; }

    private static uint bestRunMB;
    private static uint lastRunMB;

    public static float GBEarned => MBEarned / 1024f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetSession()
    {
        ResetRun();
        RunNumber = 0;
        PreviousRunMB = 0;
        HasPreviousRun = false;
        IsNewRecord = false;
        bestRunMB = 0;
        lastRunMB = 0;
    }

    private static void ResetRun()
    {
        MBEarned = 0;
        FilesInfected = 0;
        Clicks = 0;
        ComputersInfected = 0;
        TotalComputers = 0;
    }

    public static void Begin(int totalComputers)
    {
        ResetRun();
        RunNumber++;
        TotalComputers = totalComputers;
    }

    public static void Complete()
    {
        HasPreviousRun = RunNumber > 1;
        PreviousRunMB = lastRunMB;
        IsNewRecord = HasPreviousRun && MBEarned > bestRunMB;
        lastRunMB = MBEarned;
        bestRunMB = (uint)Mathf.Max(bestRunMB, MBEarned);
    }

    public static void AddMB(uint amount) => MBEarned += amount;

    public static void AddFile() => FilesInfected++;

    public static void AddClick() => Clicks++;

    public static void AddComputer() => ComputersInfected++;
}
