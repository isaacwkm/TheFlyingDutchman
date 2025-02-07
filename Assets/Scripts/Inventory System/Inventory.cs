using Needle.Console;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    public ItemCatalog catalog;
    public Transform[] iconPositionTransforms;
    public GameObject activeItemMarker;
    public Transform handPosition; // Assign the "RightHandPosition" in the Inspector
    private GameObject[] itemsInSlots = new GameObject[4];
    private GameObject[] itemIcons = new GameObject[4];
    private int currentActiveSlot = 0;
    private int inventorySize = 0;
    private int inventoryCapacity = 4;
    private int offsetActiveItemIcon = -40;

    void Awake()
    {
    }
    void OnEnable()
    {
        //interactTarget.OnItemPickup += addItem;

    }

    void OnDisable()
    {
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

    public bool isFull()
    {
        if (inventorySize >= inventoryCapacity) return true;
        else return false;
    }

    public bool attemptPickup(GameObject item)
    {
        // If inventory is full, do nothing
        // If it's not full, add it to inventory.
        if (!isFull())
        {
            item.GetComponent<DroppedItem>().disablePickup(gameObject); // passing gameObject (the player this is attached to) as the object to follow for newly gained item

            int slot = findFirstOpenSlotIndex();
            addItem(item, slot);
            return true;
        }
        return false;
    }

    private int findFirstOpenSlotIndex()
    {
        if (isFull()) throw new System.Exception("Check if inventory is full before finding first open slot index!"); ;

        for (int i = 0; i < inventoryCapacity; i++)
        {
            if (itemsInSlots[i] == null)
            {
                return i;
            }
        }
        return -1; // Should never occur
    }

    // Methods called by event subscription

    private void addItem(GameObject item, int slotNum)
    {
        inventorySize++;
        int itemID = item.GetComponent<DroppedItem>().itemID; // Used to obtain Texture data from catalog
        D.Log($"{itemID} is the item ID being added to slot ${slotNum}.", gameObject, "Inv");

        GameObject newIcon = new GameObject("ItemIcon", typeof(RectTransform), typeof(RawImage)); // Create object to hold rawimage
        newIcon.transform.SetParent(iconPositionTransforms[slotNum], false); // Set parent while preserving UI layout
        RawImage rawImage = newIcon.GetComponent<RawImage>(); // Get the RawImage component
        rawImage.texture = Instantiate(catalog.itemIconCatalog[itemID], new Vector3(0, 0, 0), Quaternion.identity); // Set the texture field of the rawimage component

        // Equip the new item if the player was empty-handed
        if (nothingEquipped())
        {
            itemsInSlots[slotNum] = item;
            itemIcons[slotNum] = newIcon;
            switchToSlot(slotNum, true);
        }
        else{
            itemsInSlots[slotNum] = item;
            itemIcons[slotNum] = newIcon;
        }
    }

    public void dropItem(){
        if (nothingEquipped()) return; // do nothing if the player is holding nothing

        // if the player is holding something...

        // First initialize some variables to drop the item
        GameObject item = itemsInSlots[currentActiveSlot];
        DroppedItem droppedItemComponent = item.GetComponent<DroppedItem>();
        Transform playerTransform = gameObject.transform;
        Vector3 dropPosition = playerTransform.position + playerTransform.forward * 0.6f + Vector3.up * 0.6f;
        ActiveItem activeItemComponent = item.GetComponent<ActiveItem>();

        activeItemComponent.enabled = false; // Disable activeItem component
        droppedItemComponent.enabled = true;
        droppedItemComponent.Drop(dropPosition); // Drop it.

        // Cleanup
        Destroy(itemIcons[currentActiveSlot]); // remove the texture
        itemIcons[currentActiveSlot] = null; // remove dangling pointer just in case unity doesnt do it for us
        itemsInSlots[currentActiveSlot] = null; // Set dropped item pointer to null
        inventorySize--; // update inventory size
    }

    public void switchToNext()
    {
        makeItemInactive(currentActiveSlot);
        currentActiveSlot++;
        if (currentActiveSlot > inventoryCapacity - 1)
        {
            currentActiveSlot = 0;
        }

        makeItemActive(currentActiveSlot);

        setActiveSlotMarker(currentActiveSlot);
    }

    public void switchToPrev()
    {
        makeItemInactive(currentActiveSlot);
        currentActiveSlot--;
        if (currentActiveSlot < 0)
        {
            currentActiveSlot = inventoryCapacity - 1;
        }

        makeItemActive(currentActiveSlot);

        setActiveSlotMarker(currentActiveSlot);
    }

    public void switchToSlot(int slot, bool forceReEquip = false)
    {
        if (slot == currentActiveSlot)
        { // Do nothing if the slot to switch to is the same as current active slot. Except in case of a forceReEquip override.

            if (forceReEquip == true)
            {
                makeItemActive(slot);
            }

            return;
        }

        if (slot < 0 || slot > inventoryCapacity - 1)
        {
            D.LogError("Out of bounds access", gameObject, "Inv");
            return;
        }
        makeItemInactive(currentActiveSlot);
        currentActiveSlot = slot;
        makeItemActive(currentActiveSlot);

        setActiveSlotMarker(currentActiveSlot);
    }

    private void setActiveSlotMarker(int activeSlot)
    {
        Vector3 iconPos = iconPositionTransforms[activeSlot].position;
        iconPos.y += offsetActiveItemIcon;
        activeItemMarker.transform.position = iconPos;
    }

    private void makeItemActive(int slotNum)
    {
        D.Log("makeItemActive", gameObject, "Inv");
        // Initialization
        GameObject item = itemsInSlots[slotNum];

        // Exit if making nothing Active
        if (item == null) return;

        // Resume initialization
        ActiveItem activeItemComponent = item.GetComponent<ActiveItem>();

        // Begin to set new item active
        item.SetActive(true);
        activeItemComponent.enabled = true;
        activeItemComponent.SnapToHand(item.transform, handPosition);
    }

    private void makeItemInactive(int slotNum)
    {
        D.Log("makeItemInactive", gameObject, "Inv");
        // Initialization
        GameObject item = itemsInSlots[slotNum];

        // Exit if making nothing Active
        if (item == null) return;

        // Resume initialization
        ActiveItem activeItemComponent = item.GetComponent<ActiveItem>();

        // Begin to set item inactive
        // activeItemComponent.SnapToHand(item.transform); // not needed, but keep for reflective programming 
        activeItemComponent.enabled = false;
        item.SetActive(false);
    }

    public bool nothingEquipped()
    {
        if (itemsInSlots[currentActiveSlot] == null)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
