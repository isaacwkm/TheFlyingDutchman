using UnityEngine;

public class HUDResourceWood : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI textObject;
    void Update()
    {
        textObject.text = $"Wood: {SceneCore.resourceInventory.log}";
    }
}
