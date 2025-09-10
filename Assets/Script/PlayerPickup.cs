using UnityEngine;

public class PlayerPickup : MonoBehaviour
{
    [Header("References")]
    public Camera playerCamera;         // Assign FPS camera
    public Transform holdPoint;         // Empty object in front of camera
    public GameObject debugCube;        // Reference to debug cube (active = empty hands)

    [Header("Pickup Settings")]
    public float pickupRange = 3f;      // Max distance for picking up objects
    public string pickupTag = "Pickup"; // Only objects with this tag are grabbable

    private Rigidbody heldObject;
    private Collider[] heldColliders;

    void Update()
    {
        // Only allow pickup when Debug Cube is active
        if (debugCube == null || !debugCube.activeSelf) return;

        if (Input.GetMouseButtonDown(0))
        {
            TryPickup();
        }

        if (Input.GetMouseButtonUp(0))
        {
            DropObject();
        }

        if (heldObject != null)
        {
            HoldObject();
        }
    }

    private void TryPickup()
    {
        if (heldObject != null) return; // Already holding something

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, pickupRange))
        {
            if (hit.collider.CompareTag(pickupTag))
            {
                Rigidbody rb = hit.collider.attachedRigidbody;
                if (rb != null)
                {
                    heldObject = rb;

                    // Disable physics
                    heldObject.useGravity = false;
                    heldObject.isKinematic = true;

                    // Cache & disable all colliders
                    heldColliders = heldObject.GetComponentsInChildren<Collider>();
                    foreach (var col in heldColliders)
                        col.enabled = false;

                    // Parent & snap to hold point
                    heldObject.transform.SetParent(holdPoint);
                    HoldObject();
                }
            }
        }
    }

    private void DropObject()
    {
        if (heldObject == null) return;

        // Unparent
        heldObject.transform.SetParent(null);

        // Re-enable colliders
        if (heldColliders != null)
        {
            foreach (var col in heldColliders)
                col.enabled = true;
        }

        // Restore physics
        heldObject.useGravity = true;
        heldObject.isKinematic = false;

        heldObject = null;
        heldColliders = null;
    }

    private void HoldObject()
    {
        // Always lock position & rotation exactly at hold point
        heldObject.transform.position = holdPoint.position;
        heldObject.transform.rotation = Quaternion.LookRotation(playerCamera.transform.forward);
    }
}
