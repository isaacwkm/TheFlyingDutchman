using UnityEngine;
using static UnityEditor.Progress;

public class ResourceInteraction : MonoBehaviour
{

    public int gold = 0;
    public int log = 0;
    private void OnTriggerEnter(Collider other)
    {
        DroppedItem droppedItem = null;
        droppedItem = other.GetComponent<DroppedItem>();
        if (droppedItem != null && droppedItem.itemID == 6)
        {
            //Store chest here
            gold = gold + 100;
            Debug.Log("Gold Amount: " +  gold);
            Destroy(other.gameObject);
        }
        else if (droppedItem != null && droppedItem.itemID == 7)
        {
            //Store log
            log = log + 10;
            Debug.Log("Log Amount: " + log);
            Destroy(other.gameObject);
        }
    }
}
