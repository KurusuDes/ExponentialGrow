using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Reward 
{
    [SerializeField, Range(0,100)] private int gold;
    [SerializeField, Range(0, 100)] private float experiencePoints;
    [SerializeField] private List<GameObject> items;



    public int Gold => gold;
    public float ExperiencePoints => experiencePoints;
    public List<GameObject> Items => items;
}
