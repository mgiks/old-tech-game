using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class OpenStartMenuButton : MonoBehaviour
{
    void Awake()
    {
        GetComponent<Button>().onClick.AddListener(Taskbar.OpenMenu);
    }
}
