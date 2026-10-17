using UnityEngine;

public class Virus : MonoBehaviour
{
    [SerializeField] private float scaleMultplicator = 3;

    void Start()
    {

    }

    void Update()
    {

    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.gameObject.CompareTag("Desktop Icon"))
        {
            return;
        }

        Vector3 scale = gameObject.transform.lossyScale;

        float xScale = scale.x;
        float yScale = scale.y;
        float zScale = scale.z;

        transform.localScale = new Vector3(xScale * scaleMultplicator, yScale * scaleMultplicator, zScale);
    }
}