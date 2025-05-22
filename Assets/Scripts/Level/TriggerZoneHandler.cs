using System;
using UnityEngine;
using UnityEngine.Events;

public class TriggerZoneHandler : MonoBehaviour
{
    public event Action<Collider> OnExit;
    public event Action<Collider> OnEnter;
    [SerializeField] private UnityEvent[] enterTasks;
    [SerializeField] private UnityEvent[] exitTasks;

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            OnExit?.Invoke(other);

            // Unity Inspector events - More visual for development - DO NOT use together with C# Events, use one or the other but not both!
            foreach (var task in exitTasks)
            {
                task.Invoke();
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            OnEnter?.Invoke(other);

            // Unity Inspector events - More visual for development - DO NOT use together with C# Events, use one or the other but not both!
            foreach (var task in enterTasks)
            {
                task.Invoke();
            }
        }
    }
}
