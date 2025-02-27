using UnityEngine;

public class LevelBounds : MonoBehaviour
{
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Checkpoint.Respawn();
        }
    }
}