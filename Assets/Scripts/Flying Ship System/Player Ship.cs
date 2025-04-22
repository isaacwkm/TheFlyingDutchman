using UnityEngine;

public class PlayerShip : MonoBehaviour
{
    [SerializeField] public DestructibleMesh geometryObject;
    [SerializeField] public FlyingVehicle physicsObject;
    public Transform transform { get => physicsObject.transform; }
}
