using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class DoorController : MonoBehaviour
{
    [Header("Setup")]
    public Transform doorPivot;       // Child pivot object
    public Renderer doorRenderer;     // Renderer for glow
    public float openAngle = 90f;
    public float peekAngle = 30f;
    public float openSpeed = 3f;
    public float peekSpeed = 1.5f;
    public float slamSpeed = 6f;

    [Header("Glow Colors")]
    public Color glowFar = Color.yellow;
    public Color glowNear = Color.cyan;
    public float glowIntensity = 2f;

    [Header("FMOD Events")]
    public EventReference doorOpenEvent;
    public EventReference doorCloseEvent;
    public EventReference doorPeekEvent;
    public EventReference doorSlamEvent;

    [HideInInspector] public bool isOpen = false;

    private Quaternion closedRot;
    private Quaternion targetRot;
    private Material doorMat;
    private bool isPeeking = false;
    private bool isSlamming = false;
    private bool isPeekSoundPlaying = false;

    void Start()
    {
        closedRot = doorPivot.localRotation;
        targetRot = closedRot;

        if (doorRenderer != null)
        {
            doorMat = doorRenderer.material;
            doorMat.DisableKeyword("_EMISSION");
        }
    }

    void Update()
    {
        // Smooth rotation
        float speed = isPeeking ? peekSpeed : (isSlamming ? slamSpeed : openSpeed);
        doorPivot.localRotation = Quaternion.Slerp(doorPivot.localRotation, targetRot, Time.deltaTime * speed);
    }

    // ----------------- TOGGLE -----------------
    public void ToggleOpen()
    {
        isOpen = !isOpen;
        isPeeking = false;
        isSlamming = false;

        targetRot = isOpen ? Quaternion.Euler(0, openAngle, 0) * closedRot : closedRot;

        PlayEvent(isOpen ? doorOpenEvent : doorCloseEvent);
    }

    // ----------------- PEEK -----------------
    public void Peek(float amount01)
    {
        isPeeking = true;
        isSlamming = false;
        targetRot = Quaternion.Euler(0, peekAngle * amount01, 0) * closedRot;

        if (!isPeekSoundPlaying)
        {
            PlayEvent(doorPeekEvent);
            isPeekSoundPlaying = true;
        }
    }

    public void StopPeek()
    {
        isPeeking = false;
        targetRot = closedRot;
        isPeekSoundPlaying = false;
    }

    // ----------------- SLAM -----------------
    public void SlamClose()
    {
        isSlamming = true;
        isPeeking = false;
        isOpen = false;

        targetRot = closedRot;
        PlayEvent(doorSlamEvent);
    }

    // ----------------- GLOW -----------------
    public void SetGlow(float distance)
    {
        if (doorMat == null) return;

        if (distance <= 2f)
        {
            doorMat.EnableKeyword("_EMISSION");
            doorMat.SetColor("_EmissionColor", glowNear * glowIntensity);
        }
        else if (distance <= 4f)
        {
            doorMat.EnableKeyword("_EMISSION");
            doorMat.SetColor("_EmissionColor", glowFar * glowIntensity);
        }
        else
        {
            doorMat.SetColor("_EmissionColor", Color.black);
        }
    }

    // ----------------- FMOD HELPER -----------------
    private void PlayEvent(EventReference evt)
    {
        if (!string.IsNullOrEmpty(evt.Path))
            RuntimeManager.PlayOneShot(evt, transform.position);
    }
}

