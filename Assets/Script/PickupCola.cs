using UnityEngine;
using FMODUnity;

public class PickupCola : MonoBehaviour
{
    [Header("Hover & Rotate")]
    public float hoverHeight = 0.25f;
    public float hoverSpeed = 2f;
    public float rotateSpeed = 60f;

    [Header("Pickup Settings")]
    public Transform player;
    public float pickupRange = 2f;

    [Header("Optional")]
    public GameObject pickupEffect;

    private Vector3 startPos;

    [Header("Sound Effects")]
    public EventReference pickupEvent;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        HoverAndRotate();
        CheckPickup();
    }

    void HoverAndRotate()
    {
        float newY = startPos.y + Mathf.Sin(Time.time * hoverSpeed) * hoverHeight;
        transform.position = new Vector3(startPos.x, newY, startPos.z);

        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime, Space.World);
    }

    void CheckPickup()
    {
        if (player == null) return;

        if (Vector3.Distance(player.position, transform.position) <= pickupRange)
        {
            OnPickup();
        }
    }

    void OnPickup()
    {
        if (pickupEffect != null)
            Instantiate(pickupEffect, transform.position, Quaternion.identity);

        RuntimeManager.PlayOneShot(pickupEvent, transform.position);
        Destroy(gameObject);
    }
}

