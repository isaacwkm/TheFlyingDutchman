using UnityEngine;

public class DestructibleMeshRepairSite : MonoBehaviour
{
    private enum ReparabilityStatus
    {
        Reparable,
        AlreadyRepaired,
        NotEnoughWood,
        DestructibilityParentBroken
    }

    [HideInInspector] public DestructibleMeshPiece pieceToRepair;

    private Interactable interactable;

    void Awake()
    {
        interactable = GetComponent<Interactable>();
    }

    void OnEnable()
    {
        interactable.OnInteract += OnInteract;
    }

    void OnDisable()
    {
        interactable.OnInteract -= OnInteract;
    }

    private ReparabilityStatus CheckReparabilityStatus()
    {
        if (pieceToRepair.attached)
        {
            return ReparabilityStatus.AlreadyRepaired;
        }
        else if (pieceToRepair.DestructibilityParentBroken())
        {
            return ReparabilityStatus.DestructibilityParentBroken;
        }
        else if (
            SceneCore.resourceInventory.log <
            pieceToRepair.repairCost
        ) {
            return ReparabilityStatus.NotEnoughWood;
        }
        else
        {
            return ReparabilityStatus.Reparable;
        }
    }

    private string GetReparabilityStatusMessage(
        ReparabilityStatus rstatus
    ) {
        switch (rstatus)
        {
            case ReparabilityStatus.NotEnoughWood:
                return $"need at least {pieceToRepair.repairCost} wood";
            case ReparabilityStatus.DestructibilityParentBroken:
                return "supporting piece still broken";
            default:
                return "";
        }
    }

    void Update()
    {
        if (pieceToRepair.attached)
        {
            Destroy(gameObject);
        }
        else
        {
            ReparabilityStatus rstatus = CheckReparabilityStatus();
            interactable.requirements.UpdateRequirement(
                "ok", rstatus == ReparabilityStatus.Reparable
            );
            interactable.actionTooltip =
                $"Repair (costs {pieceToRepair.repairCost} wood)";
            interactable.requirementTooltipText =
                $"Can't repair ({GetReparabilityStatusMessage(rstatus)})";
        }
    }

    private void OnInteract(GameObject dontCare)
    {
        if (CheckReparabilityStatus() == ReparabilityStatus.Reparable)
        {
            SceneCore.resourceInventory.log -= pieceToRepair.repairCost;
            pieceToRepair.Repair();
            Destroy(gameObject);
        }
    }
}
