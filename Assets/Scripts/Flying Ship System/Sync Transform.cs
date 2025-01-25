using System;
using System.Collections.Generic;
using UnityEngine;

/* The point of this class is to avoid having to parent the ship's actual geometry to its physics object,
because if we did that, it would affect the physics in a way there's no good way to work with nor prevent. */

public class SyncTransform : MonoBehaviour
{
    private class BodyInfluenced {
        public Transform transform;
        public Rigidbody rigidbody;
        public CharacterController characterController;
        public Vector3 relPosn;
        public BodyInfluenced(GameObject gameObject) {
            transform = gameObject.transform;
            rigidbody = gameObject.GetComponent<Rigidbody>();
            characterController = gameObject.GetComponent<CharacterController>();
            relPosn = Vector3.zero;
        }
        public void setPhysicsEnabled(bool whether) {
            if (characterController) characterController.enabled = whether;
            if (rigidbody) {
                if (whether) {
                    rigidbody.Sleep();
                } else {
                    rigidbody.WakeUp();
                }
            }
        }
        public void setPosition(Vector3 where) {
            transform.position = where;
            if (rigidbody) rigidbody.position = where;
        }
    }

    [SerializeField] private Transform target;
    [SerializeField] private Vector3 linearOffset;
    [SerializeField] private Quaternion angularOffset;
    [SerializeField] private GameObject[] initialBodiesInfluenced;
    private List<BodyInfluenced> bodiesInfluenced;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        bodiesInfluenced = new List<BodyInfluenced>();
        foreach (var other in initialBodiesInfluenced) {
            bodiesInfluenced.Add(new BodyInfluenced(other));
        }
    }

    // Update is called once per frame
    void Update()
    {
        foreach (var body in bodiesInfluenced) {
            body.relPosn = target.InverseTransformPoint(body.transform.position);
        }
        target.rotation = transform.rotation*angularOffset;
        target.position = transform.position + (
            linearOffset.x*transform.right +
            linearOffset.y*transform.up +
            linearOffset.z*transform.forward
        );
        foreach (var body in bodiesInfluenced) {
            body.setPhysicsEnabled(false);
            body.setPosition(target.TransformPoint(body.relPosn));
            body.setPhysicsEnabled(true);
        }
    }

    void OnTriggerEnter(Collider other) {
        if (other.gameObject != this.gameObject && (
            other.GetComponent<Rigidbody>() ||
            other.GetComponent<CharacterController>()
        )) {
            bodiesInfluenced.Add(new BodyInfluenced(other.gameObject));
        }
    }

    void OnTriggerExit(Collider other) {
        bodiesInfluenced.RemoveAll((body) => (body.transform.gameObject == other.gameObject));
    }
}
