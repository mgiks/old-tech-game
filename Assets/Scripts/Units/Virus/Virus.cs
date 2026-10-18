using UnityEngine;

public class Virus : MonoBehaviour
{
    [SerializeField] private ScoreManager scoreManager;

    [SerializeField] private float scaleMultplicator = 1.1f;
    [SerializeField] private bool destroyDesktopIcons = true;
    [SerializeField] private bool scale = true;
    [SerializeField] private bool increaseScore = true;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.gameObject.CompareTag("Desktop Icon"))
        {
            return;
        }

        if (scale)
        {
            Scale(scaleMultplicator);
        }

        if (destroyDesktopIcons)
        {
            Destroy(collision.gameObject);
        }

        if (increaseScore)
        {
            scoreManager.IncrementScore();
        }
    }

    public Bounds GetVisualBounds()
    {
        return GetComponent<SpriteRenderer>().bounds;
    }

    private void Scale(float multiplier)
    {
        Vector3 scale = gameObject.transform.lossyScale;
        float xScale = scale.x;
        float yScale = scale.y;
        float zScale = scale.z;
        transform.localScale = new Vector3(xScale * multiplier, yScale * multiplier, zScale);
    }
}