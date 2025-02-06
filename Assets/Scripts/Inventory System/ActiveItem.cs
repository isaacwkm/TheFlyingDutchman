using UnityEngine;

public class ActiveItem : MonoBehaviour
{
    [Tooltip("Do not re-assign during runtime.")] 
    public int itemIDPleaseDoNotChange = 1; // Do not re-assign during runtime. The weird variable naming is only to discourage ID re-assignment after it has been correctly set to the right item id.

    public void SnapToHand(Transform item, Transform handPosition)
    {
        // Parent the item to the hand position
        item.SetParent(handPosition);

        // Reset the local position and rotation to align it perfectly with the hand
        item.localScale = Vector3.one; // Reset local scale
        item.localPosition = Vector3.zero;
        item.localRotation = Quaternion.identity;
    }

    void OnEnable(){
    }

    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
