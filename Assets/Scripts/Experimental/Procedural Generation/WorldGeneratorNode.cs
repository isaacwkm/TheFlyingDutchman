using UnityEngine;

public class WorldGeneratorNode : MonoBehaviour
{
    [SerializeField] public int frequency = 1;
    [SerializeField] public bool unique = false;
    [SerializeField] public Vector3Int sizeInCells = Vector3Int.one;
    public void Rotate(int quarterTurns) {
        transform.Rotate(quarterTurns*90.0f*Vector3.up);
        if ((quarterTurns%2 + 2)%2 == 1) {
            sizeInCells = new Vector3Int(sizeInCells.z, sizeInCells.y, sizeInCells.x);
        }
    }
}
