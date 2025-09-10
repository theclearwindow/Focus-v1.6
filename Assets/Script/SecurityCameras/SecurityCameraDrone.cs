using UnityEngine;

public class DroneCircle : MonoBehaviour
{
    [Header("Circle Settings")]
    public Transform centerPoint;   // The point the drone will circle around
    public float radius = 5f;       // Distance from the center
    public float speed = 2f;        // How fast it circles
    public float height = 2f;       // Constant height above the center

    private float angle = 0f;

    void Update()
    {
        if (centerPoint == null) return;

        // Increase the angle over time
        angle += speed * Time.deltaTime;

        // Calculate new position in circle
        float x = Mathf.Cos(angle) * radius;
        float z = Mathf.Sin(angle) * radius;

        Vector3 newPos = new Vector3(centerPoint.position.x + x, centerPoint.position.y + height, centerPoint.position.z + z);
        transform.position = newPos;

        // Make the drone face forward along the circle path
        //transform.LookAt(centerPoint);
    }
}
