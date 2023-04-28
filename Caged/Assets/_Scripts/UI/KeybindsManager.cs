using UnityEngine.InputSystem;
using UnityEngine;
using TMPro;

public class KeybindsManager : MonoBehaviour
{
    public InputActionReference actionToRebind;
    private InputActionRebindingExtensions.RebindingOperation rebindOperation;
    public TextMeshProUGUI KeyBindButton;
    public void SetBind()
    {
        actionToRebind.action.Disable();
        rebindOperation = actionToRebind.action.PerformInteractiveRebinding(0).WithControlsExcluding("Mouse").WithCancelingThrough("<Keyboard>/escape").OnMatchWaitForAnother(0.2f).Start()
        .OnCancel(operation =>
        {
            actionToRebind.action.Enable();
        })
        .OnComplete(operation =>
        {
            actionToRebind.action.Enable();
            KeyBindButton.text = "[" + actionToRebind.action.GetBindingDisplayString(0).ToUpper() + "]";
        });

    }
}
