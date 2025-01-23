using UnityEngine;

public class Inventory : MonoBehaviour
{
    public GameObject[] itemCatalog;
    private int activeItem;
    private int inventorySize = 0;
    private int inventoryCapacity = 4;
    private int[] inventoryItems = {0, 0, 0, 0};
    //private int[] inventoryKeys;
    private Vector3 inventorySpaceCoords = new Vector3(12, 0, 57);

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void addItem(GameObject item){

        Vector3 newItemCoord = inventorySpaceCoords;
        newItemCoord.x += inventorySize * 2;

        Instantiate(item, newItemCoord, Quaternion.identity);
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
