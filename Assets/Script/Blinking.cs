using UnityEngine;
using UnityEngine.UI;

public class BlinkImage : MonoBehaviour
{
    public Image targetImage;  // Drag your UI Image here in the Inspector
    public float blinkInterval = 1f; // Time in seconds

    private void Start()
    {
        if (targetImage != null)
            InvokeRepeating(nameof(ToggleImage), blinkInterval, blinkInterval);
    }

    private void ToggleImage()
    {
        targetImage.enabled = !targetImage.enabled;
    }
}
