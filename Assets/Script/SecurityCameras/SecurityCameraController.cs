using UnityEngine;
using System.Collections;

public class SecurityCameraController : MonoBehaviour
{
    [Header("REC Indicator")]
    public GameObject recIndicator; // The red sphere or UI indicator
    public bool cameraOn = true;
    public float blinkInterval = 1f;

    private Coroutine blinkRoutine;

    void Start()
    {
        if (cameraOn)
            blinkRoutine = StartCoroutine(BlinkIndicator());
        else
            recIndicator.SetActive(false);
    }

    public void SetCameraState(bool on)
    {
        cameraOn = on;

        if (blinkRoutine != null)
        {
            StopCoroutine(blinkRoutine);
            blinkRoutine = null;
        }

        if (cameraOn)
            blinkRoutine = StartCoroutine(BlinkIndicator());
        else
            recIndicator.SetActive(false);
    }

    private IEnumerator BlinkIndicator()
    {
        while (cameraOn)
        {
            recIndicator.SetActive(true);
            yield return new WaitForSeconds(blinkInterval);
            recIndicator.SetActive(false);
            yield return new WaitForSeconds(blinkInterval);
        }
    }
}
