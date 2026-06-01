using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

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

    private string path;

    private void Awake()
    {
        move = InputSystem.actions.FindAction("Move");
        jump = InputSystem.actions.FindAction("Jump");
        dodge = InputSystem.actions.FindAction("Dodge");
        attack = InputSystem.actions.FindAction("Attack");
        parry = InputSystem.actions.FindAction("Parry");

        path = Application.persistentDataPath + "/keybindings.json";

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            InputSystem.actions.LoadBindingOverridesFromJson(json);
        }

        lefttext.text = move.GetBindingDisplayString(1);
        righttext.text = move.GetBindingDisplayString(2);
        jumptext.text = jump.GetBindingDisplayString(0);
        dodgetext.text = dodge.GetBindingDisplayString(0);
        attacktext.text = attack.GetBindingDisplayString(0);
        parrytext.text = parry.GetBindingDisplayString(0);
    }

    public void OnButtonLeft()
    {
        lefttext.text = "Esc to Cancel";

        move.Disable();

        var rebind = move.PerformInteractiveRebinding()
            .WithTargetBinding(1)
            .WithControlsExcluding("<Mouse>")
            .WithCancelingThrough("<Keyboard>/escape")
            .OnComplete(op => {
                op.Dispose();
                lefttext.text = move.GetBindingDisplayString(1);
                string json = InputSystem.actions.SaveBindingOverridesAsJson();
                File.WriteAllText(path, json);
                move.Enable();
            })
            .OnCancel(op => {
                op.Dispose();
                lefttext.text = move.GetBindingDisplayString(1);
                move.Enable();
            })
            .Start();
    }

    public void OnButtonRight()
    {
        righttext.text = "Esc to Cancel";
        
        move.Disable();

        var rebind = move.PerformInteractiveRebinding()
            .WithTargetBinding(2)
            .WithControlsExcluding("<Mouse>")
            .WithCancelingThrough("<Keyboard>/escape")
            .OnComplete(op => {
                op.Dispose();
                righttext.text = move.GetBindingDisplayString(2);
                string json = InputSystem.actions.SaveBindingOverridesAsJson();
                File.WriteAllText(path, json);
                move.Enable();
            })
            .OnCancel(op => {
                op.Dispose();
                righttext.text = move.GetBindingDisplayString(2);
                move.Enable();
            })
            .Start();
    }

    public void OnButtonJump()
    {
        jumptext.text = "Esc to Cancel";

        jump.Disable();

        var rebind = jump.PerformInteractiveRebinding()
            .WithTargetBinding(0)
            .WithControlsExcluding("<Mouse>")
            .WithCancelingThrough("<Keyboard>/escape")
            .OnComplete(op => {
                op.Dispose();
                jumptext.text = jump.GetBindingDisplayString(0);
                string json = InputSystem.actions.SaveBindingOverridesAsJson();
                File.WriteAllText(path, json);
                jump.Enable();
            })
            .OnCancel(op => {
                op.Dispose();
                jumptext.text = jump.GetBindingDisplayString(0);
                jump.Enable();
            })
            .Start();
    }

    public void OnButtonDodge()
    {
        dodgetext.text = "Esc to Cancel";

        dodge.Disable();

        var rebind = dodge.PerformInteractiveRebinding()
            .WithTargetBinding(0)
            .WithControlsExcluding("<Mouse>")
            .WithCancelingThrough("<Keyboard>/escape")
            .OnComplete(op => {
                op.Dispose();
                dodgetext.text = dodge.GetBindingDisplayString(0);
                string json = InputSystem.actions.SaveBindingOverridesAsJson();
                File.WriteAllText(path, json);
                dodge.Enable();
            })
            .OnCancel(op => {
                op.Dispose();
                dodgetext.text = dodge.GetBindingDisplayString(0);
                dodge.Enable();
            })
            .Start();
    }

    public void OnButtonAttack()
    {
        attacktext.text = "Esc to Cancel";

        attack.Disable();

        var rebind = attack.PerformInteractiveRebinding()
            .WithTargetBinding(0)
            .WithControlsExcluding("<Mouse>")
            .WithCancelingThrough("<Keyboard>/escape")
            .OnComplete(op => {
                op.Dispose();
                attacktext.text = attack.GetBindingDisplayString(0);
                string json = InputSystem.actions.SaveBindingOverridesAsJson();
                File.WriteAllText(path, json);
                attack.Enable();
            })
            .OnCancel(op => {
                op.Dispose();
                attacktext.text = attack.GetBindingDisplayString(0);
                attack.Enable();
            })
            .Start();
    }

    public void OnButtonParry()
    {
        parrytext.text = "Esc to Cancel";

        parry.Disable();

        var rebind = parry.PerformInteractiveRebinding()
            .WithTargetBinding(0)
            .WithControlsExcluding("<Mouse>")
            .WithCancelingThrough("<Keyboard>/escape")
            .OnComplete(op => {
                op.Dispose();
                parrytext.text = parry.GetBindingDisplayString(0);
                string json = InputSystem.actions.SaveBindingOverridesAsJson();
                File.WriteAllText(path, json);
                parry.Enable();
            })
            .OnCancel(op => {
                op.Dispose();
                parrytext.text = parry.GetBindingDisplayString(0);
                parry.Enable();
            })
            .Start();
    }
}
