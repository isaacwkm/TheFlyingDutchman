using System;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    [SerializeField] private string actionText = "Interact";
    [SerializeField] private float interactCooldownSeconds = 1;
    [SerializeField] private ActionSound interactSound = null;
    [SerializeField] private ActionSound CooldownReturnSound = null;
    [SerializeField] private string FailConditionText = "Need the right tool for this!";
    private bool canInteract = true;
    public event Action<GameObject> OnInteract;
    public event Action<GameObject> OffInteract;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public string peekActionText(){
        return actionText;
    }
    public void receiveInteract(GameObject whom) { // Whom = the player who interacted with object
        // Debug.Log("receiveInteract()");
        if (!canInteract) return;

        // Debug.Log("receiveInteract(): Interacted");
        OnInteract?.Invoke(whom);
        playInteractSound();
        canInteract = false; // Prevent interaction for a cooldown
        StartCoroutine(InteractCooldown(whom)); // Start cooldown
    }

    private void playInteractSound(){
        if (interactSound == null) return;

        Debug.Log("Playing button sound");
        interactSound.PlaySingleRandom();
    }

    private System.Collections.IEnumerator InteractCooldown(GameObject whom) // Whom = the player who initially interacted with object
    {
        // Wait for the duration
        yield return new WaitForSeconds(interactCooldownSeconds);

        // Play any sound effects associated with animation end
        if (CooldownReturnSound != null)
        CooldownReturnSound.PlaySingleRandom();

        // Send out an event
        OffInteract?.Invoke(whom);

        // Allow button to be interacted with again
        canInteract = true;
    }

    public bool hasInteractionPrerequisites(){
        return true;
    }
}