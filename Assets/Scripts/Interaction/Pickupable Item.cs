using UnityEngine;

public class PickupableItem : MonoBehaviour
{
    private Interactable interactTarget;
    [SerializeField] private Inventory inventorySystem;

    void Awake(){
        interactTarget = GetComponent<Interactable>();
    }

    void OnEnable() {
        interactTarget.OnInteract += pickUp;
    }

    void OnDisable() {
        interactTarget.OnInteract -= pickUp;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    void pickUp(GameObject player){
        inventorySystem.attemptPickup(this.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
