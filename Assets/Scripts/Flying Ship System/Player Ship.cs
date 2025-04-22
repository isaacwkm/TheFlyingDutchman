using UnityEngine;

public class PlayerShip : MonoBehaviour
{
    [SerializeField] public DestructibleMesh geometryObject;
    [SerializeField] public FlyingVehicle physicsObject;
    [SerializeField] public ResourceInteraction resourceInventory;
    public Transform transform { get => physicsObject.transform; }
}
