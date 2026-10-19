using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DefaultExecutionOrder(-200)]
public class CrtMonitor : MonoBehaviour
{
    [SerializeField] private float distanceFromMonitor = 0.76f;

    public const int Layer = 6;
    private const int TextureWidth = 1920;
    private const int TextureHeight = 1080;

    private static CrtMonitor instance;

    private Camera viewCamera;
    private RenderTexture texture;
    private Transform screen;
    private Collider glass;
    private Vector2 pointer;
    private bool onGlass;
    private Vector3 localMin;
    private Vector3 localMax;
    private Vector2 fitScale = Vector2.one;
    private Vector2 fitOffset;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetSession()
    {
        instance = null;
        MonitorScreen.ResetSession();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Create()
    {
        if (instance != null)
        {
            return;
        }

        GameObject root = new("CRT Computer");
        instance = root.AddComponent<CrtMonitor>();
        DontDestroyOnLoad(root);
    }

    void Awake()
    {
        texture = new RenderTexture(TextureWidth, TextureHeight, 24, RenderTextureFormat.ARGB32)
        {
            name = "CRT Screen",
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };
        texture.Create();

        int layer = LayerMask.NameToLayer("Monitor");
        if (layer < 0)
        {
            layer = Layer;
        }

        if (!BuildModel(layer))
        {
            return;
        }

        BuildRoom(layer);
        BuildLight();
        BuildView(layer);
    }

    void OnDestroy()
    {
        if (texture != null)
        {
            texture.Release();
            Destroy(texture);
        }

        MonitorScreen.ResetSession();
    }

    void Update()
    {
        FitView();
        UpdatePointer();
    }

    void LateUpdate()
    {
        BindGame();
    }

    private bool BuildModel(int layer)
    {
        GameObject prefab = Resources.Load<GameObject>("OldMonitor");
        if (prefab == null)
        {
            Debug.LogError("Old monitor model was not found in Resources.");
            return false;
        }

        GameObject model = Instantiate(prefab, transform);
        model.name = "Old Monitor";
        SetLayer(model, layer);

        Transform screenTransform = model.transform.Find("Screen");
        Transform corpus = model.transform.Find("Corpus");
        if (screenTransform == null)
        {
            Debug.LogError("Old monitor has no Screen mesh.");
            return false;
        }

        MeshFilter filter = screenTransform.GetComponent<MeshFilter>();
        MeshRenderer renderer = screenTransform.GetComponent<MeshRenderer>();
        Mesh mesh = Instantiate(filter.sharedMesh);
        mesh.name = "CRT Glass";
        RemapScreen(mesh, screenTransform);
        filter.sharedMesh = mesh;

        Shader shader = Shader.Find("OldTech/CrtScreen");
        if (shader == null)
        {
            Debug.LogError("CRT shader was not found.");
            return false;
        }

        Material glassMaterial = new Material(shader) { name = "CRT Glass" };
        glassMaterial.SetTexture("_MainTex", texture);
        renderer.sharedMaterial = glassMaterial;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;

        MeshCollider collider = screenTransform.gameObject.AddComponent<MeshCollider>();
        collider.sharedMesh = mesh;
        collider.convex = false;
        screen = screenTransform;
        glass = collider;

        if (corpus != null)
        {
            MeshRenderer body = corpus.GetComponent<MeshRenderer>();
            if (body != null)
            {
                body.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                body.receiveShadows = true;
            }
        }

        Bounds bounds = renderer.bounds;
        Renderer[] renderers = model.GetComponentsInChildren<Renderer>();
        for (int i = 0; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        model.transform.localScale = Vector3.one * 1.8f;
        model.transform.position = new Vector3(2.55f, 3.0f, 0f);
        BringGlassForward();
        FitPicture(renderer.sharedMaterial);
        return true;
    }

    private void BringGlassForward()
    {
        Transform corpus = screen.parent.Find("Corpus");
        MeshFilter bodyFilter = corpus != null ? corpus.GetComponent<MeshFilter>() : null;
        MeshFilter glassFilter = screen.GetComponent<MeshFilter>();
        if (bodyFilter == null || glassFilter.sharedMesh == null)
        {
            return;
        }

        Bounds glassBounds = screen.GetComponent<Renderer>().bounds;
        Vector3[] glassVertices = glassFilter.sharedMesh.vertices;
        float glassEdge = float.MaxValue;
        for (int i = 0; i < glassVertices.Length; i++)
        {
            glassEdge = Mathf.Min(glassEdge, screen.TransformPoint(glassVertices[i]).x);
        }

        const float margin = 0.08f;
        float lipFront = float.MinValue;
        Vector3[] bodyVertices = bodyFilter.sharedMesh.vertices;
        for (int i = 0; i < bodyVertices.Length; i++)
        {
            Vector3 point = corpus.TransformPoint(bodyVertices[i]);
            bool besideOpening = point.y > glassBounds.min.y - margin && point.y < glassBounds.max.y + margin
                && point.z > glassBounds.min.z - margin && point.z < glassBounds.max.z + margin;
            bool outsideGlass = point.y < glassBounds.min.y || point.y > glassBounds.max.y
                || point.z < glassBounds.min.z || point.z > glassBounds.max.z;
            if (besideOpening && outsideGlass)
            {
                lipFront = Mathf.Max(lipFront, point.x);
            }
        }

        if (lipFront == float.MinValue)
        {
            return;
        }

        float shift = lipFront - glassEdge + 0.025f;
        if (shift > 0f)
        {
            screen.position += new Vector3(shift, 0f, 0f);
        }
    }

    private void FitPicture(Material material)
    {
        Bounds bounds = screen.GetComponent<Renderer>().bounds;
        float glassAspect = bounds.size.z / Mathf.Max(0.001f, bounds.size.y);
        float contentAspect = TextureWidth / (float)TextureHeight;
        if (glassAspect > contentAspect)
        {
            float used = contentAspect / glassAspect;
            fitScale = new Vector2(1f / used, 1f);
        }
        else
        {
            float used = glassAspect / contentAspect;
            fitScale = new Vector2(1f, 1f / Mathf.Max(0.001f, used));
        }

        fitOffset = (Vector2.one - fitScale) * 0.5f;
        material.SetVector("_FitScale", new Vector4(fitScale.x, fitScale.y, 0f, 0f));
        material.SetVector("_FitOffset", new Vector4(fitOffset.x, fitOffset.y, 0f, 0f));
    }

    private Vector2 ContentToGlass(Vector2 content)
    {
        return new Vector2(
            (content.x - fitOffset.x) / Mathf.Max(0.001f, fitScale.x),
            (content.y - fitOffset.y) / Mathf.Max(0.001f, fitScale.y));
    }

    private Vector3 GlassUvToWorld(Vector2 glassUv)
    {
        float y = Mathf.Lerp(localMin.y, localMax.y, glassUv.y);
        float z = Mathf.Lerp(localMin.z, localMax.z, glassUv.x);
        return screen.TransformPoint(new Vector3(localMax.x, y, z));
    }

    private static void RemapScreen(Mesh mesh, Transform screenTransform)
    {
        Vector3[] vertices = mesh.vertices;
        Vector3 min = screenTransform.TransformPoint(vertices[0]);
        Vector3 max = min;
        Vector3[] world = new Vector3[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            world[i] = screenTransform.TransformPoint(vertices[i]);
            min = Vector3.Min(min, world[i]);
            max = Vector3.Max(max, world[i]);
        }

        Vector2[] uvs = new Vector2[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            float u = Mathf.InverseLerp(min.z, max.z, world[i].z);
            float v = Mathf.InverseLerp(min.y, max.y, world[i].y);
            uvs[i] = new Vector2(u, v);
        }

        mesh.SetUVs(0, uvs);
        mesh.RecalculateBounds();
    }

    private void BuildRoom(int layer)
    {
        Shader unlit = Shader.Find("Universal Render Pipeline/Unlit");
        Material deskMaterial = new Material(unlit);
        deskMaterial.SetColor("_BaseColor", new Color(0.42f, 0.33f, 0.26f));
        Material wallMaterial = new Material(unlit);
        wallMaterial.SetColor("_BaseColor", new Color(0.18f, 0.14f, 0.11f));

        GameObject desk = GameObject.CreatePrimitive(PrimitiveType.Cube);
        desk.name = "Desk";
        desk.transform.SetParent(transform, false);
        desk.transform.position = new Vector3(0.2f, -0.06f, 0.15f);
        desk.transform.localScale = new Vector3(6f, 0.12f, 4f);
        desk.GetComponent<Renderer>().sharedMaterial = deskMaterial;
        DestroyImmediate(desk.GetComponent<Collider>());
        SetLayer(desk, layer);

        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = "Wall";
        wall.transform.SetParent(transform, false);
        wall.transform.position = new Vector3(-1.6f, 1.4f, 0.15f);
        wall.transform.localScale = new Vector3(0.08f, 4.5f, 12f);
        wall.GetComponent<Renderer>().sharedMaterial = wallMaterial;
        DestroyImmediate(wall.GetComponent<Collider>());
        SetLayer(wall, layer);
    }

    private void BuildLight()
    {
        GameObject keyObject = new("CRT Key");
        keyObject.transform.SetParent(transform, false);
        Light key = keyObject.AddComponent<Light>();
        key.type = LightType.Directional;
        key.color = new Color(1f, 0.93f, 0.84f);
        key.intensity = 1.35f;
        key.cullingMask = 1 << Layer;
        key.shadows = LightShadows.None;
        keyObject.transform.rotation = Quaternion.LookRotation(new Vector3(-0.55f, -0.65f, 0.15f));

        GameObject fillObject = new("CRT Fill");
        fillObject.transform.SetParent(transform, false);
        Light fill = fillObject.AddComponent<Light>();
        fill.type = LightType.Directional;
        fill.color = new Color(0.75f, 0.82f, 1f);
        fill.intensity = 0.35f;
        fill.cullingMask = 1 << Layer;
        fill.shadows = LightShadows.None;
        fillObject.transform.rotation = Quaternion.LookRotation(new Vector3(0.2f, -0.4f, -0.7f));
    }

    private void BuildView(int layer)
    {
        GameObject cameraObject = new("CRT Camera");
        cameraObject.transform.SetParent(transform, false);

        viewCamera = cameraObject.AddComponent<Camera>();
        UniversalAdditionalCameraData data = cameraObject.GetComponent<UniversalAdditionalCameraData>();
        if (data == null)
        {
            data = cameraObject.AddComponent<UniversalAdditionalCameraData>();
        }

        viewCamera.clearFlags = CameraClearFlags.SolidColor;
        viewCamera.backgroundColor = new Color(0.05f, 0.035f, 0.025f, 1f);
        viewCamera.orthographic = false;
        viewCamera.fieldOfView = 32f;
        viewCamera.nearClipPlane = 0.05f;
        viewCamera.farClipPlane = 20f;
        viewCamera.depth = 20f;
        viewCamera.cullingMask = 1 << layer;
        viewCamera.allowHDR = false;
        viewCamera.tag = "Untagged";

        data.renderPostProcessing = false;
        data.volumeLayerMask = 0;
        data.SetRenderer(1);

        AudioListener listener = cameraObject.GetComponent<AudioListener>();
        if (listener != null)
        {
            listener.enabled = false;
        }

        FrameCamera();
        BuildBackdrop();
    }

    private void BuildBackdrop()
    {
        GameObject backdropObject = new("CRT Backdrop");
        backdropObject.transform.SetParent(transform, false);
        Camera backdrop = backdropObject.AddComponent<Camera>();
        backdrop.clearFlags = CameraClearFlags.SolidColor;
        backdrop.backgroundColor = Color.black;
        backdrop.cullingMask = 0;
        backdrop.depth = 10f;
        backdrop.orthographic = true;
        backdrop.allowHDR = false;
        UniversalAdditionalCameraData data = backdropObject.GetComponent<UniversalAdditionalCameraData>();
        if (data == null)
        {
            data = backdropObject.AddComponent<UniversalAdditionalCameraData>();
        }

        data.renderPostProcessing = false;
        data.volumeLayerMask = 0;
        AudioListener listener = backdropObject.GetComponent<AudioListener>();
        if (listener != null)
        {
            listener.enabled = false;
        }
    }

    private void FitView()
    {
        float windowAspect = Screen.width / Mathf.Max(1f, (float)Screen.height);
        const float targetAspect = 16f / 9f;
        if (windowAspect > targetAspect)
        {
            float width = targetAspect / windowAspect;
            viewCamera.rect = new Rect((1f - width) * 0.5f, 0f, width, 1f);
        }
        else
        {
            float height = windowAspect / targetAspect;
            viewCamera.rect = new Rect(0f, (1f - height) * 0.5f, 1f, height);
        }
    }

    private void FrameCamera()
    {
        Bounds bounds = screen.GetComponent<Renderer>().bounds;
        Renderer[] parts = screen.parent.GetComponentsInChildren<Renderer>();
        for (int i = 0; i < parts.Length; i++)
        {
            bounds.Encapsulate(parts[i].bounds);
        }

        Vector3 center = screen.GetComponent<Renderer>().bounds.center;
        Vector3 facing = screen.TransformDirection(Vector3.right);
        if (facing.sqrMagnitude < 0.0001f)
        {
            facing = Vector3.right;
        }

        facing.Normalize();
        const float fov = 30f;
        float aspect = 16f / 9f;
        float halfFov = fov * 0.5f * Mathf.Deg2Rad;
        float distanceForHeight = bounds.extents.y / Mathf.Tan(halfFov);
        float horizontalFov = Mathf.Atan(Mathf.Tan(halfFov) * aspect);
        float distanceForWidth = bounds.extents.z / Mathf.Tan(horizontalFov);

        float distance = Mathf.Max(distanceForHeight, distanceForWidth) * distanceFromMonitor;

        viewCamera.fieldOfView = fov;
        viewCamera.transform.position = center + facing * distance;
        viewCamera.transform.LookAt(center, Vector3.up);

        Vector3[] vertices = screen.GetComponent<MeshFilter>().sharedMesh.vertices;
        localMin = vertices[0];
        localMax = vertices[0];
        for (int i = 1; i < vertices.Length; i++)
        {
            localMin = Vector3.Min(localMin, vertices[i]);
            localMax = Vector3.Max(localMax, vertices[i]);
        }
    }

    private void UpdatePointer()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null || viewCamera == null || screen == null)
        {
            return;
        }

        bool hitGlass = false;
        Ray ray = viewCamera.ScreenPointToRay(mouse.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, 30f, 1 << screen.gameObject.layer, QueryTriggerInteraction.Ignore)
            && hit.collider == glass)
        {
            Vector2 content = Vector2.Scale(hit.textureCoord, fitScale) + fitOffset;
            if (content.x >= 0f && content.x <= 1f && content.y >= 0f && content.y <= 1f)
            {
                hitGlass = true;
                pointer = new Vector2(content.x * TextureWidth, content.y * TextureHeight);
            }
        }

