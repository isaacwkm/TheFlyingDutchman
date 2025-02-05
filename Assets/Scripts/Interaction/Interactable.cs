using System;
using Needle.Console;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    public string actionText = "Interact";
    public ActionSound interactSound = null;
    public bool doCooldown = true;
    public float interactCooldownSeconds = 1;
    public ActionSound CooldownReturnSound = null;
    public InteractRequirements requirements = null;
    private bool onCooldown = false;
    public event Action<GameObject> OnInteract;
    public event Action<GameObject> OffInteract;
    private Coroutine cooldownCoroutine;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public string peekActionText()
    {
        return actionText;
    }
    public void receiveInteract(GameObject whom)
    { // Whom = the player who interacted with object
        D.Log("receiveInteract()", gameObject, "Int");
        if (onCooldown) return;

        D.Log("receiveInteract(): Interacted", gameObject, "Int");
        OnInteract?.Invoke(whom);
        playInteractSound();
        if (doCooldown)
        {
            onCooldown = true; // Prevent interaction for a cooldown
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

        // Allow button to be interacted with again
        onCooldown = false;
    }

    public bool canInteract()
    {
        if (requirements == null) return true;

        else
        {
            return requirements.getInteractAllowed();
        }
    }

    public void stopInteractCooldown()
    {
        StopCoroutine(cooldownCoroutine);
    }
}