using UnityEngine;

// Continuously transforms itself to visualize a SpringJoint
public class Rope : MonoBehaviour
{
    [SerializeField] private SpringJoint springJoint;
    [SerializeField] private float normalHeight;
    private Vector3 initialScale;
    private Vector3 pointBOffset;

    public Vector3 GetPointA()
    {
        return springJoint.connectedBody.transform.TransformPoint(
            springJoint.connectedAnchor
        );
    }

    public Vector3 GetPointB()
    {
        return springJoint.transform.TransformPoint(pointBOffset);
    }

    void Start()
    {
        initialScale = transform.localScale;
        pointBOffset = springJoint.transform.InverseTransformPoint(GetPointA());
    }

    void Update()
    {
        Vector3 pointA = GetPointA();
        Vector3 pointB = GetPointB();
        transform.position = (pointA + pointB)/2.0f;
        transform.rotation = Quaternion.FromToRotation(Vector3.up, (pointA - pointB).normalized);
        transform.localScale = new Vector3(
            initialScale.x,
            initialScale.y*(pointA - pointB).magnitude/normalHeight,
            initialScale.z
        );
    }
}
