using System;
using UnityEngine;

public class Virus : MonoBehaviour
{
    [SerializeField] private ScoreManager scoreManager;

    [SerializeField] private float scaleMultplicator = 1.1f;
    [SerializeField, Min(1)] private int iconsPerGrowth = 3;
    [SerializeField] private float exponent = 1.1f;
    [SerializeField] private bool destroyDesktopIcons = true;
    [SerializeField] private bool scale = true;
    [SerializeField] private bool increaseScore = true;

    private int iconsSinceGrowth;
    private Vector3 initialScale;
    private float initialExponent;

    void Awake()
    {
        initialScale = transform.localScale;
        initialExponent = exponent;
    }

    public void ResetGrowth()
    {
        transform.localScale = initialScale;
        exponent = initialExponent;
        iconsSinceGrowth = 0;
    }

    public void Absorb(DesktopIcon icon)
    {
        if (!icon.TryConsume())
        {
            return;
        }

        if (scale)
        {
            iconsSinceGrowth++;
            if (iconsSinceGrowth >= Math.Pow(iconsPerGrowth, exponent))
            {
                iconsSinceGrowth = 0;
                Scale(scaleMultplicator);
                exponent += 2f;
            }
        }

        if (destroyDesktopIcons)
        {
            Destroy(icon.gameObject);
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

    public Vector2 GetAbsorbPoint()
    {
        return GetComponent<Collider2D>().bounds.center;
    }

    public bool ContainsPoint(Vector2 point)
    {
        return GetComponent<Collider2D>().OverlapPoint(point);
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
