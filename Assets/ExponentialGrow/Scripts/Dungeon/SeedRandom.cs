using Sirenix.OdinInspector;
using System;
using System.Diagnostics;

//[Serializable,InlineEditor]
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
        LootGenerator  = new Random(seed);
        RoomGenerator  = new Random(seed);

        Debug.WriteLine($"[SeedRandom] Initialized with seed: {seed}");
    }
    /// <summary>
    /// Returns a Number from 1 to 100
    /// </summary>
    /// <returns>int</returns>
    public int GetRandomEnemy()
    {
        return EnemyGenerator.Next(1,100);
    }
    public int EnemyRange(int min, int max)
    {
        return EnemyGenerator.Next(min, max);
    }
    public int GetRandomLoot()
    {
        return LootGenerator.Next(1,100);
    }
    public int GetRandomRoom()
    {
        return RoomGenerator.Next(1,100);
    }
}
