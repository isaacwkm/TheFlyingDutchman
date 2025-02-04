using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoxCollider))]
public class DroppedItem : MonoBehaviour
{
    public int itemId = 1;
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
        interactTarget = gameObject.GetComponent<Interactable>();
    }

    void OnEnable() {
        
        interactTarget.OnInteract += tryPickUpItem;

    }

    void OnDisable() {
        interactTarget.OnInteract -= tryPickUpItem;
    }
    
    private void Start(){
        Drop(gameObject.transform.position);
    }
    

    public void Drop(Vector3 dropPosition)
    {
        // Enable physics for the drop
        rb.isKinematic = false;
        rb.useGravity = true;
        itemCollider.enabled = true; // Enable collider for ground detection

        // Start falling
        transform.position = dropPosition;
        transform.rotation = Quaternion.Euler(90f, Random.Range(0, 360), 0f);
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
        hasLanded = true;

        // Disable physics to prevent movement
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Disable collider so player can walk through it
        if (itemCollider) itemCollider.enabled = false;

        // Parent to the surface if it's a moving object (except players)
        if (!surface.CompareTag("Player"))
        {
            transform.SetParent(surface.transform);
        }
    }

    private void tryPickUpItem(GameObject player){
        Inventory inventory = player.GetComponent<Inventory>();

        // If inventory is full, do nothing
        if (inventory.isFull()){
            return;
        }

        // If it's not full, add it to inventory and delete it from ground.
        
    }
}
