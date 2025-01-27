using UnityEngine;

public class ButtonInteraction : MonoBehaviour
{
    [SerializeField] private Interactable interactTarget;
    [SerializeField] private GameObject buttonObj;
    [SerializeField] private TaskExecutor taskList;
    
    // Class-specific
    public Vector3 pressOffset = new Vector3(0, 0, +0.1f); // Offset when pressed (relative to the button's original position)
    public float pressDuration = 1f; // Time the button stays pressed

    private Vector3 originalPosition; // Store the original position of the button


    void OnEnable() {
        interactTarget.OnInteract += DoInteraction;
        interactTarget.OffInteract += ReturnButton;
    }

    void OnDisable() {
        interactTarget.OnInteract -= DoInteraction;
        interactTarget.OffInteract -= ReturnButton;
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Save the button's starting position
        originalPosition = buttonObj.transform.localPosition;
    }

    void DoInteraction(GameObject player){ // gameObject player = reference to the player that interacted with button
    //Debug.Log("DoInteraction()");
    if (!interactTarget.isActiveAndEnabled) return;
    
    interactTarget.enabled = false;
    moveButton(buttonObj);
    doButtonFunction(player);
        
    }
    void doButtonFunction(GameObject player){
        taskList.ExecuteAllCommands(player);
    }

    void moveButton(GameObject button){
        // Move the button to the "pressed" position
        buttonObj.transform.localPosition = originalPosition + pressOffset;
    }

    void ReturnButton(GameObject player)
    {
        // Return the button to its original position
        buttonObj.transform.localPosition = originalPosition;

        // Allow button to be interacted with again
        interactTarget.enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
