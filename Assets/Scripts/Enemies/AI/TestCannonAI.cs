using UnityEngine;

public class TestCannonAI : MonoBehaviour
{
    [SerializeField] ProjectileLauncher launcher;
    [SerializeField] float targetMaxDistance = 100.0f;
    [SerializeField] float velocityTrackingCapability = 0.25f;

    // Update is called once per frame
    void Update()
    {
        Transform target;
        if (
            SceneCore.ship != null &&
            SceneCore.ship.physicsObject.Operating()
        ) {
            target = SceneCore.ship.geometryObject
                .GetHighestPriorityTarget()?.transform;
            if (target == null)
            {
                target = SceneCore.ship.transform;
            }
            var displacement = target.position - transform.position;
            float distance = displacement.magnitude;
            float estimatedTimeToImpact = distance/launcher.launchSpeed;
            Vector3 futurePosition =
                target.position +
                SceneCore.ship.physicsObject.GetComponent<Rigidbody>().linearVelocity *
                estimatedTimeToImpact;
            Vector3 realTarget = Vector3.Lerp(
                target.position,
                futurePosition,
                velocityTrackingCapability
            );
            if ((realTarget - transform.position).magnitude <= targetMaxDistance)
            {
                launcher.Aim(realTarget);
                if (launcher.targetInLineOfFire) launcher.LaunchIfReady();
            }
            else
            {
                launcher.Disarm();
            }
        }
        else
        {
            launcher.Disarm();
        }
    }
}
