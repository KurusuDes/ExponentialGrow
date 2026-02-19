
using System;

public enum KeyCapType
{
    Up,
    Down,
    Left,
    Right,
    Item
}

public enum ComboType
{
    None,
    Offensive,
    Defensive,
    Utility,
    Control
}

public enum ComboName
{
    Miss,
    Basic,
    //->Rock
    HardRockDrift,
    AltoAmperage,
    SmoothDrift,
    SourRockDrift,
    //<-
    ChargeFront,
    Parry,
    DodgeLeft,
    DodgeRight,
    
}
[Flags]
public enum ModifierType
{
    None = 0,


}
[Flags]
public enum GenreType
{
    None = 0,               // Ninguna raza afín
    OutOfTune = 1 << 0,    // Estado negativo temporal
    Clasic = 1 << 1,        // Afinidad: Vampire / Debilidad: Pop
    Rock = 1 << 2,          // Afinidad: Ogre / Debilidad: Clasic
    HipHop = 1 << 3,        // Afinidad: Orcs / Debilidad: Rock
    Salsa = 1 << 4,         // Afinidad: Fairy / Debilidad: Metal
    Electronic = 1 << 5,    // Afinidad: Undead / Debilidad: Folk
    Folk = 1 << 6,          // Afinidad: Goblin / Debilidad: Electronic
    Metal = 1 << 7,         // Afinidad: Ogre / Debilidad: Pop
    Pop = 1 << 8,           // Afinidad: Fairy / Debilidad: Metal

    All = Clasic | Rock | HipHop | Salsa | Electronic | Folk | Metal | Pop

    /*
     Clasic   Folk + Electronic

Salsa   Folk + HipHop

Metal   Rock + Electronic

Pop   HipHop + Electronic
     * */
}

public enum RacesType
{
    None,   // -> Afinidad: None / Debilidad: None

    Goblin, // -> Afinidad: Folk / Debilidad: Electronic
    Undead, // -> Afinidad: Electronic / Debilidad: Folk
    Ogre,   // -> Afinidad: Rock, Metal / Debilidad: Clasic
    Vampire,// -> Afinidad: Clasic / Debilidad: Pop
    Fairy,  // -> Afinidad: Pop, Salsa / Debilidad: Metal
    Orcs    // -> Afinidad: HipHop / Debilidad: Rock
}
public enum AttackType
{
    None,
    Bludgeoning,//Maza
    Piercing,//Daga
    Slashing,//Espadas , hachas
}
public enum AttackState
{

}
public enum StanceType
{
    None,
    Steady,
    Unstable,
    Charger
}
public enum EnemyState
{
    Idle,
    PreparingAttack,
    ChargingAttack,
    AboutToAttack,
    Attacking,
    Defeated
}

public enum GameState
{
    None,
    Explore,
    Combat
}

public enum RarityType
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}

//->Room Stuff
public enum RoomType
{
    None,
    Start,
    Combat,
    Treasure,
    Boss,
    Shop,
    Exit
}

public enum RoomState
{
    Unvisited,
    Visited,
    Cleared,

}