using System;
using UnityEngine;

public class Teleporter: ButtonTask
{
    [SerializeField] private Vector3 relativeCoordinates = Vector3.zero; // The offset from the referenceObject or scene origin
    [SerializeField] private Transform referenceObject; // The GameObject to use as a reference (e.g., the player's ship)

    public override void DoTasks(GameObject player) {

        if (player == null)
        {
            Debug.LogError("No player reference provided for teleportation!");
            return;
        }

        Vector3 targetPosition;

        if (referenceObject != null)
        {
            // Calculate position relative to the reference object
            targetPosition = referenceObject.position + relativeCoordinates;
        }
        else
        {
            // Use the offset directly from the scene's origin
            targetPosition = relativeCoordinates;
        }

        CharacterController controller = player.GetComponent<CharacterController>();
            if (controller != null)
            {
                controller.enabled = false; // Disable to prevent physics issues
                controller.transform.position = targetPosition; // Teleport the player
                controller.enabled = true; // Re-enable the controller
                Debug.Log($"{player.name} teleported to {targetPosition}.");
            }
            else
            {
                Debug.LogError("CharacterController not found on player.");
            }
    }
}
