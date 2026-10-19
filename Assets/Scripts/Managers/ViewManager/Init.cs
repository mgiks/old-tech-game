using UnityEngine;

public class Init : MonoBehaviour
{
    void Start()
    {
        if (!MainMenuBootstrap.TryEnter())
        {
            return;
        }

        gameObject.GetComponent<ViewManager>().LoadScene(Taskbar.MainMenuScene);
    }
}