using UnityEngine;
using System.Collections;

public class CameraManager : MonoBehaviour
{
    [Header("References")]
    public GameObject debugCube;             // assign in editor
    public CamcorderController camcorder;
    public PolaroidController polaroid;
    public Camera playerCamera;
    public Camera[] trailCameras;

    private enum DeviceState { DebugCube, Camcorder, Polaroid }
    private DeviceState currentState = DeviceState.DebugCube;

    private bool viewingTrailCam = false;
    private int trailCamIndex = 0;
    private bool switching = false;

    void Start()
    {
        // ✅ Start holding the debug cube only
        if (debugCube) debugCube.SetActive(true);
        if (camcorder) camcorder.gameObject.SetActive(false);
        if (polaroid) polaroid.gameObject.SetActive(false);

        if (playerCamera) playerCamera.enabled = true;
        if (trailCameras != null)
        {
            foreach (var cam in trailCameras) if (cam) cam.enabled = false;
        }

        currentState = DeviceState.DebugCube;
        viewingTrailCam = false;
    }

    void Update()
    {
        HandleDeviceSwitch();
        HandleTrailCamSwitch();
        HandleEscape();
    }

    // --- Q: DebugCube → Camcorder → Polaroid → DebugCube ---
    void HandleDeviceSwitch()
    {
        if (Input.GetKeyDown(KeyCode.Q) && !switching)
        {
            StartCoroutine(SwapDevices());
        }
    }

    private IEnumerator SwapDevices()
    {
        switching = true;

        switch (currentState)
        {
            case DeviceState.DebugCube:
                if (debugCube) debugCube.SetActive(false);

                if (camcorder)
                {
                    camcorder.gameObject.SetActive(true);
                    yield return StartCoroutine(camcorder.RaiseAndShow());
                }
                currentState = DeviceState.Camcorder;
                break;

            case DeviceState.Camcorder:
                if (camcorder)
                    yield return StartCoroutine(camcorder.LowerAndHide());

                if (polaroid)
                {
                    polaroid.gameObject.SetActive(true);
                    yield return StartCoroutine(polaroid.RaiseAndShow());
                }
                currentState = DeviceState.Polaroid;
                break;

            case DeviceState.Polaroid:
                if (polaroid)
                    yield return StartCoroutine(polaroid.LowerAndHide());

                if (debugCube) debugCube.SetActive(true);
                currentState = DeviceState.DebugCube;
                break;
        }

        switching = false;
    }

    // --- TAB: Trailcam viewing/cycling ---
    void HandleTrailCamSwitch()
    {
        if (trailCameras == null || trailCameras.Length == 0) return;

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (!viewingTrailCam)
            {
                viewingTrailCam = true;

                if (playerCamera) playerCamera.enabled = false;

                trailCamIndex = 0;
                EnableOnlyTrailCam(trailCamIndex);
            }
            else
            {
                trailCamIndex = (trailCamIndex + 1) % trailCameras.Length;
                EnableOnlyTrailCam(trailCamIndex);
            }
        }
    }

    // --- ESC: Return to player camera ---
    void HandleEscape()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            viewingTrailCam = false;

            if (trailCameras != null)
            {
                foreach (var cam in trailCameras) if (cam) cam.enabled = false;
            }

            if (playerCamera) playerCamera.enabled = true;
        }
    }

    private void EnableOnlyTrailCam(int index)
    {
        for (int i = 0; i < trailCameras.Length; i++)
        {
            if (trailCameras[i]) trailCameras[i].enabled = (i == index);
        }

        if (index >= 0 && index < trailCameras.Length && trailCameras[index])
        {
            Debug.Log($"[CameraManager] TrailCam → {index}: {trailCameras[index].name}");
        }
    }
}
