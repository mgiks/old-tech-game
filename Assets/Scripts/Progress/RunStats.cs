using UnityEngine;

public static class RunStats
{
    public static uint MBEarned { get; private set; }
    public static int FilesInfected { get; private set; }
    public static int Clicks { get; private set; }
    public static int ComputersInfected { get; private set; }
    public static int TotalComputers { get; private set; }

    public static float GBEarned => MBEarned / 1024f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    public static void Reset()
    {
        MBEarned = 0;
        FilesInfected = 0;
        Clicks = 0;
        ComputersInfected = 0;
        TotalComputers = 0;
    }

    public static void Begin(int totalComputers)
    {
        Reset();
        TotalComputers = totalComputers;
    }

    public static void AddMB(uint amount) => MBEarned += amount;

    public static void AddFile() => FilesInfected++;

    public static void AddClick() => Clicks++;

    public static void AddComputer() => ComputersInfected++;
}
