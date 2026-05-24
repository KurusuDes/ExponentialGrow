using UnityEngine;
using Sirenix.OdinInspector;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public CombatSystem CombatSystem;
    [SerializeField] private Player player;

    [InlineEditor] public KeyCapDatabaseSO KeyCapDatabase;



    public SeedRandom seedRandom = new();
    //[InlineEditor] public ComboDatabaseSO ComboDatabase;
    public EnemysDatabaseSO EnemyDatabaseSO;
    // [InlineEditor] public EffectDatabaseSO EffectDatabase;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);


        seedRandom.Initialize();
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void SetPlayer(Player player)
    {
        this.player = player;   
    }
    public SeedRandom SeedRandom => seedRandom;
    public static Player Player => Instance.player;

    public EnemySO GetRandomEnemy() => EnemyDatabaseSO.GetEnemy(seedRandom.GetRandomEnemy());

    public bool ActiveEnemy(EnemySO enemy) => seedRandom.ChanceToActiveEnemy(enemy);
}
