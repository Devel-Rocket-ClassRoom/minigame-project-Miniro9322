using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    private static readonly int Attack1 = Animator.StringToHash("Attack1");
    private static readonly int Attack2 = Animator.StringToHash("Attack2");
    private static readonly int Rush = Animator.StringToHash("Rush");

    [SerializeField]
    private PlayerInput playerInput;
    private InputAction debug;
    private InputAction debugOff;
    private InputAction attack;
    private InputAction attack2;
    private InputAction rush;
    [SerializeField]
    private Animator animator;
    [SerializeField] private Boss boss;

    private void Start()
    {
        playerInput.SwitchCurrentActionMap("Player");
        playerInput.actions.FindActionMap("Debug").Disable();
    }

    private void OnEnable()
    {
        attack = InputSystem.actions.FindAction("BossAttack");
        attack2 = InputSystem.actions.FindAction("BossAttack2");
        debug = InputSystem.actions.FindAction("DebugMode");
        debugOff = InputSystem.actions.FindAction("OffDebug");
        rush = InputSystem.actions.FindAction("Rush");

        attack.performed += OnAttack;
        attack2.performed += OnAttack2;
        rush.performed += OnRush;
        debug.performed += OnDebug;
        debugOff.performed += OffDebug;
    }

    private void OnDisable()
    {
        attack.performed -= OnAttack;
        attack2.performed -= OnAttack2;
        debug.performed -= OnDebug;
        debugOff.performed -= OffDebug;
        rush.performed -= OnRush;
    }

    private void OnRush(InputAction.CallbackContext _)
    {
        boss.SendMessage("Rush");
    }

    private void OnAttack(InputAction.CallbackContext _)
    {
        boss.SendMessage("Attack1");
    }

    private void OnAttack2(InputAction.CallbackContext _)
    {
        boss.SendMessage("Attack2");
    }

    private void OnDebug(InputAction.CallbackContext _)
    {
        Debug.Log("디버그 모드");
        playerInput.actions.FindActionMap("UI").Disable();
        playerInput.SwitchCurrentActionMap("Debug");
    }

    private void OffDebug(InputAction.CallbackContext _)
    {
        Debug.Log("디버그 모드 오프");
        playerInput.SwitchCurrentActionMap("Player");
        playerInput.actions.FindActionMap("UI").Enable();
    }
}
