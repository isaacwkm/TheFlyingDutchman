using System;
using UnityEngine;
using UnityEngine.Events;
using Needle.Console;

public class TriggerZoneHandler : MonoBehaviour
{
    public string tagToCheck = "Player";
    public event Action<Collider> OnExit;
    public event Action<Collider> OnEnter;
    [SerializeField] private UnityEvent[] enterTasks;
    [SerializeField] private UnityEvent[] exitTasks;
    private bool playerInside = false;

    public bool IsPlayerInside() => playerInside;

    void Start()
    {
        if (tagToCheck != "Player")
        {
            D.LogWarning("TriggerZoneHandler's implementation does not currently handle tags other than 'Player'. " +
                             "Please implement your own version of this script or propose changes if you need to use a different tag.",
                             gameObject, LogManager.LogCategory.Any);
        }
    }
    void OnTriggerExit(Collider other)
    {
        playerInside = false;

        if (other.CompareTag(tagToCheck))
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
        playerInside = true;

        if (other.CompareTag(tagToCheck))
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
