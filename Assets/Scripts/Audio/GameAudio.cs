using System.Collections.Generic;
using UnityEngine;

public static class GameAudio
{
    private sealed class Channel
    {
        public AudioSource source;
        public float lastPlayTime = -100f;
    }

    private static readonly Dictionary<GameSfx, Channel> channels = new();

    private static GameObject root;
    private static GameAudioLibrary library;
    private static bool missingLogged;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetSession()
    {
        channels.Clear();
        root = null;
        library = null;
        missingLogged = false;
    }

    public static void PlayMouseClick()
    {
        Play(GameSfx.MouseClick);
    }

    public static void Play(GameSfx sfx)
    {
        AudioClip clip;
        if (!TryGetClip(sfx, out clip))
        {
            return;
        }

        Channel channel = GetChannel(sfx);
        float now = Time.unscaledTime;
        if (now - channel.lastPlayTime < MinInterval(sfx))
        {
            return;
        }

        if (!Retrigger(sfx) && channel.source.isPlaying)
        {
            return;
        }

        channel.lastPlayTime = now;
        channel.source.loop = false;
        channel.source.volume = Volume(sfx);
        channel.source.clip = clip;
        channel.source.Play();
    }

    public static void StartLoop(GameSfx sfx)
    {
        AudioClip clip;
        if (!TryGetClip(sfx, out clip))
        {
            return;
        }

        Channel channel = GetChannel(sfx);
        if (channel.source.isPlaying && channel.source.loop && channel.source.clip == clip)
        {
            return;
        }

        channel.lastPlayTime = Time.unscaledTime;
        channel.source.loop = true;
        channel.source.volume = Volume(sfx);
        channel.source.clip = clip;
        channel.source.Play();
    }

    public static void Stop(GameSfx sfx)
    {
        Channel channel;
        if (!channels.TryGetValue(sfx, out channel))
        {
            return;
        }

        channel.source.loop = false;
        channel.source.Stop();
    }

    private static bool TryGetClip(GameSfx sfx, out AudioClip clip)
    {
        clip = null;
        if (library == null)
        {
            library = Resources.Load<GameAudioLibrary>("GameAudioLibrary");
        }

        if (library == null)
        {
            if (!missingLogged)
            {
                missingLogged = true;
                Debug.LogWarning("GameAudioLibrary is missing from Resources.");
            }

            return false;
        }

        clip = library.Get(sfx);
        if (clip != null && clip.loadState == AudioDataLoadState.Unloaded)
        {
            clip.LoadAudioData();
        }

        return clip != null;
    }

    private static Channel GetChannel(GameSfx sfx)
    {
        Channel channel;
        if (channels.TryGetValue(sfx, out channel))
        {
            return channel;
        }

        if (root == null)
        {
            root = new GameObject("Game Audio");
            Object.DontDestroyOnLoad(root);
        }

        AudioSource source = root.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 0f;
        source.dopplerLevel = 0f;
        source.priority = sfx == GameSfx.VirusElectric ? 200 : 64;
        source.ignoreListenerPause = sfx == GameSfx.MouseClick;
        channel = new Channel { source = source };
        channels.Add(sfx, channel);
        return channel;
    }

    private static float Volume(GameSfx sfx)
    {
        switch (sfx)
        {
            case GameSfx.MouseClick: return 0.25f;
            case GameSfx.Keyboard: return 0.35f;
            case GameSfx.Startup: return 0.4f;
            case GameSfx.CriticalError: return 0.5f;
            case GameSfx.VirusElectric: return 0.16f;
            case GameSfx.Icq: return 0.4f;
            case GameSfx.VirusEating: return 0.34f;
            case GameSfx.LevelUp: return 0.45f;
            case GameSfx.HardwareInsert: return 0.45f;
            default: return 0.4f;
        }
    }

    private static float MinInterval(GameSfx sfx)
    {
        switch (sfx)
        {
            case GameSfx.MouseClick: return 0.06f;
            case GameSfx.VirusEating: return 0.18f;
            case GameSfx.LevelUp: return 0.15f;
            default: return 0f;
        }
    }

    private static bool Retrigger(GameSfx sfx)
    {
        switch (sfx)
        {
            case GameSfx.Keyboard:
            case GameSfx.Startup:
            case GameSfx.Icq:
                return false;
            default:
                return true;
        }
    }
}
