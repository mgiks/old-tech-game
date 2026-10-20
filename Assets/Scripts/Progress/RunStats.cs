using UnityEngine;

public static class RunStats
{
    public static long MBEarned { get; private set; }
    public static int FilesInfected { get; private set; }
    public static int Clicks { get; private set; }
    public static int ComputersInfected { get; private set; }
    public static int TotalComputers { get; private set; }

    public static int RunNumber { get; private set; }
    public static long PreviousRunMB { get; private set; }
    public static bool HasPreviousRun { get; private set; }
    public static bool IsNewRecord { get; private set; }
    public static int TotalFiles { get; private set; }
    public static int TotalClicks { get; private set; }

    public static double GBEarned => MBEarned / 1024d;

    private static long bestRunMB;
    private static long lastRunMB;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    public static void ResetSession()
    {
        ResetRun();
        RunNumber = 0;
        PreviousRunMB = 0;
        HasPreviousRun = false;
        IsNewRecord = false;
        TotalFiles = 0;
        TotalClicks = 0;
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
        if (MBEarned > bestRunMB)
        {
            bestRunMB = MBEarned;
        }
        TotalFiles += FilesInfected;
        TotalClicks += Clicks;
    }

    public static void AddMB(long amount)
    {
        if (amount > 0)
        {
            MBEarned = EconomyMath.AddClamped(MBEarned, amount);
        }
    }

    public static void AddFile() => FilesInfected++;

    public static void AddClick() => Clicks++;

    public static void AddComputer() => ComputersInfected++;
}
