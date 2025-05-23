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
    public bool isUnlocked = false;


    void Awake()
    {
        interactCollider = gameObject.GetComponent<Collider>();
    }

    public string peekActionText()
    {
        return actionTooltip;
    }

    public string peekRequirementText()
    {
        return requirementTooltipText;
    }
    public void receiveInteract(GameObject whom)
    {
        if (onCooldown) return;
        if (!canInteract(whom)) return;

        
        if (!isUnlocked && requirements != null)
        {
            
            if (requirements.getInteractAllowed(whom))
            {
                isUnlocked = true;
                
                UnlockDoorVisuals();
            }
        }

        OnInteract?.Invoke(whom);
        playInteractSound();

        if (doCooldown)
        {
            onCooldown = true;
            DisableInteractions();
            cooldownCoroutine = StartCoroutine(InteractCooldown(whom));
        }
    }
    private void UnlockDoorVisuals()
    {
        
        Debug.Log("Door unlocked!");
        
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

    public bool canInteract(GameObject player)
    {
        if (isUnlocked) return true;  
        if (requirements == null) return true;
        return requirements.getInteractAllowed(player);
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
