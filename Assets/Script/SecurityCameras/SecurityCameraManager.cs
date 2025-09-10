using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class CameraGroup
{
    public string groupName;
    public Transform groupRoot;        // Parent of all cameras in this group
    public GameObject monitorScreen;   // Quad object with collider + renderer
    public GameObject debugButton;     // Debug cube object with collider

    [HideInInspector] public Renderer monitorRenderer; // Cached at runtime
}

public class SecurityCameraManager : MonoBehaviour
{
    [Header("Camera Groups")]
    public CameraGroup[] cameraGroups;

    [Header("Player")]
    public Camera playerCamera;
    public float interactRange = 4f;
    public float lerpSpeed = 4f;
    public float zoomDistance = 0.5f; // How far in front of the monitor the cam goes

    [Header("Cycle Settings")]
    public float cycleInterval = 2f;

    [Header("Inactive Monitor Material")]
    public Material inactiveMaterial;

    // Internal state
    private Dictionary<string, Camera[]> groupCameras = new Dictionary<string, Camera[]>();
    private Dictionary<string, int> currentIndex = new Dictionary<string, int>();
    private Dictionary<string, Material> monitorMaterials = new Dictionary<string, Material>();
    private Dictionary<string, bool> groupActive = new Dictionary<string, bool>();

    private bool viewingCams = false;
    private string currentGroup = null;

    private Transform originalCamParent;
    private Vector3 originalPos;
    private Quaternion originalRot;

    private Transform targetLerpPoint;
    private bool isHoldingE = false;

    void Start()
    {
        originalCamParent = playerCamera.transform.parent;

        foreach (var group in cameraGroups)
        {
            if (group.monitorScreen != null)
                group.monitorRenderer = group.monitorScreen.GetComponent<Renderer>();

            Camera[] cams = group.groupRoot.GetComponentsInChildren<Camera>(true);
            groupCameras[group.groupName] = cams;
            currentIndex[group.groupName] = 0;
            groupActive[group.groupName] = true;

            Material mat = new Material(Shader.Find("Unlit/Texture"));
            monitorMaterials[group.groupName] = mat;
            if (group.monitorRenderer != null)
                group.monitorRenderer.material = mat;

            foreach (var cam in cams)
            {
                RenderTexture rt = new RenderTexture(512, 512, 16);
                cam.targetTexture = rt;

                var controller = cam.GetComponent<SecurityCameraController>();
                if (controller != null)
                    controller.SetCameraState(true);
            }

            if (cams.Length > 0 && group.monitorRenderer != null)
                monitorMaterials[group.groupName].mainTexture = cams[0].targetTexture;

            StartCoroutine(AutoCycle(group.groupName));
        }
    }

    private IEnumerator AutoCycle(string groupName)
    {
        // Randomized interval for each cycle
        float baseInterval = cycleInterval;
        float randomOffset = Random.Range(0.1f, 0.3f);
        float interval = baseInterval + randomOffset;

        while (true)
        {
            yield return new WaitForSeconds(interval);

            if (!groupActive[groupName] || groupCameras[groupName].Length == 0 || viewingCams)
                continue;

            currentIndex[groupName] = (currentIndex[groupName] + 1) % groupCameras[groupName].Length;
            Camera currentCam = groupCameras[groupName][currentIndex[groupName]];
            monitorMaterials[groupName].mainTexture = currentCam.targetTexture;

            // Recalculate interval for next cycle
            randomOffset = Random.Range(0.1f, 0.3f);
            interval = baseInterval + randomOffset;
        }
    }

    void Update()
    {
        HandlePlayerInput();
        HandleCameraLerp();
    }

    private void HandlePlayerInput()
    {
        // --- Holding E ---
        if (Input.GetKey(KeyCode.E) && !viewingCams)
        {
            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, interactRange))
            {
                foreach (var group in cameraGroups)
                {
                    if (group.monitorScreen != null && hit.collider.gameObject == group.monitorScreen)
                    {
                        if (!isHoldingE)
                        {
                            StartLerpToMonitor(group.monitorScreen.transform);
                            currentGroup = group.groupName;
                        }
                        isHoldingE = true;
                        return;
                    }

                    if (group.debugButton != null && hit.collider.gameObject == group.debugButton)
                    {
                        if (Input.GetKeyDown(KeyCode.E))
                            ToggleGroup(group.groupName);
                        return;
                    }
                }
            }
        }
        else if (Input.GetKeyUp(KeyCode.E) && !viewingCams && isHoldingE)
        {
            ResetCameraPosition();
            isHoldingE = false;
        }

        // --- Inside POV ---
        if (viewingCams)
        {
            if (Input.GetKeyDown(KeyCode.Tab)) CycleCurrentGroup();
            if (Input.GetKeyDown(KeyCode.Escape)) ExitCameraView();
        }
    }

    private void HandleCameraLerp()
    {
        if (targetLerpPoint != null)
        {
            playerCamera.transform.position = Vector3.Lerp(
                playerCamera.transform.position,
                targetLerpPoint.position,
                Time.deltaTime * lerpSpeed
            );

            playerCamera.transform.rotation = Quaternion.Slerp(
                playerCamera.transform.rotation,
                targetLerpPoint.rotation,
                Time.deltaTime * lerpSpeed
            );

            if (!viewingCams && Vector3.Distance(playerCamera.transform.position, targetLerpPoint.position) < 0.05f)
            {
                EnterCameraView(currentGroup);
            }
        }
    }

    private void StartLerpToMonitor(Transform monitor)
    {
        originalPos = playerCamera.transform.position;
        originalRot = playerCamera.transform.rotation;

        GameObject lerpPoint = new GameObject("CamLerpPoint");
        lerpPoint.transform.position = monitor.position + monitor.forward * zoomDistance;
        lerpPoint.transform.rotation = monitor.rotation;
        targetLerpPoint = lerpPoint.transform;
    }

    private void ResetCameraPosition()
    {
        viewingCams = false;
        currentGroup = null;
        targetLerpPoint = null;

        playerCamera.enabled = true;
        playerCamera.transform.position = originalPos;
        playerCamera.transform.rotation = originalRot;
    }

    private void EnterCameraView(string groupName)
    {
        viewingCams = true;
        playerCamera.enabled = false;

        Camera cam = groupCameras[groupName][currentIndex[groupName]];
        cam.enabled = true;
    }

    private void ExitCameraView()
    {
        if (currentGroup != null)
            groupCameras[currentGroup][currentIndex[currentGroup]].enabled = false;

        ResetCameraPosition();
    }

    private void CycleCurrentGroup()
    {
        if (currentGroup == null) return;

        int idx = currentIndex[currentGroup];
        groupCameras[currentGroup][idx].enabled = false;

        idx = (idx + 1) % groupCameras[currentGroup].Length;
        currentIndex[currentGroup] = idx;

        Camera cam = groupCameras[currentGroup][idx];
        cam.enabled = true;

        monitorMaterials[currentGroup].mainTexture = cam.targetTexture;
    }

    public void ToggleGroup(string groupName)
    {
        groupActive[groupName] = !groupActive[groupName];

        foreach (Camera cam in groupCameras[groupName])
        {
            var controller = cam.GetComponent<SecurityCameraController>();
            if (controller != null)
                controller.SetCameraState(groupActive[groupName]);
        }

        if (groupActive[groupName])
            monitorMaterials[groupName].mainTexture = groupCameras[groupName][currentIndex[groupName]].targetTexture;
        else
            monitorMaterials[groupName].mainTexture = inactiveMaterial.mainTexture;
    }
}
