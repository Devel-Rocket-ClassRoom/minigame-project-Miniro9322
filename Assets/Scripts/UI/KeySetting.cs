using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class KeySetting : MonoBehaviour
{
    private InputAction move;
    private InputAction jump;
    private InputAction dodge;
    private InputAction attack;
    private InputAction parry;

    [SerializeField] private TextMeshProUGUI lefttext;
    [SerializeField] private TextMeshProUGUI righttext;
    [SerializeField] private TextMeshProUGUI jumptext;
    [SerializeField] private TextMeshProUGUI dodgetext;
    [SerializeField] private TextMeshProUGUI attacktext;
    [SerializeField] private TextMeshProUGUI parrytext;

    [SerializeField] private Button leftButton;
    [SerializeField] private Button rightButton;
    [SerializeField] private Button jumpButton;
    [SerializeField] private Button dodgeButton;
    [SerializeField] private Button attackButton;
    [SerializeField] private Button parryButton;

    private string path;

    private void Awake()
    {
        move   = InputSystem.actions.FindAction("Move");
        jump   = InputSystem.actions.FindAction("Jump");
        dodge  = InputSystem.actions.FindAction("Dodge");
        attack = InputSystem.actions.FindAction("Attack");
        parry  = InputSystem.actions.FindAction("Parry");

        path = Application.persistentDataPath + "/keybindings.json";

        if (File.Exists(path))
            InputSystem.actions.LoadBindingOverridesFromJson(File.ReadAllText(path));

        lefttext.text   = move.GetBindingDisplayString(1);
        righttext.text  = move.GetBindingDisplayString(2);
        jumptext.text   = jump.GetBindingDisplayString(0);
        dodgetext.text  = dodge.GetBindingDisplayString(0);
        attacktext.text = attack.GetBindingDisplayString(0);
        parrytext.text  = parry.GetBindingDisplayString(0);
    }

    public void OnButtonLeft()   => StartRebind(move,   1, lefttext);
    public void OnButtonRight()  => StartRebind(move,   2, righttext);
    public void OnButtonJump()   => StartRebind(jump,   0, jumptext);
    public void OnButtonDodge()  => StartRebind(dodge,  0, dodgetext);
    public void OnButtonAttack() => StartRebind(attack, 0, attacktext);
    public void OnButtonParry()  => StartRebind(parry,  0, parrytext);

    private void StartRebind(InputAction action, int bindingIndex, TextMeshProUGUI label)
    {
        label.text = "Esc to Cancel";
        action.Disable();
        SetButtonsInteractable(false);

        action.PerformInteractiveRebinding()
            .WithTargetBinding(bindingIndex)
            .WithControlsExcluding("<Mouse>")
            .WithCancelingThrough("<Keyboard>/escape")
            .OnComplete(op =>
            {
                op.Dispose();
                label.text = action.GetBindingDisplayString(bindingIndex);
                File.WriteAllText(path, InputSystem.actions.SaveBindingOverridesAsJson());
                action.Enable();
                SetButtonsInteractable(true);
            })
            .OnCancel(op =>
            {
                op.Dispose();
                label.text = action.GetBindingDisplayString(bindingIndex);
                action.Enable();
                SetButtonsInteractable(true);
            })
            .Start();
    }

    private void SetButtonsInteractable(bool interactable)
    {
        leftButton.interactable   = interactable;
        rightButton.interactable  = interactable;
        jumpButton.interactable   = interactable;
        dodgeButton.interactable  = interactable;
        attackButton.interactable = interactable;
        parryButton.interactable  = interactable;
    }
}
