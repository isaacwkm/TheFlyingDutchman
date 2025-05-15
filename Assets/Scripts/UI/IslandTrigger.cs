using UnityEngine;
//Attached to the trigger object
// This script handles the trigger for displaying the island name when the player enters the trigger zone
// Enter the name of the island in the inspector
// Attach the IslandNameDisplay in the inspector
public class IslandTrigger : MonoBehaviour
{
    public string islandName;
    public IslandDisplay islandDisplay; // Reference to the IslandDisplay script

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Show the island name when the player enters the trigger
            islandDisplay.ShowIslandName(islandName);
        }
    }
}
