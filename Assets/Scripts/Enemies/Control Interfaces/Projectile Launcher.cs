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

    // Visual effects object to spawn alongside projectile. May be null.
    [SerializeField] private GameObject vfx = null;
    // Audio source that should play a sound effect on launch. May be null.
    [SerializeField] private AudioSource sfx = null;
    // Point where the projectile spawns from, relative to the launcher.
    [SerializeField] private Vector3 projectileSpawnOffset = Vector3.zero;
    // Initial speed of projectile when launched.
    [SerializeField] private float launchSpeed = 1.0f;
    // Cooldown between consecutive launches.
    [SerializeField] private float cooldown = 0.0f;
    // Whether to limit aim speed. If not, always aim directly at target.
    [SerializeField] private bool useAimSpeed = false;
    // Aim speed in degrees per second.
    [SerializeField] private float aimSpeed = 15.0f;
    // Least (upwardmost) allowed pitch.
    [SerializeField] private float minPitch = -90.0f;
    // Greatest (downwardmost) allowed pitch.
    [SerializeField] private float maxPitch = 90.0f;
    // Least allowed yaw.
    [SerializeField] private float minYaw = -180.0f;
    // Greatest allowed yaw.
    [SerializeField] private float maxYaw = 180.0f;
    // If target is this close or closer, do not track it.
    [SerializeField] private float aimDistanceThreshold = 0.01f;

    private Vector3? _aimTarget = null;
    private float desiredPitch = float.NaN;
    private float desiredYaw = float.NaN;
    private float cooldownTimer = 0.0f;

    /* Point to try to hit.
     * Note that since this is a nullable object,
     * there are two caveats in its use:
     * -    you can assign null to it even though it's a Vector3;
     * -    when you read it back, you have to check for null,
     *      and/or cast to a pure Vector3 if you're sure. */
    public Vector3? aimTarget
    {
        get => _aimTarget;
        /* Setting this property immediately recalibrates
         * public aim properties.
         * Additionally, if we don't use aim speed,
         * then our actual aim angles are immediately updated as well. */
        set
        {
            _aimTarget = value;
            Recalibrate();
            if (!useAimSpeed) TurnToAim();
        }
    }

    /* Whether the point we want to hit is in-range.
     * This does not necessarily mean we are aiming at it yet;
     * for that, see aimTargetInLineOfFire.
     * *This* property merely indicates whether we are oriented in such a way
     * that it is *possible* to aim at the point we want to hit. */
    public bool aimTargetInRange { get; private set; }
    /* Whether we will hit the point we want to hit if we launch right now
     * (not accounting for obstructions).
     * In practice, this is true iff two other things are both true:
     * -    the target point is in range;
     * -    we have finished aiming at it
     *      (or we don't use an aim speed limit).
     * Note that even if this property is true,
     * it does not guarantee whatever *object* we were aiming for
     * will *still be at* that point. */
    public bool aimTargetInLineOfFire { get; private set; }
    /* Current pitch (up/down aim).
     * Animator component should use this e.g.
     * to update the child objects' orientations. */
    public float aimPitch { get; private set; }
    /* Current yaw (turning-around aim).
     * Animator component should use this e.g.
     * to update the child objects' orientations. */
    public float aimYaw { get; private set; }

    void Start()
    {
        aimPitch = 0.0f;
        aimYaw = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        TurnToAim(Time.deltaTime);
        if (cooldownTimer > 0.0f) cooldownTimer -= Time.deltaTime;
    }

    /* Possibly moves a single angle component of our aim
     * toward where it's supposed to be going.
     * A deltaTime of NaN means immediately snap to desired.
     * A desired of NaN means don't update at all. */
    private float PartialAim
    (
        float current,
        float desired,
        float deltaTime = float.NaN
    )
    {
        if (float.IsNaN(desired))
        {
            aimTargetInLineOfFire = false;
        }
        else
        {
            float diff = desired - current;
            float diffAbs = Mathf.Abs(diff);
            float diffSign = Mathf.Sign(diff);
            float delta = deltaTime*aimSpeed;
            if (
                // snap if told to snap
                float.IsNaN(deltaTime) ||
                // snap if we always snap
                !useAimSpeed ||
                /* snap if we're supposed to get there this frame
                 * (to avoid overrotating) */
                diffAbs <= delta
            )
            {
                current = desired;
            }
            else
            {
                /* If we didn't snap,
                 * then that means we aren't there yet,
                 * which means the target is not in line of fire.
                 * Also, keep going there. */
                aimTargetInLineOfFire = false;
                current += diffSign*delta;
            }
        }
        return current;
    }

    /* Turns our aim toward where it's supposed to be pointing.
     * If deltaTime is given and we are using aim speed,
     * we turn gradually. Otherwise, we snap to the target. */
    public void TurnToAim(float deltaTime = float.NaN)
    {
        // assume true so we can &&=
        aimTargetInLineOfFire = true;
        aimPitch = PartialAim(aimPitch, desiredPitch, deltaTime);
        aimYaw = PartialAim(aimYaw, desiredYaw, deltaTime);
    }

    /* Constrains theta in three ways:
     * -    first, ensures it's normalized to the 360-degree range;
     * -    then, makes it a signed angle (i.e. subtracts 360 if 180+);
     * -    then, clamps it within min and max.
     * If the final step, the clamping, changes the value,
     * then wasWithinMinMax is set to true.
     * Otherwise, it's set to false,
     * even if the prior steps changed theta. */
    private float ConstrainAimAngle(
        float theta,
        float min,
        float max,
        out bool wasWithinMinMax
    )
    {
        // constrain to within one full turn
        theta = Mathf.Repeat(theta, 360.0f);
        // make signed
        if (theta >= 180.0f) theta -= 360.0f;
        // constrain within min/max
        if (theta < min)
        {
            theta = min;
            wasWithinMinMax = false;
        }
        else if (theta > max)
        {
            theta = max;
            wasWithinMinMax = false;
        }
        else
        {
            wasWithinMinMax = true;
        }
        return theta;
    }

    private Vector3 GetLaunchSpawnPosition(float yaw)
    {
        return Quaternion.Euler(0.0f, yaw, 0.0f) *
            projectileSpawnOffset;
    }

    /* Re-checks where we should be pointing based on where we are
     * relative to the point we want to hit.
     * Uses trajectory calculations. */
    private void Recalibrate()
    {
        desiredPitch = float.NaN;
        desiredYaw = float.NaN;
        if (aimTarget == null)
        {
            /* trivial case: no target;
             * in this case, by definition it cannot be in range,
             * nor should the aiming direction change
             * from whatever it already was */
            aimTargetInRange = false;
            aimTargetInLineOfFire = false;
        }
        else
        {
            // assume in range at first so we can use &&=
            aimTargetInRange = true;
            /* calculate naive displacement
             * assuming projectile fires from the center of the launcher;
             * this is strictly for finding yaw */
            var displacement = aimTarget.Value - transform.position;
            if (displacement.magnitude <= aimDistanceThreshold)
            {
                /* trivial case: target is too close;
                 * this is similar to the case of there being no target,
                 * except instead the target is by definition
                 * both in range and in line of fire */
                aimTargetInLineOfFire = true;
            }
            else
            {
                // calculate yaw
                desiredYaw = ConstrainAimAngle(
                    Quaternion.LookRotation(
                        displacement.normalized,
                        Vector3.up
                    ).eulerAngles.y,
                    minYaw, maxYaw,
                    out var yawInRange
                );
                aimTargetInRange = aimTargetInRange && yawInRange;
                /* update displacement to no longer naively assume
                 * we are firing from the center of the launcher,
                 * using calculated yaw to determine the offset */
                displacement -= GetLaunchSpawnPosition(desiredYaw);
                /* https://en.wikipedia.org/wiki/Projectile_motion
                 * "To hit a target at range x and altitude y
                 * when fired from (0,0) and with initial speed v,
                 * the required angle(s) of launch theta are:
                 * theta = atan(
                 *      (v^2 +/- sqrt(v^4 - g^2x^2 - 2gyv^2)) /
                 *      gx
                 * )" */
                // calculate the plus and minus terms under the radical
                float v = launchSpeed;
                float g = Physics.gravity.magnitude;
                Vector3 xvec = displacement;
                xvec.y = 0.0f;
                float y = displacement.y;
                float x = xvec.magnitude;
                float posradterm = v*v*v*v;
                float negradterm = g*g*x*x + 2.0f*g*y*v*v;
                // check whether result would be a real number
                if (posradterm >= negradterm)
                {
                    // calc radical and try the plus of the plus-or-minus first
                    float rad = Mathf.Sqrt(posradterm - negradterm);
                    desiredPitch = ConstrainAimAngle(
                        -Mathf.Atan((v*v + rad)/(g*x))*180.0f/Mathf.PI,
                        minPitch, maxPitch,
                        out var pitchInRange
                    );
                    // if the plus doesn't work, try the minus
                    if (!pitchInRange)
                    {
                        desiredPitch = ConstrainAimAngle(
                            -Mathf.Atan((v*v - rad)/(g*x))*180.0f/Mathf.PI,
                            minPitch, maxPitch,
                            out pitchInRange
                        );
                        // if the minus doesn't work, give up
                        if (!pitchInRange)
                        {
                            aimTargetInRange = false;
                            desiredPitch = float.NaN;
                        }
                    }
                }
                else
                {
                    // if nonreal result, give up
                    aimTargetInRange = false;
                    desiredPitch = float.NaN;
                }
                /* finally, if we couldn't determine an appropriate pitch,
                 * use whichever of the two limits aims closest
                 * based on how high the target is */
                if (float.IsNaN(desiredPitch))
                {
                    if (displacement.y > 0)
                    {
                        desiredPitch = minPitch;
                    }
                    else
                    {
                        desiredPitch = maxPitch;
                    }
                }
            }
        }
    }

    /* If cooldown allows, launches and returns true.
     * Otherwise, returns false.
     * Scripts such as enemy AI should probably check
     * the property aimTargetInLineOfFire before calling this.
     * If a Rigidbody is given, it is instantiated
     * instead of this launcher's standard projectile.
     * If, in addition, useExisting is set to true,
     * then the rigidbody argument is directly manipulated,
     * not instantiated. */
    public bool Launch(Rigidbody projectile = null, bool useExisting = false)
    {
        if (projectile == null) projectile = this.projectile;
        if (cooldownTimer > 0.0f)
        {
            return false;
        }
        else
        {
            cooldownTimer = cooldown;
            Vector3 spawnPosition =
                transform.position +
                GetLaunchSpawnPosition(aimYaw);
            Quaternion spawnRotation =
                Quaternion.Euler(aimPitch, aimYaw, 0.0f);
            // instantiate projectile
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
                    projectile.gameObject, spawnPosition, spawnRotation
                ).GetComponent<Rigidbody>();
            }
            rbody.linearVelocity = Vector3.zero;
            rbody.angularVelocity = Vector3.zero;
            rbody.AddForce(
                launchSpeed*rbody.transform.forward*rbody.mass,
                ForceMode.Impulse
            );
            // instantiate vfx
            if (vfx != null)
            {
                Instantiate(vfx, spawnPosition, spawnRotation);
            }
            // play launch sound
            if (sfx != null && sfx.clip != null)
            {
                sfx.Play();
            }
            // raise event
            launch?.Invoke(rbody);
            return true;
        }
    }

    // Various clarity wrappers for setting aimTarget.
    public void Aim(Vector3 target, bool snap = false)
    {
        aimTarget = target;
        if (snap) TurnToAim();
    }
    public void Aim(GameObject target, bool snap = false)
    {
        Aim(target.transform.position, snap);
    }
    public void Aim<T>(T target, bool snap = false) where T : Component
    {
        Aim(target.gameObject, snap);
    }
    public void Disarm()
    {
        aimTarget = null;
    }
}
