using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CrtPowerOn : MonoBehaviour
{
    [SerializeField] private Image blackout;
    [SerializeField] private RectTransform beam;
    [SerializeField] private Graphic beamGraphic;
    [SerializeField] private ScreenShake shake;
    [SerializeField] private PostFxPunch postFx;
    [SerializeField] private float lineDuration = 0.22f;
    [SerializeField] private float openDuration = 0.35f;
    [SerializeField] private float closeDuration = 0.25f;
    [SerializeField] private float collapseDuration = 0.15f;

    void Start()
    {
        StartCoroutine(PowerOnRoutine());
    }

    public void PlayOff(Action onFinished)
    {
        gameObject.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(PowerOffRoutine(onFinished));
    }

    private IEnumerator PowerOnRoutine()
    {
        SetBlackout(1f);
        beamGraphic.color = Color.white;
        beam.localScale = new Vector3(0f, 1f, 1f);

        yield return Animate(lineDuration, k => beam.localScale = new Vector3(EaseOutCubic(k), 1f, 1f));

        if (postFx != null)
        {
            postFx.Punch(0.8f);
        }

        if (shake != null)
        {
            shake.Add(10f);
        }

        float fullScale = GetFullScreenBeamScale();
        yield return Animate(openDuration, k =>
        {
            float eased = EaseOutCubic(k);
            beam.localScale = new Vector3(1f, Mathf.Lerp(1f, fullScale, eased), 1f);
            beamGraphic.color = new Color(1f, 1f, 1f, 1f - eased);
            SetBlackout(1f - eased);
        });

        gameObject.SetActive(false);
    }

    private IEnumerator PowerOffRoutine(Action onFinished)
    {
        float fullScale = GetFullScreenBeamScale();
        beam.localScale = new Vector3(1f, fullScale, 1f);

        if (postFx != null)
        {
            postFx.Punch(1f);
        }

        yield return Animate(closeDuration, k =>
        {
            float eased = EaseInCubic(k);
            beam.localScale = new Vector3(1f, Mathf.Lerp(fullScale, 1f, eased), 1f);
            beamGraphic.color = new Color(1f, 1f, 1f, eased);
            SetBlackout(eased);
        });

        yield return Animate(collapseDuration, k => beam.localScale = new Vector3(1f - EaseInCubic(k), 1f, 1f));

        onFinished?.Invoke();
    }

    private static IEnumerator Animate(float duration, Action<float> step)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            step(Mathf.Clamp01(elapsed / duration));
            yield return null;
        }

        step(1f);
    }

    private float GetFullScreenBeamScale()
    {
        float screenHeight = ((RectTransform)transform).rect.height;
        return screenHeight / Mathf.Max(beam.rect.height, 1f);
    }

    private void SetBlackout(float alpha)
    {
        blackout.color = new Color(0f, 0f, 0f, alpha);
    }

    private static float EaseOutCubic(float t) => 1f - Mathf.Pow(1f - t, 3f);

    private static float EaseInCubic(float t) => t * t * t;
}
