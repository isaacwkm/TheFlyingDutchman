using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    [SerializeField] public bool destroyOnContact = false;
    [SerializeField] public TemporaryGameObject effectOnDestroy = null;
    [SerializeField] public float blastRadius = 1.0f;
    [SerializeField] public bool blastRadiusScalesWithDamage = false;
    [SerializeField] public bool useDamageOverride = false;
    [SerializeField] public float damageOverride = 1.0f;
    [HideInInspector] public GameObject source = null;

    void OnCollisionEnter(Collision collision)
    {
        if (!collision.transform.IsChildOf(source.transform))
        {
            var dmp = collision.collider.GetComponent<DestructibleMeshPiece>();
            if (dmp != null)
            {
                var contact = Vector3.zero;
                for (int i = 0; i < collision.contactCount; i++)
                {
                    contact += collision.GetContact(i).point;
                }
                contact /= collision.contactCount;
                Vector3 damage = collision.impulse;
                if (useDamageOverride)
                {
                    damage = damage.normalized*damageOverride;
                }
                dmp.ReportHit(
                    contact, damage,
                    blastRadiusScalesWithDamage ?
                        blastRadius*damage.magnitude :
                        blastRadius
                );
            }
            if (destroyOnContact)
            {
                if (effectOnDestroy != null)
                {
                    Instantiate(
                        effectOnDestroy.gameObject,
                        transform.position,
                        transform.rotation
                    );
                }
                Destroy(gameObject);
            }
        }
    }
}
