using UnityEngine;
using UnityEngine.SceneManagement;

public static class MainMenuBootstrap
{
    public static bool Entered { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetSession()
    {
        Entered = false;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnterMenu()
    {
        if (Entered)
        {
            return;
        }

        Scene active = SceneManager.GetActiveScene();
        if (active.name == Taskbar.MainMenuScene)
        {
            Entered = true;
            return;
        }

        if (active.name == "ViewManager")
        {
            return;
        }

        GameObject boot = new GameObject("Main Menu Boot");
        Object.DontDestroyOnLoad(boot);
        boot.AddComponent<MainMenuBoot>();
    }

    public static bool TryEnter()
    {
        if (Entered)
        {
            return false;
        }

        Entered = true;
        return true;
    }
}

public class MainMenuBoot : MonoBehaviour
{
    void Start()
    {
        if (MainMenuBootstrap.TryEnter())
        {
            SceneManager.LoadScene(Taskbar.MainMenuScene);
        }

        Destroy(gameObject);
    }
}
