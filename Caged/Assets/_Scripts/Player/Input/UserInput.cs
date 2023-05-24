using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;
public class UserInput : MonoBehaviourPun
{
    public static UserInput instance;

    public Vector2 moveInput { get; private set; }
    public Vector2 lookInput { get; private set; }
    public bool SprintPressed { get; private set; }
    public bool SprintHeld { get; private set; }
    public bool CrouchPressed { get; private set; }
    public bool CrouchHeld { get; private set; }
    public bool PronePressed { get; private set; }
    public bool ProneHeld { get; private set; }
    public bool InteractPressed { get; private set; }
    public bool UsePressed { get; private set; }
    public bool ThrowReleased { get; private set; }
    public bool ThrowHeld { get; private set; }
    public bool FlashlightTogglePressed { get; private set; }
    public bool FlashlightZoomHeld { get; private set; }
    public bool FlashlightZoomReleased {get; private set; }
    public bool RightBumperPressed { get; private set; }
    public bool LeftBumperPressed { get; private set; }
    public bool ExitPressed { get; private set; }
    public bool MenuPressed { get; private set; }
    public bool Slot1Pressed { get; private set; }
    public bool Slot2Pressed { get; private set; }
    public bool Slot3Pressed { get; private set; }
    public bool Slot4Pressed { get; private set; }
    public bool Slot5Pressed { get; private set; }

    private PlayerInput _playerInput;
    public InputDevice currentLookInput = Mouse.current;
    private InputAction _moveAction;
    private InputAction _lookAction;
    private InputAction _sprintAction;
    private InputAction _crouchAction;
    private InputAction _proneAction;
    private InputAction _interactAction;
    [HideInInspector] public InputAction _useAction;
    private InputAction _throwAction;
    private InputAction _flashlightToggleAction;
    private InputAction _flashlightZoomAction;
    private InputAction _rbumperAction;
    private InputAction _lbumperAction;
    private InputAction _exitAction;
    private InputAction _menuAction;
    private InputAction _slot1Action;
    private InputAction _slot2Action;
    private InputAction _slot3Action;
    private InputAction _slot4Action;
    private InputAction _slot5Action;


    private void Awake()
    {
        if (!photonView.IsMine) return;
        if (instance == null) { instance = this; }
        _playerInput = GetComponent<PlayerInput>();
        SetupInputActions();
    }
    private void Update()
    {
        if (!photonView.IsMine) return;
        UpdateInputs();
    }
    public void SetupInputActions()
    {
        _moveAction = _playerInput.actions["Move"];
        _lookAction = _playerInput.actions["Look"];
        _sprintAction = _playerInput.actions["Sprint"];
        _crouchAction = _playerInput.actions["Crouch"];
        _proneAction = _playerInput.actions["Prone"];
        _interactAction = _playerInput.actions["Interact"];
        _useAction = _playerInput.actions["Use"];
        _throwAction = _playerInput.actions["Throw"];
        _flashlightToggleAction = _playerInput.actions["Flashlight"];
        _flashlightZoomAction = _playerInput.actions["FlashlightZoom"];
        _rbumperAction = _playerInput.actions["RightBumper"];
        _lbumperAction = _playerInput.actions["LeftBumper"];
        _exitAction = _playerInput.actions["Exit"];
        _menuAction = _playerInput.actions["Menu"];
        _slot1Action = _playerInput.actions["Slot1"];
        _slot2Action = _playerInput.actions["Slot2"];
        _slot3Action = _playerInput.actions["Slot3"];
        _slot4Action = _playerInput.actions["Slot4"];
        _slot5Action = _playerInput.actions["Slot5"];
        if (_lookAction.ReadValue<Vector2>() != Vector2.zero) { currentLookInput = _lookAction.activeControl.device; } else return;
    }
    private void UpdateInputs()
    {
        moveInput = _moveAction.ReadValue<Vector2>();
        lookInput = _lookAction.ReadValue<Vector2>();
        SprintPressed = _sprintAction.WasPressedThisFrame();
        SprintHeld = _sprintAction.IsPressed();
        CrouchPressed = _crouchAction.WasPressedThisFrame();
        CrouchHeld = _crouchAction.IsPressed();
        PronePressed = _proneAction.WasPressedThisFrame();
        ProneHeld = _proneAction.IsPressed();
        InteractPressed = _interactAction.WasPressedThisFrame();
        UsePressed = _useAction.WasPressedThisFrame();
        ThrowReleased = _throwAction.WasReleasedThisFrame();
        ThrowHeld = _throwAction.IsPressed();
        FlashlightTogglePressed = _flashlightToggleAction.WasPressedThisFrame();
        FlashlightZoomHeld = _flashlightZoomAction.IsPressed();
        FlashlightZoomReleased = _flashlightZoomAction.WasReleasedThisFrame();
        RightBumperPressed = _rbumperAction.WasPressedThisFrame();
        LeftBumperPressed = _lbumperAction.WasPressedThisFrame();
        ExitPressed = _exitAction.WasPressedThisFrame();
        MenuPressed = _menuAction.WasPressedThisFrame();
        Slot1Pressed = _slot1Action.WasPressedThisFrame();
        Slot2Pressed = _slot2Action.WasPressedThisFrame();
        Slot3Pressed = _slot3Action.WasPressedThisFrame();
        Slot4Pressed = _slot4Action.WasPressedThisFrame();
        Slot5Pressed = _slot5Action.WasPressedThisFrame();
        if (lookInput != Vector2.zero && currentLookInput != _lookAction.activeControl.device) { currentLookInput = _lookAction.activeControl.device; } else return;
    }
}
