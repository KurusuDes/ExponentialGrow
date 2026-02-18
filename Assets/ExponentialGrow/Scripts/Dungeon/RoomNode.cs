
using System;
using System.Collections.Generic;
using UnityEngine;

public class RoomNode : MonoBehaviour
{

    public RoomType roomType;
    public RoomState roomState = RoomState.Unvisited;
    public List<RoomNode> connectedRooms;

    public List<BaseEnemy> Enemies;


    public Action OnPlayerEnterRoom;
    public Action OnPlayerExitRoom;



    public Transform StartPosition;
    void Start()
    {
        OnPlayerEnterRoom += LoadLevel;
        OnPlayerExitRoom += HideLevel;
    }
    public void Set()
    {

    }
    private void HideLevel()
    {
        roomState = RoomState.Cleared;
    }

    private void LoadLevel()
    {
        roomState = RoomState.Visited;

    }

   
}
