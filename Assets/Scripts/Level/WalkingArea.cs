using UnityEngine;
using Needle.Console;

[RequireComponent(typeof(TriggerZoneHandler))]
public class WalkingArea : MonoBehaviour
{
    [Tooltip("Speed multiplier to apply while inside (0.5 = half speed)")]
    [Range(0f, 1f)]
    public float speedMultiplier = 0.5f;

    public TriggerZoneHandler triggerZone;
    private bool isPlayerInside = false;
    private float originalWalkSpeed = -1f;

    private void OnEnable()
    {
        triggerZone.OnEnter += HandlePlayerEnter;
        triggerZone.OnExit += HandlePlayerExit;
    }

    private void OnDisable()
    {
        triggerZone.OnEnter -= HandlePlayerEnter;
        triggerZone.OnExit -= HandlePlayerExit;
    }

    private void HandlePlayerEnter(Collider playerCollider)
    {
        if (isPlayerInside) return; // Already applied

        ApplySpeedModifier(playerCollider.gameObject, speedMultiplier);
        isPlayerInside = true;

        Debug.Log($"[WalkingArea] Applied speed multiplier {speedMultiplier} to {playerCollider.name}");
    }

    private void HandlePlayerExit(Collider playerCollider)
    {
        if (!isPlayerInside) return; // Already reset

        ResetPlayerSpeed(playerCollider.gameObject);
        isPlayerInside = false;

        Debug.Log($"[WalkingArea] Reset speed to normal for {playerCollider.name}");
    }



    private void ApplySpeedModifier(GameObject player, float multiplier)
    {
        var controller = player.GetComponent<PlayerCharacterController>();
        if (controller != null)
        {
            if (originalWalkSpeed < 0f)
            {
                // Store the original speed only once
                originalWalkSpeed = controller.GetWalkSpeed();
            }

            controller.SetWalkSpeed(originalWalkSpeed * multiplier);
            D.Log($"[WalkingArea] Applied walk speed: {controller.GetWalkSpeed()} (multiplier {multiplier})");
        }
    }

    private void ResetPlayerSpeed(GameObject player)
    {
        var controller = player.GetComponent<PlayerCharacterController>();
        if (controller != null && originalWalkSpeed >= 0f)
        {
            controller.SetWalkSpeed(originalWalkSpeed);
            Debug.Log($"[WalkingArea] Reset walk speed to original: {controller.GetWalkSpeed()}");
        }
    }
}