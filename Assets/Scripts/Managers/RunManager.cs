using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class RunManager : MonoBehaviour
{
    [SerializeField] private SkillTreeDefinition skillTree;
    [SerializeField] private Virus virus;
    [SerializeField] private DesktopIconManager iconManager;
    [SerializeField] private RunEndScreen endScreen;
    [SerializeField] private GameIntro intro;
    [SerializeField] private EatFeedback feedback;
    [SerializeField] private GameEnding ending;
    [SerializeField] private string skillTreeSceneName = "Skill Tree";

    [Header("Timer")]
    [SerializeField] private float baseRunDurationInSeconds = 8f;
    [SerializeField] private Image timerFill;
    [SerializeField] private TextMeshProUGUI timerDisplay;
    [SerializeField] private Color timerFullColor = new(0.3f, 1f, 0.5f, 1f);
    [SerializeField] private Color timerEmptyColor = new(1f, 0.25f, 0.25f, 1f);
    [SerializeField] private float timerWarningSeconds = 3f;

    [Header("Computers")]
    [SerializeField, Min(1)] private int totalComputers = 11;
    [SerializeField, Min(1)] private int baseFilesPerComputer = 8;
    [SerializeField, Min(1f)] private float filesPerComputerGrowth = 1.25f;
    [SerializeField] private TextMeshProUGUI computerDisplay;
    [SerializeField] private Color serverDeniedColor = new(1f, 0.15f, 0.2f, 1f);

    [Header("Jump to next computer")]
    [SerializeField] private float jumpDuration = 0.8f;
    [SerializeField] private float jumpScaleMultiplier = 6f;
    [SerializeField] private Image jumpFlash;

    private float runDuration;
    private float timeLeft;
    private int filesAtComputerStart;
    private bool jumping;
    private bool started;
    private bool finished;

    void OnEnable()
    {
        GameSettings.Changed += UpdateDisplay;
    }

    void OnDisable()
    {
        GameSettings.Changed -= UpdateDisplay;
    }

    void Start()
    {
        Time.timeScale = 1f;
        RunStats.Begin(totalComputers);
        runDuration = baseRunDurationInSeconds + skillTree.GetStat(SkillStat.RunTime);
        timeLeft = runDuration;

        if (jumpFlash != null)
        {
            jumpFlash.color = new Color(1f, 1f, 1f, 0f);
        }

        UpdateDisplay();

        if (intro != null && intro.ShouldPlay)
        {
            intro.Play(BeginRun);
        }
        else
        {
            BeginRun();
        }
    }

    private void BeginRun()
    {
        started = true;
        iconManager.StartSpawning();
    }

    void OnDestroy()
    {
        Time.timeScale = 1f;
    }

    void Update()
    {
        if (finished || !started || GamePause.IsPaused)
        {
            return;
        }

        CountClicks();

        if (!jumping && RunStats.FilesInfected - filesAtComputerStart >= GetFilesForCurrentComputer())
        {
            StartCoroutine(JumpToNextComputer());
        }

        if (!jumping)
        {
            timeLeft -= Time.deltaTime;
            if (timeLeft <= 0f)
            {
                timeLeft = 0f;
                Finish();
            }
        }

        UpdateDisplay();
    }

    private int GetFilesForCurrentComputer()
    {
        return Mathf.CeilToInt(baseFilesPerComputer * Mathf.Pow(filesPerComputerGrowth, RunStats.ComputersInfected));
    }

    private static void CountClicks()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null)
        {
            return;
        }

        if (mouse.leftButton.wasPressedThisFrame || mouse.rightButton.wasPressedThisFrame || mouse.middleButton.wasPressedThisFrame)
        {
            RunStats.AddClick();
        }
    }

    private IEnumerator JumpToNextComputer()
    {
        jumping = true;
        iconManager.ClearDesktop();

        Vector3 startScale = virus.transform.localScale;
        Vector3 endScale = startScale * jumpScaleMultiplier;
        for (float t = 0f; t < jumpDuration; t += Time.deltaTime)
        {
            float k = t / jumpDuration;
            virus.transform.localScale = Vector3.Lerp(startScale, endScale, k * k);
            SetFlash(k);
            yield return null;
        }

        RunStats.AddComputer();
        Vector2 center = virus.GetAbsorbPoint();
        if (feedback != null)
        {
            feedback.Burst(center, new Color(0.45f, 1f, 0.55f, 1f), 50, 12f);
            feedback.Ring(center, Color.white, 14f);
            feedback.Shake(0.4f);
        }

        if (RunStats.ComputersInfected >= totalComputers)
        {
            SetFlash(0f);
            if (ending != null)
            {
                finished = true;
                iconManager.StopSpawning();
                RunStats.Complete();
                GameProgress.GameCompleted = true;
                ending.Play();
            }
            else
            {
                Finish();
            }

            yield break;
        }

        if (IsServerNext() && skillTree.GetStat(SkillStat.ServerAccess) <= 0f)
        {
            yield return ServerDenied(startScale);
            Finish();
            yield break;
        }

        float bonusTime = skillTree.GetStat(SkillStat.TimePerComputer);
        if (bonusTime > 0f)
        {
            timeLeft += bonusTime;
            runDuration = Mathf.Max(runDuration, timeLeft);
            if (feedback != null)
            {
                feedback.ShowPopup(center + Vector2.down * 1.5f, Loc.F("popup.time", bonusTime), new Color(0.45f, 0.9f, 1f, 1f), 1.4f);
            }
        }

        virus.ResetGrowth();
        iconManager.SpawnStartingIcons();
        filesAtComputerStart = RunStats.FilesInfected;

        for (float t = 0f; t < jumpDuration * 0.5f; t += Time.deltaTime)
        {
            SetFlash(1f - t / (jumpDuration * 0.5f));
            yield return null;
        }

        SetFlash(0f);
        jumping = false;
    }

    private bool IsServerNext()
    {
        return RunStats.ComputersInfected == totalComputers - 1;
    }

    private IEnumerator ServerDenied(Vector3 virusScale)
    {
        if (feedback != null)
        {
            Vector2 center = virus.GetAbsorbPoint();
            feedback.ShowPopup(center, Loc.T("popup.server_locked"), serverDeniedColor, 1.8f);
            feedback.ShowPopup(center + Vector2.down * 1.2f, Loc.T("popup.need_worm"), Color.white, 1f);
            feedback.Burst(center, serverDeniedColor, 40, 10f);
            feedback.Shake(0.5f);
        }

        Vector3 bigScale = virus.transform.localScale;
        float duration = 1.4f;
        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            float k = t / duration;
            float bounce = Mathf.Abs(Mathf.Sin(k * Mathf.PI * 4f)) * (1f - k) * 0.25f;
            virus.transform.localScale = Vector3.Lerp(bigScale, virusScale, 1f - Mathf.Pow(1f - Mathf.Min(1f, k * 3f), 3f)) * (1f + bounce);
            if (jumpFlash != null)
            {
                Color c = serverDeniedColor;
                c.a = (1f - k) * (0.5f + 0.5f * Mathf.Sin(k * 40f));
                jumpFlash.color = c;
            }

            yield return null;
        }

        virus.transform.localScale = virusScale;
        SetFlash(0f);
    }

    private void SetFlash(float alpha)
    {
        if (jumpFlash != null)
        {
            jumpFlash.color = new Color(1f, 1f, 1f, alpha);
        }
    }

    private void Finish()
    {
        finished = true;
        RunStats.Complete();
        UpdateDisplay();
        Time.timeScale = 0f;
        endScreen.Show(GoToSkillTree, StartNextRun);
    }

    private void GoToSkillTree()
    {
        Time.timeScale = 1f;
        ViewManager.Switch(gameObject.scene.name, skillTreeSceneName);
    }

    private void StartNextRun()
    {
        Time.timeScale = 1f;
        ViewManager.Switch(gameObject.scene.name, gameObject.scene.name);
    }

    private void UpdateDisplay()
    {
        float fraction = runDuration > 0f ? timeLeft / runDuration : 0f;
        Color color = Color.Lerp(timerEmptyColor, timerFullColor, fraction);
        if (timeLeft <= timerWarningSeconds && !finished)
        {
            color = Color.Lerp(color, Color.white, 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 18f));
        }

        timerFill.fillAmount = fraction;
        timerFill.color = color;

        if (timerDisplay != null)
        {
            int seconds = Mathf.CeilToInt(timeLeft);
            timerDisplay.text = $"{seconds / 60}:{seconds % 60:00}";
        }

        if (computerDisplay != null)
        {
            int current = Mathf.Min(RunStats.ComputersInfected + 1, totalComputers);
            int eaten = Mathf.Min(RunStats.FilesInfected - filesAtComputerStart, GetFilesForCurrentComputer());
            string key = current == totalComputers ? "hud.server" : "hud.pc";
            computerDisplay.text = Loc.F(key, current, totalComputers, eaten, GetFilesForCurrentComputer());
        }
    }
}
