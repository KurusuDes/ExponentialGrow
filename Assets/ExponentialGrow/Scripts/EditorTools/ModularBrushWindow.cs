using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;



public class ModularBrushWindow : OdinEditorWindow
{
    [MenuItem("Sowtank Tools/Modular Brush")]
    public static void Open()
    {
        GetWindow<ModularBrushWindow>("Modular Brush");
    }

    // =========================
    // ENUM
    // =========================
    public enum BrushCategory
    {
        Floors1, Floors2,
        Walls1, Walls2,
        Misc1, Misc2,
        Custom
    }

    // =========================
    // STATE
    // =========================
    [Title("Brush State")]
    [Button(ButtonSizes.Large)]
    [GUIColor("@brushEnabled ? Color.green : Color.red")]
    private void ToggleBrush()
    {
        brushEnabled = !brushEnabled;

        if (!brushEnabled && previewInstance != null)
            previewInstance.SetActive(false);

        SceneView.RepaintAll();
    }

    [HideInInspector]
    public bool brushEnabled;

    private bool lastBrushState;

    // =========================
    // GRID & SNAP
    // =========================
    [Title("Grid & Snap")]
    public bool enableSnap = true;

    [ShowIf(nameof(enableSnap))]
    public float gridSize = 5f;

    // =========================
    // HEIGHT
    // =========================
    [Title("Height Settings")]
    public bool useHitHeight = true;

    [ShowIf(nameof(IsNotUsingHitHeight))]
    public bool autoStackHeight = true;

    [ShowIf(nameof(ShowStackLayers))]
    public LayerMask stackingLayers;

    public float baseHeight = 0f;
    public float heightOffset = 0f;

    // =========================
    // ROTATION
    // =========================
    [Title("Rotation Settings")]
    public bool enableRotation = true;

    [ShowIf(nameof(enableRotation))]
    public float rotationStep = 90f;

    private float currentRotationY;

    // =========================
    // CATEGORY
    // =========================
    [Title("Brush Category")]
    public BrushCategory category;

    // =========================
    // PREFAB LIBRARIES
    // =========================
    [Title("Prefab Libraries")]
    [ShowIf(nameof(IsFloors1))] public List<GameObject> floors1 = new();
    [ShowIf(nameof(IsFloors2))] public List<GameObject> floors2 = new();
    [ShowIf(nameof(IsWalls1))] public List<GameObject> walls1 = new();
    [ShowIf(nameof(IsWalls2))] public List<GameObject> walls2 = new();
    [ShowIf(nameof(IsMisc1))] public List<GameObject> misc1 = new();
    [ShowIf(nameof(IsMisc2))] public List<GameObject> misc2 = new();
    [ShowIf(nameof(IsCustom))] public List<GameObject> customObjects = new();

    // =========================
    // INTERNAL
    // =========================
    private GameObject previewInstance;

    protected override void OnEnable()
    {
        base.OnEnable();
        SceneView.duringSceneGui += OnSceneGUI;
        lastBrushState = brushEnabled;
    }

