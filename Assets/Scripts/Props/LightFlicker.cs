using UnityEngine;

[RequireComponent(typeof(Light))]
public class LightFlicker : MonoBehaviour
{
    [Header("Flicker Settings")]
    public float flickerSpeed = 1f;       // Speed of noise change
    public float intensityAmplitude = 0.5f; // How much to vary intensity (range)
    public float baseIntensity = 1f;      // Base intensity around which flicker happens

    private Light targetLight;
    private float noiseSeed;

    private void Start()
    {
        targetLight = GetComponent<Light>();
        noiseSeed = Random.Range(0f, 1000f); // Unique seed per light
    }

    private void Update()
    {
        float noise = Mathf.PerlinNoise(noiseSeed, Time.time * flickerSpeed);
        float flicker = baseIntensity + (noise - 0.5f) * 2f * intensityAmplitude;
        flicker = Mathf.Max(0f, flicker); // Clamp to non-negative

        targetLight.intensity = flicker;
    }
}
