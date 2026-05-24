using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance;

    [FoldoutGroup("Settings"), SerializeField, Range(2, 10)] private int roomIterations = 3;
    [FoldoutGroup("Settings"), SerializeField, Range(0, 100)] private int siblingConnectionChance = 30;
    [FoldoutGroup("Settings"), SerializeField, Range(1, 27)] private int maxDoorsPerRoom = 3;
    [FoldoutGroup("Settings"), SerializeField] private List<RoomNode> roomNodePrefabs = new();

    [FoldoutGroup("Layout"), SerializeField] private float horizontalSpacing = 15f;
    [FoldoutGroup("Layout"), SerializeField] private float verticalSpacing   = 15f;

    [FoldoutGroup("Debug"), SerializeField, ReadOnly] private List<RoomNode> generatedRooms = new();
    [FoldoutGroup("Debug"), SerializeField, ReadOnly] private RoomNode currentRoom;

    public const int MAX_ENEMIES = 4;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // ────────────────────────────────────────────────
    //  Buttons
    // ────────────────────────────────────────────────

    [Button]
    public void TestGeneration()
    {
        GenerateMap();
        PrintMapDebug();
    }

    [Button]
    public void ClearMap()
    {
        foreach (var room in generatedRooms)
        {
            if (room == null) continue;

            if (Application.isPlaying)
                Destroy(room.gameObject);
            else
                DestroyImmediate(room.gameObject);
        }

        generatedRooms.Clear();
        currentRoom = null;
    }

    // ────────────────────────────────────────────────
    //  Generation
    // ────────────────────────────────────────────────

    public void GenerateMap()
    {
        ClearMap();

        if (roomNodePrefabs.Count == 0)
        {
            print("[RoomManager] No hay prefabs de RoomNode asignados.");
            return;
        }

        // Raiz — Nivel 1
        RoomNode root = SpawnRoom();
        root.SetDepth(1);
        root.SetRoomType(RoomType.Start);
        currentRoom = root;

        List<RoomNode> frontier = new() { root };

        // Cada iteracion expande un nivel del arbol
        for (int iteration = 1; iteration < roomIterations; iteration++)
        {
            List<RoomNode> nextFrontier = new();

            foreach (RoomNode room in frontier)
            {
                int slots = GameManager.Instance.seedRandom.RoomRange(1, room.MaxDoors + 1);

                for (int j = 0; j < slots; j++)
                {
                    // Intentar conexion hermana (slot alternativo a crear hijo)
                    if (!room.HasSiblingConnection && TryConnectSibling(room, frontier))
                        continue;

                    // Crear hijo — solo padre apunta a hijo (no hay puerta de regreso)
                    RoomNode child = SpawnRoom();
                    child.SetDepth(room.Depth + 1);
                    child.SetParent(room);
                    ConnectParentToChild(room, child);
                    nextFrontier.Add(child);
                }
            }

            frontier = nextFrontier;
            if (frontier.Count == 0) break;
        }

        AssignRoomTypes();
        PositionRooms();
    }

    public void EnterRoom(RoomNode room)
    {
        currentRoom?.OnPlayerExitRoom?.Invoke();
        currentRoom = room;
        currentRoom.OnPlayerEnterRoom?.Invoke();

        if (room.RoomType == RoomType.Combat)
            CombatSystem.OnCreateEnemy?.Invoke();
    }

    // ────────────────────────────────────────────────
    //  Graph
    // ────────────────────────────────────────────────

    private bool TryConnectSibling(RoomNode room, List<RoomNode> sameLevelRooms)
    {
        int roll = GameManager.Instance.seedRandom.RoomRange(1, 100);
        if (roll > siblingConnectionChance) return false;

        List<RoomNode> candidates = new();
        foreach (var candidate in sameLevelRooms)
        {
            if (candidate == room) continue;
            if (candidate.HasSiblingConnection) continue;
            if (candidate.ConnectedRooms.Contains(room)) continue;
            if (candidate.ConnectedRooms.Count >= candidate.MaxDoors) continue;
            candidates.Add(candidate);
        }

        if (candidates.Count == 0) return false;

        int idx = GameManager.Instance.seedRandom.RoomRange(0, candidates.Count);
        ConnectSiblings(room, candidates[idx]);
        return true;
    }

    // Unidireccional: solo el padre apunta al hijo
    // Regla: ningun cuarto tiene puerta hacia su padre ni hacia niveles inferiores
    private void ConnectParentToChild(RoomNode parent, RoomNode child)
    {
        if (parent.ConnectedRooms.Contains(child)) return;
        if (parent.ConnectedRooms.Count >= parent.MaxDoors) return;
        parent.ConnectedRooms.Add(child);
    }

    // Bidireccional: hermanos del mismo nivel se apuntan mutuamente
    private void ConnectSiblings(RoomNode a, RoomNode b)
    {
        if (a == b) return;

        if (!a.ConnectedRooms.Contains(b) && a.ConnectedRooms.Count < a.MaxDoors)
            a.ConnectedRooms.Add(b);

        if (!b.ConnectedRooms.Contains(a) && b.ConnectedRooms.Count < b.MaxDoors)
            b.ConnectedRooms.Add(a);

        a.SetHasSiblingConnection(true);
        b.SetHasSiblingConnection(true);
    }

    private RoomNode SpawnRoom()
    {
        int index     = generatedRooms.Count;
        int prefabIdx = GameManager.Instance.seedRandom.RoomRange(0, roomNodePrefabs.Count);
        RoomNode room = Instantiate(roomNodePrefabs[prefabIdx], transform);
        room.name     = $"Room_{index}";
        room.SetMaxDoors(GameManager.Instance.seedRandom.RoomRange(1, maxDoorsPerRoom + 1));
        generatedRooms.Add(room);
        return room;
    }

    // ────────────────────────────────────────────────
    //  Layout — distribucion en arbol
    // ────────────────────────────────────────────────

    private void PositionRooms()
    {
        // Agrupar cuartos por nivel de profundidad
        Dictionary<int, List<RoomNode>> byDepth = new();

        foreach (var room in generatedRooms)
        {
            if (!byDepth.ContainsKey(room.Depth))
                byDepth[room.Depth] = new();
            byDepth[room.Depth].Add(room);
        }

        foreach (var kvp in byDepth)
        {
            int depth = kvp.Key;
            List<RoomNode> roomsAtLevel = kvp.Value;
            int count = roomsAtLevel.Count;

            // Centrar el nivel en X, avanzar en Z segun profundidad
            float totalWidth = (count - 1) * horizontalSpacing;
            float startX     = -totalWidth / 2f;
            float z          = (depth - 1) * verticalSpacing;

            for (int i = 0; i < count; i++)
            {
                float x = startX + i * horizontalSpacing;
                roomsAtLevel[i].transform.position = new Vector3(x, 0f, z);
            }
        }
    }

    // ────────────────────────────────────────────────
    //  Room types
    // ────────────────────────────────────────────────

    private void AssignRoomTypes()
    {
        if (generatedRooms.Count == 0) return;

        // Raiz ya es Start; ultimo generado es Exit
        generatedRooms[generatedRooms.Count - 1].SetRoomType(RoomType.Exit);

        int bossDepth = Mathf.Max(1, roomIterations / 2);

        for (int i = 1; i < generatedRooms.Count - 1; i++)
        {
            RoomNode room = generatedRooms[i];
            int roll      = GameManager.Instance.seedRandom.RoomRange(1, 100);
            RoomType type = RoomType.Combat;

            if      (roll > 85)                             type = RoomType.Treasure;
            else if (roll > 75)                             type = RoomType.Shop;
            else if (roll > 65 && room.Depth == bossDepth) type = RoomType.Boss;

            room.SetRoomType(type);

            if (type == RoomType.Combat)
                PopulateEnemies(room);
        }
    }

    private void PopulateEnemies(RoomNode room)
    {
        int count = GameManager.Instance.seedRandom.RoomRange(1, MAX_ENEMIES + 1);

        for (int i = 0; i < count; i++)
            room.AddEnemyToPool(GameManager.Instance.GetRandomEnemy());
    }

    // ────────────────────────────────────────────────
    //  Debug
    // ────────────────────────────────────────────────

    private void PrintMapDebug()
    {
        print("=== DUNGEON MAP ===");

        foreach (var room in generatedRooms)
        {
            string parent      = room.ParentRoom != null ? room.ParentRoom.name : "raiz";
            string sibling     = room.HasSiblingConnection ? "[hermano]" : "";
            string connections = string.Join(", ", room.ConnectedRooms.ConvertAll(r => $"{r.name}({r.RoomType})"));

            print($"[{room.name}] Nivel:{room.Depth} | {room.RoomType} | Padre:{parent} {sibling} | puertas:{room.ConnectedRooms.Count}/{room.MaxDoors} → {connections}");
        }
    }

    public RoomNode CurrentRoom => currentRoom;
}
