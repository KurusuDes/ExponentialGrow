using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;



[CreateAssetMenu(fileName = "EnemySO", menuName = "Scriptable Objects/EnemySO")]
public class EnemySO : ScriptableObject
{
    [FoldoutGroup("CoreSettings") ,SerializeField] private string entityName;
   // [FoldoutGroup("CoreSettings") ,SerializeField] private GameObject prefab;
    [FoldoutGroup("CoreSettings") ,SerializeField] private RacesType race;
    [FoldoutGroup("CoreSettings"), SerializeField, Range(1, 100)] private int chanceToInvoke;
    [FoldoutGroup("CoreSettings") ,SerializeField, Range(1,10)] private int hitpoints = 1;
    [FoldoutGroup("CoreSettings") ,SerializeField, Range(0, 20)] private float actionSpeed;
    [FoldoutGroup("CoreSettings") ,SerializeField] private List<CombatActionSO> actions;
    [FoldoutGroup("CoreSettings"), SerializeField] private Reward reward;
    [FoldoutGroup("Visuals"), SerializeField] private Sprite icon;

    [SerializeField,ReadOnly] private int entityValue;

    private void OnValidate()
    {
        entityValue = GetAbilitiesValue();
    }
    private int GetAbilitiesValue()
    {
        int abilityValue = 0;

        foreach (var action in actions)
        {
            abilityValue += action.abilityValue;
        }
        abilityValue /= actions.Count;

        return abilityValue;
    }


    public string EntityName => entityName;
    public RacesType Race => race;
    public int HitPoints => hitpoints;
    public float ActionSpeed => actionSpeed;
    public List<CombatActionSO> Actions => actions;
    public Reward Reward => reward;
    public Sprite Icon => icon;
    public int ChanceToInvoke => chanceToInvoke;
}
