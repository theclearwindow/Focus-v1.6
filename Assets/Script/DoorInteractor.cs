using UnityEngine;

public class DoorInteractor : MonoBehaviour
{
    public Camera playerCam;
    public float interactRange = 4f;
    public KeyCode interactKey = KeyCode.E;

    private DoorController currentDoor;

    void Update()
    {
        Ray ray = new Ray(playerCam.transform.position, playerCam.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactRange))
        {
            DoorController door = hit.collider.GetComponentInParent<DoorController>();
            if (door != null)
            {
                currentDoor = door;

                float distance = Vector3.Distance(transform.position, door.transform.position);
                door.SetGlow(distance);

                // Toggle (4 units)
                if (distance <= 4f && distance > 2f && Input.GetKeyDown(interactKey))
                {
                    door.ToggleOpen();
                }

                // Peek (2 units)
                if (distance <= 2f)
                {
                    if (Input.GetKey(interactKey))
                    {
                        door.Peek(0.5f); // example peek amount
                    }
                    else if (Input.GetKeyUp(interactKey))
                    {
                        door.StopPeek();
                    }
                }
            }
        }
        else if (currentDoor != null)
        {
            currentDoor.SetGlow(Mathf.Infinity); // reset glow
            currentDoor = null;
        }
    }
}
