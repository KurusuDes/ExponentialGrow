using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EffectDatabaseSO", menuName = "Scriptable Objects/EffectManagerSO")]
public class EffectDatabaseSO : SerializedScriptableObject
{
    [OdinSerialize] private Dictionary<ComboName, CombatActionSO> effects = new();

    public CombatActionSO GetEffect(ComboName type)
    {
        if(effects.TryGetValue(type, out CombatActionSO value))
            return value;

        Debug.Log("EFFECT NOT FOUND returning default");
        return GetEffect(ComboName.Miss);
    }



    public Dictionary<ComboName, CombatActionSO> Effects => effects;
}
