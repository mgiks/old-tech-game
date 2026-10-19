using UnityEngine;

public class Init : MonoBehaviour
{
    void Start()
    {
        if (MainMenuBootstrap.Entered)
        {
            return;
        }

        gameObject.GetComponent<ViewManager>().LoadScene(Taskbar.MainMenuScene);
    }
}