using UnityEngine;

public class PrefabInstanceInitializer_AlienScrollUI : PrefabInstanceInitializer
{
    [TextArea(15,30)] public string text;
    override public void Initialize(GameObject instance)
    {
        var modal = instance.GetComponent<ModalMessage>();
        modal.text = text;
    }
}
