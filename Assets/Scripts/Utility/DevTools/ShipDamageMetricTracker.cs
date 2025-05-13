using UnityEngine;

public class ShipDamageMetricTracker : MonoBehaviour
{
    private float lastKnownShipHealth;
    private DestructibleMesh shipDM;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        shipDM = SceneCore.ship.geometryObject.GetComponent<DestructibleMesh>();
        lastKnownShipHealth = shipDM.GetHealth();
    }

    // Update is called once per frame
    void Update()
    {
        float prev = lastKnownShipHealth;
        float curr = lastKnownShipHealth = shipDM.GetHealth();
        if (curr < prev)
        {
            Metrics.Set("ship damage", Metrics.Get("ship damage") + prev - curr);
        }
    }
}
