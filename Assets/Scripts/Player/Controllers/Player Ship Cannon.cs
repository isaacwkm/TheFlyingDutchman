using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(ProjectileLauncher))]
public class PlayerShipCannon : MonoBehaviour
{
    [SerializeField] private float targetPointMinDistance = 50.0f;
    [SerializeField] private float targetPointMaxDistance = 100.0f;
    [SerializeField] private float targetAdjustmentSpeed = 1.0f;
    [SerializeField] private Transform reticle = null;
    [SerializeField] private float cooldown = 1.0f;

    private ProjectileLauncher launcher = null;
    private float durationCharged = 0.0f;
    private bool charging = false;

    private Action<InputAction.CallbackContext> attackPerformedAction;
    private Action<InputAction.CallbackContext> attackCanceledAction;

    void OnEnable()
    {
        var inputActions = InputModeManager.Instance.inputActions;
        inputActions.Flying.Attack.performed += (
            attackPerformedAction = ctx => charging = true
        );
        inputActions.Flying.Attack.canceled += (
            attackCanceledAction = ctx => ReleaseCharge()
        );
        charging = false;
    }

    void OnDisable()
    {
        var inputActions = InputModeManager.Instance.inputActions;
        inputActions.Flying.Attack.performed -= attackPerformedAction;
        inputActions.Flying.Attack.canceled -= attackCanceledAction;
        charging = false;
    }

    void Start()
    {
        launcher = GetComponent<ProjectileLauncher>();
    }

    void Update()
    {
        if (charging)
        {
            var target = SceneCore.camera.transform.position +
                SceneCore.camera.transform.forward*Mathf.Lerp(
                    targetPointMinDistance,
                    targetPointMaxDistance,
                    durationCharged*targetAdjustmentSpeed
                );
            launcher.Aim(target);
            if (reticle) reticle.position = target;
            durationCharged += Time.deltaTime;
        }
        else if (durationCharged < 0.0f)
        {
            durationCharged += Time.deltaTime;
        }
        else
        {
            durationCharged = 0.0f;
        }
    }

    private void ReleaseCharge()
    {
        if (TryFire()) durationCharged = -cooldown;
        else if (durationCharged > 0.0f) durationCharged = 0.0f;
        charging = false;
        launcher.Disarm();
    }

    public bool Ready()
    {
        return charging && durationCharged >= 0.0f && launcher.targetInRange;
    }

    public bool TryFire()
    {
        if (Ready())
        {
            Fire();
            return true;
        }
        else
        {
            return false;
        }
    }

    public void Fire()
    {
        launcher.Launch();
    }
}
