using UnityEngine;

public class EnemyShipController : MonoBehaviour
{
    [SerializeField] private Rigidbody rbody;
    [SerializeField] private float linearAcceleration = 2.0f;
    [SerializeField] private float angularAcceleration = 12.0f;
    [SerializeField] private float brake = 0.01f;
    [SerializeField] private float tiltStrength = 2.0f;
    [SerializeField] private float stabilizeStrength = 1.0f;
    [SerializeField] private float traction = 0.01f;

    private Vector3 _impetus = Vector3.zero;

    public new Transform transform { get => rbody.transform; }

    [HideInInspector] public Vector3 impetus
    {
        get => _impetus;
        set { _impetus = value.normalized; }
    }

    void Update()
    {
        /* I realize this duplicates code from Flying Vehicle,
         * but I don't want to risk damaging Flying Vehicle
         * trying to refactor it to extract a common component.
         * We can worry about that after graduation. */
        // apply forces
        rbody.AddTorque(
            transform.up *
            impetus.x *
            rbody.mass *
            angularAcceleration *
            Time.deltaTime,
            ForceMode.Impulse
        );
        rbody.AddForce(
            transform.forward *
            impetus.z *
            rbody.mass *
            linearAcceleration *
            Time.deltaTime,
            ForceMode.Impulse
        );
        // steer
        rbody.linearVelocity = Vector3.Lerp(
            rbody.linearVelocity,
            Vector3.ProjectOnPlane(rbody.linearVelocity, transform.right),
            1.0f - Mathf.Pow(1.0f - traction, Time.deltaTime*60.0f)
        );
        // brake
        if (Vector3.Dot(transform.forward * impetus.z, rbody.linearVelocity) < 0.0f)
        {
            rbody.linearVelocity = Vector3.Lerp(
                rbody.linearVelocity,
                -rbody.linearVelocity.normalized,
                1.0f - Mathf.Pow(1.0f - brake, Time.deltaTime*60.0f)
            );
        }
        // tilt
        if (impetus.z > 0.0f)
        {
            rbody.AddTorque(
                -transform.right *
                impetus.y *
                rbody.mass *
                tiltStrength *
                angularAcceleration *
                Vector3.Dot(transform.up, Vector3.up) *
                Time.deltaTime /
                4.0f,
                ForceMode.Impulse
            );
        }
        // stabilize
        rbody.AddTorque(
            Vector3.Cross(transform.up, Vector3.up) *
            rbody.mass *
            stabilizeStrength *
            angularAcceleration *
            Time.deltaTime,
            ForceMode.Impulse
        );
    }
}
