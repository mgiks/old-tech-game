using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Graphic))]
public class SkillTreeBackground : MonoBehaviour
{
    private static readonly int CenterId = Shader.PropertyToID("_Center");
    private static readonly int ZoomId = Shader.PropertyToID("_Zoom");
    private static readonly int FxTimeId = Shader.PropertyToID("_FxTime");
    private static readonly int ShockCenterId = Shader.PropertyToID("_ShockCenter");
    private static readonly int ShockTimeId = Shader.PropertyToID("_ShockTime");
    private static readonly int ShockColorId = Shader.PropertyToID("_ShockColor");

    [SerializeField] private RectTransform treeContent;
    [SerializeField] private float shockColorIntensity = 1.5f;

    private Material material;
    private Canvas canvas;

    void Awake()
    {
        Graphic graphic = GetComponent<Graphic>();
        material = new Material(graphic.material);
        graphic.material = material;
        canvas = GetComponentInParent<Canvas>();
        material.SetFloat(ShockTimeId, -100f);
    }

    void OnDestroy()
    {
        Destroy(material);
    }

    void Update()
    {
        material.SetVector(CenterId, ScreenToShaderSpace(WorldToScreen(treeContent.position)));
        material.SetFloat(ZoomId, treeContent.localScale.x);
        material.SetFloat(FxTimeId, Time.unscaledTime);
    }

    public void Shockwave(Vector3 worldPosition, Color color)
    {
        material.SetVector(ShockCenterId, ScreenToShaderSpace(WorldToScreen(worldPosition)));
        material.SetFloat(ShockTimeId, Time.unscaledTime);
        material.SetColor(ShockColorId, color * shockColorIntensity);
    }

    private Vector2 WorldToScreen(Vector3 worldPosition)
    {
        Camera uiCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        return RectTransformUtility.WorldToScreenPoint(uiCamera, worldPosition);
    }

    private static Vector2 ScreenToShaderSpace(Vector2 screenPoint)
    {
        float aspect = Screen.width / (float)Screen.height;
        return new Vector2((screenPoint.x / Screen.width - 0.5f) * aspect, screenPoint.y / Screen.height - 0.5f);
    }
}
