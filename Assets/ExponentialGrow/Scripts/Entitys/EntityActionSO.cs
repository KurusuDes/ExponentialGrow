using Sirenix.OdinInspector;
using UnityEngine;

//-> Considerar unificar el EffectSO y el EntityActionSO en uno solo hacen basicamente lo mismo

[CreateAssetMenu(fileName = "ActionSO", menuName = "Scriptable Objects/ActionSO"),InlineEditor]
public class EntityActionSO : ScriptableObject
{
    [SerializeField] private GameObject Vfx;

    [SerializeField] private AttackType attackType;
    [SerializeField] private StanceType stanceType;
    [SerializeField] private GenreType genreType;
    [SerializeField] private int baseDamage = 1000;
    [SerializeField] private int baseDefense = 1000;

    [ReadOnly] public int abilityValue;

    public void OnValidate()
    {
        abilityValue = baseDamage + baseDefense / 2;
    }


    public AttackType AttackType => attackType;
    public StanceType StanceType => stanceType;
    public GenreType GenreType => genreType;
    public int BaseDamage => baseDamage;
    public int BaseDefense => baseDefense;
}
