using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "KeyCapDatabase", menuName = "Scriptable Objects/KeyCapDatabase")]
public class KeyCapDatabaseSO : SerializedScriptableObject
{
    [OdinSerialize] private Dictionary<KeyCapType,KeyCapSO> data = new();

    public GameObject GetKeycapPrefab(KeyCapType cap)
    {
        return data[cap].Prefab;
    }

    public Dictionary<KeyCapType,KeyCapSO> Data => data;
}
