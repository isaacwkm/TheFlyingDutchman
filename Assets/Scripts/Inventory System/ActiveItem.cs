using UnityEngine;

public class ActiveItem : MonoBehaviour
{
    public Transform handPosition; // Assign the "RightHandPosition" in the Inspector

    public void SnapToHand(Transform item)
    {
        // Parent the item to the hand position
        item.SetParent(handPosition);

        // Reset the local position and rotation to align it perfectly with the hand
        item.localPosition = Vector3.zero;
        item.localRotation = Quaternion.identity;
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
