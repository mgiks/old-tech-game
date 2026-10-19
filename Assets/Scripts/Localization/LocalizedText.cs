using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class LocalizedText : MonoBehaviour
{
    [SerializeField] private string key;

    private TMP_Text text;
    private GlitchText glitch;

    public string Key
    {
        get => key;
        set
        {
            key = value;
            Apply();
        }
    }

    void Awake()
    {
        text = GetComponent<TMP_Text>();
        glitch = GetComponent<GlitchText>();
    }

    void OnEnable()
    {
        GameSettings.Changed += Apply;
        Apply();
    }

    void OnDisable()
    {
        GameSettings.Changed -= Apply;
    }

    private void Apply()
    {
        if (text == null || string.IsNullOrEmpty(key))
        {
            return;
        }

        string value = Loc.T(key);
        if (glitch != null)
        {
            glitch.SetText(value);
        }
        else
        {
            text.text = value;
        }
    }
}
