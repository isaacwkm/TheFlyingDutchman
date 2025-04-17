using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    [SerializeField] public bool destroyOnContact = false;
    [SerializeField] public TemporaryGameObject effectOnDestroy = null;
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
                dmp.ReportHit(contact, collision.impulse);
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
