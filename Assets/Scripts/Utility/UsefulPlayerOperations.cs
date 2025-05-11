using UnityEngine;

public class UsefulCommands : MonoBehaviour
{
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
        PlayerShip playerShip = subject.GetComponent<PlayerShip>();
        if (controller != null)
        {
            controller.enabled = false;
            controller.transform.position = targetPosition;
            controller.transform.rotation = targetRotation;
            controller.enabled = true;
            Debug.Log($"{subject.name} teleported to {targetPosition}.");
        }
        else if (playerShip != null)
        {
            FlyingVehicle flyingVehicle = playerShip.physicsObject.GetComponent<FlyingVehicle>();
            flyingVehicle.TeleportShip(targetPosition, targetRotation);
            Debug.Log($"Special teleport just for the ship! Teleported to {targetPosition}.");
        }
        else
        {
            subject.transform.position = targetPosition;
            subject.transform.rotation = targetRotation;
            Debug.Log($"{subject.name} teleported to {targetPosition}.");
        }
    }
}
