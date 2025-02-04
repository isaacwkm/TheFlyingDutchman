using UnityEditor.PackageManager;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public ItemCatalog catalog;
    private ActiveItem itemSnapper;
    private int activeItem;
    private int inventorySize = 0;
    private int inventoryCapacity = 4;
    private int[] inventoryItems = {0, 0, 0, 0};
    //private int[] inventoryKeys;
    private Vector3 inventorySpaceCoords = new Vector3(12, 0, 57);

    void Awake(){
        // Find the ItemSnapper component on the same GameObject
        itemSnapper = GetComponent<ActiveItem>();
    }
    void OnEnable() {
        //interactTarget.OnItemPickup += addItem;

    }

    void OnDisable() {
        //interactTarget.OnItemPickup += addItem;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    public bool isFull(){
        if (inventorySize >= inventoryCapacity) return true;
        else return false;
    }

    public void attemptPickup(GameObject item){
        if (!isFull()){
            int slot = findFirstOpenSlotIndex();

            addItem(this.gameObject, slot);
            gameObject.SetActive(false); // Disable the GameObject
            this.enabled = false;
        }
    }

    private int findFirstOpenSlotIndex(){
        if (isFull()) throw new System.Exception("Check if inventory is full before finding first open slot index!");;

        for (int i = 0; i < inventoryCapacity; i++){
            if (inventoryItems[i] == 0){
                return i;
            }
        }
        return -1; // Should never occur
    }

    // Methods called by event subscription

    private void addItem(GameObject item, int slotNum){
        
    }

    public void switchToNext(){

    }

    public void switchToPrev(){

    }

    private void switchItem(int slot){

    }

    /*
    OnCrouchInput?.Invoke(isCrouching); publish
    InputEventDispatcher.OnMovementInput += HandleMovementInput; subscribe
    
    private void HandleMovementInput(Vector2 movement); implementation
    {
        ...
    }

    */
}
