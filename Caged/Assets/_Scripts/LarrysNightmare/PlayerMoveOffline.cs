using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
public class PlayerMoveOffline : MonoBehaviourPun
{
    [Header("Variables")]
    public bool isInGUI = false;
    [SerializeField] private Slider slider;
    [SerializeField] private CharacterController controller;
    [SerializeField] private float standingHeight = 2.5f;
    [SerializeField] private float mouseSensX = 2f;
    [SerializeField] private float mouseSensY = 2f;
    [SerializeField] private float controllerSensX = 25f;
    [SerializeField] private float controllerSensY = 25f;
    [SerializeField] private bool _crouching;
    [SerializeField] private bool _isprone;
    [SerializeField] private bool _walking;
    [SerializeField] private bool _running;
    private CapsuleCollider capsuleCollider;
    public Transform playerbody;
    public Transform headlight;
    public Canvas canvas = null;
    public Camera playerCam;
    public Animator anim = null;
    public FootstepManager footstepManager;
    public LayerMask groundLayerMask;
    public bool ToggleSprint = false;
    public bool ToggleCrouch = false;
    public bool ToggleProne = false;
    private bool isRunningToggle = false;
    private bool isCrouchingToggle = false;
    private bool isProneToggle = false;
    public bool isBlocked = false;
    private float xRotation = 0f;
    public float walkingSpeed = 3f;
    public float runningSpeed = 6f;
    public float proneSpeed = 1.25f;
    public float crouchSpeed = 2f;
    public float gravity = 9.81f;
    public float walkingFootstepDistance = 10f;
    public float runningFootstepDistance = 5;
    public float crouchingFootstepDistance = 12;
    public float proneFootstepDistance = 16f;
    private float _footstepDistanceCounter;
    public float stamina = 100f;
    private float StaminaRegenTimer = 0.0f;
    private const float StaminaTimeToRegen = 0.5f;
    public float StaminaRegenMultiplier;
    public float StaminaDecreaseMultiplier;

    public void Awake()
    {
        if (!photonView.IsMine) { 
            playerCam.GetComponent<AudioListener>().enabled = false; 
            playerCam.GetComponent<Camera>().enabled = false; 
            return; }
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        canvas.gameObject.SetActive(true);
        playerbody.gameObject.SetActive(false);
        headlight.gameObject.SetActive(false);
        capsuleCollider = GetComponent<CapsuleCollider>();
    }
    public void Update()
    {
        if (!photonView.IsMine) { return; }
        Look();
        Move();
        ManageToggles();
    }
    public void ManageToggles()
    {
        if (ToggleSprint)
        {
            if (UserInput.instance.SprintPressed) { isRunningToggle = !isRunningToggle; }
        }
        else if (isRunningToggle) { isRunningToggle = false; }
        if (ToggleCrouch)
        {
            if (UserInput.instance.CrouchPressed) { isRunningToggle = false; isCrouchingToggle = !isCrouchingToggle;}
        }
        else if (isCrouchingToggle) { isCrouchingToggle = false; }
        if (ToggleProne)
        {
            if (UserInput.instance.PronePressed) { isRunningToggle = false; isProneToggle = !isProneToggle; }
        }
        else if (isProneToggle) { isProneToggle = false; }
    }
    public void Look()
    {
        Vector2 mouseLook = UserInput.instance.lookInput;

        if (mouseLook == Vector2.zero) { return; }

        bool usingMouse = UserInput.instance.currentLookInput is Mouse;
        float sensX = usingMouse ? mouseSensX : controllerSensX;
        float sensY = usingMouse ? mouseSensY : controllerSensY;

        float lookX = mouseLook.x * sensX * Time.fixedDeltaTime;
        float lookY = mouseLook.y * sensY * Time.fixedDeltaTime;

        xRotation -= lookY;
        xRotation = Mathf.Clamp(xRotation, -90, 90);

        playerCam.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
        transform.Rotate(Vector3.up, lookX);
    }

