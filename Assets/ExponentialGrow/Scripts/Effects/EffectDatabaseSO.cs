using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EffectDatabaseSO", menuName = "Scriptable Objects/EffectManagerSO")]
public class EffectDatabaseSO : SerializedScriptableObject
{
    [OdinSerialize] private Dictionary<ComboName, EffectSO> effects = new();

    public EffectSO GetEffect(ComboName type)
    {
        if(effects.TryGetValue(type, out EffectSO value))
            return value;

        Debug.Log("EFFECT NOT FOUND returning default");
        return GetEffect(ComboName.Miss);
    }



    public Dictionary<ComboName, EffectSO> Effects => effects;
}
