using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "CombatActionSO", menuName = "Scriptable Objects/CombatActionSO"), InlineEditor]
public class CombatActionSO : ScriptableObject
{
    [SerializeField] private GameObject vfx;

    [SerializeField] private AttackType attackType;
    [SerializeField] private StanceType stanceType;
    [SerializeField] private GenreType genreType = GenreType.None;
    [SerializeField] private int baseDamage = 1000;
    [SerializeField] private int baseDefense = 1000;

    [ReadOnly] public int abilityValue;

    private void OnValidate()
    {
        abilityValue = baseDamage + baseDefense / 2;
    }

    public GameObject Vfx => vfx;
    public AttackType AttackType => attackType;
    public StanceType StanceType => stanceType;
    public GenreType GenreType => genreType;
    public int BaseDamage => baseDamage;
    public int BaseDefense => baseDefense;
}
