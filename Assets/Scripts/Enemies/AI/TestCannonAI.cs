using UnityEngine;

public class TestCannonAI : MonoBehaviour
{
    [SerializeField] ProjectileLauncher launcher;
    [SerializeField] float targetMaxDistance = 100.0f;

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
        }
        else
        {
            target = SceneCore.playerCharacter.transform;
        }
        var displacement = target.position - transform.position;
        if (displacement.magnitude <= targetMaxDistance)
        {
            launcher.Aim(target);
            if (launcher.aimTargetInLineOfFire) launcher.Launch();
        }
        else
        {
            launcher.Disarm();
        }
    }
}
