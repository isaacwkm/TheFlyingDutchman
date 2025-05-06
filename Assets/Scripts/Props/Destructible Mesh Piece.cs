using UnityEngine;
using System;

public class DestructibleMeshPiece : MonoBehaviour
{
    public event Action hit;
    public event Action broken;
    public event Action repaired;

    [SerializeField] private DestructibleMesh _ownerDestructibleMesh = null;
    [SerializeField] public TemporaryGameObject effectOnHit = null;
    [SerializeField] public TemporaryGameObject effectOnBreak = null;
    /* If a piece has a destructibility parent, the piece will always break
     * whenever its destructibility parent is broken. */
    [SerializeField] public DestructibleMeshPiece destructibilityParent = null;
    /* Total impulse which the part can withstand
     * across multiple projectile collisions
     * before it will break off. */
    [SerializeField] public float maxHealth = 1.0f;
    [SerializeField] public bool reparable = true;
    [SerializeField] public int repairCost = 1;
    [SerializeField] public ActionSound repairSound = null;
    [SerializeField] public bool spawnsDebris = true;
    [SerializeField] public float mass = 1.0f;
    [SerializeField] public float collisionFalsePositivePreventionTimeout = 0.25f;
    [SerializeField] public int aiTargetPriority = 1;

    public DestructibleMesh ownerDestructibleMesh {get => _ownerDestructibleMesh;}
    public float health {get; private set;}
    public bool attached {get; private set;}

    private float collisionFalsePositivePreventionTimer = 0.0f;

    void Awake()
    {
        ownerDestructibleMesh.RegisterPiece(this);
    }

    void Start()
    {
        if (ownerDestructibleMesh.commonMaterialOverride)
        {
            var renderer = GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.material =
                    ownerDestructibleMesh.commonMaterialOverride;
            }
        }
        health = maxHealth;
        attached = true;
    }

    void Update()
    {
        if (collisionFalsePositivePreventionTimer > 0.0f)
        {
            collisionFalsePositivePreventionTimer -= Time.deltaTime;
        }
        if (health <= 0.0f || DestructibilityParentBroken()) {
            BreakOff(Vector3.zero);
        }
    }

    public bool DestructibilityParentBroken()
    {
        return destructibilityParent != null &&
            !destructibilityParent.attached;
    }

    public void ReportHit(
        Vector3 source,
        Vector3 impulse,
        float blastRadius
    ) {
        ownerDestructibleMesh.TakeHit(source, impulse, blastRadius);
    }

    public void TakeDamage(Vector3 impulse)
    {
        if (collisionFalsePositivePreventionTimer <= 0.0f) {
            health -= impulse.magnitude;
            /*Needle.Console.D.Log(
                $"Destructible mesh piece {this} " +
                $"took {impulse.magnitude} damage " +
                $"and has {health} remaining health",
                this, "Combat"
            );*/
            if (health <= 0.0f)
            {
                BreakOff(impulse);
            }
            else
            {
                hit?.Invoke();
                collisionFalsePositivePreventionTimer =
                    collisionFalsePositivePreventionTimeout;
            }
            if (effectOnHit != null)
            {
                Instantiate(
                    effectOnHit.gameObject,
                    transform.position,
                    transform.rotation
                );
            }
        }
    }

    public void BreakOff(Vector3 impulse)
    {
        if (attached)
        {
            broken?.Invoke();
            health = 0.0f;
            attached = false;
            if (reparable) CreateRepairSite();
            if (spawnsDebris) CreateDebris(impulse);
            if (effectOnBreak != null)
            {
                Instantiate(
                    effectOnBreak.gameObject,
                    transform.position,
                    transform.rotation
                );
            }
            gameObject.SetActive(false);
        }
    }

    public void Repair()
    {
        gameObject.SetActive(true);
        health = maxHealth;
        if (!attached)
        {
            attached = true;
            repaired?.Invoke();
        }
    }

    private GameObject CreateDummy()
    {
        return Misc.Imitate(
            gameObject,
            typeof(MeshFilter),
            typeof(MeshRenderer),
            typeof(MeshCollider),
            typeof(BoxCollider),
            typeof(SphereCollider),
            typeof(CapsuleCollider)
        );
    }

    private void CreateRepairSite()
    {
        var repairSiteObject = CreateDummy();
        var interactable = repairSiteObject.AddComponent<Interactable>();
        interactable.interactSound = repairSound;
        interactable.requirements =
            repairSiteObject.AddComponent<InteractRequirements>();
        interactable.requirements.requirements = new string[] {"ok"};
        interactable.requirements.activeItemIdsNeeded = new int[] {};
        interactable.requirements.Reinitialize();
        var repairSite =
            repairSiteObject.AddComponent<DestructibleMeshRepairSite>();
        repairSite.pieceToRepair = this;
        Misc.RecursiveChangeMaterial(
            repairSiteObject,
            ownerDestructibleMesh.repairSiteMaterial
        );
        foreach (var componentType in new Type[] {
            typeof(MeshCollider),
            typeof(BoxCollider),
            typeof(SphereCollider),
            typeof(CapsuleCollider)
        }) {
            var component = repairSite.GetComponent(componentType);
            if (component != null)
            {
                if (component is MeshCollider)
                {
                    (component as MeshCollider).convex = true;
                }
                (component as Collider).isTrigger = true;
            }
        }
    }

    private void CreateDebris(Vector3 impulse)
    {
        var debris = CreateDummy().AddComponent<Rigidbody>();
        var temp = debris.gameObject.AddComponent<TemporaryGameObject>();
        temp.destroyAfterTimeout = true;
        temp.destroyAtKillPlane = true;
        temp.destroyWhenNotVisible = true;
        var srcRend = GetComponent<MeshRenderer>();
        var dstRend = debris.GetComponent<MeshRenderer>();
        if (srcRend && dstRend)
        {
            dstRend.material = srcRend.material;
        }
        debris.mass = mass;
        debris.AddForce(impulse, ForceMode.Impulse);
        foreach (var componentType in new Type[] {
            typeof(MeshCollider),
            typeof(BoxCollider),
            typeof(SphereCollider),
            typeof(CapsuleCollider)
        }) {
            var component = debris.GetComponent(componentType);
            if (component != null)
            {
                (component as Collider).excludeLayers =
                    LayerMask.GetMask("Vehicle convex colliders");
            }
        }
    }
}
