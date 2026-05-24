using DamageNumbersPro;
using System;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    public EffectDatabaseSO EffectDatabase;

    public static Action<Effect> OnCastEffect;

    public DamageNumber effectPrefab;
    
    private void Awake()
    {
        ComboManager.OnComboTypeTrigger += OnEffectTrigger;
        OnCastEffect += ShowVisualCombo;
    }

    private void OnDestroy()
    {
        ComboManager.OnComboTypeTrigger -= OnEffectTrigger;
        OnCastEffect -= ShowVisualCombo;
    }

  

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnEffectTrigger(ComboName type)
    {
        CombatActionSO data = EffectDatabase.GetEffect(type);
        Effect effect = new(data, type);
        OnCastEffect?.Invoke(effect);
    }
    private void ShowVisualCombo(Effect effect)
    {
        DamageNumber popupText = effectPrefab.Spawn(transform.position, effect.Damage);
        popupText.topText = effect.comboType.ToString();
        popupText.bottomText = effect.genreType.ToString();
    }

}
