using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ReourceUIDatabaseSO", menuName = "Scriptable Objects/ReourceUIDatabaseSO")]
public class ReourceUIDatabaseSO : SerializedScriptableObject
{
    [OdinSerialize] private Dictionary<KeyCapType, Sprite> keyCapsImageData = new();

    [OdinSerialize] private Dictionary<ComboType, Sprite> comboTypeImageData = new();

    public Sprite GetKeyCapSprite(KeyCapType keyCap)
    {
        if (keyCapsImageData.TryGetValue(keyCap, out Sprite value))
            return value;
        Debug.Log("KeyCap Image not found returning null");
        return null;
    }
    public Sprite GetComboTypeSprite(ComboType comboType)
    {
        if (comboTypeImageData.TryGetValue(comboType, out Sprite value))
            return value;
        Debug.Log("ComboType Image not found returning null");
        return null;
    }

}
