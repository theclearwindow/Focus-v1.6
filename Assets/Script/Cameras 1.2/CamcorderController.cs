using UnityEngine;
using System.Collections;
using FMODUnity;

public class CamcorderController : MonoBehaviour
{
    [Header("Cameras")]
    public Camera playerCamera;
    public Camera camcorderCamera;
    public RenderTexture camcorderLCD;

    [Header("Animation")]
    public Transform camcorderModel; // visual model to lower/raise
    public Vector3 loweredOffset = new Vector3(0, -0.5f, 0);
    public float switchSpeed = 5f;

    [Header("FMOD Events")]
    public EventReference raiseSound;
    public EventReference lowerSound;

    private Vector3 startLocalPos;
    private bool camcorderActive = false;

    void Start()
    {
        if (!playerCamera || !camcorderCamera || !camcorderLCD || !camcorderModel)
        {
            Debug.LogError("Assign cameras, render texture, and camcorderModel!");
            enabled = false;
            return;
        }

        startLocalPos = camcorderModel.localPosition;

        camcorderCamera.targetTexture = camcorderLCD;
        camcorderCamera.enabled = true;
    }

    void Update()
    {
        if (!gameObject.activeInHierarchy) return; // ⛔ Only runs if active

        HandleViewToggle();
    }

    void HandleViewToggle()
    {
        if (Input.GetMouseButtonDown(1))
        {
            camcorderCamera.targetTexture = null;
            playerCamera.enabled = false;
            camcorderActive = true;
        }
        else if (Input.GetMouseButtonUp(1))
        {
            camcorderCamera.targetTexture = camcorderLCD;
            playerCamera.enabled = true;
            camcorderActive = false;
        }
    }

    // 🔹 Animate lowering/raising
    public IEnumerator LowerAndHide()
    {
        if (!string.IsNullOrEmpty(lowerSound.Path))
            RuntimeManager.PlayOneShot(lowerSound, transform.position);

        yield return LerpModelPosition(startLocalPos + loweredOffset);
        gameObject.SetActive(false);
    }

    public IEnumerator RaiseAndShow()
    {
        camcorderModel.localPosition = startLocalPos + loweredOffset;
        gameObject.SetActive(true);

        if (!string.IsNullOrEmpty(raiseSound.Path))
            RuntimeManager.PlayOneShot(raiseSound, transform.position);

        yield return LerpModelPosition(startLocalPos);
    }

    private IEnumerator LerpModelPosition(Vector3 targetPos)
    {
        while (Vector3.Distance(camcorderModel.localPosition, targetPos) > 0.01f)
        {
            camcorderModel.localPosition = Vector3.Lerp(camcorderModel.localPosition, targetPos, Time.deltaTime * switchSpeed);
            yield return null;
        }
        camcorderModel.localPosition = targetPos;
    }
}
