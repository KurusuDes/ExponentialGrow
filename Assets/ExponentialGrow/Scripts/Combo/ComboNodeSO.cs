using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ComboNodeSO", menuName = "Scriptable Objects/ComboNodeSO")]
public class ComboNodeSO : SerializedScriptableObject
{
    [OdinSerialize] private Dictionary<KeyCapType, ComboNodeSO> paths = new();

    [SerializeField] private ComboName comboName = ComboName.Miss;
    [SerializeField] private ComboType comboType = ComboType.None;  

    public ComboName Value => comboName;
    public ComboType Type => comboType;
    public Dictionary<KeyCapType, ComboNodeSO> Paths => paths;
}