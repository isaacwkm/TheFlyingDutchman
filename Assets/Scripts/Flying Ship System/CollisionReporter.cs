using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CollisionReporter : MonoBehaviour
{
    [SerializeField] private bool cacheCollisions = true;
    [SerializeField] private bool cacheTriggers = true;

    public event Action<Collision> collisionEnter;
    public event Action<Collision> collisionExit;
    public event Action<Collision> collisionStay;
    public event Action<Collider> triggerEnter;
    public event Action<Collider> triggerExit;
    public event Action<Collider> triggerStay;

    private List<Collider> currentCollisions;
    private List<Collider> currentTriggers;

    public void Start()
    {
        if (cacheCollisions)
        {
            currentCollisions = new();
        }
        if (cacheTriggers)
        {
            currentTriggers = new();
        }
    }

    public void OnCollisionEnter(Collision arg)
    {
        if (cacheCollisions && !currentCollisions.Contains(arg.collider))
        {
            currentCollisions.Add(arg.collider);
        }
        collisionEnter?.Invoke(arg);
    }

    public void OnCollisionExit(Collision arg)
    {
        if (cacheCollisions && currentCollisions.Contains(arg.collider))
        {
            currentCollisions.Remove(arg.collider);
        }
        collisionExit?.Invoke(arg);
    }

    public void OnCollisionStay(Collision arg)
    {
        collisionStay?.Invoke(arg);
    }

    public void OnTriggerEnter(Collider arg)
    {
        if (cacheTriggers && !currentTriggers.Contains(arg))
        {
            currentTriggers.Add(arg);
        }
        triggerEnter?.Invoke(arg);
    }

    public void OnTriggerExit(Collider arg)
    {
        if (cacheTriggers && currentTriggers.Contains(arg))
        {
            currentTriggers.Remove(arg);
        }
        triggerExit?.Invoke(arg);
    }

    public void OnTriggerStay(Collider arg)
    {
        triggerStay?.Invoke(arg);
    }

    public ReadOnlyCollection<Collider> collisions
    {
        get { return currentCollisions.AsReadOnly(); }
    }

    public ReadOnlyCollection<Collider> triggers
    {
        get { return currentTriggers.AsReadOnly(); }
    }
}
