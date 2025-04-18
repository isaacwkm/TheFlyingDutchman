using UnityEngine;
using System.Collections.Generic;
using System.Collections.ObjectModel;

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

    private List<DestructibleMeshPiece> _pieces = new();
    public ReadOnlyCollection<DestructibleMeshPiece> pieces
    {
        get => _pieces.AsReadOnly();
    }

    public void RegisterPiece(DestructibleMeshPiece piece)
    {
        if (piece.ownerDestructibleMesh != this)
        {
            throw new ExceptionAbout<DestructibleMesh>(
                $"Tried to register destructible mesh piece {piece} " +
                $"with non-owning destructible mesh {this}"
            );
        }
        else if (!_pieces.Contains(piece))
        {
            _pieces.Add(piece);
        }
    }

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
                        float lerpWeight = disp.magnitude/radius;
                        piece.TakeDamage(Vector3.Slerp(
                            impulse.normalized,
                            disp.normalized,
                            lerpWeight
                        ).normalized*Mathf.Lerp(
                            impulse.magnitude, 0.0f,
                            lerpWeight
                        ));
                    }
                }
            }
            if (syncedRigidbody != null)
            {
                syncedRigidbody.AddForceAtPosition(
                    -impulse, source, ForceMode.Impulse
                );
            }
        }
    }

    public float GetHealth()
    {
        float health = 0.0f;
        foreach (var piece in _pieces)
        {
            health += piece.health;
        }
        return health;
    }

    public float GetMaxHealth()
    {
        float maxHealth = 0.0f;
        foreach (var piece in _pieces)
        {
            maxHealth += piece.maxHealth;
        }
        return maxHealth;
    }

    public void FullRepair()
    {
        foreach (var piece in _pieces)
        {
            piece.Repair();
        }
    }

    public void Explode(float impulsePerUnitDistance = 1.0f)
    {
        foreach (var piece in _pieces)
        {
            piece.BreakOff(impulsePerUnitDistance*(
                piece.transform.position - transform.position
            ));
        }
    }
}
