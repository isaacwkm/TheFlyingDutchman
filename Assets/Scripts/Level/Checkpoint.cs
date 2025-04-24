using UnityEngine;
using System;
using System.Collections;
using Unity.VisualScripting;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] private bool useAsDefaultCheckpoint;

    public static event Action<Checkpoint> OnSet;
    public static event Action<Checkpoint> OnRespawn;
    public TriggerZoneHandler levelBounds;
    private static Checkpoint current = null;
    public UsefulCommands commands;

    void Awake()
    {
        commands = SceneCore.commands;
    }
    void OnEnable()
    {
        levelBounds.OnExit += Respawn;
    }
    void OnDisable()
    {
        levelBounds.OnExit -= Respawn;
    }
    public void Start()
    {
        /* If any checkpoint is marked default,
         * the last one marked default will be used by default. */
        if (useAsDefaultCheckpoint)
        {
            Set(this);
        }
    }

    private void OnTriggerEnter(Collider other) // If you touch it, it becomes a new checkpoint.
    {
        if (other.GetComponent<PlayerCharacterController>())
        {
            Set(this);
        }
    }

    public void Set(Checkpoint which)
    {
        current = which;
        OnSet?.Invoke(current);
    }

    public void Respawn(Collider playerCol)
    {
        commands.Teleport(playerCol.gameObject, gameObject.transform);
    }
}
