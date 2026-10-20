using System;

public enum CompletionSource
{
    None,
    Manual,
    MiniVirus,
    Chain,
    Passive,
    Fragment
}

public static class GameEvents
{
    public static event Action<DesktopIcon, CompletionSource> FileDownloadStarted;
    public static event Action<DesktopIcon> FileDownloadProgressChanged;
    public static event Action<DesktopIcon, CompletionSource> FileDownloadCompleted;
    public static event Action<DesktopIcon> FileConsumed;
    public static event Action<DesktopIcon> FileInfected;
    public static event Action<int> ChainStarted;
    public static event Action<DesktopIcon> ChainTargetCompleted;
    public static event Action<int> MiniVirusSpawned;
    public static event Action OverloadStarted;
    public static event Action OverloadEnded;
    public static event Action<long> FragmentCollected;
    public static event Action<long> CurrencyEarned;
    public static event Action<string, int> SkillPurchased;
    public static event Action<string> ProgressionMilestoneReached;

    public static void RaiseDownloadStarted(DesktopIcon icon, CompletionSource source) => FileDownloadStarted?.Invoke(icon, source);
    public static void RaiseProgress(DesktopIcon icon) => FileDownloadProgressChanged?.Invoke(icon);
    public static void RaiseDownloadCompleted(DesktopIcon icon, CompletionSource source) => FileDownloadCompleted?.Invoke(icon, source);
    public static void RaiseConsumed(DesktopIcon icon) => FileConsumed?.Invoke(icon);
    public static void RaiseInfected(DesktopIcon icon) => FileInfected?.Invoke(icon);
    public static void RaiseChainStarted(int id) => ChainStarted?.Invoke(id);
    public static void RaiseChainTarget(DesktopIcon icon) => ChainTargetCompleted?.Invoke(icon);
    public static void RaiseMiniSpawned(int count) => MiniVirusSpawned?.Invoke(count);
    public static void RaiseOverloadStarted() => OverloadStarted?.Invoke();
    public static void RaiseOverloadEnded() => OverloadEnded?.Invoke();
    public static void RaiseFragment(long amount) => FragmentCollected?.Invoke(amount);
    public static void RaiseCurrency(long amount) => CurrencyEarned?.Invoke(amount);
    public static void RaisePurchased(string id, int level) => SkillPurchased?.Invoke(id, level);
    public static void RaiseMilestone(string id) => ProgressionMilestoneReached?.Invoke(id);

    public static void Clear()
    {
        FileDownloadStarted = null;
        FileDownloadProgressChanged = null;
        FileDownloadCompleted = null;
        FileConsumed = null;
        FileInfected = null;
        ChainStarted = null;
        ChainTargetCompleted = null;
        MiniVirusSpawned = null;
        OverloadStarted = null;
        OverloadEnded = null;
        FragmentCollected = null;
        CurrencyEarned = null;
        SkillPurchased = null;
        ProgressionMilestoneReached = null;
    }
}
