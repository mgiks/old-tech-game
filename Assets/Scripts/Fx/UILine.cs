using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class UILine : MaskableGraphic
{
    [SerializeField] private Vector2 from;
    [SerializeField] private Vector2 to;
    [SerializeField] private float thickness = 22f;
    [SerializeField] private float uvUnitLength = 60f;
    [SerializeField, Range(0f, 1f)] private float progress = 1f;

    public float Progress
    {
        get => progress;
        set
        {
            float clamped = Mathf.Clamp01(value);
            if (!Mathf.Approximately(clamped, progress))
            {
                progress = clamped;
                SetVerticesDirty();
            }
        }
    }

    public void Set(Vector2 start, Vector2 end, float lineThickness)
    {
        from = start;
        to = end;
        thickness = lineThickness;

        rectTransform.anchorMin = rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = (start + end) * 0.5f;
        rectTransform.sizeDelta = new Vector2(Mathf.Abs(end.x - start.x), Mathf.Abs(end.y - start.y)) + Vector2.one * thickness;

        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        Vector2 center = (from + to) * 0.5f;
        Vector2 start = from - center;
        Vector2 end = Vector2.Lerp(from, to, progress) - center;
        Vector2 direction = end - start;
        float length = direction.magnitude;
        if (length < 0.01f)
        {
            return;
        }

        Vector2 normal = new Vector2(-direction.y, direction.x) / length * (thickness * 0.5f);
        float u = length / uvUnitLength;

        UIVertex vertex = UIVertex.simpleVert;
        vertex.color = color;

        vertex.position = start - normal;
        vertex.uv0 = new Vector4(0f, 0f);
        vh.AddVert(vertex);

        vertex.position = start + normal;
        vertex.uv0 = new Vector4(0f, 1f);
        vh.AddVert(vertex);

        vertex.position = end + normal;
        vertex.uv0 = new Vector4(u, 1f);
        vh.AddVert(vertex);

        vertex.position = end - normal;
        vertex.uv0 = new Vector4(u, 0f);
        vh.AddVert(vertex);

        vh.AddTriangle(0, 1, 2);
        vh.AddTriangle(2, 3, 0);
    }
}
