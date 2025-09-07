using UnityEngine;

public class SkyboxRotator : MonoBehaviour
{
    [Header("Rotation Settings")]
    public float rotationSpeed = 1f; // Degrees per second

    [Header("Day/Night Cycle")]
    public float cycleDuration = 60f; // Seconds for a full day->night->day
    public float minExposure = 0.5f;
    public float maxExposure = 1.5f;

    [Header("Sun/Moon Light")]
    public Light sunMoonLight;
    public float maxLightIntensity = 1f;
    public float minLightIntensity = 0f;

    private float time;

    void Update()
    {
        // --- Rotate Skybox ---
        float rotation = RenderSettings.skybox.GetFloat("_Rotation");
        rotation += rotationSpeed * Time.deltaTime;
        RenderSettings.skybox.SetFloat("_Rotation", rotation);

        // --- Day/Night Cycle Progress ---
        time += Time.deltaTime;
        float cycle = Mathf.PingPong(time / (cycleDuration / 2f), 1f);

        // --- Exposure ---
        float exposure = Mathf.Lerp(minExposure, maxExposure, cycle);
        RenderSettings.skybox.SetFloat("_Exposure", exposure);

        // --- Sun/Moon Light ---
        if (sunMoonLight != null)
        {
            // Rotate the light around the X axis like the sun rising/setting
            float angle = cycle * 180f; // 0=midnight, 90=sunrise, 180=next midnight
            sunMoonLight.transform.rotation = Quaternion.Euler(new Vector3(angle - 90f, 0f, 0f));

            // Fade intensity with cycle
            float intensity = Mathf.Lerp(minLightIntensity, maxLightIntensity, Mathf.Sin(cycle * Mathf.PI));
            sunMoonLight.intensity = intensity;
        }
    }
}
