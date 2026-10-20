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
    [SerializeField] private float lineThickness = 10f;
    [SerializeField] private Material ownedLineMaterial;
    [SerializeField] private Material lockedLineMaterial;
    [SerializeField] private Color ownedLineColor = new(0.38f, 0.55f, 0.40f, 1f);
    [SerializeField] private Color lockedLineColor = new(0.55f, 0.53f, 0.48f, 0.95f);

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

    public SkillTreeDefinition Definition => skillTree;

    void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
        lineThickness = 10f;
        ownedLineColor = new Color(0.38f, 0.55f, 0.40f, 1f);
        lockedLineColor = new Color(0.62f, 0.60f, 0.55f, 1f);
        _ = ownedLineMaterial;
        _ = lockedLineMaterial;
        _ = introDelay;
        _ = introSecondsPerPixel;
        _ = revealDelay;
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

    System.Collections.IEnumerator Start()
    {
        yield return null;
        ResetView();
    }

    public void ResetView()
    {
        bool any = false;
        float minX = 0f;
        float maxX = 0f;
        float minY = 0f;
        float maxY = 0f;
        foreach (SkillTreeDefinition.Node node in skillTree.Nodes)
        {
            if (!skillTree.IsRevealed(node))
            {
                continue;
            }

            if (!any)
            {
                minX = maxX = node.position.x;
                minY = maxY = node.position.y;
                any = true;
            }
            else
            {
                minX = Mathf.Min(minX, node.position.x);
                maxX = Mathf.Max(maxX, node.position.x);
                minY = Mathf.Min(minY, node.position.y);
                maxY = Mathf.Max(maxY, node.position.y);
            }
        }

        if (!any)
        {
            content.localScale = Vector3.one;
            content.anchoredPosition = Vector2.zero;
            return;
        }

        Rect area = ((RectTransform)transform).rect;
        Vector2 focus = new((minX + maxX) * 0.5f, (minY + maxY) * 0.5f);
        float spanX = Mathf.Max(280f, maxX - minX + 280f);
        float spanY = Mathf.Max(280f, maxY - minY + 280f);
        float zoom = 1f;
        if (area.width > 1f && area.height > 1f)
        {
            zoom = Mathf.Min(area.width / spanX, area.height / spanY);
        }

        zoom = Mathf.Clamp(zoom, minZoom, 1f);
        content.localScale = new Vector3(zoom, zoom, 1f);
        content.anchoredPosition = -focus * zoom;
    }

    public bool TryGetNodeRect(string skillId, out RectTransform nodeRect)
    {
        foreach (KeyValuePair<SkillTreeDefinition.Node, SkillNodeView> pair in nodeViews)
        {
            if (pair.Key.id == skillId && pair.Value.gameObject.activeInHierarchy)
            {
                nodeRect = (RectTransform)pair.Value.transform;
                return true;
            }
        }

        nodeRect = null;
        return false;
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
        content.anchoredPosition += eventData.delta * MonitorScreen.DragScale / canvas.scaleFactor;
    }

    public void OnScroll(PointerEventData eventData)
    {
        if (Mathf.Approximately(eventData.scrollDelta.y, 0f))
        {
            return;
        }

        Camera uiCamera = GetUiCamera();
        Vector2 pointer = MonitorScreen.Active ? MonitorScreen.Pointer : eventData.position;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(content, pointer, uiCamera, out Vector2 before);

        float zoom = content.localScale.x * (1f + zoomStep * Mathf.Sign(eventData.scrollDelta.y));
        zoom = Mathf.Clamp(zoom, minZoom, maxZoom);
        content.localScale = new Vector3(zoom, zoom, 1f);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(content, pointer, uiCamera, out Vector2 after);
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
            RectTransform nodeRect = (RectTransform)view.transform;
            nodeRect.anchoredPosition = node.position;
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
        graphic.material = null;
        graphic.Set(from.position, to.position, lineThickness);

        return new Line { graphic = graphic, from = from, to = to };
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
            view.Refresh();
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
            line.graphic.material = null;
            line.graphic.color = owned ? ownedLineColor : lockedLineColor;
        }
    }

    private Camera GetUiCamera()
    {
        return canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
    }
}
