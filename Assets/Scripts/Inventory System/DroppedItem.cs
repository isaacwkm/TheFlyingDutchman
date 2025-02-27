using System;
using Needle.Console;
using UnityEngine;

[RequireComponent(typeof(Interactable))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoxCollider))]
public class DroppedItem : MonoBehaviour
{
    public Quaternion defaultRotation = Quaternion.Euler(0, 0, 0);
    public ActionSound dropSound;
    [HideInInspector] public int itemID = 1;
    private Rigidbody rb;
    private Collider itemCollider;
    private bool hasLanded = false;
    private Interactable interactTarget;
    private bool silentDropEnabled = false;

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
    
    public void enableSilentDrop(bool enabled){ // If enabled, the object is invisble while falling and produces no sound. Great for spawning items in without any visible drop animation.
        silentDropEnabled = enabled;
    }
    private void Start(){
        int ActiveItemID = gameObject.GetComponent<ActiveItem>().itemIDPleaseDoNotChange;
        itemID = ActiveItemID;
        if (silentDropEnabled == true){
            gameObject.GetComponent<MeshRenderer>().enabled = false; // make the object invisible while it is falling
        }
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
        itemCollider.enabled = true;
        itemCollider.isTrigger = false; // Enable collider for ground detection
        hasLanded = false; // Reset hasLanded

        // Start falling
        // Set drop position
        transform.position = dropPosition;

        // Set Drop rotation
        Quaternion rotationModifier = Quaternion.Euler(0f, UnityEngine.Random.Range(0, 360), 90f);
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

        // Play drop sound
        if (silentDropEnabled == false && dropSound != null){
            dropSound.PlaySingleRandom();
        }
        else{
            gameObject.GetComponent<MeshRenderer>().enabled = true; // make the object visible again if silent drop was enabled - silent drop makes the object invisible as it is falling.
        }
        

        // Disable physics to prevent movement
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;
        rb.useGravity = false;

        // Disable collider so player can walk through it
        if (itemCollider) itemCollider.isTrigger = true;

        transform.SetParent(surface.transform, worldPositionStays: true);

    }

    public void silentDrop(){

    }

    private void tryPickUpItem(GameObject player){
        // don't try to add the item to inventory if it doesn't have any behavior defined for when it is held in inventory
        if (gameObject.GetComponent<ActiveItem>() == null) return;
        
        Inventory inventory = player.GetComponent<Inventory>();

        inventory.attemptPickup(gameObject);
    }

    public void disablePickup(GameObject player){
        gameObject.transform.SetParent(player.transform, false);
        itemCollider.enabled = false;
        this.enabled = false;
        gameObject.SetActive(false);
    }
}
