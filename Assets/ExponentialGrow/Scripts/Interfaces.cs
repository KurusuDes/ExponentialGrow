
using UnityEngine;

public interface IAttackModifier
{
    public void ApplyModifier(Effect effect )
    {

    }
}
public interface IDamageable
{
    public void OnTakeDamage(int amount ,GenreType genretype, IDamageable sender)
    {

    }
    
}