        onGlass = hitGlass;
        Vector3 left = GlassUvToWorld(ContentToGlass(new Vector2(0f, 0.5f)));
        Vector3 right = GlassUvToWorld(ContentToGlass(new Vector2(1f, 0.5f)));
        float viewWidth = Vector2.Distance(viewCamera.WorldToScreenPoint(left), viewCamera.WorldToScreenPoint(right));
        float dragScale = viewWidth > 1f ? TextureWidth / viewWidth : 1f;
        MonitorScreen.SetActive(texture, dragScale, onGlass, pointer);
    }

    private void BindGame()
    {
        if (texture == null)
        {
            return;
        }

        int monitorLayer = screen != null ? screen.gameObject.layer : Layer;
        Camera game = null;
        Scene active = SceneManager.GetActiveScene();

        foreach (Camera camera in FindObjectsByType<Camera>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            if (!camera.orthographic || camera.transform.IsChildOf(transform))
            {
                continue;
            }

            camera.targetTexture = texture;
            camera.cullingMask &= ~(1 << monitorLayer);
            if (camera.gameObject.scene == active)
            {
                game = camera;
            }

            Physics2DRaycaster physics = camera.GetComponent<Physics2DRaycaster>();
            if (physics != null && physics is not MonitorPhysics2DRaycaster)
            {
                LayerMask mask = physics.eventMask;
                DestroyImmediate(physics);
                MonitorPhysics2DRaycaster replacement = camera.gameObject.AddComponent<MonitorPhysics2DRaycaster>();
                replacement.eventMask = mask;
            }
        }

        if (game == null)
        {
            return;
        }

        foreach (Canvas canvas in FindObjectsByType<Canvas>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            if (canvas.renderMode == RenderMode.WorldSpace)
            {
                continue;
            }

            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.planeDistance = 9f;
                if (canvas.sortingOrder < 50)
                {
                    canvas.sortingOrder = 100;
                }
            }

            canvas.worldCamera = game;
            if (canvas.GetComponent<MonitorGraphicRaycaster>() == null)
            {
                GraphicRaycaster existing = canvas.GetComponent<GraphicRaycaster>();
                if (existing != null)
                {
                    DestroyImmediate(existing);
                }

                canvas.gameObject.AddComponent<MonitorGraphicRaycaster>();
            }
        }
    }

    private static void SetLayer(GameObject root, int layer)
    {
        Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < transforms.Length; i++)
        {
            transforms[i].gameObject.layer = layer;
        }
    }
}
