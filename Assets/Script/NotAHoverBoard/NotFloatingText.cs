using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class NotFloatingText : MonoBehaviour
{
    public GameObject hoverboard;      // assign the hoverboard object
    public Text easterEggText;         // assign the UI Text
    public float displayTime = 2f;     // how long the text appears
    public float floatAmplitude = 10f; // vertical movement in pixels
    public float floatSpeed = 2f;      // how fast it bobs

    private bool hasHoverboard = false;
    private bool isShowing = false;
    private Vector3 startPos;

    void Start()
    {
        if (easterEggText != null)
        {
            easterEggText.gameObject.SetActive(false);
            startPos = easterEggText.rectTransform.localPosition;
        }
    }

    void Update()
    {
        if (!hasHoverboard && hoverboard == null)
        {
            hasHoverboard = true;
        }

        if (Input.GetKeyDown(KeyCode.H) && !hasHoverboard && !isShowing)
        {
            StartCoroutine(ShowEasterEggText());
        }

        // Animate bobbing
        if (isShowing && easterEggText != null)
        {
            Vector3 pos = startPos;
            pos.y += Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
            easterEggText.rectTransform.localPosition = pos;
        }
    }

    private IEnumerator ShowEasterEggText()
    {
        if (easterEggText == null) yield break;

        isShowing = true;
        easterEggText.gameObject.SetActive(true);

        yield return new WaitForSeconds(displayTime);

        easterEggText.gameObject.SetActive(false);
        easterEggText.rectTransform.localPosition = startPos;
        isShowing = false;
    }
}
