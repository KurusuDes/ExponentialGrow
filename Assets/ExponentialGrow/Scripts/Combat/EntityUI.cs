using DamageNumbersPro;
using System;
using UnityEngine;
using UnityEngine.UI;


public class EntityUI: MonoBehaviour
{
    public HealthBarUI healthBarUI;
    public DamageNumber entityAttackPrefab;
    public RectTransform holder;
    public Slider ActionSlider;

    public void Start()
    {
        CombatSystem.OnInstantiateEnemy += BindEnemy;
    }

    public void BindEnemy(BaseEnemy enemy)
    {
        enemy.OnActionStarted += ResetSlider;
        enemy.OnActionProgress += UpdateSlider;
        enemy.OnAttack += ShowAttackVisuals;
        enemy.OnDefeated += ResetSlider;

        healthBarUI.Set(enemy.Data.HitPoints, enemy.Data.HitPoints);
        enemy.OnGetHit += healthBarUI.Set;
        enemy.OnDefeated += healthBarUI.ResetHealthBar;
    }

    private void ShowAttackVisuals(EntityAction action)
    {
        DamageNumber popupText = entityAttackPrefab.SpawnGUI(holder,holder.anchoredPosition,action.BaseDamage);

        popupText.leftText = action.AttackType.ToString();
        popupText.rightText = action.GenreType.ToString();
    }

    private void ResetSlider(float duration)
    {
        ActionSlider.value = 0f;
    }
    private void ResetSlider()
    {
        ActionSlider.value = 0f;
    }

    private void UpdateSlider(float value)
    {
        ActionSlider.value = value;
    }
}
