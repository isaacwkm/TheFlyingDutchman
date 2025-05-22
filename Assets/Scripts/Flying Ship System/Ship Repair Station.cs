using UnityEngine;
using System;

[RequireComponent(typeof(Interactable))]
public class ShipRepairStation : MonoBehaviour
{
    private Action<GameObject> repair;
    private Interactable interactable { get => GetComponent<Interactable>(); }
    private DestructibleMesh shipDM {
        get => SceneCore.ship.geometryObject.GetComponent<DestructibleMesh>();
    }

    void OnEnable()
    {
        interactable.OnInteract += repair = (_) => shipDM.FullRepair();
    }

    void OnDisable()
    {
        interactable.OnInteract -= repair;
    }

    void Update()
    {
        interactable.requirements?.UpdateRequirement("-", shipDM.GetHealth() < shipDM.GetMaxHealth());
    }
}
