using UnityEngine.InputSystem;
using UnityEngine;

public class ResetDeviceBindings : MonoBehaviour
{
    [SerializeField] private InputActionAsset _inputAction;
    [SerializeField] private string _targetControlScheme;

    public void ResetAllBindings()
    {
        foreach (InputActionMap map in _inputAction.actionMaps)
        {
            map.RemoveAllBindingOverrides();
        }
    }
    public void ResetControlSchemeBinding()
    {
        foreach (InputActionMap map in _inputAction.actionMaps)
        {
            foreach (InputAction action in map.actions)
            {
                action.RemoveBindingOverride(InputBinding.MaskByGroup(_targetControlScheme));
            }
        }
        Debug.Log("Reset Binds for " + _targetControlScheme);
    }
}
