using UnityEngine;

public class Init : MonoBehaviour
{
    void Start()
    {
        gameObject.GetComponent<ViewManager>().LoadScene("Main");
    }
}