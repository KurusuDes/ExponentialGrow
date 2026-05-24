using System;
using UnityEngine;

[Serializable]
public class EntityAction 
{
    private CombatActionSO source;

    private AttackType attackType;
    private StanceType stanceType;
    private GenreType genreType;

    private int currentDamage;
    private int currentDefense;

    public EntityAction(CombatActionSO data)
    {
        source = data;

        attackType = data.AttackType;
        stanceType = data.StanceType;
        genreType = data.GenreType;

        currentDamage = data.BaseDamage;
        currentDefense = data.BaseDefense;
    }
    //->aqui se deberia llamar la raza el tipo y calculo de da�o
    //algo como una clase estatica damagecalculator
    public void ApplyEffect(IDamageable target,IDamageable sender)
    {
        target.OnTakeDamage(currentDamage, genreType, sender);
    }
    public AttackType AttackType => attackType;
    public StanceType StanceType => stanceType;
    public GenreType GenreType => genreType;

    public int BaseDamage => currentDamage;
    public int BaseDefense => currentDefense;

    public CombatActionSO Source => source;

   

}
