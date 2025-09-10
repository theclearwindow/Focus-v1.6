using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ChairPushIn : MonoBehaviour
{
    [Header("References")]
    public Transform startPoint;  // GameObject representing the start
    public Transform endPoint;    // GameObject representing the end

    [Header("Movement Settings")]
    public float duration = 3f;     // total time for the lerp
    public float rotateAngle = 15f; // degrees to rotate on Y axis

    private float elapsed = 0f;
    private Quaternion startRot;
    private Quaternion targetRot;

    void Start()
    {
        if (startPoint == null || endPoint == null)
        {
            Debug.LogError("StartPoint or EndPoint not assigned!");
            enabled = false;
            return;
        }

        transform.position = startPoint.position;
        startRot = transform.rotation;
        targetRot = startRot * Quaternion.Euler(0, rotateAngle, 0);
    }

    void Update()
    {
        if (elapsed >= duration) return;

        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / duration);

        // Custom easing: normal speed until 80%, then slow down
        float easedT;
        if (t < 0.8f)
        {
            easedT = t / 0.8f * 0.8f; // linear mapping of first 80%
        }
        else
        {
            float slowPart = (t - 0.8f) / 0.2f; // goes from 0 to 1 in last 20%
            easedT = 0.8f + Mathf.SmoothStep(0f, 0.2f, slowPart); 
        }

        // Position lerp
        transform.position = Vector3.Lerp(startPoint.position, endPoint.position, easedT);

        // Smooth Y rotation sway
        transform.rotation = Quaternion.Slerp(startRot, targetRot, Mathf.Sin(easedT * Mathf.PI));
    }
}
