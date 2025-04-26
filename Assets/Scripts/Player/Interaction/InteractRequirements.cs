using System;
using UnityEngine;
using System.Collections.Generic;

public class InteractRequirements : MonoBehaviour
{
    [SerializeField] 
    public string[] requirements; // Array of strings for the user to create requirements in
    [SerializeField][Tooltip("Include any items that can be used to allow interaction with object.")]
    public int[] activeItemIdsNeeded;
    private Dictionary<string, bool> requirementsMap = null; // Map of requirements and their status for internal data use.
    private int requirementsLeft = 0;
    public event Action<GameObject> OnInteract;
    public event Action<GameObject> OffInteract;

    public void Reinitialize()
    {
        requirementsMap = new Dictionary<string, bool>();
        foreach (string requirementStr in requirements)
        {
            requirementsMap[requirementStr] = false; // initialize all requirements to false
            requirementsLeft++;
        }
    }

    void Awake()
    {
        if (requirements != null)
        {
            Reinitialize();
        }
    }

    public bool getInteractAllowed(GameObject player)
    {
        if (requirementsLeft != 0) return false; // unused - planned to be used for event systems (Progress is gated until a condition is fulfilled).

        if (!checkHeldItem(player)) return false; // if The required item is not held, return false

        return true;
    }

    private bool checkHeldItem(GameObject player){
        if (activeItemIdsNeeded.Length == 0) return true; // short-circuit evaluation - don't need to check other held item requirements if you dont need any items.

        int heldItemId = player.GetComponent<Inventory>().getHeldItemId();

        foreach(int itemId in activeItemIdsNeeded){
            if (itemId == heldItemId){
                return true;
            }
        }

        return false; // Return false if no matching ids were found.
    }
    public void UpdateRequirement(string requirement, bool status)
    {
        if (requirementsMap[requirement] == status){ // Return if there is no change to be made
            return;
        }

        if (status == true){
            setRequirementTrue(requirement);
            
        }
        else{
            setRequirementFalse(requirement);
            
        }
    }

    private void setRequirementTrue(string requirement){
        requirementsMap[requirement] = true;
        requirementsLeft--; // decrement the amount of requirements needed, in order to eliminate need for any search algorithm
    }

    private void setRequirementFalse(string requirement){
        requirementsMap[requirement] = false;
        requirementsLeft++;
    }

    public void printAllRequirements()
    {
        foreach (var requirement in requirementsMap)
        {
            Debug.Log($"Requirement: {requirement.Key}, Met: {requirement.Value}");
        }
    }
}
