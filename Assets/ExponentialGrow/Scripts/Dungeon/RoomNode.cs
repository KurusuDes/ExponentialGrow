using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

public class RoomNode : MonoBehaviour
{
    [FoldoutGroup("Settings"), SerializeField] private RoomType roomType;
    [FoldoutGroup("Settings"), SerializeField] private RoomState roomState = RoomState.Unvisited;
    [FoldoutGroup("Settings"), SerializeField, ReadOnly] private int maxDoors;
    [FoldoutGroup("Settings"), SerializeField, ReadOnly] private int depth;

    [FoldoutGroup("Connections"), SerializeField] private RoomNode parentRoom;
    [FoldoutGroup("Connections"), SerializeField] private List<RoomNode> connectedRooms = new();
    [FoldoutGroup("Connections"), SerializeField, ReadOnly] private bool hasSiblingConnection;

    [FoldoutGroup("Enemies"), SerializeField] private List<EnemySO> enemyPool = new();

    [FoldoutGroup("Visuals")] public Transform StartPosition;

    public Action OnPlayerEnterRoom;
    public Action OnPlayerExitRoom;

    private void Awake()
    {
        OnPlayerEnterRoom += LoadLevel;
        OnPlayerExitRoom  += HideLevel;
    }

    private void OnDestroy()
    {
        OnPlayerEnterRoom -= LoadLevel;
        OnPlayerExitRoom  -= HideLevel;
    }

    // ────────────────────────────────────────────────
    //  Setup (llamado desde RoomManager al generar)
    // ────────────────────────────────────────────────

    public void SetRoomType(RoomType type, RoomState state = RoomState.Unvisited)
    {
        roomType  = type;
        roomState = state;
    }

    public void SetMaxDoors(int value)
    {
        maxDoors = value;
    }

    public void SetDepth(int value)
    {
        depth = value;
    }

    //-> solo se llama al crear un hijo, nunca al conectar hermanos
    public void SetParent(RoomNode parent)
    {
        parentRoom = parent;
    }

    public void SetHasSiblingConnection(bool value)
    {
        hasSiblingConnection = value;
    }

    public void AddEnemyToPool(EnemySO enemy)
    {
        enemyPool.Add(enemy);
    }

    // ────────────────────────────────────────────────
    //  Room events
    // ────────────────────────────────────────────────

    private void LoadLevel()
    {
        roomState = RoomState.Visited;
    }

    private void HideLevel()
    {
        roomState = RoomState.Cleared;
    }

    // ────────────────────────────────────────────────
    //  Properties
    // ────────────────────────────────────────────────

    public RoomType RoomType             => roomType;
    public RoomState RoomState           => roomState;
    public int MaxDoors                  => maxDoors;
    public int Depth                     => depth;
    public bool HasSiblingConnection     => hasSiblingConnection;
    public RoomNode ParentRoom           => parentRoom;
    public List<RoomNode> ConnectedRooms => connectedRooms;
    public List<EnemySO> EnemyPool       => enemyPool;
}
