using UnityEngine;

public class TransformDrift : MonoBehaviour
{
    public enum DriftDirections
    {
        UP,
        DOWN,
        X_POSITIVE,
        X_NEGATIVE,
        Z_POSITIVE,
        Z_NEGATIVE
    }

    public DriftDirections driftDirection;
    public float driftSpeed = 1f;

    private Vector3 originalPosition;
    private bool isPaused = false;

    void Awake()
    {
        originalPosition = transform.position;
    }

    void Update()
    {
        if (isPaused) return;

        Vector3 direction = driftDirection switch
        {
            DriftDirections.UP => Vector3.up,
            DriftDirections.DOWN => Vector3.down,
            DriftDirections.X_POSITIVE => Vector3.right,
            DriftDirections.X_NEGATIVE => Vector3.left,
            DriftDirections.Z_POSITIVE => Vector3.forward,
            DriftDirections.Z_NEGATIVE => Vector3.back,
            _ => Vector3.zero
        };

        transform.position += direction * driftSpeed * Time.deltaTime;
    }

    public void ResetPosition()
    {
        transform.position = originalPosition;
    }

    public void SetPaused(bool pause)
    {
        isPaused = pause;
    }

    public void TogglePaused()
    {
        isPaused = !isPaused;
    }

    public bool IsPaused()
    {
        return isPaused;
    }
}
