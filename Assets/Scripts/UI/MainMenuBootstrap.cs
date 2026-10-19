using UnityEngine;
using UnityEngine.SceneManagement;

public static class MainMenuBootstrap
{
    private static bool started;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetSession()
    {
        started = false;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnterMenu()
    {
        if (started)
        {
            return;
        }

        Scene active = SceneManager.GetActiveScene();
        if (active.name != "ViewManager")
        {
            return;
        }

        started = true;

        Init init = Object.FindAnyObjectByType<Init>();
        if (init != null)
        {
            init.enabled = false;
        }

        if (SceneManager.GetSceneByName(Taskbar.MainMenuScene).isLoaded)
        {
            return;
        }

        ViewManager viewManager = Object.FindAnyObjectByType<ViewManager>();
        if (viewManager != null)
        {
            viewManager.LoadScene(Taskbar.MainMenuScene);
        }
        else
        {
            SceneManager.LoadScene(Taskbar.MainMenuScene);
        }
    }
}
