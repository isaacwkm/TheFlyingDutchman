using UnityEngine;
using System;
using System.Collections;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] private bool useAsDefaultCheckpoint;

    public static event Action<Checkpoint> OnSet;
    public static event Action<Checkpoint> OnRespawn;

    private static Checkpoint current = null;

    public void Start()
    {
        /* If any checkpoint is marked default,
         * the last one marked default will be used by default. */
        if (useAsDefaultCheckpoint)
        {
            Set(this);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerCharacterController>())
        {
            Set(this);
        }
    }

    public static void Set(Checkpoint which)
    {
        current = which;
        OnSet?.Invoke(current);
    }

    public static void Respawn(Checkpoint which = null)
    {
        (which ? which : current)?.StartCoroutine("RespawnCoro");
    }

    private IEnumerator RespawnCoro()
    {
        InputModeManager.Instance.DisableAllControls();
        SceneCore.playerCharacter.enabled = false;
        SceneCore.playerCharacter.transform.position = transform.position;
        OnRespawn?.Invoke(this);
        yield return new WaitForSeconds(0.02f);
        SceneCore.playerCharacter.enabled = true;
        InputModeManager.Instance.SwitchToPlayerControls();
    }
}