    protected override void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
        DestroyPreview();
        base.OnDisable();
    }

    // =========================
    // SCENE GUI
    // =========================
    private void OnSceneGUI(SceneView sceneView)
    {
        // 🔑 Detecta cambio real de estado
        if (brushEnabled != lastBrushState)
        {
            lastBrushState = brushEnabled;

            if (!brushEnabled && previewInstance != null)
                previewInstance.SetActive(false);
        }

        if (!brushEnabled)
            return;

        Event e = Event.current;

        // ROTATION (CTRL + SCROLL)
        if (enableRotation && e.control && e.type == EventType.ScrollWheel)
        {
            currentRotationY += Mathf.Sign(e.delta.y) * rotationStep;
            currentRotationY = Mathf.Repeat(currentRotationY, 360f);
            e.Use();
        }

        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        Vector3 pos;

        if (Physics.Raycast(ray, out RaycastHit hit, 1000f) && useHitHeight)
        {
            pos = hit.point;
        }
        else
        {
            Plane plane = new Plane(Vector3.up, Vector3.up * baseHeight);
            if (!plane.Raycast(ray, out float dist))
                return;

            pos = ray.GetPoint(dist);
            pos.y = baseHeight;
        }

        if (enableSnap)
            pos = SnapXZ(pos);

        if (!useHitHeight && autoStackHeight)
            pos.y = GetHighestStackY(pos);

        pos.y += heightOffset;

        UpdatePreview(pos);

        if (e.type == EventType.MouseDown && e.button == 0 && !e.alt)
        {
            PlaceObject(pos);
            e.Use();
        }

        sceneView.Repaint();
    }

    // =========================
    // SNAP
    // =========================
    private Vector3 SnapXZ(Vector3 pos)
    {
        pos.x = Mathf.Round(pos.x / gridSize) * gridSize;
        pos.z = Mathf.Round(pos.z / gridSize) * gridSize;
        return pos;
    }

    // =========================
    // AUTO STACK HEIGHT
    // =========================
    private float GetHighestStackY(Vector3 pos)
    {
        Vector3 center = new(pos.x, baseHeight + 50f, pos.z);
        Vector3 half = new(gridSize * 0.45f, 50f, gridSize * 0.45f);

        Collider[] hits = Physics.OverlapBox(center, half, Quaternion.identity, stackingLayers);

        float highest = baseHeight;
        foreach (var c in hits)
            highest = Mathf.Max(highest, c.bounds.max.y);

        return highest;
    }

    // =========================
    // PREVIEW
    // =========================
    private void UpdatePreview(Vector3 position)
    {
        if (previewInstance == null)
            CreatePreview();

        if (previewInstance == null)
            return;

        previewInstance.SetActive(true);
        previewInstance.transform.position = position;
        previewInstance.transform.rotation = Quaternion.Euler(0, currentRotationY, 0);
    }

    private void CreatePreview()
    {
        DestroyPreview();

        GameObject prefab = GetPreviewPrefab();
        if (prefab == null)
            return;

        previewInstance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        previewInstance.hideFlags = HideFlags.HideAndDontSave;

        foreach (var c in previewInstance.GetComponentsInChildren<Collider>())
            c.enabled = false;

        ApplyPreviewMaterial(previewInstance);
        previewInstance.SetActive(false); // 🔑 evita (0,0,0)
    }

    private void DestroyPreview()
    {
        if (previewInstance != null)
            DestroyImmediate(previewInstance);
    }

    private void ApplyPreviewMaterial(GameObject obj)
    {
        foreach (var r in obj.GetComponentsInChildren<MeshRenderer>())
        {
            Material m = new Material(Shader.Find("Standard"));
            m.color = new Color(0, 1, 0, 0.35f);
            m.SetFloat("_Mode", 3);
            m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            m.SetInt("_ZWrite", 0);
            m.EnableKeyword("_ALPHABLEND_ON");
            m.renderQueue = 3000;
            r.sharedMaterial = m;
        }
    }

    // =========================
    // SPAWN
    // =========================
    private void PlaceObject(Vector3 position)
    {
        List<GameObject> list = GetActiveList();
        if (list == null || list.Count == 0)
            return;

        GameObject prefab = list[Random.Range(0, list.Count)];
        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);

        Undo.RegisterCreatedObjectUndo(instance, "Place Modular Object");
        instance.transform.position = position;
        instance.transform.rotation = Quaternion.Euler(0, currentRotationY, 0);
    }

    private GameObject GetPreviewPrefab()
    {
        List<GameObject> list = GetActiveList();
        return list != null && list.Count > 0 ? list[0] : null;
    }

    private List<GameObject> GetActiveList()
    {
        return category switch
        {
            BrushCategory.Floors1 => floors1,
            BrushCategory.Floors2 => floors2,
            BrushCategory.Walls1 => walls1,
            BrushCategory.Walls2 => walls2,
            BrushCategory.Misc1 => misc1,
            BrushCategory.Misc2 => misc2,
            BrushCategory.Custom => customObjects,
            _ => null
        };
    }

    // =========================
    // SHOW IF
    // =========================
    private bool IsNotUsingHitHeight() => !useHitHeight;
    private bool ShowStackLayers() => !useHitHeight && autoStackHeight;

    private bool IsFloors1() => category == BrushCategory.Floors1;
    private bool IsFloors2() => category == BrushCategory.Floors2;
    private bool IsWalls1() => category == BrushCategory.Walls1;
    private bool IsWalls2() => category == BrushCategory.Walls2;
    private bool IsMisc1() => category == BrushCategory.Misc1;
    private bool IsMisc2() => category == BrushCategory.Misc2;
    private bool IsCustom() => category == BrushCategory.Custom;
}
