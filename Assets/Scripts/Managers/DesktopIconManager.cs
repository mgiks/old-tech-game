using UnityEngine;

public class DesktopIconManager : MonoBehaviour
{
    [SerializeField] private GameObject icon;

    void Start()
    {
        Instantiate(icon, new Vector3(0, 0, 0), icon.transform.rotation);
    }
}