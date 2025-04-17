using UnityEngine;
using System.Collections.Generic;

public class DestructibleMesh : MonoBehaviour
{
    [SerializeField] public Material commonMaterialOverride = null;
    [SerializeField] public Material repairSiteMaterial = null;
    /* The radius of the spherecast used to determine which parts are affected
     * by a projectile collision equals this value times the impulse.
     * (The value's units are therefore seconds per kilogram.) */
    [SerializeField] public float blastRadiusMultiplier = 1.0f;
    /* If a Rigidbody is associated by SyncTransform to this DestructibleMesh
     * but is intangible to Projectiles (so that they can pass through it
     * to get to the DestructibleMesh's pieces), specify that Rigidbody here.
     * TakeHit will then propagate impulses it receives to the Rigidbody. */
    [SerializeField] public Rigidbody syncedRigidbody = null;

    public void TakeHit(Vector3 source, Vector3 impulse)
    {
        if (impulse != Vector3.zero)
        {
            float radius = blastRadiusMultiplier*impulse.magnitude;
            foreach (var collider in Physics.OverlapSphere(source, radius)) {
                var piece = collider.GetComponent<DestructibleMeshPiece>();
                if (piece != null && piece.ownerDestructibleMesh == this)
                {
                    var disp = piece.transform.position - source;
                    if (disp == Vector3.zero)
                    {
                        piece.TakeDamage(impulse);
                    }
                    else
                    {
                        piece.TakeDamage(Vector3.Slerp(
                            impulse.normalized,
                            disp.normalized,
                            disp.magnitude/radius
                        ).normalized*impulse.magnitude);
                    }
                }
            }
            /*if (syncedRigidbody != null)
            {
                syncedRigidbody.AddForceAtPosition(
                    -impulse, source, ForceMode.Impulse
                );
            }*/
        }
    }
}
