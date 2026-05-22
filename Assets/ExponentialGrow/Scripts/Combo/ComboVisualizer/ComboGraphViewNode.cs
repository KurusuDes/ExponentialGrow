using UnityEditor;
using UnityEngine;

public class ComboGraphViewNode
{
    #region Public Properties
    public ComboNodeSO Data => data;
    public Rect Rect => rect;
    #endregion

    #region Private Variables
    private ComboNodeSO data;
    private Rect rect;
    private bool isDragged;
    private bool isSelected;

    private static GUIStyle _headerStyle;
    private static GUIStyle _bodyStyle;
    private static GUIStyle _mutedStyle;
    #endregion

    #region Constructor
    public ComboGraphViewNode(ComboNodeSO data, Vector2 pos)
    {
        this.data = data;
        this.rect = new Rect(pos.x, pos.y, 250, 200);
    }
    #endregion

    #region Public Methods
    public void SetPosition(Vector2 pos)
    {
        rect.position = pos;
    }

    public void Drag(Vector2 delta)
    {
        rect.position += delta;
    }

    public void Draw()
    {
        EnsureStyles();

        // Selection outline — 2px border behind the node
        if (isSelected)
        {
            EditorGUI.DrawRect(
                new Rect(rect.x - 2, rect.y - 2, rect.width + 4, rect.height + 4),
                new Color(1f, 0.8f, 0.2f, 1f)
            );
        }

        // Dark body background
        EditorGUI.DrawRect(rect, new Color(0.13f, 0.13f, 0.13f, 1f));

        // Colored header strip
        Rect headerRect = new Rect(rect.x, rect.y, rect.width, 30);
        Color headerColor = (data != null) ? GetNodeColor(data.Type) : new Color(0.22f, 0.22f, 0.38f, 1f);
        EditorGUI.DrawRect(headerRect, headerColor);

        // Header title
        string title = (data != null) ? data.Value.ToString() : "ROOT DATABASE";
        GUI.Label(
            new Rect(headerRect.x + 8, headerRect.y + 5, headerRect.width - 16, 20),
            title, _headerStyle
        );

        // Body content
        float y = rect.y + 36;
        float x = rect.x + 10;
        float w = rect.width - 20;

        if (data == null)
        {
            GUI.Label(new Rect(x, y, w, 20), "Starting Point", _mutedStyle);
        }
        else
        {
            GUI.Label(new Rect(x, y, w, 20), $"Type:  {data.Type}", _bodyStyle);
            y += 24;

            GUI.Label(new Rect(x, y, w, 20), "Paths:", _mutedStyle);
            y += 20;

            foreach (var path in data.Paths)
            {
                string next = (path.Value != null) ? path.Value.Value.ToString() : "—";
                GUI.Label(new Rect(x, y, w, 18), $"  {path.Key}  →  {next}", _bodyStyle);
                y += 18;
            }
        }
    }

    public bool ProcessEvents(Event currentEvent, float zoom)
    {
        switch (currentEvent.type)
        {
            case EventType.MouseDown:
                if (currentEvent.button == 0)
                {
                    if (rect.Contains(currentEvent.mousePosition))
                    {
                        isDragged = true;
                        isSelected = true;
                        GUI.changed = true;
                    }
                    else
                    {
                        isSelected = false;
                        GUI.changed = true;
                    }
                }
                break;

            case EventType.MouseUp:
                isDragged = false;
                break;

            case EventType.MouseDrag:
                if (currentEvent.button == 0 && isDragged)
                {
                    Drag(currentEvent.delta / zoom);
                    currentEvent.Use();
                    return true;
                }
                break;
        }

        return false;
    }
    #endregion

    #region Private Methods
    private static void EnsureStyles()
    {
        if (_headerStyle != null) return;

        _headerStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 12,
            normal = { textColor = Color.white }
        };
        _bodyStyle = new GUIStyle(EditorStyles.label)
        {
            fontSize = 11,
            normal = { textColor = new Color(0.85f, 0.85f, 0.85f) }
        };
        _mutedStyle = new GUIStyle(EditorStyles.label)
        {
            fontSize = 10,
            normal = { textColor = new Color(0.55f, 0.55f, 0.55f) }
        };
    }

    private Color GetNodeColor(ComboType type) => type switch
    {
        ComboType.Offensive => new Color(0.8f, 0.3f, 0.3f),
        ComboType.Defensive => new Color(0.3f, 0.5f, 0.9f),
        ComboType.Utility   => new Color(0.3f, 0.8f, 0.4f),
        ComboType.Control   => new Color(0.8f, 0.7f, 0.3f),
        _                   => new Color(0.4f, 0.4f, 0.4f)
    };
    #endregion
}
