using System;
using UnityEngine;

public static class GamePause
{
    private static float savedTimeScale = 1f;

    public static bool IsPaused { get; private set; }

    public static event Action Changed;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetSession()
    {
        IsPaused = false;
        savedTimeScale = 1f;
        Changed = null;
    }

    public static void Pause()
    {
        if (IsPaused)
        {
            return;
        }

        IsPaused = true;
        savedTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        AudioListener.pause = true;
        Changed?.Invoke();
    }

    public static void Resume()
    {
        if (!IsPaused)
        {
            return;
        }

        IsPaused = false;
        Time.timeScale = savedTimeScale;
        AudioListener.pause = false;
        Changed?.Invoke();
    }
}
