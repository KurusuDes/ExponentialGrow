using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputs : MonoBehaviour
{
    public InputSystem_Actions inputs;

    public static Action<KeyCapType> OnAttack;
    public static Action OnInteraction;


    Dictionary<string, KeyCapType> map = new() {
    { "Up", KeyCapType.Up },
    { "Down", KeyCapType.Down },
    { "Left", KeyCapType.Left },
    { "Right", KeyCapType.Right }
};

    private void Awake()
    {
        inputs = new();
    }
    private void OnEnable()
    {
        inputs.Enable();
        inputs.PlayerDungeon.Up.performed += OnActionPerformed;
        inputs.PlayerDungeon.Down.performed += OnActionPerformed;
        inputs.PlayerDungeon.Left.performed += OnActionPerformed;
        inputs.PlayerDungeon.Right.performed += OnActionPerformed;

        inputs.PlayerDungeon.Interaction.performed += OnInteractionPerformed;
    }
    private void OnDisable()
    {
        inputs.PlayerDungeon.Up.performed -= OnActionPerformed;
        inputs.PlayerDungeon.Down.performed -= OnActionPerformed;
        inputs.PlayerDungeon.Left.performed -= OnActionPerformed;
        inputs.PlayerDungeon.Right.performed -= OnActionPerformed;

        inputs.PlayerDungeon.Interaction.performed -= OnInteractionPerformed;
        inputs.Disable();
    }

    private void OnInteractionPerformed(InputAction.CallbackContext context)
    {
        OnInteraction?.Invoke();
    }

    private void OnActionPerformed(InputAction.CallbackContext context)
    {
      //  print(context.action.name);
        OnAttack?.Invoke(map[context.action.name]);

    }

   
    void Start()
    {
        
    }
}
