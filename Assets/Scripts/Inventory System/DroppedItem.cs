using Needle.Console;
using UnityEngine;

[RequireComponent(typeof(Interactable))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoxCollider))]
public class DroppedItem : MonoBehaviour
{
    public Quaternion defaultRotation = Quaternion.Euler(0, 0, 0);
    [HideInInspector] public int itemID = 1;
    private Rigidbody rb;
    private Collider itemCollider;
    private bool hasLanded = false;
    private Interactable interactTarget;

    public bool InteractRequirementsMet()
    {
        return true;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        itemCollider = GetComponent<Collider>();
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic; // Prevent tunneling
        interactTarget = gameObject.GetComponent<Interactable>();
    }

    void OnEnable() {
        
        interactTarget.OnInteract += tryPickUpItem;

    }

    void OnDisable() {
        interactTarget.OnInteract -= tryPickUpItem;
    }
    
    private void Start(){
        int ActiveItemID = gameObject.GetComponent<ActiveItem>().itemIDPleaseDoNotChange;
        itemID = ActiveItemID;
        Drop(gameObject.transform.position);
    }

    public void Drop(Vector3 dropPosition)
    {
        D.Log("Dropping!", gameObject, "Item");
        // Unparent
        gameObject.transform.SetParent(null, worldPositionStays: true);

        // Enable physics for the drop
        rb.isKinematic = false;
        rb.useGravity = true;
        itemCollider.isTrigger = false; // Enable collider for ground detection
        hasLanded = false; // Reset hasLanded

        // Start falling
        // Set drop position
        transform.position = dropPosition;

        // Set Drop rotation
        Quaternion rotationModifier = Quaternion.Euler(0f, Random.Range(0, 360), 90f);
        Quaternion targetRotation = rotationModifier * defaultRotation;
        transform.rotation = targetRotation;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hasLanded) return; // Ensure landing only happens once

        GameObject hitObject = collision.gameObject;

        // Ignore collisions with the player
        if (hitObject.CompareTag("Player")) return;

        // Stick to the first surface it touches
        LandOnSurface(hitObject);
    }

    private void LandOnSurface(GameObject surface)
    {
        D.Log($"Item landed on {surface.name}!", gameObject, "Item");
        hasLanded = true;

        // Disable physics to prevent movement
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;
        rb.useGravity = false;

        // Disable collider so player can walk through it
        if (itemCollider) itemCollider.isTrigger = true;

        // Parent to the surface if it's a moving object (except players)
        if (!surface.CompareTag("Player"))
        {
            transform.SetParent(surface.transform, worldPositionStays: true);
        }
    }

    private void tryPickUpItem(GameObject player){
        Inventory inventory = player.GetComponent<Inventory>();

        inventory.attemptPickup(gameObject);
    }

    public void disablePickup(GameObject player){
        gameObject.transform.SetParent(player.transform, false);
        this.enabled = false;
        gameObject.SetActive(false);
    }
}
