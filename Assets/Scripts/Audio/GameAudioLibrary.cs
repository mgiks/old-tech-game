using UnityEngine;

public class GameAudioLibrary : ScriptableObject
{
    public AudioClip mouseClick;
    public AudioClip keyboard;
    public AudioClip startup;
    public AudioClip criticalError;
    public AudioClip virusElectric;
    public AudioClip icq;
    public AudioClip virusEating;
    public AudioClip levelUp;
    public AudioClip hardwareInsert;

    public AudioClip Get(GameSfx sfx)
    {
        switch (sfx)
        {
            case GameSfx.MouseClick: return mouseClick;
            case GameSfx.Keyboard: return keyboard;
            case GameSfx.Startup: return startup;
            case GameSfx.CriticalError: return criticalError;
            case GameSfx.VirusElectric: return virusElectric;
            case GameSfx.Icq: return icq;
            case GameSfx.VirusEating: return virusEating;
            case GameSfx.LevelUp: return levelUp;
            case GameSfx.HardwareInsert: return hardwareInsert;
            default: return null;
        }
    }
}
