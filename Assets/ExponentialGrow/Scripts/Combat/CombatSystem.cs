using Sirenix.OdinInspector;
using System;
using System.Collections;
using UnityEngine;

public class CombatSystem : MonoBehaviour
{
    //public EnemysDatabaseSO EnemyDatabaseSO;
    public EntityUI CombatSystemUI;

    public BaseEnemy EnemyPrefab;


    public BaseEnemy CurrentTarget;


    public static Action<Reward> OnEnemyDefeated;
    public static Action<Effect,IDamageable> OnEffectCasted;
    public static Action<BaseEnemy> OnInstantiateEnemy;
    public static Action OnCreateEnemy;
    
    void Start()
    {
        OnEnemyDefeated += RewardMechanism;
        OnEffectCasted += OnRegisterAttack;
        OnEnemyDefeated += SpawnNextEnemy;
        OnCreateEnemy += CreateEnemyTest;
    }

    private void OnDestroy()
    {
        OnEnemyDefeated -= RewardMechanism;
        OnEffectCasted -= OnRegisterAttack;
        OnEnemyDefeated -= SpawnNextEnemy;
        OnCreateEnemy -= CreateEnemyTest;
    }




    // Update is called once per frame
    void Update()
    {
        
    }
    [Button]
    public void SpawnEnemy(EnemySO data)
    {
        if(CurrentTarget) return;
        BaseEnemy enemy = Instantiate(EnemyPrefab,transform);
        enemy.Set(data);
        CurrentTarget = enemy;

        OnInstantiateEnemy?.Invoke(CurrentTarget);
    }

    private void RewardMechanism(Reward reward)
    {
        print("Has ganado: " + reward.Gold + " de oro. \n" + "Has ganado: " + reward.ExperiencePoints + " de experiencia.");


        ResetCombat();
        
    }
    private void OnRegisterAttack(Effect effect,IDamageable sender)
    {
        if (!CurrentTarget)
        {
            print("No existe target");
            return;
        }
        print("Recive Damage");
        CurrentTarget?.OnTakeDamage(effect.Damage,effect.genreType,sender);
    }
    public void ResetCombat()
    {
        if (!CurrentTarget)
            return;

        Destroy(CurrentTarget.gameObject);
        CurrentTarget = null;
        //SpawnEnemy(EnemyDatabaseSO.GetEnemy());
    }

    private void SpawnNextEnemy(Reward reward)
    {
        StartCoroutine(TriggerAfterDelay());
    }
    public IEnumerator TriggerAfterDelay()
    {
        yield return new WaitForSeconds(2f);
        OnCreateEnemy?.Invoke();
    }


    #region Helpers
    [Button]
    public void CreateEnemyTest()
    {
        ResetCombat();
        SpawnEnemy(GameManager.Instance.GetRandomEnemy());
    }
    #endregion
}
