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
    [SerializeField] private GameObject syncTransformer;
    [SerializeField] private GameObject[] deleteOnFall;
    [SerializeField] private Animator shipFallAnimation;
    [SerializeField] private float bobSpeed = 1.0f;
    [SerializeField] private float bobRange = 1.0f;
    [SerializeField] private float linearAcceleration = 3.0f;
    [SerializeField] private float verticalAcceleration = 1.0f;
    [SerializeField] private float angularAcceleration = 12.0f;
    [SerializeField] private float traction = 0.01f;
    [SerializeField] private float stabilizeStrength = 1.0f;
    private Vector3 impetus = Vector3.zero;
    private float baseY;
    private float bobDirection = -1.0f;
    private float targetY;
    private Rigidbody rbody;
    private bool nightmareFalling = false;
    private GameObject playerObj;

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
        if (nightmareFalling == true){
            spinPlayerBackwards();
            return;
        };


        impetus = Vector3.zero;

        // Bobbing
        targetY += bobDirection * bobSpeed * Time.deltaTime;
        if ((targetY - baseY) / bobDirection > bobRange)
        {
            bobDirection = -bobDirection;
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

        // Stabilize
        rbody.AddTorque(
            rbody.mass * angularAcceleration * Time.deltaTime *
                stabilizeStrength *
                Vector3.Cross(transform.up, Vector3.up),
            ForceMode.Impulse
        );
    }

    void spinPlayerBackwards() // Make the player go fkin crazy
    {
        const float spinSpeed = 20f; // Degrees per second
        Transform playerTransform = playerObj.transform; // Assign the head transform in the Inspector
        playerTransform.Rotate(Vector3.back * spinSpeed * Time.deltaTime, Space.Self);
    }


    void doRudderInteraction(GameObject player)
    {
        playerObj = player;
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
                pcm.enableJumping(false);
                pcm.enableSuperJumping(false);
                wheelPopsOffAnimation.Play("NightmareRudder", -1, 0);
                StartCoroutine(playParticleEffect(player));
            }

        }
    }

    private System.Collections.IEnumerator playParticleEffect(GameObject player)
    {
        // Wait for the duration
        yield return new WaitForSeconds(0.5f);
        fallParticleEffect.Play();
        StartCoroutine(makeShipFall(player));
    }

    private System.Collections.IEnumerator makeShipFall(GameObject player)
    {
        // Wait for the duration
        yield return new WaitForSeconds(2f);

        // Disable Sync Transformer on ship
        syncTransformer.SetActive(false);
        // Disable Rigidbody on ship
        gameObject.GetComponent<Rigidbody>().isKinematic = true;

        // Set script flag
        nightmareFalling = true;

        // Hide/Remove objects that don't fall with the ship
        foreach (GameObject objectToBeHidden in deleteOnFall)
        {
            objectToBeHidden.SetActive(false);
        }

        shipFallAnimation.Play("NightmareFallingShip", -1, 0);

    }



}
