using UnityEngine;

public class DestructibleMeshRepairSite : MonoBehaviour
{
    [HideInInspector] public DestructibleMeshPiece pieceToRepair;

    public int repairCost {get => pieceToRepair.repairCost;}

    public bool CanRepair()
    {
        return !pieceToRepair.attached &&
            !pieceToRepair.DestructibilityParentBroken();
    }

    public void Repair()
    {
        pieceToRepair.Repair();
        Destroy(gameObject);
    }

    public void Update()
    {
        if (pieceToRepair.attached)
        {
            Destroy(gameObject);
        }
    }
}
