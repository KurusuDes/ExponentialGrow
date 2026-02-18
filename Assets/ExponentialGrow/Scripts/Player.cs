
using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

public class Player : MonoBehaviour ,IDamageable
{

    [ShowInInspector] public List<IAttackModifier> Modifiers = new();//->los modificadores deberian estar en el player?

    
    void Start()
    {
        GameManager.Instance.SetPlayer( this);
        EffectManager.OnCastEffect += OnCastEffect;
    }

   

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnCastEffect(Effect effect)
    {
        Effect newEffect = effect.ActiveEffect(Modifiers);

        //print("Effect Casted");
        CombatSystem.OnEffectCasted?.Invoke(newEffect, this);
    }
    public void OnTakeDamage(int amount, GenreType genretype, IDamageable sender)
    {
        BaseEnemy obj = sender as BaseEnemy;
        


        print("Player take damage Damage:" + amount + " , Genre: " + genretype + " , From: " + obj.EntityName);
    }
}
