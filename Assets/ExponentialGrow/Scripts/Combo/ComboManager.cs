using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;


public class ComboManager : MonoBehaviour
{
    public static ComboManager Instance;

    [InlineEditor] public ComboDatabaseSO ComboDatabase;

    public float maxInputDelay;
    public float currentComboTimer;
    public bool OnCombo;



    public Queue<KeyCapType> comboCaps = new();


    public static Action<Queue<KeyCapType>> OnComboTrigger;
    public static Action<Queue<KeyCapType>> OnComboQueueAdded;
    public static Action<ComboName> OnComboTypeTrigger;
    public static Action<List<KeyBaseData>, List<KeyBaseData>> OnComboPredictTrigger;
    public static Action OnComboEnded;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
           // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        //PlayerInputs.OnAttack += ShowVisualAttack;
        PlayerInputs.OnAttack += RegisterInput;
        OnComboTrigger += GetComboType;
        OnComboQueueAdded += GetPosiblePaths;
    }

   

    void Start()
    {
        
    }
    void Update()
    {
        OnComboMechanism();
    }
    public void OnComboMechanism()
    {
        if (!OnCombo) return;
        
        currentComboTimer += Time.deltaTime;

        if (currentComboTimer >= maxInputDelay)
        {
            Queue<KeyCapType> caps = new(comboCaps);
            OnComboTrigger?.Invoke(caps);

            ClearCombo();
        }
    }
    private void RegisterInput(KeyCapType cap)
    {
        if (!OnCombo)
        {
            OnCombo = true;
           // print("ComboStart");
        }
        comboCaps.Enqueue(cap);

        OnComboQueueAdded?.Invoke(new Queue<KeyCapType>(comboCaps));
        currentComboTimer = 0;
    }
    private void ShowVisualAttack(KeyCapType cap)
    {
        GameObject keycap = Instantiate(GameManager.Instance.KeyCapDatabase.GetKeycapPrefab(cap));
        keycap.GetComponent<AttackDirectionVisualController>().Set(cap);
        keycap.transform.position = new Vector3(UnityEngine.Random.Range(-5, 5), UnityEngine.Random.Range(-5, 5), 0);

    }
    private void GetComboType(Queue<KeyCapType> combo)
    {
       // print(combo.Count);
        string comboPrint = "{ ";
        foreach (KeyCapType cap in combo)
        {
            comboPrint += cap.ToString() + " , ";
        }
      //  print(comboPrint + "}");


        ComboName type = ComboDatabase.GetComboType(combo);

        //print("Combo type : " + type);
        OnComboTypeTrigger?.Invoke(type);
    }
    public void ClearCombo()
    {
        OnCombo = false;
        currentComboTimer = 0;
        comboCaps.Clear();
        OnComboEnded?.Invoke();
    }


    private void GetPosiblePaths(Queue<KeyCapType> queue)//-> comboPredictUI
    {
        List<KeyCapType> queueToList = new(queue);

        List<KeyBaseData> currentCombo = new();
        foreach (var cap in queueToList)
        {
            currentCombo.Add(new(cap,ComboType.None));
        }
        List<KeyBaseData> predictCombos =  ComboDatabase.GetFirstLevelBaseData(ComboDatabase.GetComboNode(queue));
    

        OnComboPredictTrigger?.Invoke(currentCombo, predictCombos);
    }
}
