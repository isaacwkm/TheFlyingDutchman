using UnityEngine;

public class UsefulCommands : MonoBehaviour
{
    // Static reference to the instance
    private static UsefulCommands _instance;

    // Property to access the singleton instance
    public static UsefulCommands Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<UsefulCommands>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject("UsefulPlayerOperations");
                    _instance = obj.AddComponent<UsefulCommands>();
                }
            }
            return _instance;
        }
    }

    public void Teleport(GameObject subject, Transform referenceObject, Vector3 relativeCoordinates = default)
    {
        if (subject == null)
        {
            Debug.LogError("No subject provided for teleportation!");
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

        CharacterController controller = subject.GetComponent<CharacterController>();
        if (controller != null)
        {
            controller.enabled = false;
            controller.transform.position = targetPosition;
            controller.transform.rotation = targetRotation;
            controller.enabled = true;
            Debug.Log($"{subject.name} teleported to {targetPosition}.");
        }
        else
        {
            Debug.LogError("CharacterController not found on player.");
            subject.transform.position = targetPosition;
            subject.transform.rotation = targetRotation;
        }
    }
}
