using UnityEngine;

[RequireComponent(typeof(EnemyShipController))]
public class EnemyShipAI : MonoBehaviour
{
    [SerializeField] private ProjectileLauncher leftCannon;
    [SerializeField] private ProjectileLauncher rightCannon;
    [SerializeField] private float pursuitRange = 1000.0f;
    [SerializeField] private float attackRange = 200.0f;
    [SerializeField] private DestructibleMesh dmesh;
    [SerializeField] private float minHealthFactorBeforeExplode = 0.125f;
    [SerializeField] private float explosionImpulsePerUnitDistance = 5.0f;

    private EnemyShipController controller;

    public new Transform transform { get => controller.transform; }

    void Start()
    {
        controller = GetComponent<EnemyShipController>();
    }

    private bool CannonLive(ProjectileLauncher cannon)
    {
        var dmp = cannon.GetComponent<DestructibleMeshPiece>();
        return dmp == null || dmp.attached;
    }

    void Update()
    {
        float health = dmesh.GetHealth();
        if (health <= 0.0f || health/dmesh.GetMaxHealth() <= minHealthFactorBeforeExplode)
        {
            // if defeated, explode
            dmesh.Explode(explosionImpulsePerUnitDistance);
        }
        else if (!SceneCore.ship.physicsObject.Operating())
        {
            // if player not sailing, don't engage
            controller.impetus = Vector3.zero;
        }
        else
        {
            Vector3 displacement = SceneCore.ship.transform.position - transform.position;
            float distance = displacement.magnitude;
            if (distance > pursuitRange)
            {
                // if out of pursuit range, don't engage
                controller.impetus = Vector3.zero;
            }
            else
            {
                bool leftCannonLive = CannonLive(leftCannon);
                bool rightCannonLive = CannonLive(rightCannon);
                float displacementRightness = Vector3.Dot(displacement, transform.right);
                float displacementUpness = Vector3.Dot(displacement, Vector3.up);
                float displacementForwardness = Vector3.Dot(displacement, transform.forward);
                Vector3 xToward, yToward, zToward, turnForRightCannon;
                if (displacementRightness > 0.0f) xToward = Vector3.right;
                else xToward = -Vector3.right;
                if (displacementUpness > 0.0f) yToward = Vector3.up;
                else yToward = -Vector3.up;
                if (displacementForwardness > 0.0f)
                {
                    zToward = Vector3.forward;
                    turnForRightCannon = -Vector3.right;
                }
                else
                {
                    zToward = -Vector3.forward;
                    turnForRightCannon = Vector3.right;
                }
                if (!leftCannonLive && !rightCannonLive)
                {
                    // if both cannons broken, flee
                    controller.impetus = -xToward - yToward - zToward;
                }
                else if (distance > attackRange)
                {
                    // if either cannon operational but out of attack range, pursue
                    controller.impetus = xToward + yToward + zToward;
                }
                else if (leftCannonLive && !rightCannonLive)
                {
                    // if only left cannon works, face ship with own left side
                    controller.impetus = -turnForRightCannon;
                }
                else if (!leftCannonLive && rightCannonLive)
                {
                    // if only right cannon works, face ship with own right side
                    controller.impetus = turnForRightCannon;
                }
                // if both cannons work, turn to face ship in shorter direction
                else if (displacementRightness > 0.0f)
                {
                    controller.impetus = turnForRightCannon;
                }
                else
                {
                    controller.impetus = -turnForRightCannon;
                }
            }
        }
    }
}
