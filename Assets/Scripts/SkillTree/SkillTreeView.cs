using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SkillTreeView : MonoBehaviour, IDragHandler, IScrollHandler
{
    private class Line
    {
        public UILine graphic;
        public SkillTreeDefinition.Node from;
        public SkillTreeDefinition.Node to;
        public bool revealed;
        public float drawStart;
        public float drawDuration;
    }

    [SerializeField] private SkillTreeDefinition skillTree;
    [SerializeField] private RectTransform content;
    [SerializeField] private RectTransform linesRoot;
    [SerializeField] private RectTransform nodesRoot;
    [SerializeField] private SkillNodeView nodePrefab;
    [SerializeField] private SkillTooltip tooltip;
    [SerializeField] private SkillTreeFx fx;

    [Header("Lines")]
    [SerializeField] private float lineThickness = 22f;
    [SerializeField] private Material ownedLineMaterial;
    [SerializeField] private Material lockedLineMaterial;
    [SerializeField] private Color ownedLineColor = new(0.3f, 0.9f, 1f, 1f);
    [SerializeField] private Color lockedLineColor = new(0.35f, 0.4f, 0.6f, 0.5f);

    [Header("Animation")]
    [SerializeField] private float introDelay = 0.45f;
    [SerializeField] private float introSecondsPerPixel = 0.0006f;
    [SerializeField] private float revealDelay = 0.3f;
    [SerializeField] private float lineDrawDuration = 0.3f;

    [Header("Zoom")]
    [SerializeField] private float minZoom = 0.4f;
    [SerializeField] private float maxZoom = 2f;
    [SerializeField] private float zoomStep = 0.1f;

    private readonly Dictionary<SkillTreeDefinition.Node, SkillNodeView> nodeViews = new();
    private readonly List<Line> lines = new();
    private Canvas canvas;

    void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
        Build();
    }

    void OnEnable()
    {
        GameProgress.Changed += OnProgressChanged;
        GameSettings.Changed += OnSettingsChanged;
        Refresh(false);
    }

    void OnDisable()
    {
        GameProgress.Changed -= OnProgressChanged;
        GameSettings.Changed -= OnSettingsChanged;
    }

    private void OnSettingsChanged()
    {
        Refresh(false);
    }

    void Start()
    {
        PlayIntro();
    }

    void Update()
    {
        float now = Time.unscaledTime;
        foreach (Line line in lines)
        {
            if (!line.revealed)
            {
                continue;
            }

            float t = line.drawDuration <= 0f ? 1f : Mathf.Clamp01((now - line.drawStart) / line.drawDuration);
            line.graphic.Progress = 1f - Mathf.Pow(1f - t, 3f);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        content.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnScroll(PointerEventData eventData)
    {
        if (Mathf.Approximately(eventData.scrollDelta.y, 0f))
        {
            return;
        }

        Camera uiCamera = GetUiCamera();
        RectTransformUtility.ScreenPointToLocalPointInRectangle(content, eventData.position, uiCamera, out Vector2 before);

        float zoom = content.localScale.x * (1f + zoomStep * Mathf.Sign(eventData.scrollDelta.y));
        zoom = Mathf.Clamp(zoom, minZoom, maxZoom);
        content.localScale = new Vector3(zoom, zoom, 1f);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(content, eventData.position, uiCamera, out Vector2 after);
        content.anchoredPosition += (after - before) * zoom;
    }

    private void Build()
    {
        foreach (SkillTreeDefinition.Node node in skillTree.Nodes)
        {
            if (node.requires == null)
            {
                continue;
            }

            foreach (string requiredId in node.requires)
            {
                SkillTreeDefinition.Node required = skillTree.Find(requiredId);
                if (required != null)
                {
                    lines.Add(CreateLine(required, node));
                }
            }
        }

        foreach (SkillTreeDefinition.Node node in skillTree.Nodes)
        {
            SkillNodeView view = Instantiate(nodePrefab, nodesRoot);
            view.name = $"Node {node.id}";
            ((RectTransform)view.transform).anchoredPosition = node.position;
            view.Init(skillTree, node, tooltip, fx);
            nodeViews.Add(node, view);
        }
    }

    private Line CreateLine(SkillTreeDefinition.Node from, SkillTreeDefinition.Node to)
    {
        GameObject go = new("Line", typeof(RectTransform), typeof(UILine));
        go.transform.SetParent(linesRoot, false);

        UILine graphic = go.GetComponent<UILine>();
        graphic.raycastTarget = false;
        graphic.Set(from.position, to.position, lineThickness);

        return new Line { graphic = graphic, from = from, to = to };
    }

    private void PlayIntro()
    {
        float now = Time.unscaledTime;

        foreach (KeyValuePair<SkillTreeDefinition.Node, SkillNodeView> pair in nodeViews)
        {
            if (skillTree.IsRevealed(pair.Key))
            {
                pair.Value.PlayPop(GetIntroDelay(pair.Key), false);
            }
        }

        foreach (Line line in lines)
        {
            if (!line.revealed)
            {
                continue;
            }

            float startDelay = GetIntroDelay(line.from);
            line.drawStart = now + startDelay;
            line.drawDuration = Mathf.Max(0.05f, GetIntroDelay(line.to) - startDelay);
            line.graphic.Progress = 0f;
        }
    }

    private float GetIntroDelay(SkillTreeDefinition.Node node)
    {
        return introDelay + node.position.magnitude * introSecondsPerPixel;
    }

    private void OnProgressChanged()
    {
        Refresh(true);
    }

    private void Refresh(bool animate)
    {
        float now = Time.unscaledTime;

        foreach (SkillNodeView view in nodeViews.Values)
        {
            view.Refresh(animate ? revealDelay : -1f);
        }

        foreach (Line line in lines)
        {
            bool revealed = skillTree.IsRevealed(line.to);
            if (revealed && !line.revealed)
            {
                line.drawStart = now;
                line.drawDuration = animate ? lineDrawDuration : 0f;
                line.graphic.Progress = animate ? 0f : 1f;
            }

            line.revealed = revealed;
            line.graphic.gameObject.SetActive(revealed);

            bool owned = skillTree.GetLevel(line.to) > 0;
            line.graphic.material = owned ? ownedLineMaterial : lockedLineMaterial;
            line.graphic.color = owned ? ownedLineColor : lockedLineColor;
        }
    }

    private Camera GetUiCamera()
    {
        return canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
    }
}
