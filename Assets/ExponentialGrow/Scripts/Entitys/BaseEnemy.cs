using MoreMountains.Feedbacks;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BaseEnemy : MonoBehaviour ,IDamageable
{
    [DisableInEditorMode,DisableInPlayMode] public EnemySO Data;

    [FoldoutGroup("Settings"),SerializeField] private string entityName;
    [FoldoutGroup("Settings"),SerializeField] private RacesType race;
    [FoldoutGroup("Settings"), SerializeField] private EnemyState state = EnemyState.Idle;
    [FoldoutGroup("Settings"),SerializeField] private float speed;
    [FoldoutGroup("Settings"),SerializeField] private int hitpoints = 1;
    [FoldoutGroup("Settings"),SerializeField] private List<CombatActionSO> actionList = new();
    [FoldoutGroup("Settings"), SerializeField] private Reward reward;
    [FoldoutGroup("Visuals"),SerializeField] private SpriteRenderer sprite;


    private Queue<EntityAction> actions = new();
    [SerializeField] private EntityAction currentAction;
    #region Actions
    public Action OnSpawn;
    public Action<EntityAction> OnAttack;
    public Action<int,int> OnGetHit;
    public Action OnDefeated;

    public Action<float> OnActionProgress; // 0..1
    public Action<float> OnActionStarted;  // duraci�n
    #endregion
    #region Feedabacks
    public MMF_Player OnSpawnFeedback;

    public MMF_Player OnPrepareAttackFeedback;
    public MMF_Player OnChargeAttackFeedback;
    public MMF_Player OnAboutToAttackFeedback;

    public MMF_Player OnAttackFeedback;

    public MMF_Player OnHitFeedback;
    public MMF_Player OnDefeatedFeedback;
    #endregion
    [Button]
    public void Set(EnemySO data)
    {
        Data = data;
        entityName = data.EntityName;
        hitpoints = data.HitPoints;
        actionList = new(data.Actions);
        race = data.Race;
        reward = data.Reward;
        speed = data.ActionSpeed;
       // sprite.sprite = data.Icon;
        foreach (var action in actionList)
        {
            EntityAction entityAction = new(action);
            actions.Enqueue(entityAction);
            //actions.Reverse();
        }
    }
    private void Awake()
    {
        OnSpawn += OnSpawnEvent;
        OnAttack += OnAttackEvent;
        OnGetHit += OnHitEvent;
        OnDefeated += OnDefeatedEvent;
        OnGetHit += ActionResolver;

    }

 

    void Start()
    {

        if (Data != null)
        {
            Set(Data);
        }

      //  StartCoroutine(ActionTimer(speed));
        currentAction = actions.Peek();

        
    }
    //->hook this to an event that pass the turn once the player end his combo or fail his combo
    [Button]
    public void ActiveEntity()
    {
        if (state == EnemyState.Idle)
        {
            if (!GameManager.Instance.ActiveEnemy(Data))
                return;
        }
        print("Enemigo se activo");

        ChangeState(GetNextState(state));
    }
    private void ChangeState(EnemyState newState)
    {
        if (state == newState)
            return;

        ExitState(state);

        state = newState;

        EnterStateVFX(state);
        EnterState(state);

    }
    private void EnterState(EnemyState state)
    {
            switch (state)
            {
                case EnemyState.Idle:
                    break;
                case EnemyState.PreparingAttack:
                    break;
                case EnemyState.ChargingAttack:
                    break;
                case EnemyState.AboutToAttack:
                    break;
                case EnemyState.Attacking:
                    ActionsMechanism();
                    break;
                case EnemyState.Defeated:
                    break;
                default:
                    break;
        }
    }
    private void EnterStateVFX(EnemyState state)
    {
        switch (state)
        {
            case EnemyState.PreparingAttack:
                OnPrepareAttackFeedback.PlayFeedbacks();
                break;
            case EnemyState.ChargingAttack:
                OnChargeAttackFeedback.PlayFeedbacks();
                break;
            case EnemyState.AboutToAttack:
                OnAboutToAttackFeedback.PlayFeedbacks();
                break;
            case EnemyState.Attacking:
                OnAttackFeedback.PlayFeedbacks();
                break;
        }
    }

    private void ExitState(EnemyState state)
    {
        StopAllVFX();
    }
    public void StopAllVFX()
    {
       // if(OnChargeAttackFeedback.IsPlaying)
            OnChargeAttackFeedback.StopFeedbacks();
        //if(OnPrepareAttackFeedback.IsPlaying)
            OnPrepareAttackFeedback.StopFeedbacks();
        //if(OnAboutToAttackFeedback.IsPlaying)
            OnAboutToAttackFeedback.StopFeedbacks();
       // if(OnAttackFeedback.IsPlaying)
            OnAttackFeedback.StopFeedbacks();

        /*ParticleSystem ps = new();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);*/
    }
    //ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    private EnemyState GetNextState(EnemyState current)
    {
        switch (current)
        {
            case EnemyState.Idle:
                return EnemyState.PreparingAttack;

            case EnemyState.PreparingAttack:
                return EnemyState.ChargingAttack;

            case EnemyState.ChargingAttack:
                return EnemyState.AboutToAttack;

            case EnemyState.AboutToAttack:
                return EnemyState.Attacking;

            case EnemyState.Attacking:
                return EnemyState.Idle;

            default:
                return current;
        }
    }
    void Update()
    {
        
    }

    #region ActionTimer
    /*
      public IEnumerator ActionLoop(float delay)
{
    while (true)
    {
        yield return new WaitForSeconds(delay);
        ActionsMechanism();
    }
}
     */
   /* public void JumpToNextAction()
    {
        print("SkipTo next action");
        StopAllCoroutines();
        ActionsMechanism(true);
        StartCoroutine(ActionTimer(speed));
    }*/
    public IEnumerator ActionTimer(float delay)
    {
        float elapsed = 0f;
        OnActionStarted?.Invoke(delay);

        while (elapsed < delay)
        {
            elapsed += Time.deltaTime;
            float normalized = elapsed / delay;
            OnActionProgress?.Invoke(normalized);
            yield return null;
        }
        ActionsMechanism();
        StartCoroutine(ActionTimer(delay));
    }
    private void ActionsMechanism(bool alteredState = false)
    {
        EntityAction action = actions.Dequeue();

        ExcecuteAction(action);

        actions.Enqueue(action);

        currentAction = actions.Peek();
    }
    #endregion
    #region Events
    private void OnSpawnEvent()
    {
        OnSpawnFeedback.PlayFeedbacks();
    }
    private void OnAttackEvent(EntityAction action)
    {
        OnAttackFeedback.PlayFeedbacks();
    }
    private void OnHitEvent(int hitpoints, int maxHitpoints)
    {
        OnHitFeedback.PlayFeedbacks();
    }

    private void OnDefeatedEvent()
    {
        OnDefeatedFeedback.PlayFeedbacks();
        Defeated();
    }

    private void ActionResolver(int hitpoints, int maxHitpoints)
    {
        switch (currentAction.StanceType)
        {
            case StanceType.None:
                break;
            case StanceType.Steady:

                break;
            case StanceType.Unstable:
               
                break;
        }
    }
    #endregion
    #region Intefaces
    [Button]
    public void OnTakeDamage(int amount,GenreType genreType = GenreType.None, IDamageable sender = null)
    {
        //->llamar a clase que lea el da�o y el typo y o reste da�o o duplique el da�o, Idea: las habilidades tienen cd si spameas una habilidad pierde el bono de genero
        hitpoints = (int)MathF.Max((hitpoints - amount), 0);

        if (hitpoints <= 0)
            OnDefeated?.Invoke();
        else
            OnGetHit?.Invoke(hitpoints,Data.HitPoints);
       
    }
    #endregion
    public void ExcecuteAction(EntityAction action)
    {
        action.ApplyEffect(GameManager.Player, this);
        OnAttack?.Invoke(action);

        print(entityName + " - Realizo: " + action.AttackType.ToString());
    }

    public void Defeated()
    {
        print(entityName + " a sido derrotado");
        CombatSystem.OnEnemyDefeated?.Invoke(reward);
        StopAllCoroutines();
    }
    private void OnDestroy()
    {
        print("Me destruyeron");
    }


    public string EntityName => entityName;
    public int Hitpoints => hitpoints;
}
