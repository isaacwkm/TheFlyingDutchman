using System;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    [SerializeField] private string actionText = "Interact";
    public event Action<GameObject> OnInteract;

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
    public void receiveInteract(GameObject whom) {
        // Debug.Log("receiveInteract()");
        OnInteract?.Invoke(whom);
    }
}