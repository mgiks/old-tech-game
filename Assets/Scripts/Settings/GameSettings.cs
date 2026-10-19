using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public enum GameLanguage
{
    Ru,
    En
}

public static class GameSettings
{
    private const string VolumeKey = "settings.volume";
    private const string LanguageKey = "settings.language";
    private const string EffectsKey = "settings.screenEffects";
    private const string ShakeKey = "settings.screenShake";
    private const string WidthKey = "settings.width";
    private const string HeightKey = "settings.height";

    public static float MasterVolume { get; private set; } = 1f;
    public static GameLanguage Language { get; private set; } = GameLanguage.En;
    public static bool ScreenEffects { get; private set; } = true;
    public static bool ScreenShake { get; private set; } = true;

    public static event Action Changed;

    private static Vector2Int chosenResolution;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetSession()
    {
        Changed = null;
        chosenResolution = Vector2Int.zero;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Load()
    {
        MasterVolume = PlayerPrefs.GetFloat(VolumeKey, 1f);
        Language = (GameLanguage)PlayerPrefs.GetInt(LanguageKey, (int)GameLanguage.En);
        ScreenEffects = PlayerPrefs.GetInt(EffectsKey, 1) == 1;
        ScreenShake = PlayerPrefs.GetInt(ShakeKey, 1) == 1;

        AudioListener.volume = MasterVolume;

        int width = PlayerPrefs.GetInt(WidthKey, 0);
        int height = PlayerPrefs.GetInt(HeightKey, 0);
        if (width > 0 && height > 0 && !Application.isEditor)
        {
            Screen.SetResolution(width, height, Screen.fullScreenMode);
        }

        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public static void SetVolume(float volume)
    {
        MasterVolume = Mathf.Clamp01(volume);
        AudioListener.volume = MasterVolume;
        PlayerPrefs.SetFloat(VolumeKey, MasterVolume);
        Notify();
    }

    public static void SetLanguage(GameLanguage language)
    {
        if (Language == language)
        {
            return;
        }

        Language = language;
        PlayerPrefs.SetInt(LanguageKey, (int)language);
        Notify();
    }

    public static void SetScreenEffects(bool enabled)
    {
        ScreenEffects = enabled;
        PlayerPrefs.SetInt(EffectsKey, enabled ? 1 : 0);
        ApplyScreenEffects();
        Notify();
    }

    public static void SetScreenShake(bool enabled)
    {
        ScreenShake = enabled;
        PlayerPrefs.SetInt(ShakeKey, enabled ? 1 : 0);
        Notify();
    }

    public static Vector2Int Resolution => chosenResolution.x > 0 ? chosenResolution : new Vector2Int(Screen.width, Screen.height);

    public static void SetResolution(int width, int height)
    {
        chosenResolution = new Vector2Int(width, height);
        Screen.SetResolution(width, height, Screen.fullScreenMode);
        PlayerPrefs.SetInt(WidthKey, width);
        PlayerPrefs.SetInt(HeightKey, height);
        Notify();
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyScreenEffects();
    }

    private static void ApplyScreenEffects()
    {
        foreach (Volume volume in UnityEngine.Object.FindObjectsByType<Volume>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            volume.weight = ScreenEffects ? 1f : 0f;
        }
    }

    private static void Notify()
    {
        PlayerPrefs.Save();
        Changed?.Invoke();
    }
}
