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
        public void setPosition(Vector3 what) {
            if (characterController) characterController.enabled = false;
            transform.position = what;
            if (rigidbody) rigidbody.position = what;
            if (characterController) characterController.enabled = true;
        }
    }

    [SerializeField] private Transform target;
    [SerializeField] private Vector3 linearOffset;
    [SerializeField] private Quaternion angularOffset;
    private List<BodyInfluenced> bodiesInfluenced;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        bodiesInfluenced = new List<BodyInfluenced>();
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
            body.setPosition(target.TransformPoint(body.relPosn));
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
