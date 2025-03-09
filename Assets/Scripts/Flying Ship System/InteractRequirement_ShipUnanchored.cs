using UnityEngine;

[RequireComponent(typeof(InteractRequirements))]
public class InteractRequirement_ShipUnanchored : MonoBehaviour
{
    [SerializeField] private AnchorControl anchorControl;
    private InteractRequirements ireqs;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ireqs = GetComponent<InteractRequirements>();
    }

    // Update is called once per frame
    void Update()
    {
        ireqs.UpdateRequirement("unanchor", !anchorControl.anchored);
    }
}
