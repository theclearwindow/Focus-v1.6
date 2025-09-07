using UnityEngine;
using UnityEngine.UI;
using FMODUnity;

[RequireComponent(typeof(Rigidbody))]
public class FPVCharacterController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 3f;
    public float sprintSpeed = 6f;
    public float crouchSpeed = 1.5f;

    [Header("Stamina Settings")]
    public float maxStamina = 5f;
    public float staminaRegenRate = 1f;
    public float staminaDrainRate = 1.5f;
    public Slider staminaBar;

    [Header("Footsteps FMOD")]
    public float stepInterval = 0.5f;
    private float stepTimer;

    [Header("Footstep Surfaces")]
    public LayerMask groundLayer;          // Layers considered as ground
    public string defaultSurfaceTag = "Default";
    public EventReference defaultFootstep;
    public EventReference stoneFootstep;
    public EventReference metalFootstep;
    public EventReference woodFootstep;

    private Rigidbody rb;
    private CapsuleCollider col;
    private float currentStamina;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();
        currentStamina = maxStamina;
    }

    void Update()
    {
        HandleMovement();
        HandleFootsteps();
        UpdateStaminaUI();
    }

    /// <summary>
    /// Handles player movement including walking, sprinting, and stamina consumption.
    /// </summary>
    void HandleMovement()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 inputDir = new Vector3(h, 0, v).normalized;

        float speed = walkSpeed;

        // Sprinting consumes stamina
        if (Input.GetKey(KeyCode.LeftShift) && inputDir.magnitude > 0 && currentStamina > 0)
        {
            speed = sprintSpeed;
            currentStamina -= staminaDrainRate * Time.deltaTime;
        }
        else
        {
            // Regenerate stamina when not sprinting
            currentStamina += staminaRegenRate * Time.deltaTime;
        }
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);

        // Convert input direction to world space
        Vector3 move = transform.TransformDirection(inputDir) * speed;

        // Preserve current vertical velocity (gravity)
        Vector3 velocity = move;
        velocity.y = rb.linearVelocity.y;
        rb.linearVelocity = velocity;
    }

    /// <summary>
    /// Plays footstep sounds when the player is moving and grounded.
    /// </summary>
    void HandleFootsteps()
    {
        if (!IsMoving() || !IsGrounded())
            return;

        stepTimer -= Time.deltaTime;
        if (stepTimer > 0) return;

        stepTimer = stepInterval;

        // Cast a ray downward to detect surface type
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, col.height / 2 + 0.5f, groundLayer))
        {
            string tag = hit.collider.tag;
            EventReference footstepEvent = GetFootstepEvent(tag);
            RuntimeManager.PlayOneShot(footstepEvent, transform.position);
        }
    }

    /// <summary>
    /// Maps surface tag to FMOD footstep event.
    /// </summary>
    EventReference GetFootstepEvent(string surfaceTag)
    {
        switch (surfaceTag)
        {
            case "Stone": return stoneFootstep;
            case "Metal": return metalFootstep;
            case "Wood": return woodFootstep;
            default: return defaultFootstep;
        }
    }

    /// <summary>
    /// Returns true if the player is pressing movement keys.
    /// </summary>
    bool IsMoving() => Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0;

    /// <summary>
    /// Returns true if the player is grounded.
    /// </summary>
    bool IsGrounded() => Physics.Raycast(transform.position, Vector3.down, col.height / 2 + 0.1f, groundLayer);

    /// <summary>
    /// Updates the stamina UI slider if assigned.
    /// </summary>
    void UpdateStaminaUI()
    {
        if (staminaBar != null)
        {
            staminaBar.value = currentStamina / maxStamina;
        }
    }
}
