using UnityEngine;

public class Ladder : MonoBehaviour
{
    void OnTriggerEnter(Collider other) {
        var pcc = other.GetComponent<PlayerCharacterController>();
        if (pcc) {
            pcc.AttachToLadder(gameObject);
        }
    }
    void OnTriggerExit(Collider other) {
        var pcc = other.GetComponent<PlayerCharacterController>();
        if (pcc && pcc.AttachedTo(gameObject)) {
            pcc.DetachFromLadder();
        }
    }
}
