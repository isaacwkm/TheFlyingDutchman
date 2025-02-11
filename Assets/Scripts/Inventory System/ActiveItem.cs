using UnityEngine;

[RequireComponent(typeof(DroppedItem))]
public class ActiveItem : MonoBehaviour
{
    [Tooltip("Do not re-assign during runtime.")] 
    public int itemIDPleaseDoNotChange = 1; // Do not re-assign during runtime. The weird variable naming is only to discourage ID re-assignment after it has been correctly set to the right item id.
    public Vector3 heldPositionOffset;
    public Vector3 heldRotationOffset;
    public Animator handAnim;
    public bool hasAttack = false;
    public string attackAnimName;
    private Quaternion defaultRotation;

    void Awake(){
        defaultRotation = gameObject.GetComponent<DroppedItem>().defaultRotation;
    }
    public void SnapToHand(Transform item, Transform handPosition)
    {
        // Parent the item to the hand position
        item.SetParent(handPosition);

        // Reset the local position and rotation to align it perfectly with the hand
        item.localScale = Vector3.one; // Reset local scale
        item.localPosition = Vector3.zero + heldPositionOffset;
        item.localRotation = defaultRotation * Quaternion.Euler(heldRotationOffset);
    }

    public void doAttack(){
        if (!hasAttack) return;

        if (handAnim.GetCurrentAnimatorStateInfo(0).normalizedTime > 1 && !handAnim.IsInTransition(0)){
            handAnim.Play(attackAnimName, -1, 0);
        }
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
