using UnityEngine;
using UnityEngine.UI;
using FMODUnity;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class FPVCharacterController : MonoBehaviour
{

    //Not a hoverboard controller
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
    public LayerMask groundLayer;           // Layers considered as ground
    public string defaultSurfaceTag = "Default";
    public EventReference defaultFootstep;
    public EventReference stoneFootstep;
    public EventReference metalFootstep;
    public EventReference woodFootstep;

    [Header("Head Bob Settings")]
    public bool EnableHeadBob = true;

    private Rigidbody rb;
    private CapsuleCollider col;
    private float currentStamina;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();
        if (rb == null) Debug.LogError("Rigidbody component missing!");
        if (col == null) Debug.LogError("CapsuleCollider component missing!");

        currentStamina = maxStamina;
        stepTimer = stepInterval;
    }

    private void Update()
    {
        HandleMovement();
        HandleFootsteps();
        UpdateStaminaUI();
    }

    /// <summary>
    /// Handles player movement including walking, sprinting, and stamina consumption.
    /// </summary>
    private void HandleMovement()
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

        currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);

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
    private void HandleFootsteps()
    {
        if (!IsMoving() || !IsGrounded()) return;

        stepTimer -= Time.deltaTime;
        if (stepTimer > 0f) return;

        stepTimer = stepInterval;

        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, col.height / 2 + 0.5f, groundLayer))
        {
            string tag = hit.collider.CompareTag("") ? defaultSurfaceTag : hit.collider.tag;
            EventReference footstepEvent = GetFootstepEvent(tag);
            RuntimeManager.PlayOneShot(footstepEvent, transform.position);
        }
    }

    /// <summary>
    /// Maps surface tag to FMOD footstep event.
    /// </summary>
    private EventReference GetFootstepEvent(string surfaceTag)
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
    private bool IsMoving() => Mathf.Abs(Input.GetAxis("Horizontal")) > 0f || Mathf.Abs(Input.GetAxis("Vertical")) > 0f;

    /// <summary>
    /// Returns true if the player is grounded.
    /// </summary>
    private bool IsGrounded() => Physics.Raycast(transform.position, Vector3.down, col.height / 2 + 0.1f, groundLayer);

    /// <summary>
    /// Updates the stamina UI slider if assigned.
    /// </summary>
    private void UpdateStaminaUI()
    {
        if (staminaBar != null)
        {
            staminaBar.value = currentStamina / maxStamina;
        }
    }
}
