using UnityEngine;

public class UsefulPlayerOperations : MonoBehaviour
{
    // Static reference to the instance
    private static UsefulPlayerOperations _instance;

    // Property to access the singleton instance
    public static UsefulPlayerOperations Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<UsefulPlayerOperations>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject("UsefulPlayerOperations");
                    _instance = obj.AddComponent<UsefulPlayerOperations>();
                }
            }
            return _instance;
        }
    }

    public void TeleportPlayer(GameObject player, Transform referenceObject, Vector3 relativeCoordinates = default)
    {
        if (player == null)
        {
            Debug.LogError("No player reference provided for teleportation!");
            return;
        }

        Vector3 targetPosition;
        Quaternion targetRotation;

        if (referenceObject != null)
        {
            targetPosition = referenceObject.position + relativeCoordinates;
            targetRotation = referenceObject.rotation;
        }
        else
        {
            targetPosition = relativeCoordinates;
            targetRotation = Quaternion.identity;
        }

        CharacterController controller = player.GetComponent<CharacterController>();
        if (controller != null)
        {
            controller.enabled = false;
            controller.transform.position = targetPosition;
            controller.transform.rotation = targetRotation;
            controller.enabled = true;
            Debug.Log($"{player.name} teleported to {targetPosition}.");
        }
        else
        {
            Debug.LogError("CharacterController not found on player.");
        }
    }
}
