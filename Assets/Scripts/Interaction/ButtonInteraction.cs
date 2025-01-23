using UnityEngine;

public class ButtonInteraction : MonoBehaviour
{
    [SerializeField] private Interactable interactTarget;
    [SerializeField] private GameObject buttonObj;
    [SerializeField] private ButtonTask buttonTasksObj;
    
    // Class-specific
    public Vector3 pressOffset = new Vector3(0, 0, +0.1f); // Offset when pressed (relative to the button's original position)
    public float pressDuration = 1f; // Time the button stays pressed

    private Vector3 originalPosition; // Store the original position of the button


    void OnEnable() {
        interactTarget.OnInteract += DoInteraction;
    }

    void OnDisable() {
        interactTarget.OnInteract -= DoInteraction;
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Save the button's starting position
        originalPosition = buttonObj.transform.localPosition;
    }

    void DoInteraction(GameObject player){ // gameObject player = reference to the player that interacted with button

    interactTarget.OnInteract -= DoInteraction;
    moveButton(buttonObj);
    doButtonFunction();
        
    }

    void doButtonFunction(){
        buttonTasksObj.doTasks();
    }

    void moveButton(GameObject button){

        // Start the press animation
        StartCoroutine(PressAnimation());
    }

    private System.Collections.IEnumerator PressAnimation()
    {
        // Move the button to the "pressed" position
        buttonObj.transform.localPosition = originalPosition + pressOffset;

        // Wait for the duration
        yield return new WaitForSeconds(pressDuration);

        // Return the button to its original position
        buttonObj.transform.localPosition = originalPosition;

        // Allow button to be interacted with again
        interactTarget.OnInteract += DoInteraction;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
