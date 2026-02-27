using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using Sirenix.Serialization;

public enum HeartVisualLvl
{
    Lvl1,
    Lvl2,
    Lvl3,
    Lvl4,
    Lvl5

}
public enum HeartSpaces
{
    One,
    Two,
    Three
}


public class HealthBarView : SerializedMonoBehaviour
{
    [SerializeField] private GameObject hearthPrefab;
    public HeartSpaces HeartSpaces = HeartSpaces.One;


    [SerializeField] private Dictionary<HeartVisualLvl, Material> heartVisuals = new();

    [SerializeField] private Dictionary<HeartSpaces, List<Vector2>> heartOffsets = new();


    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
