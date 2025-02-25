using System;
using Needle.Console;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class NightmareShip : MonoBehaviour
{
    [SerializeField] private Interactable rudderInteractTarget;
    [SerializeField] private Animator wheelPopsOffAnimation;
    [SerializeField] private ParticleSystem fallParticleEffect;
    [SerializeField] private float bobSpeed = 1.0f;
    [SerializeField] private float bobRange = 1.0f;
    [SerializeField] private float linearAcceleration = 3.0f;
    [SerializeField] private float verticalAcceleration = 1.0f;
    [SerializeField] private float angularAcceleration = 12.0f;
    [SerializeField] private float traction = 0.01f;
    private Vector3 cameraInitialDisplacement;
    private Transform sceneCore; // return the player here after un-parenting
    private Vector2 xzMovementInput = Vector2.zero;
    private float yMovementInput = 0.0f;
    private Vector2 lookInput = Vector2.zero;
    private Vector3 impetus = Vector3.zero;
    private float baseY;
    private float bobDirection = -1.0f;
    private float targetY;
    private Rigidbody rbody;
    private GameObject currentPlayer = null;

    void Awake()
    {

    }
    void OnEnable()
    {
        rudderInteractTarget.OnInteract += doRudderInteraction;
    }

    void OnDisable()
    {
        rudderInteractTarget.OnInteract -= doRudderInteraction;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        targetY = baseY = transform.position.y;
        rbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        // Move altitude targets if ascending/descending
        if (impetus.y == 0.0f)
        {
            // Bobbing
            targetY += bobDirection * bobSpeed * Time.deltaTime;
            if ((targetY - baseY) / bobDirection > bobRange)
            {
                bobDirection = -bobDirection;
            }
        }
        else
        {
            targetY = transform.position.y;
            baseY = transform.position.y;
        }
        // Apply Forces
        rbody.AddTorque(transform.up * impetus.x * rbody.mass * angularAcceleration * Time.deltaTime, ForceMode.Impulse);
        rbody.AddForce(transform.forward * impetus.z * rbody.mass * linearAcceleration * Time.deltaTime, ForceMode.Impulse);
        rbody.AddForce(Vector3.up * rbody.mass * (targetY - transform.position.y) * bobSpeed * Time.deltaTime, ForceMode.Impulse);
        rbody.linearVelocity = Vector3.Lerp(
            rbody.linearVelocity,
            Vector3.ProjectOnPlane(rbody.linearVelocity, transform.right),
            1.0f - Mathf.Pow(1.0f - traction, Time.deltaTime * 60.0f)
        );
    }

    void doRudderInteraction(GameObject player)
    {
        var pcm = player.GetComponent<PlayerCharacterController>();
        if (pcm)
        {
            // Trigger nightmare sequence here
            if (wheelPopsOffAnimation.GetCurrentAnimatorStateInfo(0).normalizedTime > 1 && !wheelPopsOffAnimation.IsInTransition(0))
            {
                GameObject rudder = wheelPopsOffAnimation.gameObject;
                Rigidbody rb = rudder.GetComponent<Rigidbody>();
                MeshCollider mc = rudder.GetComponent<MeshCollider>();
                if (rb)
                {
                    rb.isKinematic = true;
                }
                if (mc)
                {
                    mc.enabled = false;
                }

                wheelPopsOffAnimation.Play("NightmareRudder", -1, 0);
                playParticleEffect();
            }

        }
    }

    private System.Collections.IEnumerator playParticleEffect()
    {
        D.Log("played fall particle effect 1", gameObject, "Any");
        // Wait for the duration
        yield return new WaitForSeconds(0.5f);
        D.Log("played fall particle effect 2", gameObject, "Any");
        fallParticleEffect.Play();
        D.Log("played fall particle effect 3", gameObject, "Any");
    }



}
