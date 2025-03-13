using System;
using UnityEngine;

public class FloatPropertyInterpolator : MonoBehaviour
{
    [SerializeField] private Component targetComponent;
    [SerializeField] private string targetProperty;

    private PropertyReference propRef;
    private float targetValue;
    private float speed;

    public float currentValue
    {
        get { return (float) propRef.value; }
        set { propRef.value = value; }
    }

    public void SetTarget(Component targetComponent, string targetProperty)
    {
        this.targetComponent = targetComponent;
        this.targetProperty = targetProperty;
        propRef = new PropertyReference(targetComponent, targetProperty);
        targetValue = currentValue;
        speed = 0.0f;
    }

    void Start()
    {
        if (targetComponent != null)
        {
            SetTarget(targetComponent, targetProperty);
        }
    }

    void Update()
    {
        if (propRef != null && currentValue != targetValue)
        {
            float diffSignBefore = Mathf.Sign(targetValue - currentValue);
            currentValue += speed*Time.deltaTime*diffSignBefore;
            float diffSignAfter = Mathf.Sign(targetValue - currentValue);
            if (Mathf.Abs(diffSignBefore - diffSignAfter) >= 0.1f)
            {
                currentValue = targetValue;
                speed = 0.0f;
            }
        }
    }

    public void SetWithSpeed(float targetValue, float speed)
    {
        if (speed == 0.0f)
        {
            this.targetValue = currentValue;
            this.speed = 0.0f;
        }
        else {
            this.targetValue = targetValue;
            this.speed = Mathf.Abs(speed);
        }
    }

    public void SetWithDuration(float targetValue, float duration)
    {
        this.targetValue = targetValue;
        if (duration == 0.0f || targetValue == currentValue)
        {
            this.speed = 0.0f;
            this.currentValue = targetValue;
        }
        else
        {
            this.speed = Mathf.Abs((targetValue - currentValue)/duration);
        }
    }
}
