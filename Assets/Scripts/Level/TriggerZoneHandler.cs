using System;
using UnityEngine;

public class TriggerZoneHandler : MonoBehaviour
{
    public event Action OnExit;
    public event Action OnEnter;
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            OnExit?.Invoke();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            OnEnter?.Invoke();
        }
    }
}
