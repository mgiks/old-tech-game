using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillTreeScreen : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI mbDisplay;
    [SerializeField] private Button startRunButton;
    [SerializeField] private string runSceneName = "Main";
    [SerializeField] private CrtPowerOn crt;
    [SerializeField] private float mbCountSpeed = 12f;
    [SerializeField] private float mbPunchScale = 1.35f;

    private float shownMB;
    private float mbPunch;

    void Awake()
    {
        startRunButton.onClick.AddListener(StartRun);
        ArtBindings.StyleXpButton(startRunButton);
        HideGlow(startRunButton.transform);
        shownMB = GameProgress.MB;
        mbDisplay.enableWordWrapping = true;
        mbDisplay.overflowMode = TextOverflowModes.Overflow;
    }

    void OnEnable()
    {
        GameProgress.Changed += OnProgressChanged;
        UpdateDisplay();
    }

    void OnDisable()
    {
        GameProgress.Changed -= OnProgressChanged;
    }

    void Update()
    {
        float dt = Time.unscaledDeltaTime;
        shownMB = Mathf.Lerp(shownMB, GameProgress.MB, 1f - Mathf.Exp(-mbCountSpeed * dt));
        if (Mathf.Abs(shownMB - GameProgress.MB) < 0.5f)
        {
            shownMB = GameProgress.MB;
        }

        mbPunch *= Mathf.Exp(-8f * dt);
        UpdateDisplay();
    }

    private static void HideGlow(Transform button)
    {
        Transform parent = button.parent;
        if (parent == null)
        {
            return;
        }

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child.name.Contains("Glow"))
            {
                child.gameObject.SetActive(false);
            }
        }
    }

    private void OnProgressChanged()
    {
        mbPunch = 1f;
    }

    private void StartRun()
    {
        GameAudio.PlayMouseClick();
        startRunButton.interactable = false;

        if (crt != null)
        {
            crt.PlayOff(SwitchToRun);
        }
        else
        {
            SwitchToRun();
        }
    }

    private void SwitchToRun()
    {
        ViewManager.Switch(gameObject.scene.name, runSceneName);
    }

    private void UpdateDisplay()
    {
        mbDisplay.text = $"MB: {NumberFormat.Mb(GameProgress.MB)}\n{Loc.F("tree.counter", GameProgress.TotalPurchases)}\n{Loc.F("tree.file_reward", NumberFormat.Mb(GameplayQuery.StandardReward()))}";
        if (GameProgress.TotalPurchases >= ProgressionConfig.TargetPurchases)
        {
            mbDisplay.text += "\n" + Loc.T("tree.evolved");
        }

        mbDisplay.rectTransform.localScale = Vector3.one * Mathf.Lerp(1f, mbPunchScale, mbPunch);
        mbDisplay.color = Color.Lerp(new Color(0.3f, 1f, 0.5f, 1f), new Color(1f, 0.4f, 0.45f, 1f), mbPunch);
    }
}
