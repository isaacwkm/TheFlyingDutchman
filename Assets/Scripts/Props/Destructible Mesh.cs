using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

public class DestructibleMesh : MonoBehaviour
{
    public event Action<DestructibleMeshPiece> hit;
    public event Action<DestructibleMeshPiece> broken;
    public event Action<DestructibleMeshPiece> repaired;

    private class PieceActions
    {
        public Action hit;
        public Action broken;
        public Action repaired;
        public PieceActions(
            Action hit,
            Action broken,
            Action repaired
        ) {
            this.hit = hit;
            this.broken = broken;
            this.repaired = repaired;
        }
    }

    private struct Impact
    {
        public DestructibleMeshPiece piece;
        public Vector3 impulse;
        public Impact(
            DestructibleMeshPiece piece,
            Vector3 impulse
        ) {
            this.piece = piece;
            this.impulse = impulse;
        }
        public void Apply()
        {
            piece.TakeDamage(impulse);
        }
    }

    [SerializeField] public Material commonMaterialOverride = null;
    [SerializeField] public Material repairSiteMaterial = null;
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

    private Dictionary<DestructibleMeshPiece, PieceActions> actionsTable = new();
    private Queue<Impact> queuedImpacts = new();

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
            var row = new PieceActions(
                () => hit?.Invoke(piece),
                () => broken?.Invoke(piece),
                () => repaired?.Invoke(piece)
            );
            actionsTable.Add(piece, row);
            if (enabled)
            {
                piece.hit += row.hit;
                piece.broken += row.broken;
                piece.repaired += row.repaired;
            }
        }
    }

    void OnEnable()
    {
        foreach (var piece in _pieces)
        {
            var row = actionsTable[piece];
            piece.hit += row.hit;
            piece.broken += row.broken;
            piece.repaired += row.repaired;
        }
    }

    void OnDisable()
    {
        foreach (var piece in _pieces)
        {
            var row = actionsTable[piece];
            piece.hit -= row.hit;
            piece.broken -= row.broken;
            piece.repaired -= row.repaired;
        }
    }

    public void TakeHit(Vector3 source, Vector3 impulse, float radius)
    {
        if (impulse != Vector3.zero)
        {
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
                        queuedImpacts.Enqueue(new Impact(piece, Vector3.Slerp(
                            impulse.normalized,
                            disp.normalized,
                            lerpWeight
                        ).normalized*Mathf.Lerp(
                            impulse.magnitude, 0.0f,
                            lerpWeight
                        )));
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

    void Update()
    {
        if (queuedImpacts.Count > 0)
        {
            queuedImpacts.Dequeue().Apply();
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

    public DestructibleMeshPiece GetHighestPriorityTarget()
    {
        DestructibleMeshPiece result = null;
        int priority = 0;
        foreach (var piece in _pieces)
        {
            if (
                piece.attached &&
                piece.aiTargetPriority > priority
            ) {
                result = piece;
                priority = piece.aiTargetPriority;
            }
        }
        return result;
    }
}
