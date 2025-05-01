using System;
using UnityEngine;

public class ProjectileLauncher : MonoBehaviour
{
    // Raised when the launcher launches. Passes the projectile instance.
    public event Action<Rigidbody> launch;

    /* Projectile to instantiate and launch when Launch is called.
     * Note that this property is public: other scripts can change it.
     * (Alternatively, for one-time overrides,
     * an alternative projectile to instantiate can be passed to Launch.) */
    [SerializeField] public Rigidbody projectile = null;

    /* Point where the projectile spawns from,
     * in local space relative to the launcher,
     * when the launcher is aiming directly forward in that space. */
    [SerializeField] private Vector3 projectileSpawnOffset = Vector3.zero;
    // Effects object to spawn alongside projectile. May be null.
    [SerializeField] private TemporaryGameObject effectOnLaunch = null;

    /* Game object to try to hit.
     * Note that this property is public: other scripts can change it.
     * Depending on how the script using this control interface is implemented,
     * it may or may not make sense to leave this null
     * (as the caller script might be responsible for setting it). */
    [SerializeField] public Transform targetObject = null;

    // Initial speed of projectile when launched.
    [SerializeField] private float launchSpeed = 1.0f;
    // Cooldown between consecutive launches.
    [SerializeField] private float cooldown = 0.0f;
    // Whether to limit aim speed. If not, always aim directly at target.
    [SerializeField] private bool useAimSpeed = false;
    // Aim speed in degrees per second.
    [SerializeField] private float aimSpeed = 15.0f;
    // Least allowed pitch.
    [SerializeField] private float minPitch = -90.0f;
    // Greatest allowed pitch.
    [SerializeField] private float maxPitch = 90.0f;
    // Least allowed yaw.
    [SerializeField] private float minYaw = -180.0f;
    // Greatest allowed yaw.
    [SerializeField] private float maxYaw = 180.0f;
    // If target is this close or closer, do not track it.
    [SerializeField] private float aimDistanceThreshold = 0.01f;

    private Vector3 targetPoint = Misc.NaNVec;
    private float cooldownTimer = 0.0f;

    /* Point to try to hit.
     * This variable is computed and cannot be set.
     * To alter it, use the Aim method.
     * If this point is Misc.NaNVec,
     * it indicates the launcher is currently disarmed
     * (has no target). */
    public Vector3 target
    {
        get
        {
            if (targetObject == null) return targetPoint;
            else return targetObject.position;
        }
    }

    /* Unit vector representing the direction the launcher must shoot in
     * to hit its target. */
    public Vector3 aim { get; private set; }
    /* Unit vector representing the direction the launcher will shoot in
     * if Launch is called right at this moment.
     * This should only ever differ from aim proper
     * if useAimSpeed is true, and if it differs thus,
     * then that indicates the launcher's calibration
     * is lagging behind its target. */
    public Vector3 currentAim { get; private set; }

    /* Point in world space
     * where a projectile launched right at this moment will spawn. */
    public Vector3 projectileSpawnPoint
    {
        get => CalculateProjectileSpawnPoint(currentAim);
    }

    /* Whether the launcher's angle constraints and transform allow
     * for it to aim at its target. */
    public bool targetInRange
    {
        get => !Misc.IsNaNVec(aim);
    }

    /* Whether the launcher is currently aiming at its target,
     * as opposed to trying to catch up to it. */
    public bool targetInLineOfFire
    {
        get => Mathf.Approximately(0.0f, Vector3.Angle(currentAim, aim));
    }

    /* Current yaw (turning-around aim).
     * Animator component should use this e.g.
     * to update the child objects' orientations. */
    public float yaw { get => CalculateYaw(currentAim); }
    /* Current pitch (up/down aim).
     * Animator component should use this e.g.
     * to update the child objects' orientations. */
    public float pitch { get => CalculatePitch(currentAim); }

    void Start()
    {
        aim = Misc.NaNVec;
        currentAim = transform.forward;
    }

    void Update()
    {
        Recalibrate();
        TurnToAim();
        if (cooldownTimer > 0.0f) cooldownTimer -= Time.deltaTime;
    }

    /* Given a unit vector representing a direction,
     * calculates where a launched projectile will spawn
     * if the launcher is aiming in the given direction at the time. */
    private Vector3 CalculateProjectileSpawnPoint(Vector3 givenAim)
    {
        Vector3 xz, y;
        xz = y = projectileSpawnOffset;
        y.x = y.z = 0.0f;
        xz.y = 0.0f;
        return transform.position +
            transform.up*y.magnitude +
            givenAim.normalized*xz.magnitude;
    }

