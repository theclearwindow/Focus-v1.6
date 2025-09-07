using UnityEngine;
using System.Collections;

public class CameraManager : MonoBehaviour
{
    [Header("References")]
    public CamcorderController camcorder;
    public PolaroidController polaroid;
    public Camera playerCamera;
    public Camera[] trailCameras;

    private bool usingCamcorder = true;
    private bool viewingTrailCam = false;
    private int trailCamIndex = 0;
    private bool switching = false;

    void Start()
    {
        // ✅ On start, player camera is active, camcorder root active, polaroid root inactive
        playerCamera.enabled = true;
        foreach (var cam in trailCameras) cam.enabled = false;

        camcorder.gameObject.SetActive(true);
        polaroid.gameObject.SetActive(false);

        usingCamcorder = true;
        viewingTrailCam = false;
    }

    void Update()
    {
        HandleDeviceSwitch();
        HandleTrailCamSwitch();
        HandleEscape();
    }

    // Q → switch between camcorder and polaroid
    void HandleDeviceSwitch()
    {
        if (Input.GetKeyDown(KeyCode.Q) && !switching) // ✅ removed "!viewingTrailCam"
        {
            switching = true;
            StartCoroutine(SwapDevices());
        }
    }

    IEnumerator SwapDevices()
    {
        if (usingCamcorder)
        {
            yield return StartCoroutine(camcorder.LowerAndHide());
            polaroid.gameObject.SetActive(true);
            yield return StartCoroutine(polaroid.RaiseAndShow());
        }
        else
        {
            yield return StartCoroutine(polaroid.LowerAndHide());
            camcorder.gameObject.SetActive(true);
            yield return StartCoroutine(camcorder.RaiseAndShow());
        }

        usingCamcorder = !usingCamcorder;
        switching = false;
    }

    // TAB → cycle forward through trail cameras
    void HandleTrailCamSwitch()
    {
        if (trailCameras.Length == 0) return;

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (!viewingTrailCam)
            {
                // Entering trailcam mode
                viewingTrailCam = true;

                // ❌ Leave camcorder/polaroid active so models stay visible
                playerCamera.enabled = false;

                trailCamIndex = 0;
                EnableOnlyTrailCam(trailCamIndex);
            }
            else
            {
                // Cycle forward
                trailCamIndex = (trailCamIndex + 1) % trailCameras.Length;
                EnableOnlyTrailCam(trailCamIndex);
            }
        }
    }

    // ✅ Helper restored with debug logging
    private void EnableOnlyTrailCam(int index)
    {
        for (int i = 0; i < trailCameras.Length; i++)
            trailCameras[i].enabled = (i == index);

        if (trailCameras.Length > 0 && index >= 0 && index < trailCameras.Length)
        {
            Debug.Log($"[CameraManager] TrailCam switched to index {index} → {trailCameras[index].name}");
        }
    }

    // ESC → return to player camera
    void HandleEscape()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            viewingTrailCam = false;

            foreach (var cam in trailCameras)
                cam.enabled = false;

            playerCamera.enabled = true;

            Debug.Log("[CameraManager] Returned to player camera.");
        }
    }
}

