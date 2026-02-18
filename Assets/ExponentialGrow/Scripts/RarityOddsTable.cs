using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System.Collections.Generic;
using UnityEngine;

[InlineEditor]
public class RarityOdds
{
    public RarityType rarity;
    [Range(0, 100)]
    public int weight;

    public RarityOdds(RarityType type , int weight)
    {
        rarity = type;
        this.weight = weight;
    }
}

//[InlineEditor]
public class RarityOddsTable 
{
    [OdinSerialize] public List<RarityOdds> odds = new();
    private const int TOTAL = 100;

    private void OnValidate()
    {
       
        AutoBalance();
    }

    void AutoBalance()
    {
        if (odds.Count == 0) return;

        int sum = 0;
        foreach (var o in odds)
            sum += o.weight;

        if (sum == TOTAL) return;

        int diff = TOTAL - sum;

        // Ajustar todas MENOS la que el usuario tocó
        for (int i = odds.Count - 1; i >= 0 && diff != 0; i--)
        {
            if (odds[i].weight <= 0 && diff < 0) continue;

            int change = Mathf.Clamp(diff, -odds[i].weight, 1);
            odds[i].weight += change;
            diff -= change;
        }
    }

    [Button]
    public void SetTable()
    {
        odds.Clear();
       
        odds.Add(new(RarityType.Legendary, 5));
        odds.Add(new(RarityType.Epic, 10));
        odds.Add(new(RarityType.Rare, 20));
        odds.Add(new(RarityType.Uncommon, 30));
        odds.Add(new(RarityType.Common, 35));

        odds.Sort((a, b) => b.weight.CompareTo(a.weight));
    }

    public RarityType GetRarityByOdds(int roll)
    {
        odds.Sort((a, b) => b.weight.CompareTo(a.weight));

        int cumulative = 0;
        foreach (var o in odds)
        {
            cumulative += o.weight;
            if (roll < cumulative)
            {
                return o.rarity;
            }
        }
        return RarityType.Common; 
    }
}
