using UnityEngine;

public class Virus : MonoBehaviour
{
    void Start()
    {

    }

    void Update()
    {

    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Desktop Icon"))
        {
            Debug.Log("Something happened");
        }
    }
}