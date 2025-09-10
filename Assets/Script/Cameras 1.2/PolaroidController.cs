using UnityEngine;
using System.Collections;
using FMODUnity;

public class PolaroidController : MonoBehaviour
{
    [Header("Cameras")]
    public Camera playerCamera;               // Always main player camera
    public Camera polaroidCamera;             // Polaroid camera
    public RenderTexture polaroidViewfinder;  // Live preview
    public RenderTexture polaroidPhoto;       // Static photo after capture

    [Header("Models")]
    public Transform polaroidModel;           // Physical camera model
    public GameObject heldPhoto;              // Displayed photo after capture

    [Header("Animation")]
    public Vector3 loweredOffset = new Vector3(0, -0.5f, 0);
    public float switchSpeed = 5f;

    [Header("FMOD Events")]
    public EventReference raiseSound;
    public EventReference lowerSound;
    public EventReference shutterSound;

    private Vector3 startLocalPos;
    private bool holdingPhoto = false;
    private const float raisedRelativeY = 0.2f; // Height relative to player

    void Start()
    {
        if (!playerCamera || !polaroidCamera || !polaroidModel || !polaroidViewfinder || !polaroidPhoto || !heldPhoto)
        {
            Debug.LogError("PolaroidController: Missing references!");
            enabled = false;
            return;
        }

        startLocalPos = polaroidModel.localPosition;

        polaroidCamera.targetTexture = polaroidViewfinder;
        polaroidCamera.enabled = true;

        polaroidModel.gameObject.SetActive(true);
        heldPhoto.SetActive(false);
    }

    void Update()
    {
        HandlePhotoToggle();
    }

    private void HandlePhotoToggle()
    {
        if (Input.GetMouseButtonDown(1))
        {
            if (!holdingPhoto)
            {
                // Take photo: freeze polaroid camera
                polaroidCamera.targetTexture = polaroidPhoto;
                polaroidCamera.Render();
                polaroidCamera.targetTexture = polaroidViewfinder;

                if (!string.IsNullOrEmpty(shutterSound.Path))
                    RuntimeManager.PlayOneShot(shutterSound, transform.position);

                StartCoroutine(SwitchToPhoto());
            }
            else
            {
                StartCoroutine(SwitchToViewfinder());
            }
        }
    }

    private IEnumerator SwitchToPhoto()
    {
        // Lower polaroid smoothly
        yield return LerpModelPosition(GetLoweredPosition());

        // After lowering, show captured photo
        heldPhoto.SetActive(true);
        holdingPhoto = true;

        // Keep polaroid model active for next raise
    }

    private IEnumerator SwitchToViewfinder()
    {
        // Make sure model is active and start from lowered position
        polaroidModel.gameObject.SetActive(true);
        polaroidModel.localPosition = GetLoweredPosition();

        // Raise smoothly to player-relative height
        yield return LerpModelPosition(GetRaisedPosition());

        // After raising, hide the held photo
        heldPhoto.SetActive(false);
        holdingPhoto = false;
    }

    public IEnumerator LowerAndHide()
    {
        if (!string.IsNullOrEmpty(lowerSound.Path))
            RuntimeManager.PlayOneShot(lowerSound, transform.position);

        yield return LerpModelPosition(GetLoweredPosition());

        gameObject.SetActive(false);
    }

    public IEnumerator RaiseAndShow()
    {
        gameObject.SetActive(true);

        // Reset to lowered position first
        polaroidModel.localPosition = GetLoweredPosition();

        if (!string.IsNullOrEmpty(raiseSound.Path))
            RuntimeManager.PlayOneShot(raiseSound, transform.position);

        // Raise to player-relative height
        yield return LerpModelPosition(GetRaisedPosition());
    }

    public void ForceHide()
    {
        gameObject.SetActive(false);
        heldPhoto.SetActive(false);
    }

    // Lowered position relative to player
    private Vector3 GetLoweredPosition()
    {
        return new Vector3(
            startLocalPos.x,
            raisedRelativeY + loweredOffset.y,
            startLocalPos.z
        );
    }

    // Raised position relative to player
    private Vector3 GetRaisedPosition()
    {
        return new Vector3(
            startLocalPos.x,
            raisedRelativeY,
            startLocalPos.z
        );
    }

    private IEnumerator LerpModelPosition(Vector3 targetPos)
    {
        while (Vector3.Distance(polaroidModel.localPosition, targetPos) > 0.01f)
        {
            polaroidModel.localPosition = Vector3.Lerp(polaroidModel.localPosition, targetPos, Time.deltaTime * switchSpeed);
            yield return null;
        }
        polaroidModel.localPosition = targetPos;
    }
}
