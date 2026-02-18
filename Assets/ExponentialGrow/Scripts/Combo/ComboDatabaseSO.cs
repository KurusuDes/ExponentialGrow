using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using Sirenix.Serialization;
using System;

public struct KeyBaseData
{
    public KeyCapType KeyCap;
    public ComboType comboType;
    public KeyBaseData(KeyCapType keyCap, ComboType comboType)
    {
        KeyCap = keyCap;
        this.comboType = comboType;
    }
    public KeyBaseData(KeyCapType type)
    {
        KeyCap = type;
        this.comboType = ComboType.None;
    }
}
[CreateAssetMenu(fileName = "ComboDatabaseSO", menuName = "Scriptable Objects/ComboDatabaseSO")]
public class ComboDatabaseSO : SerializedScriptableObject
{
    [OdinSerialize] private Dictionary<KeyCapType, ComboNodeSO> comboData = new();

    public ComboName GetComboType(Queue<KeyCapType> combo)
    {
        Queue<KeyCapType> localCoppy = new (combo);

        Debug.Log(localCoppy.Count);

        if(comboData.TryGetValue(localCoppy.Dequeue(), out ComboNodeSO value))
            return GetComboType(localCoppy, value);

        //Safe exit
        return ComboName.Miss;
    }

    public ComboName GetComboType(Queue<KeyCapType> combo , ComboNodeSO comboNode)
    {
        if (combo.Count == 0)
        {
            Debug.Log("Combo Terminated" + comboNode.Value);
            return comboNode.Value;
        }
        if (comboNode.Paths.TryGetValue(combo.Dequeue(), out ComboNodeSO value))
        {
            if(value == null)
            {
                Debug.Log("Empty Combo Slot");
                return ComboName.Miss; ;
            }
            return GetComboType(combo, value);
        }

        Debug.Log("Try do a combo that dosent exist");
        return ComboName.Miss;
        
 
    }
    public ComboNodeSO GetComboNode(Queue<KeyCapType> combo)
    {
        if (combo == null || combo.Count == 0)
            return null;

        Queue<KeyCapType> localCopy = new(combo);

        // Primer input
        if (!comboData.TryGetValue(localCopy.Dequeue(), out ComboNodeSO currentNode))
            return null;

        // Recorremos el resto del combo
        while (localCopy.Count > 0)
        {
            var key = localCopy.Dequeue();

            if (!currentNode.Paths.TryGetValue(key, out ComboNodeSO nextNode))
                return null;

            if (nextNode == null)
                return null;

            currentNode = nextNode;
        }

        return currentNode;
    }
    [Button]
    public List<List<KeyCapType>> GetAllPossibleCombos(ComboNodeSO rootNode)
    {
        List<List<KeyCapType>> result = new();
        List<KeyCapType> currentPath = new();

        TraverseNode(rootNode, currentPath, result);

        foreach (var path in result)
        {
            string pathString = string.Join(" -> ", path);
            Debug.Log(pathString);
        }

        return result;
    }

 
    public List<KeyBaseData> GetFirstLevelBaseData(ComboNodeSO node)
    {
        List<KeyBaseData> options = new();

        if (node == null || node.Paths.Count == 0)
            return options;

        foreach (var path in node.Paths)
        {
            options.Add(new(path.Key,path.Value.Type));
            
        }

        return options;
    }
    public List<KeyCapType> GetFirstLevelOptions(ComboNodeSO node)
    {
        List<KeyCapType> options = new();

        if (node == null || node.Paths.Count == 0)
            return options;

        foreach (var path in node.Paths)
        {
            options.Add(path.Key);
        }
    
        return options;
    }
    private void TraverseNode(ComboNodeSO node, List<KeyCapType> currentPath, List<List<KeyCapType>> result)
    {
        if (node == null)
            return;

        if (node.Paths.Count == 0)
        {
            result.Add(new List<KeyCapType>(currentPath));
            return;
        }

        foreach (var path in node.Paths)
        {
            // Añadimos la dirección
            currentPath.Add(path.Key);

            // Seguimos recorriendo
            TraverseNode(path.Value, currentPath, result);

            // Backtrack
            currentPath.RemoveAt(currentPath.Count - 1);
        }
    }

}
