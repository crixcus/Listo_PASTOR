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
    [Tooltip("Speed multiplier when in flood water. 0.5 = half speed.")]
    [Range(0.1f, 1f)]
    public float waterSpeedMultiplier = 0.4f;

    [Tooltip("How smoothly the slow effect fades in and out.")]
    public float slowTransitionSpeed = 3f;

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

    // Water slow state
    private float _currentSpeedMultiplier = 1f;
    private float _targetSpeedMultiplier = 1f;

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
        // Ground check
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        bool isMoving = moveInput.magnitude > 0.1f;
        animator.SetBool("isMoving", isMoving);

        // Read input
        moveInput = moveAction.ReadValue<Vector2>();
        isSprinting = sprintAction.IsPressed();

        // Mouse look
        Vector2 lookInput = lookAction.ReadValue<Vector2>();
        float mouseX = lookInput.x * mouseSensitivity;
        float mouseY = lookInput.y * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, -maxLookAngle, maxLookAngle);
        cameraTransform.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);

        // Smoothly transition speed multiplier
        _currentSpeedMultiplier = Mathf.Lerp(
            _currentSpeedMultiplier,
            _targetSpeedMultiplier,
            Time.deltaTime * slowTransitionSpeed
        );

        // Movement — apply speed multiplier
        float currentSpeed = (isSprinting ? sprintSpeed : walkSpeed) * _currentSpeedMultiplier;
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        controller.Move(move * currentSpeed * Time.deltaTime);

        // Jumping
        if (jumpAction.triggered && isGrounded)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        // Gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    // ------------------------------------------------------------------
    // Public API
    // ------------------------------------------------------------------

    /// <summary>
    /// Called by WaterTraumaTrigger when the player enters or exits the water.
    /// Smoothly transitions the player's speed to the water slow multiplier.
    /// </summary>
    public void SetInWater(bool inWater)
    {
        _targetSpeedMultiplier = inWater ? waterSpeedMultiplier : 1f;
    }
}