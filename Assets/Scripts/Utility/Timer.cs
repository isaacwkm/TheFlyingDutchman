using UnityEngine;
using System;

public class Timer : MonoBehaviour
{
    [SerializeField] private float initialValue = 0.0f;
    [SerializeField] private bool countUp;
    public float time {get; private set;}
    public event Action expired;
    void Start()
    {
        Reset();
    }
    void Update()
    {
        if (countUp)
        {
            time += Time.deltaTime;
        }
        else if (time > 0.0f)
        {
            time -= Time.deltaTime;
            if (time <= 0.0f)
            {
                time = 0.0f;
                expired?.Invoke();
            }
        }
    }
    public void Reset(float initialValue = float.NaN)
    {
        if (!float.IsNaN(initialValue)) this.initialValue = initialValue;
        if (countUp) time = 0.0f;
        else time = this.initialValue;
    }
}
