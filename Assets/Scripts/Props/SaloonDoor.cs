using UnityEngine;

public class DoorPushTrigger : MonoBehaviour
{
    public Rigidbody doorRigidbody;
    public float pushForce = 50f;
    public ActionSound DoorOpenChimeSound;

    void OnTriggerEnter(Collider other)
    {
        DoorOpenChimeSound.PlaySingleRandom();
        if (other.CompareTag("Player"))
        {
            Vector3 direction = (doorRigidbody.position - other.transform.position).normalized;
            direction.y = 0; // Keep it horizontal
            doorRigidbody.AddForce(direction * pushForce, ForceMode.Impulse);
        }
    }
}
