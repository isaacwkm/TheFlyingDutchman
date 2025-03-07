using UnityEngine;

public class AnchorControl : MonoBehaviour
{
    [SerializeField] private Interactable interactTarget;
    [SerializeField] private SpringJoint anchorSpringJoint;
    [SerializeField] private float anchorMaxDropDistance = 30.0f;
    [SerializeField] private HingeJoint controlWheelHingeJoint;
    [SerializeField] private float controlWheelSpinFactor = 10.0f;
    [SerializeField] private Rigidbody shipPhysicsController;
    [SerializeField] private float shipDecelerationWhenAnchored = 0.05f;
    [SerializeField] private float shipStabilizationWhenAnchored = 120.0f;
    private bool extended = false;
    private Rigidbody anchorRbody;

    private void OnEnable()
    {
        interactTarget.OnInteract += HandleInteract;
    }

    private void OnDisable()
    {
        interactTarget.OnInteract -= HandleInteract;
    }

    private void HandleInteract(GameObject whom)
    {
        if (extended)
        {
            anchorSpringJoint.maxDistance = 0.0f;
            extended = false;
        }
        else
        {
            anchorSpringJoint.maxDistance = anchorMaxDropDistance;
            extended = true;
        }
    }

    private void Start()
    {
        anchorRbody = anchorSpringJoint.GetComponent<Rigidbody>();
    }

    private void Update()
    {
        // spin control wheel according to vertical motion of anchor relative to ship
        var motor = controlWheelHingeJoint.motor;
        var velDiff = anchorRbody.linearVelocity - shipPhysicsController.linearVelocity;
        motor.targetVelocity = controlWheelSpinFactor *
            Vector3.Project(velDiff, transform.up).magnitude *
            Mathf.Sign(Vector3.Dot(velDiff, transform.up));
        controlWheelHingeJoint.motor = motor;
        // if the anchor is out, make the ship behave itself
        if (extended)
        {
            shipPhysicsController.linearVelocity =
                Vector3.Lerp(
                    shipPhysicsController.linearVelocity,
                    Vector3.zero,
                    1.0f - Mathf.Pow(
                        1.0f - shipDecelerationWhenAnchored,
                        Time.deltaTime * 60.0f
                    )
                );
            shipPhysicsController.AddTorque(
                shipPhysicsController.mass*shipStabilizationWhenAnchored*Time.deltaTime *
                    Vector3.Cross(shipPhysicsController.transform.up, Vector3.up),
                ForceMode.Impulse
            );
        }
    }
}
