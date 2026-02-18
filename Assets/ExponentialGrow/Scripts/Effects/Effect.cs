
using System.Collections.Generic;
using UnityEngine;

public class Effect
{
    public EffectSO effectSO;
    public GenreType genreType;
    public ComboName comboType;
    public int Damage;
    public int Shield;
   
    public Effect(EffectSO effectSO, ComboName comboType)
    {
        this.effectSO = effectSO;
        this.comboType = comboType;
        genreType = effectSO.GenreType;
        Damage = effectSO.BaseDamage;
        Shield = effectSO.BaseDefense;
    }

    public Effect ActiveEffect(List<IAttackModifier> modifiers)
    {
        foreach (var modifier in modifiers)
        {
            modifier.ApplyModifier(this);
        }
        return this;
        
    }
    
}
