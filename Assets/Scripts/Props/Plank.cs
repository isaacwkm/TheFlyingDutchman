using Unity.VisualScripting;
using UnityEngine;

public class Plank : MonoBehaviour
{
    private StrikeDummy strikeDummy;
    public InteractRequirements interactRequirements;
    public string requirementName;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        strikeDummy = this.gameObject.GetComponent<StrikeDummy>();
        strikeDummy.deathCheck += setRequirementTrue;
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void setRequirementTrue()
    {
        interactRequirements.UpdateRequirement(requirementName, true);
    }
}
