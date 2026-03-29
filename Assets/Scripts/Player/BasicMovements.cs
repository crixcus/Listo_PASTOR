using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class BasicMovements : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private Animator animator;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.4f;
    [SerializeField] private LayerMask groundMask;

    [Header("Camera Settings")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float maxLookAngle = 80f;

    [Header("Water Slow")]
    [Tooltip("Speed multiplier when in flood water.")]
    [Range(0.1f, 1f)]
    public float waterSpeedMultiplier = 0.4f;

    [Tooltip("How smoothly speed transitions in and out.")]
    public float slowTransitionSpeed = 3f;

    // ------------------------------------------------------------------
    // Private state
    // ------------------------------------------------------------------

    private CharacterController controller;
    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction sprintAction;
    private InputAction lookAction;

    private Vector2 moveInput;
    private Vector3 velocity;
    private bool isGrounded;
    private bool isSprinting;
    private float cameraPitch = 0f;

    // Combined multiplier from water slow + stamina exhaustion
    // Both systems set their own target and the lowest one wins
    private float _waterMultiplier = 1f;
    private float _staminaMultiplier = 1f;
    private float _currentSpeedMultiplier = 1f;

    // ------------------------------------------------------------------
    // Unity lifecycle
    // ------------------------------------------------------------------

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();

        moveAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];
        sprintAction = playerInput.actions["Sprint"];
        lookAction = playerInput.actions["Look"];

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnEnable()
    {
        moveAction.Enable();
        jumpAction.Enable();
        sprintAction.Enable();
        lookAction.Enable();
    }

    private void OnDisable()
    {
        moveAction.Disable();
        jumpAction.Disable();
        sprintAction.Disable();
        lookAction.Disable();
    }

    private void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        bool isMoving = moveInput.magnitude > 0.1f;
        animator.SetBool("isMoving", isMoving);

        moveInput = moveAction.ReadValue<Vector2>();
        isSprinting = sprintAction.IsPressed();

        Vector2 lookInput = lookAction.ReadValue<Vector2>();
        transform.Rotate(Vector3.up * lookInput.x * mouseSensitivity);

        cameraPitch -= lookInput.y * mouseSensitivity;
        cameraPitch = Mathf.Clamp(cameraPitch, -maxLookAngle, maxLookAngle);
        cameraTransform.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);

        // Combined multiplier — use the lowest active multiplier
        float targetMultiplier = Mathf.Min(_waterMultiplier, _staminaMultiplier);
        _currentSpeedMultiplier = Mathf.Lerp(
            _currentSpeedMultiplier,
            targetMultiplier,
            Time.deltaTime * slowTransitionSpeed
        );

        float currentSpeed = (isSprinting ? sprintSpeed : walkSpeed) * _currentSpeedMultiplier;
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        controller.Move(move * currentSpeed * Time.deltaTime);

        if (jumpAction.triggered && isGrounded)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    // ------------------------------------------------------------------
    // Public API
    // ------------------------------------------------------------------

    /// <summary>
    /// Called by WaterTraumaTrigger when player enters or exits flood water.
    /// </summary>
    public void SetInWater(bool inWater)
    {
        _waterMultiplier = inWater ? waterSpeedMultiplier : 1f;
    }

    /// <summary>
    /// Called by StaminaSystem when player becomes exhausted or recovers.
    /// Multiplier stacks with water slow independently.
    /// </summary>
    public void SetStaminaMultiplier(float multiplier)
    {
        _staminaMultiplier = multiplier;
    }
}