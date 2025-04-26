using System;
using Needle.Console;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    [SerializeField] public string actionTooltip = "Interact";
    public ActionSound interactSound = null;
    public bool doCooldown = true;
    public float interactCooldownSeconds = 1;
    public ActionSound CooldownReturnSound = null;
    public InteractRequirements requirements = null;
    public string requirementTooltipText = "Need key";
    private bool onCooldown = false;
    public event Action<GameObject> OnInteract;
    public event Action<GameObject> OffInteract;
    private Coroutine cooldownCoroutine;
    private Collider interactCollider; // Collider that the player has to interact with

    void Awake()
    {
        interactCollider = gameObject.GetComponent<Collider>();
    }

    public string peekActionText()
    {
        return actionTooltip;
    }

    public string peekRequirementText(){
        return requirementTooltipText;
    }
    public void receiveInteract(GameObject whom)
    { // Whom = the player who interacted with object
        D.Log("receiveInteract()", gameObject, "Int");

        if (onCooldown) return;
        if (!canInteract(whom)) return;

        // At this point, all checks have been passed and the object will be interacted with.
        D.Log("receiveInteract(): Interacted", gameObject, "Int");
        OnInteract?.Invoke(whom);
        playInteractSound();
        if (doCooldown)
        {
            onCooldown = true; // Prevent interaction for a cooldown
            DisableInteractions();
            cooldownCoroutine = StartCoroutine(InteractCooldown(whom)); // Start cooldown
        }
    }

    private void playInteractSound()
    {
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

        // Allow object to be interacted with again
        onCooldown = false;
        EnableInteractions();
    }

    public bool canInteract(GameObject player) // Asks whether the player can interact
    {
        if (requirements == null) return true; // Defaults to true if no requirements were given

        else
        {
            return requirements.getInteractAllowed(player); // Loops through requirement conditions if requirements were provided.
        }
    }

    public void stopInteractCooldown()
    {
        StopCoroutine(cooldownCoroutine);
    }

    public void DisableInteractions() // Useful for when interacting with an object should hide the interact tooltip. Also used during cooldowns.
    {
        interactCollider.enabled = false;
    }

    public void EnableInteractions()
    {
        interactCollider.enabled = true;
    }
}
