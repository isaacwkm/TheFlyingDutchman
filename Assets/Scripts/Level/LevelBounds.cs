using UnityEngine;

public class LevelBounds : MonoBehaviour
{
    void OnTriggerExit(Collider playerCollider)
    {
        if (playerCollider.GetComponent<PlayerCharacterController>())
        {
            Checkpoint.Respawn();
        }
    }
}
