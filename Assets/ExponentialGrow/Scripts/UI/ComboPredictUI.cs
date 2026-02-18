
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ComboPredictUI : MonoBehaviour
{
    public Transform currentComboHolder;
    public Transform predictComboHolder;

    public Transform comboRowPrefab;
    public ArrowKeyUI arrowKeyUIPrefab;

    public List<GameObject> currentKeys = new();
    public List<GameObject> currentHolders = new();
    void Start()
    {
        ComboManager.OnComboPredictTrigger += DrawKeycaps;
        ComboManager.OnComboEnded += ClearSlots;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
    private void DrawKeycaps(List<KeyBaseData> currentCombo, List<KeyBaseData> predictCombos)
    {
        ClearSlots();
        FillSlot(currentCombo, currentComboHolder, SpriteType.Full);

        FillSlot(predictCombos, predictComboHolder, SpriteType.Half);
       /* foreach (var combo in predictCombos)
        {
            Transform row = Instantiate(comboRowPrefab, predictComboHolder);
            FillSlot(predictCombos, predictComboHolder, SpriteType.Half);
            currentHolders.Add(row.gameObject);
        }*/
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
    }
  /*  private void DrawKeycaps(List<KeyCapType> currentCombo, List<KeyBaseData> predictCombos)
    {
        ClearSlots();
        FillSlot(currentCombo, currentComboHolder,SpriteType.Full);
        foreach (var combo in predictCombos)
        {
            Transform row = Instantiate(comboRowPrefab, predictComboHolder);
            FillSlot(predictCombos, row, SpriteType.Half);
            currentHolders.Add(row.gameObject);
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
    }*/
    public void FillSlot(List<KeyBaseData> caps, Transform slot ,SpriteType type)
    {
        foreach (var cap in caps)
        {
            ArrowKeyUI arrowKey = Instantiate(arrowKeyUIPrefab, slot).GetComponent<ArrowKeyUI>();
            arrowKey.Set(cap.KeyCap,type,cap.comboType);
            currentKeys.Add(arrowKey.gameObject);
        }
    }

    public void ClearSlots()
    {
        foreach (GameObject cap in currentKeys)
            Destroy(cap);

        currentKeys.Clear();

        foreach (GameObject holder in currentHolders)
            Destroy(holder);
        currentHolders.Clear();
    }
}
