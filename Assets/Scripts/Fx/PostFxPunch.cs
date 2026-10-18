using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PostFxPunch : MonoBehaviour
{
    [SerializeField] private Volume volume;
    [SerializeField] private float baseBloom = 1.1f;
    [SerializeField] private float punchBloom = 3.5f;
    [SerializeField] private float baseChromatic = 0.12f;
    [SerializeField] private float punchChromatic = 0.9f;
    [SerializeField] private float punchLensDistortion = -0.25f;
    [SerializeField] private float decay = 4f;

    [Header("Idle glitches")]
    [SerializeField] private Vector2 idleGlitchInterval = new(4f, 9f);
    [SerializeField] private float idleGlitchStrength = 0.35f;

    private Bloom bloom;
    private ChromaticAberration chromatic;
    private LensDistortion lens;
    private float punch;
    private float nextIdleGlitch;

    void Awake()
    {
        VolumeProfile profile = volume.profile;
        profile.TryGet(out bloom);
        profile.TryGet(out chromatic);
        profile.TryGet(out lens);
        ScheduleIdleGlitch();
    }

    public void Punch(float amount)
    {
        punch = Mathf.Clamp01(punch + amount);
    }

    void Update()
    {
        float now = Time.unscaledTime;
        if (now >= nextIdleGlitch)
        {
            Punch(idleGlitchStrength);
            ScheduleIdleGlitch();
        }

        punch *= Mathf.Exp(-decay * Time.unscaledDeltaTime);
        float flicker = 1f + (Mathf.PerlinNoise(now * 2.5f, 0.37f) - 0.5f) * 0.15f;

        if (bloom != null)
        {
            bloom.intensity.value = (baseBloom + punchBloom * punch) * flicker;
        }

        if (chromatic != null)
        {
            chromatic.intensity.value = Mathf.Clamp01(baseChromatic + punchChromatic * punch);
        }

        if (lens != null)
        {
            lens.intensity.value = punchLensDistortion * punch;
        }
    }

    private void ScheduleIdleGlitch()
    {
        nextIdleGlitch = Time.unscaledTime + Random.Range(idleGlitchInterval.x, idleGlitchInterval.y);
    }
}
