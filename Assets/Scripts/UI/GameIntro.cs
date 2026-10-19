using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class GameIntro : MonoBehaviour
{
    [SerializeField] private Virus virus;
    [SerializeField] private EatFeedback feedback;
    [SerializeField] private DoubleClickTarget folder;
    [SerializeField] private RectTransform window;
    [SerializeField] private DoubleClickTarget file;
    [SerializeField] private TextMeshProUGUI hint;
    [SerializeField] private float revealDuration = 0.7f;
    [SerializeField] private float feedHintDuration = 3f;
    [SerializeField] private Color revealColor = new(0.45f, 1f, 0.55f, 1f);

    public bool ShouldPlay => !GameProgress.IntroDone;

    private Action onFinished;
    private string hintKey;
    private Vector3 virusScale;
    private SpriteRenderer virusRenderer;
    private Collider2D virusCollider;

    void Awake()
    {
        virusRenderer = virus.GetComponent<SpriteRenderer>();
        virusCollider = virus.GetComponent<Collider2D>();
        folder.DoubleClicked += OpenFolder;
        file.DoubleClicked += RunFile;
        gameObject.SetActive(false);
    }

    public void Play(Action finished)
    {
        onFinished = finished;
        virusScale = virus.transform.localScale;
        virusRenderer.enabled = false;
        virusCollider.enabled = false;

        gameObject.SetActive(true);
        folder.gameObject.SetActive(true);
        window.gameObject.SetActive(false);
        SetHint("intro.open_folder");
    }

    private void OpenFolder()
    {
        folder.SetSelected(false);
        window.gameObject.SetActive(true);
        StartCoroutine(PopWindow());
        SetHint("intro.open_file");
    }

    private void RunFile()
    {
        file.SetSelected(false);
        StartCoroutine(RevealGlitch());
    }

    private IEnumerator PopWindow()
    {
        for (float t = 0f; t < 0.18f; t += Time.deltaTime)
        {
            window.localScale = Vector3.one * Mathf.Lerp(0.6f, 1f, 1f - Mathf.Pow(1f - t / 0.18f, 3f));
            yield return null;
        }

        window.localScale = Vector3.one;
    }

    private IEnumerator RevealGlitch()
    {
        SetHint(null);
        Vector2 basePosition = window.anchoredPosition;
        for (float t = 0f; t < 0.35f; t += Time.deltaTime)
        {
            window.anchoredPosition = basePosition + UnityEngine.Random.insideUnitCircle * 18f;
            window.localScale = new Vector3(1f + UnityEngine.Random.Range(-0.08f, 0.08f), 1f + UnityEngine.Random.Range(-0.08f, 0.08f), 1f);
            yield return null;
        }

        window.anchoredPosition = basePosition;
        window.gameObject.SetActive(false);
        folder.gameObject.SetActive(false);

        virusRenderer.enabled = true;
        virusCollider.enabled = true;
        Vector2 center = virus.GetAbsorbPoint();
        feedback.Burst(center, revealColor, 40, 9f);
        feedback.Ring(center, revealColor, 6f);
        feedback.Shake(0.35f);

        for (float t = 0f; t < revealDuration; t += Time.deltaTime)
        {
            float k = t / revealDuration;
            float elastic = 1f + Mathf.Sin(k * Mathf.PI * 3f) * (1f - k) * 0.4f;
            virus.transform.localScale = virusScale * (k * elastic);
            yield return null;
        }

        virus.transform.localScale = virusScale;
        GameProgress.IntroDone = true;
        onFinished?.Invoke();

        SetHint("intro.feed");
        yield return new WaitForSeconds(feedHintDuration);
        gameObject.SetActive(false);
    }

    void OnEnable()
    {
        GameSettings.Changed += RefreshHint;
    }

    void OnDisable()
    {
        GameSettings.Changed -= RefreshHint;
    }

    private void SetHint(string key)
    {
        hintKey = key;
        RefreshHint();
    }

    private void RefreshHint()
    {
        bool visible = !string.IsNullOrEmpty(hintKey);
        hint.text = visible ? Loc.T(hintKey) : string.Empty;
        hint.gameObject.SetActive(visible);
    }
}
