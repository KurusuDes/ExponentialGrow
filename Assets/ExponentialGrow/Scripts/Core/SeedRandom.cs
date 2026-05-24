using System;
//using UnityEngine;

public class SeedRandom
{
    public Random EnemyGenerator;
    public Random LootGenerator;
    public Random RoomGenerator;

    public void Initialize(int seed = 0)
    {
        if (seed == 0)
            seed = (int)DateTime.Now.Ticks;

        EnemyGenerator = new Random(seed);
        LootGenerator  = new Random(seed + 1);
        RoomGenerator  = new Random(seed + 2);

       // Debug.Log($"[SeedRandom] Initialized with seed: {seed}");
    }

    /// <summary>
    /// Returns a Number from 1 to 100
    /// </summary>
    public bool ChanceToActiveEnemy(EnemySO so)
    {
        return EnemyGenerator.Next(1, 100) < so.ChanceToInvoke;
    }

    public int GetRandomEnemy()
    {
        return EnemyGenerator.Next(1, 100);
    }

    public int EnemyRange(int min, int max)
    {
        return EnemyGenerator.Next(min, max);
    }

    public int GetRandomLoot()
    {
        return LootGenerator.Next(1, 100);
    }

    public int GetRandomRoom()
    {
        return RoomGenerator.Next(1, 100);
    }

    public int RoomRange(int min, int max)
    {
        return RoomGenerator.Next(min, max);
    }
}
