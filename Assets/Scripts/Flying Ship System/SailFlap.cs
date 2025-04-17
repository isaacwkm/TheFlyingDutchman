using UnityEngine;

public class SailFlap : MonoBehaviour
{
    [Header("How violently the sail flaps. (0 = no flap, 10 = big flap)")]
    [Range(0f, 10f)]
    [SerializeField] float flapIntensity = 5f;   // new single slider

    [Header("How fast the sail flaps. (1.0 = base speed)")]
    [SerializeField] float flapSpeed = 1.2f;    // gust speed

    Vector3 baseScale;
    float   seed;

    void Awake()
    {
        baseScale = transform.localScale;
        seed      = Random.value * 100f;
    }

    void Update()
    {
        // Perlin mash‑up (same as before, just moved down the file)
        float baseN  = Mathf.PerlinNoise(Time.time * flapSpeed, seed);
        float wobble = Mathf.PerlinNoise(Time.time * 0.3f,     seed + 50f);
        float n      = Mathf.Clamp01(baseN * wobble);

        // ----- Intensity → min / max range around 1.0 -----
        float delta = flapIntensity * 0.1f;      // 5   → 0.5
        float minY  = 1f - delta;                // 0.5
        float maxY  = 1f + delta;                // 1.5
        // --------------------------------------------------

        float y = Mathf.Lerp(minY, maxY, n);

        transform.localScale = new Vector3(baseScale.x,
                                           y * baseScale.y,
                                           baseScale.z);
    }

    public void ChangeFlapIntensity(float intensity)
    {
        if (intensity < 0)
        {
            intensity = 0;
        }
        if (intensity > 10)
        {
            intensity = 10;
        }

        flapIntensity = intensity;
    }

    public void ChangeFlapSpeed(float speed)
    {
        if (speed < 0)
        {
            speed = 0;
        }

        flapSpeed = speed;
    }
}
