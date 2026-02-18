using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "EnemyDatabaseSO", menuName = "Scriptable Objects/EnemyDatabaseSO")]
public class EnemysDatabaseSO : SerializedScriptableObject
{
    [OdinSerialize] private Dictionary<RarityType, List<EnemySO>> enemiesData;

    [OdinSerialize] public RarityOddsTable rarityOddsTable = new ();

  
    public EnemySO GetEnemy()
    {
        int randomEnemyNumber = GameManager.Instance.seedRandom.GetRandomEnemy();
        RarityType type = rarityOddsTable.GetRarityByOdds(randomEnemyNumber);
        int randomEnemyIndex = GameManager.Instance.seedRandom.EnemyRange(0, enemiesData[type].Count);

        return enemiesData[type][randomEnemyIndex];
    }
    public EnemySO GetEnemy(int randomSeed)//->Completar
    {
        RarityType type = rarityOddsTable.GetRarityByOdds(randomSeed);
      

        int randomEnemyIndex = GameManager.Instance.seedRandom.EnemyRange(0, enemiesData[type].Count);
        Debug.Log("Rariry type: " + type.ToString() + " Number: " + randomEnemyIndex);
        return enemiesData[type][randomEnemyIndex];
    }
}
