using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ViewManager : MonoBehaviour
{
    public static void Switch(string fromSceneName, string toSceneName)
    {
        ViewManager viewManager = null;
        foreach (ViewManager candidate in FindObjectsByType<ViewManager>(FindObjectsSortMode.None))
        {
            if (candidate.gameObject.scene.name != fromSceneName)
            {
                viewManager = candidate;
                break;
            }
        }

        if (viewManager != null)
        {
            viewManager.SwitchScene(fromSceneName, toSceneName);
        }
        else
        {
            SceneManager.LoadScene(toSceneName);
        }
    }

    public void LoadScene(string sceneName)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        operation.completed += _ => SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
    }

    public void UnloadScene(string sceneName)
    {
        SceneManager.UnloadSceneAsync(sceneName);
    }

    public void SwitchScene(string fromSceneName, string toSceneName)
    {
        StartCoroutine(SwitchSceneRoutine(fromSceneName, toSceneName));
    }

    private IEnumerator SwitchSceneRoutine(string fromSceneName, string toSceneName)
    {
        yield return SceneManager.UnloadSceneAsync(fromSceneName);
        LoadScene(toSceneName);
    }
}