    public void Move()
    {   
        Vector2 move = UserInput.instance.moveInput;
        bool isCrouching = UserInput.instance.CrouchHeld || isCrouchingToggle;
        bool isProne = UserInput.instance.ProneHeld|| isProneToggle;
        bool isRunning = UserInput.instance.SprintHeld && controller.velocity.magnitude > 0
            && stamina > 0f && !isBlocked && !isProne && !isCrouching && move.y >= 0
            || isRunningToggle && controller.velocity.magnitude > 0 && stamina > 0f
            && !isBlocked && !isProne && !isCrouching && move.y >= 0;
        float speed = isRunning ? runningSpeed : isCrouching ? crouchSpeed : isProne ? proneSpeed : walkingSpeed;
        Vector3 movement = move.y * transform.forward + move.x * transform.right;
        if (!controller.isGrounded)
        {
            movement.y -= gravity * Time.deltaTime;
        }
        controller.Move(movement * speed * Time.deltaTime);
        Stamina(isRunning);
        Crouch(isCrouching, isProne, speed);
        if(PhotonNetwork.OfflineMode){return;}
        Animation(isRunning);
    }
    public void Crouch(bool isCrouching, bool isProne, float speed)
    {
        // Crouch
        if (isCrouching)
        {
            Prone = false;
            Crouching = true;
            controller.height = 1.25f;
            capsuleCollider.height = 1.25f;
            controller.center = new Vector3(0, -0.625f, 0);
            capsuleCollider.center = new Vector3(0, -0.625f, 0);
        }
        // Prone
        else if (isProne)
        {
            Prone = true;
            Crouching = false;
            controller.height = 0.75f;
            capsuleCollider.height = 0.75f;
            controller.center = new Vector3(0, -0.875f, 0);
            capsuleCollider.center = new Vector3(0, -0.875f, 0);
        }
        // Stand up
        else if (!isCrouching && !isProne && controller.height != standingHeight)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.up, out hit, 2.5f))
            {
                standingHeight = hit.distance - 0.01f;
                isBlocked = true;
            }
            else
            {
                standingHeight = 2.5f;
                isBlocked = false;
            }

            if (standingHeight > controller.height)
            {
                controller.height = standingHeight;
                capsuleCollider.height = standingHeight;
                controller.center = new Vector3(0, 0, 0);
                capsuleCollider.center = new Vector3(0, 0, 0);
                Crouching = false;
                Prone = false;
            }
        }
        if (controller.isGrounded && (_walking || _running))
        {
            float footstepDistance;
            if (_isprone)
            {
                footstepDistance = proneFootstepDistance;
            }
            else if (_crouching)
            {
                footstepDistance = crouchingFootstepDistance;
            }
            else
            {
                footstepDistance = _walking ? walkingFootstepDistance : runningFootstepDistance;
            }
            _footstepDistanceCounter += speed /2 * Time.fixedDeltaTime;
            if (_footstepDistanceCounter >= footstepDistance)
            {
                _footstepDistanceCounter = 0;
                SurfaceType surfaceType = DetectSurfaceType();
                footstepManager.PlayFootstep(surfaceType, _isprone, _crouching, _walking, _running);
            }
        }
    }
    public void Stamina(bool isRunning)
    {
        if (isRunning)
        {
            stamina = Mathf.Clamp(stamina - (StaminaDecreaseMultiplier * Time.deltaTime), 0.0f, 100f);
            StaminaRegenTimer = 0.0f;
        }
        else if (stamina < 100f)
        {
            if (StaminaRegenTimer >= StaminaTimeToRegen)
            {
                stamina = Mathf.Clamp(stamina + (StaminaRegenMultiplier * Time.deltaTime), 0.0f, 100f);
            }
            else
            {
                StaminaRegenTimer += Time.deltaTime;
            }
        }
        slider.value = stamina;
        if (slider.value > 50f)
        {
            slider.fillRect.GetComponent<Image>().color = Color.green;
        }
        else if (slider.value < 50f && slider.value > 25f)
        {
            slider.fillRect.GetComponent<Image>().color = Color.yellow;
        }
        else
        {
            slider.fillRect.GetComponent<Image>().color = Color.red;
        }
    }
    public void Animation(bool running)
    {
        if(PhotonNetwork.OfflineMode){return;}
        if (controller.isGrounded)
        {
            if (controller.velocity.magnitude > 0)
            {
                if (running)
                {
                    Running = true;
                    Walking = false;
                }
                if (!running)
                {
                    Walking = true;
                    Running = false;
                }
            }
            else
            {
                Walking = false;
                Running = false;
            }
        }
    }
    private SurfaceType DetectSurfaceType()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 2f, groundLayerMask))
        {
            switch (hit.collider.tag)
            {
                case "Grass":
                    return SurfaceType.Grass;
                case "Wood":
                    return SurfaceType.Wood;
                case "Cement":
                    return SurfaceType.Cement;
                case "Vent":
                    return SurfaceType.Vent;
                default:
                    return SurfaceType.Default;
            }
        }
        return SurfaceType.Default;
    }
    public bool Crouching
    {
        get { return _crouching; }
        set
        {
            if (value == _crouching) return;
            _crouching = value;
            if(PhotonNetwork.OfflineMode){return;}
            anim.SetBool("Crouching", _crouching);
        }
    }
    public bool Walking
    {
        get { return _walking; }
        set
        {
            if (value == _walking) return;
            _walking = value;
            if(PhotonNetwork.OfflineMode){return;}
            anim.SetBool("Walking", _walking);
        }
    }
    public bool Running
    {
        get { return _running; }
        set
        {
            if (value == _running) return;
            _running = value;
            if(PhotonNetwork.OfflineMode){return;}
            anim.SetBool("Running", _running);
        }
    }
    public bool Prone
    {
        get { return _isprone; }
        set
        {
            if (value == _isprone) return;
            _isprone = value;
            if(PhotonNetwork.OfflineMode){return;}
            anim.SetBool("Prone", _isprone);
        }
    }
}
