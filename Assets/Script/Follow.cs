using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class Follow : MonoBehaviour
{
    [Header("Movement")]
    public Transform targetObj;       // The object the monster moves toward
    public Transform player;          // Player position
    public float detectionRange = 10f;
    public float moveSpeed = 10f;

    [Header("Sound Effects")]
    public EventReference monsterGrowlEvent;

    private float growlTimer = 0f;

    void Update()
    {
        if (player == null || targetObj == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer < detectionRange)
        {
            // Move toward target
            Vector3 previousPosition = transform.position;
            transform.position = Vector3.MoveTowards(transform.position, targetObj.position, moveSpeed * Time.deltaTime);

            // Rotate smoothly toward player
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * moveSpeed);

            // Only play growl if monster is moving
            if (Vector3.Distance(transform.position, previousPosition) > 0.01f)
            {
                growlTimer -= Time.deltaTime;

                if (growlTimer <= 0f)
                {
                    // Determine growl interval and volume based on distance
                    float interval;
                    float volume;

                    if (distanceToPlayer <= 4f)
                    {
                        interval = 0.2f;
                        volume = 1f;
                    }
                    else if (distanceToPlayer <= 6f)
                    {
                        interval = 1f;
                        volume = 0.7f;
                    }
                    else
                    {
                        interval = 3f;
                        volume = 0.5f;
                    }

                    // Play growl
                    EventInstance growlInstance = RuntimeManager.CreateInstance(monsterGrowlEvent);
                    growlInstance.set3DAttributes(RuntimeUtils.To3DAttributes(transform.position));
                    growlInstance.setVolume(volume);
                    growlInstance.start();
                    growlInstance.release();

                    growlTimer = interval;
                }
            }
        }
    }
}


