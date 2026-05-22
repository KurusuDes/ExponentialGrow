using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ComboGraphWindow : EditorWindow
{
    #region Menu
    [MenuItem("Tools/DungeonDiscotecDisaster/Combo Graph Window")]
    public static void OpenWindow()
    {
        GetWindow<ComboGraphWindow>("Combo Graph");
    }
    #endregion

    #region Private Variables
    private ComboDatabaseSO database;

    private readonly List<ComboGraphViewNode> graphNodes = new();
    private readonly HashSet<ComboNodeSO> visitedNodes = new();

    private float zoom = 1f;
    private const float MIN_ZOOM = 0.3f;
    private const float MAX_ZOOM = 2f;

    private Vector2 panOffset = Vector2.zero;
    private Matrix4x4 _prevMatrix;
    private const float TOOLBAR_HEIGHT = 21f;
    #endregion

    #region Unity Methods
    private void OnGUI()
    {
        Event currentEvent = Event.current;

        ProcessEvents(currentEvent);
        ProcessNodeEvents(currentEvent);

        DrawGrid(20, 0.15f, Color.gray);
        DrawGrid(100, 0.3f, Color.gray);

        BeginZoom();
        DrawConnections();
        DrawNodes();
        EndZoom();

        DrawToolbar();

        if (GUI.changed) Repaint();
    }

    private void OnDisable()
    {
        SaveLayout();
    }
    #endregion

    #region Draw Methods
    private void DrawToolbar()
    {
        GUILayout.BeginHorizontal(EditorStyles.toolbar);

        database = (ComboDatabaseSO)EditorGUILayout.ObjectField(
            database,
            typeof(ComboDatabaseSO),
            false,
            GUILayout.Width(250)
        );

        if (GUILayout.Button("Generate Graph", EditorStyles.toolbarButton))
        {
            GenerateGraph();
        }

        GUILayout.EndHorizontal();
    }

    private void DrawNodes()
    {
        foreach (ComboGraphViewNode node in graphNodes)
            node.Draw();
    }

    private void DrawConnections()
    {
        if (database == null) return;

        var rootData = GetRootDictionary();
        var centralNode = graphNodes.Find(x => x.Data == null);

        if (centralNode != null && rootData != null)
        {
            foreach (var entry in rootData)
            {
                var target = graphNodes.Find(x => x.Data == entry.Value);
                if (target != null) DrawLink(centralNode.Rect, target.Rect, entry.Key);
            }
        }

        foreach (var node in graphNodes)
        {
            if (node.Data == null) continue;
            foreach (var path in node.Data.Paths)
            {
                var target = graphNodes.Find(x => x.Data == path.Value);
                if (target != null) DrawLink(node.Rect, target.Rect, path.Key);
            }
        }
    }

    private void DrawLink(Rect startRect, Rect endRect, KeyCapType dir)
    {
        Vector2 start = GetSide(startRect, dir);
        Vector2 end   = GetSide(endRect, GetOpposite(dir));

        Vector2 dirVec       = GetDirVector(dir);
        float   tangentSize  = 80f;
        Vector3 startTangent = (Vector3)start + (Vector3)(dirVec * tangentSize);
        Vector3 endTangent   = (Vector3)end   - (Vector3)(dirVec * tangentSize);

        Color lineColor = new Color(0.3f, 0.8f, 1f, 1f);
        Handles.DrawBezier(start, end, startTangent, endTangent, lineColor, null, 3f);

        // Filled arrowhead at the endpoint
        float   arrowSize = 10f;
        Vector3 back  = new Vector3(-dirVec.x, -dirVec.y, 0);
        Vector3 perp  = new Vector3(-dirVec.y,  dirVec.x, 0);
        Vector3 tip   = end;
        Vector3 baseL = (Vector3)end + back * arrowSize + perp * (arrowSize * 0.5f);
        Vector3 baseR = (Vector3)end + back * arrowSize - perp * (arrowSize * 0.5f);

        Color prevHandlesColor = Handles.color;
        Handles.color = lineColor;
        Handles.DrawAAConvexPolygon(tip, baseL, baseR);
        Handles.color = prevHandlesColor;

        // Direction label at midpoint
        GUIStyle labelStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            normal    = { textColor = GetDirectionColor(dir) },
            alignment = TextAnchor.MiddleCenter
        };

        Vector2 mid      = Vector2.Lerp(start, end, 0.5f);
        Rect    labelRect = new Rect(mid.x - 40, mid.y - 10, 80, 20);
        GUI.Box(labelRect, "", EditorStyles.helpBox);
        GUI.Label(labelRect, dir.ToString().ToUpper(), labelStyle);
    }

    private Color GetDirectionColor(KeyCapType dir) => dir switch
    {
        KeyCapType.Up    => new Color(1f, 0.3f, 0.3f),
        KeyCapType.Down  => new Color(0.3f, 1f, 0.3f),
        KeyCapType.Left  => new Color(0.3f, 0.6f, 1f),
        KeyCapType.Right => new Color(1f, 0.9f, 0.3f),
        _                => Color.white
    };

    private Vector2 GetSide(Rect r, KeyCapType dir) => dir switch
    {
        KeyCapType.Up    => new Vector2(r.center.x, r.yMin),
        KeyCapType.Down  => new Vector2(r.center.x, r.yMax),
        KeyCapType.Left  => new Vector2(r.xMin, r.center.y),
        KeyCapType.Right => new Vector2(r.xMax, r.center.y),
        _                => r.center
    };

    private KeyCapType GetOpposite(KeyCapType d) => d switch
    {
        KeyCapType.Up    => KeyCapType.Down,
        KeyCapType.Down  => KeyCapType.Up,
        KeyCapType.Left  => KeyCapType.Right,
        KeyCapType.Right => KeyCapType.Left,
        _ => d
    };

    private void DrawGrid(float gridSpacing, float gridOpacity, Color gridColor)
    {
        Handles.BeginGUI();
        Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

        float scaledSpacing = gridSpacing * zoom;
        float offsetX = (panOffset.x % scaledSpacing + scaledSpacing) % scaledSpacing;
        float offsetY = (panOffset.y % scaledSpacing + scaledSpacing) % scaledSpacing;

        int widthLines  = Mathf.CeilToInt(position.width  / scaledSpacing) + 1;
        int heightLines = Mathf.CeilToInt(position.height / scaledSpacing) + 1;

        for (int i = -1; i < widthLines; i++)
        {
            float x = i * scaledSpacing + offsetX;
            Handles.DrawLine(new Vector3(x, 0), new Vector3(x, position.height));
        }

        for (int j = -1; j < heightLines; j++)
        {
            float y = j * scaledSpacing + offsetY;
            Handles.DrawLine(new Vector3(0, y), new Vector3(position.width, y));
        }

        Handles.color = Color.white;
        Handles.EndGUI();
    }
    #endregion

    #region Zoom
    private void BeginZoom()
    {
        _prevMatrix = GUI.matrix;
        GUI.EndGroup();
        Vector2 adjustedPan = panOffset + new Vector2(0, TOOLBAR_HEIGHT);
        GUI.matrix = Matrix4x4.TRS(adjustedPan, Quaternion.identity, new Vector3(zoom, zoom, 1f));
    }

    private void EndZoom()
    {
        GUI.matrix = _prevMatrix;
        GUI.BeginGroup(new Rect(0, TOOLBAR_HEIGHT, position.width, position.height - TOOLBAR_HEIGHT));
    }
    #endregion

    #region Graph Generation
    private void GenerateGraph()
    {
        graphNodes.Clear();
        visitedNodes.Clear();

        if (database == null) return;

        ComboGraphViewNode rootNode = new ComboGraphViewNode(null, Vector2.zero);
        graphNodes.Add(rootNode);

        var rootData = GetRootDictionary();
        if (rootData == null) return;

        foreach (var entry in rootData)
        {
            if (entry.Value == null) continue;

            Vector2 childPos = GetDirVector(entry.Key) * 300f;
            BuildGraphRecursive(entry.Value, childPos, entry.Key);
        }

        // Restore saved positions after building all nodes
        LoadLayout();
    }

    private void BuildGraphRecursive(ComboNodeSO node, Vector2 position, KeyCapType direction)
    {
        if (node == null || visitedNodes.Contains(node)) return;
        visitedNodes.Add(node);

        ComboGraphViewNode viewNode = new ComboGraphViewNode(node, position);
        graphNodes.Add(viewNode);

        foreach (var path in node.Paths)
        {
            if (path.Value == null) continue;

            Vector2 childPos = position + GetDirVector(path.Key) * 250f;
            BuildGraphRecursive(path.Value, childPos, path.Key);
        }
    }

    private Vector2 GetDirVector(KeyCapType key) => key switch
    {
        KeyCapType.Up    => new Vector2( 0, -1),
        KeyCapType.Down  => new Vector2( 0,  1),
        KeyCapType.Left  => new Vector2(-1,  0),
        KeyCapType.Right => new Vector2( 1,  0),
        _ => Vector2.right
    };
    #endregion

    #region Persistence
    private void SaveLayout()
    {
        if (database == null || graphNodes.Count == 0) return;

        foreach (var node in graphNodes)
        {
            string key = GetNodeKey(node);
            EditorPrefs.SetFloat(key + "_x", node.Rect.x);
            EditorPrefs.SetFloat(key + "_y", node.Rect.y);
        }
    }

    private void LoadLayout()
    {
        foreach (var node in graphNodes)
        {
            string key = GetNodeKey(node);
            if (!EditorPrefs.HasKey(key + "_x")) continue;

            node.SetPosition(new Vector2(
                EditorPrefs.GetFloat(key + "_x"),
                EditorPrefs.GetFloat(key + "_y")
            ));
        }
    }

    private string GetNodeKey(ComboGraphViewNode node)
    {
        if (node.Data == null)
            return $"ComboGraph_{database.name}_root";

        // Hash the asset path to avoid illegal EditorPrefs key characters
        return $"ComboGraph_{database.name}_{AssetDatabase.GetAssetPath(node.Data).GetHashCode()}";
    }
    #endregion

    #region Event Methods
    private void ProcessEvents(Event currentEvent)
    {
        switch (currentEvent.type)
        {
            case EventType.MouseDrag:
                if (currentEvent.button == 2)
                {
                    panOffset += currentEvent.delta;
                    GUI.changed = true;
                }
                break;

            case EventType.ScrollWheel:
                ZoomGraph(currentEvent);
                break;
        }
    }

    private void ZoomGraph(Event currentEvent)
    {
        Vector2 mousePos          = currentEvent.mousePosition;
        Vector2 graphPosBeforeZoom = ScreenToGraph(mousePos);

        zoom = Mathf.Clamp(zoom - currentEvent.delta.y * 0.03f, MIN_ZOOM, MAX_ZOOM);

        Vector2 mouseInCanvas = mousePos - new Vector2(0, TOOLBAR_HEIGHT);
        panOffset = mouseInCanvas - (graphPosBeforeZoom * zoom);

        currentEvent.Use();
        GUI.changed = true;
    }

    private void ProcessNodeEvents(Event currentEvent)
    {
        Vector2 correctedMouse  = ScreenToGraph(currentEvent.mousePosition);
        Event   correctedEvent  = new Event(currentEvent);
        correctedEvent.mousePosition = correctedMouse;

        for (int i = graphNodes.Count - 1; i >= 0; i--)
        {
            if (graphNodes[i].ProcessEvents(correctedEvent, zoom))
                GUI.changed = true;
        }
    }
    #endregion

    #region Helpers
    private Vector2 ScreenToGraph(Vector2 screenPosition)
    {
        return (screenPosition - new Vector2(0, TOOLBAR_HEIGHT) - panOffset) / zoom;
    }
    #endregion

    #region Reflection Helper
    private Dictionary<KeyCapType, ComboNodeSO> GetRootDictionary()
    {
        var field = typeof(ComboDatabaseSO).GetField(
            "comboData",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
        );

        return field?.GetValue(database) as Dictionary<KeyCapType, ComboNodeSO>;
    }
    #endregion
}
