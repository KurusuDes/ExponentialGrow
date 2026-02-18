using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

[CreateAssetMenu(fileName = "EffectSO", menuName = "Scriptable Objects/EffectSO"),InlineEditor]
public class EffectSO : ScriptableObject
{
    [SerializeField] private GenreType genreType = GenreType.None;
    
    [SerializeField] private int baseDamage = 1000;
    [SerializeField] private int baseDefense = 1000;

    public GenreType GenreType=> genreType;
    public int BaseDamage => baseDamage;
    public int BaseDefense => baseDefense;



}
