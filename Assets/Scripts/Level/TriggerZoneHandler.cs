using System;
using UnityEngine;

public class TriggerZoneHandler : MonoBehaviour
{
    public event Action<Collider> OnExit;
    public event Action<Collider> OnEnter;
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            OnExit?.Invoke(other);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            OnEnter?.Invoke(other);
        }
    }
}