    /* Sets aim (not currentAim) to the direction necessary
     * to hit the current target, accounting for displacement,
     * launch speed, and gravity, and disallowing directions
     * which are outside the launcher's angular constraints.
     * Since trajectory is a parabola, there are two possible results.
     * One (the minus result) will hit the target
     * by aiming a little bit above it
     * to account for gravity and hit it square on,
     * and the other (the plus result) will hit the target
     * by aiming up into the sky
     * and relying on the projectile falling onto the target from above.
     * Since the latter will obviously take longer,
     * we prefer the prior if it's in-range,
     * but will otherwise resort to the latter. */
    private void Recalibrate()
    {
        Vector3 displacement = target - transform.position;
        if (
            !Misc.IsNaNVec(target) &&
            displacement.magnitude >= aimDistanceThreshold
        ) {
            Vector3 estimatedSpawnPoint =
                CalculateProjectileSpawnPoint(displacement);
            Vector3 plusResult, minusResult;
            if (Misc.CalculateLaunchAngle(
                target - estimatedSpawnPoint,
                launchSpeed,
                out plusResult,
                out minusResult
            )) {
                if (WithinConstraints(minusResult)) aim = minusResult;
                else if (WithinConstraints(plusResult)) aim = plusResult;
                else aim = Misc.NaNVec;
            }
            else aim = Misc.NaNVec;
        }
        else aim = Misc.NaNVec;
    }

    /* Calculates the yaw (turning-around angle)
     * implied by aiming in the given direction
     * in the context of the current local space. */
    private float CalculateYaw(Vector3 unitVec)
    {
        return Vector3.SignedAngle(
            transform.forward,
            Vector3.ProjectOnPlane(unitVec, transform.up),
            transform.up
        );
    }

    /* Calculates the pitch (up-down angle)
     * implied by aiming in the given direction
     * in the context of the current local space. */
    private float CalculatePitch(Vector3 unitVec)
    {
        var forward = Vector3.ProjectOnPlane(unitVec, transform.up);
        var plane = Vector3.Cross(transform.up, forward).normalized;
        return Vector3.SignedAngle(
            forward,
            Vector3.ProjectOnPlane(unitVec, plane),
            plane
        );
    }

    /* Whether the launcher is allowed to aim in the given direction
     * considering its angular constraints. */
    private bool WithinConstraints(Vector3 unitVec)
    {
        float yaw = CalculateYaw(unitVec);
        float pitch = CalculatePitch(unitVec);
        return yaw >= minYaw && yaw <= maxYaw &&
            pitch >= minPitch && pitch <= maxPitch;
    }

    /* Rotates currentAim toward aim in accordance with aimSpeed,
     * or instantaneously if useAimSpeed is false. */
    private void TurnToAim()
    {
        if (!Misc.IsNaNVec(aim))
        {
            if (useAimSpeed)
            {
                currentAim = Vector3.RotateTowards(
                    currentAim, aim,
                    aimSpeed*Time.deltaTime*Mathf.PI/180.0f,
                    1.0f
                ).normalized;
            }
            else
            {
                currentAim = aim;
            }
        }
    }

    // Whether cooldown from the last launch has expired.
    public bool Ready()
    {
        return cooldownTimer <= 0.0f;
    }

    /* Launches the launcher's projectile.
     * If a Rigidbody is given, it is instantiated
     * instead of this launcher's standard projectile.
     * If, in addition, useExisting is set to true,
     * then the rigidbody argument is directly manipulated,
     * not instantiated. The cooldown is advisory;
     * the launcher will launch even if the cooldown is in effect. */
    public void Launch(
        Rigidbody projectile = null,
        bool useExisting = false
    ) {
        if (projectile == null) projectile = this.projectile;
        cooldownTimer = cooldown;
        Vector3 spawnPosition = projectileSpawnPoint;
        Quaternion spawnRotation = Quaternion.LookRotation(aim, transform.up);
        Rigidbody rbody;
        if (useExisting)
        {
            rbody = projectile;
            rbody.position = spawnPosition;
            rbody.rotation = spawnRotation;
        }
        else
        {
            rbody = Instantiate(
                projectile.gameObject,
                spawnPosition, spawnRotation
            ).GetComponent<Rigidbody>();
        }
        rbody.linearVelocity = Vector3.zero;
        rbody.angularVelocity = Vector3.zero;
        rbody.AddForce(launchSpeed*aim*rbody.mass, ForceMode.Impulse);
        var asProjectile = rbody.GetComponent<Projectile>();
        if (asProjectile != null) asProjectile.source = this.gameObject;
        if (effectOnLaunch != null)
        {
            Instantiate(effectOnLaunch, spawnPosition, spawnRotation);
        }
        launch?.Invoke(rbody);
    }

    /* Launches if and only if cooldown has expired.
     * Returns true if and only if the launch occurred. */
    public bool LaunchIfReady(
        Rigidbody projectile = null,
        bool useExisting = false
    ) {
        if (Ready())
        {
            Launch(projectile, useExisting);
            return true;
        }
        else
        {
            return false;
        }
    }

    /* Until Aim or Disarm is next called,
     * launcher will aim at the given world space point. */
    public void Aim(Vector3 where)
    {
        targetObject = null;
        targetPoint = where;
        Recalibrate();
        if (!useAimSpeed) TurnToAim();
    }

    /* Until Aim or Disarm is next called,
     * launcher will aim at the given object. */
    public void Aim(GameObject what)
    {
        targetObject = what.transform;
        targetPoint = Misc.NaNVec;
        Recalibrate();
        if (!useAimSpeed) TurnToAim();
    }

    /* Until Aim or Disarm is next called,
     * launcher will aim at the game object
     * to which the given component is attached. */
    public void Aim<T>(T what) where T : Component
    {
        Aim(what.gameObject);
    }

    // Until Aim is next called, launcher will not aim.
    public void Disarm()
    {
        targetObject = null;
        targetPoint = Misc.NaNVec;
    }
}
