using UnityEngine;
using TMPro;
using FMODUnity;
using System.Collections;

public class DefinitelyNotForAHoverboard : MonoBehaviour
{
    [Header("Hoverboard Settings")]
    public float hoverHeight = 0.5f;          // how high it floats
    public float hoverBobSpeed = 2f;          // speed of bobbing
    public float hoverBobAmount = 0.05f;      // vertical bob amount
    public float speedBoostMultiplier = 1.2f; // 20% faster
    public float interactDistance = 2f;       // distance player can equip
    public EventReference hoverHumSFX;        // FMOD sound when mounted

    [Header("UI Easter Egg")]
    public TextMeshProUGUI easterEggText;
    public float messageDuration = 2f;

    private bool equipped = false;
    private bool collected = false;
    private Transform player;
    private FPVCharacterController playerController;
    private Vector3 originalLocalPos;
    private float originalWalkSpeed;
    private float originalSprintSpeed;
    private float originalCrouchSpeed;

    void Start()
    {
        originalLocalPos = transform.localPosition;

        if (easterEggText != null)
            easterEggText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!collected)
        {
            // Easter egg: H pressed before collection
            if (Input.GetKeyDown(KeyCode.H) && easterEggText != null)
            {
                StartCoroutine(ShowEasterEgg("Definitely not for a hoverboard..."));
            }

            // Check for pickup
            TryPickup();
            return;
        }

        // Toggle mount/unmount
        if (Input.GetKeyDown(KeyCode.H) && collected)
        {
            if (!equipped)
                Equip();
            else
                Unequip();
        }

        if (equipped)
            HoverEffect();
    }

    private void TryPickup()
    {
        if (player == null) return;

        if (Vector3.Distance(player.position, transform.position) <= interactDistance && Input.GetKeyDown(KeyCode.E))
        {
            collected = true;
            Equip();
        }
    }

    private void Equip()
    {
        if (playerController == null) return;

        equipped = true;
        transform.SetParent(player);
        transform.localPosition = new Vector3(0, -1f, 0);
        transform.localRotation = Quaternion.identity;

        // Boost player speed
        originalWalkSpeed = playerController.walkSpeed;
        originalSprintSpeed = playerController.sprintSpeed;
        originalCrouchSpeed = playerController.crouchSpeed;

        playerController.walkSpeed *= speedBoostMultiplier;
        playerController.sprintSpeed *= speedBoostMultiplier;
        playerController.crouchSpeed *= speedBoostMultiplier;

        // Disable head bob
        playerController.EnableHeadBob = false;

        // Play hoverboard hum
        RuntimeManager.PlayOneShot(hoverHumSFX, transform.position);
    }

    private void Unequip()
    {
        equipped = false;
        transform.SetParent(null);
        transform.localPosition = originalLocalPos;

        if (playerController == null) return;

        // Restore player speed
        playerController.walkSpeed = originalWalkSpeed;
        playerController.sprintSpeed = originalSprintSpeed;
        playerController.crouchSpeed = originalCrouchSpeed;

        // Re-enable head bob
        playerController.EnableHeadBob = true;
    }

    private void HoverEffect()
    {
        float bob = Mathf.Sin(Time.time * hoverBobSpeed) * hoverBobAmount;
        transform.localPosition = new Vector3(0, -1f + hoverHeight + bob, 0);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player = other.transform;
            playerController = player.GetComponent<FPVCharacterController>();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (equipped)
                Unequip();

            player = null;
            playerController = null;
        }
    }

    private IEnumerator ShowEasterEgg(string message)
    {
        easterEggText.text = message;
        easterEggText.gameObject.SetActive(true);
        yield return new WaitForSeconds(messageDuration);
        easterEggText.gameObject.SetActive(false);
    }
}