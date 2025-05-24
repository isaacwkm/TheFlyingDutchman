using UnityEngine;
using Needle.Console;

public class DoubleDoorTrigger : MonoBehaviour
{
    [System.Serializable]
    public class DoorLeaf
    {
        public Rigidbody doorRigidbody;
        public bool reverseSwing = false; // Flip the push direction if needed
    }

    [Tooltip("Array of door leaves (e.g., left and right doors)")]
    public DoorLeaf[] doorLeaves;

    [Tooltip("Force applied when pushing the door")]
    public float pushForce = 5f;

    [Tooltip("Animator controlling the door chime")]
    public Animator doorChimeAnimator;

    [Tooltip("Name of the door chime animation clip to play")]
    public string doorChimeAnimationClipName;

    [Tooltip("Audio component or script to play door open chime sound")]
    public ActionSound DoorOpenChimeSound;

    private void OnTriggerEnter(Collider other)
    {
        DoorOpenChimeSound.PlaySingleRandom();

        if (other.CompareTag("Player"))
        {
            if (doorLeaves.Length == 0)
            {
                D.LogWarning("No door leaves assigned to the trigger.", gameObject, LogManager.LogCategory.Any);
                return;
            }

            foreach (var leaf in doorLeaves)
            {
                Rigidbody doorRb = leaf.doorRigidbody;

                if (doorRb == null)
                {
                    D.LogWarning("One of the door leaves has no Rigidbody assigned.", gameObject, LogManager.LogCategory.Any);
                    continue;
                }

                // Get the door's forward axis
                Vector3 doorForward = doorRb.transform.forward;
                doorForward.y = 0;
                doorForward.Normalize();

                // Determine which side the player is on relative to the door
                Vector3 toPlayer = other.transform.position - doorRb.position;
                toPlayer.y = 0;

                float side = Vector3.Dot(toPlayer, doorForward); // + = front, - = back

                // Always push along door's aligned axis, based on which side the player is on
                Vector3 pushDirection = (side >= 0) ? doorForward : -doorForward;

                // Apply reversal if this door leaf swings opposite
                if (leaf.reverseSwing)
                {
                    pushDirection = -pushDirection;
                }

                // Apply the force
                doorRb.AddForce(pushDirection * pushForce, ForceMode.Impulse);
            }

            // Play the door chime animation
            if (doorChimeAnimator != null && !string.IsNullOrEmpty(doorChimeAnimationClipName))
            {
                doorChimeAnimator.Play(doorChimeAnimationClipName, -1, 0);
            }
        }
    }
}
