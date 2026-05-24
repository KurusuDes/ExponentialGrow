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
        int roll = GameManager.Instance.seedRandom.GetRandomEnemy();
        return ResolveEnemy(roll);
    }

    public EnemySO GetEnemy(int randomSeed)
    {
        return ResolveEnemy(randomSeed);
    }

    private EnemySO ResolveEnemy(int roll)
    {
        RarityType type = rarityOddsTable.GetRarityByOdds(roll);

        if (!HasEnemies(type))
        {
            Debug.LogWarning($"[EnemyDatabase] Sin enemigos para {type}, usando Common.");
            type = RarityType.Common;
        }

        if (!HasEnemies(type))
        {
            Debug.LogError("[EnemyDatabase] La base de datos no tiene enemigos registrados.");
            return null;
        }

        int index = GameManager.Instance.seedRandom.EnemyRange(0, enemiesData[type].Count);
        return enemiesData[type][index];
    }

    private bool HasEnemies(RarityType type)
    {
        return enemiesData.ContainsKey(type) && enemiesData[type] != null && enemiesData[type].Count > 0;
    }
}
