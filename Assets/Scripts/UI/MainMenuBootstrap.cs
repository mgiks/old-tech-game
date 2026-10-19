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

        Init init = Object.FindAnyObjectByType<Init>(FindObjectsInactive.Include);
        if (init != null)
        {
            init.enabled = false;
        }

        Entered = true;

        if (active.name == "ViewManager")
        {
            ViewManager viewManager = Object.FindAnyObjectByType<ViewManager>();
            if (viewManager != null)
            {
                viewManager.LoadScene(Taskbar.MainMenuScene);
                return;
            }
        }

        SceneManager.LoadScene(Taskbar.MainMenuScene);
    }
}
