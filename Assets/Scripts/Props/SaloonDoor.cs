using UnityEngine;
using Needle.Console;
public class DoorPushTrigger : MonoBehaviour
{
    public Rigidbody[] doorRigidbodies;
    public float pushForce = 50f;
    public ActionSound DoorOpenChimeSound;
    public Animator doorChimeAnimator;
    public string doorChimeAnimationClipName = "MUST ASSIGN ANIMATION CLIP NAME";

    void OnTriggerEnter(Collider other)
    {
        DoorOpenChimeSound.PlaySingleRandom();
        if (other.CompareTag("Player"))
        {
            if (doorRigidbodies.Length == 0)
            {
                D.LogWarning("No door rigidbodies assigned to the trigger.", gameObject, LogManager.LogCategory.Any);
                return;
            }

            // Play the door chime animation
            doorChimeAnimator.Play(doorChimeAnimationClipName,-1, 0);

            Vector3 direction = (doorRigidbodies[0].position - other.transform.position).normalized;
            direction.y = 0; // Keep it horizontal

            for (int i = 0; i < doorRigidbodies.Length; i++)
            {
                // Apply force to each door's Rigidbody
                doorRigidbodies[i].AddForce(direction * pushForce, ForceMode.Impulse);
            }
        }
    }
}
