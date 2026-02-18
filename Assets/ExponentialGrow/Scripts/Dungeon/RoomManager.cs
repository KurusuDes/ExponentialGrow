using UnityEngine;
using System;
using Sirenix.OdinInspector;

public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        
    }
    private void Start()
    {
        
    }
    private void Update()
    {
            
    }
    [Button]
    public void TestRng()
    {
        print(GameManager.Instance.seedRandom.GetRandomEnemy());

    //CombatSystem.
    }

}
