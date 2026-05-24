
using System.Collections.Generic;
using UnityEngine;

public class Effect
{
    public CombatActionSO effectSO;
    public GenreType genreType;
    public ComboName comboType;
    public int Damage;
    public int Shield;

    public Effect(CombatActionSO data, ComboName comboType)
    {
        this.effectSO = data;
        this.comboType = comboType;
        genreType = data.GenreType;
        Damage = data.BaseDamage;
        Shield = data.BaseDefense;
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
