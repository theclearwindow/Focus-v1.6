using UnityEngine;

public class FanSpin : MonoBehaviour
{
    [Header("Fan Settings")]
    public float rotationSpeed = 100f;       // degrees per second
    public Vector3 rotationAxis = Vector3.up; // axis of rotation, usually Y

    void Update()
    {
        // Rotate the fan around the chosen axis
        transform.Rotate(rotationAxis * rotationSpeed * Time.deltaTime);
    }
}
