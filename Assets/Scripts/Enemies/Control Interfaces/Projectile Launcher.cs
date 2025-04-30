using System;
using UnityEngine;

public class ProjectileLauncher : MonoBehaviour
{
    public event Action<Rigidbody> launch;

    [SerializeField] public Rigidbody projectile = null;
    [SerializeField] private Vector3 projectileSpawnOffset = Vector3.zero;
    [SerializeField] private TemporaryGameObject effectOnLaunch = null;
    [SerializeField] public Transform targetObject = null;
    [SerializeField] private float launchSpeed = 1.0f;
    [SerializeField] private float cooldown = 0.0f;
    [SerializeField] private bool useAimSpeed = false;
    [SerializeField] private float aimSpeed = 15.0f;
    [SerializeField] private float minPitch = -90.0f;
    [SerializeField] private float maxPitch = 90.0f;
    [SerializeField] private float minYaw = -180.0f;
    [SerializeField] private float maxYaw = 180.0f;
    [SerializeField] private float aimDistanceThreshold = 0.01f;

    private Vector3 targetPoint = Misc.NaNVec;
    private float cooldownTimer = 0.0f;

    public Vector3 target
    {
        get
        {
            if (targetObject == null)
            {
                return targetPoint;
            }
            else
            {
                return targetObject.position;
            }
        }
    }

    public Vector3 aim { get; private set; }
    public Vector3 currentAim { get; private set; }

    public Vector3 projectileSpawnPoint
    {
        get => CalculateProjectileSpawnPoint(currentAim);
    }

    public bool targetInRange
    {
        get => !Misc.IsNaNVec(aim);
    }

    public bool targetInLineOfFire
    {
        get => Mathf.Approximately(0.0f, Vector3.Angle(currentAim, aim));
    }

    public float yaw { get => CalculateYaw(currentAim); }
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

    private float CalculateYaw(Vector3 unitVec)
    {
        return Vector3.SignedAngle(
            transform.forward,
            Vector3.ProjectOnPlane(unitVec, transform.up),
            transform.up
        );
    }

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

    private bool WithinConstraints(Vector3 unitVec)
    {
        float yaw = CalculateYaw(unitVec);
        float pitch = CalculatePitch(unitVec);
        return yaw >= minYaw && yaw <= maxYaw &&
            pitch >= minPitch && pitch <= maxPitch;
    }

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

    public bool Ready()
    {
        return cooldownTimer <= 0.0f;
    }

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

    public void Aim(Vector3 where)
    {
        targetObject = null;
        targetPoint = where;
    }

    public void Aim(GameObject what)
    {
        targetObject = what.transform;
        targetPoint = Misc.NaNVec;
    }

    public void Aim<T>(T what) where T : Component
    {
        Aim(what.gameObject);
    }

    public void Disarm()
    {
        targetObject = null;
        targetPoint = Misc.NaNVec;
    }
}
