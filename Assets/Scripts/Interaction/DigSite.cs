using Needle.Console;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DigSite : MonoBehaviour
{
    [SerializeField] private Interactable interactTarget;
    [SerializeField] private Transform digSpotTransform;
    [SerializeField] private Transform[] pileTransforms;
    [SerializeField] private Transform itemInstantiateSpot;
    [SerializeField] private GameObject itemToInstantiate;
    [SerializeField] private int digsNeeded = 7;
    [SerializeField] private int digsNeededPerStage = 2;
    [SerializeField] private ActionSound digSound;
    private const float dirtPileHeight = 0.1f;
    private Transform currentPile;
    private int digProgress = 0; // overall dig progress
    private  int digPileStage = 0; // Keeps track of current stage
    private int digsDoneInCurrentStage = 0; // Keeps track of digs done in current stage
    private int digsNeededInCurrentStage = 0; // changes whenever a new stage is reached

    private float digSiteRadiusIncreaseAmount; // Set to be 1 divided by digs needed.
    void OnEnable() {
        interactTarget.OnInteract += Dig;
    }

    void OnDisable() {
        interactTarget.OnInteract -= Dig;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        digSiteRadiusIncreaseAmount = 1f / digsNeeded;
        D.Log($"Digs Needed Amount: {digsNeeded}", gameObject, "Dig");
        D.Log($"Dig Site Radius Increase Amount: {digSiteRadiusIncreaseAmount}", gameObject, "Dig");
        MeshRenderer digSpotMeshRender = digSpotTransform.gameObject.GetComponent<MeshRenderer>();
        digSpotMeshRender.enabled = false;
        foreach (Transform pile in pileTransforms){
            pile.gameObject.SetActive(false);
        }
    }

    void Dig(GameObject player){
        D.Log("Dig() called", gameObject, "Dig");
        if (digProgress == -1) return; // is set to -1 when fully dug out.

        // Play dig animation
        Inventory playerInv = player.GetComponent<Inventory>();
        GameObject currItem = playerInv.currentItem();
        ActiveItem activeItem = currItem.GetComponent<ActiveItem>();

        activeItem.doAttack(forcePlayAnim: true); // Performs dig animation and interrupts any ongoing hand animation.

        StartCoroutine(updateDigProgress());
    }

    private System.Collections.IEnumerator updateDigProgress(){
        // Wait for the duration
        yield return new WaitForSeconds(1);

        // Play sound effect
        if (digSound){
        digSound.PlaySingleRandom();
        }

        // Handle first dig or digs that come after it
        if (digProgress == 0){ // On first dig, don't create a pile, just show the spot with a small dirt hole
            createFirstDig();
        }
        else if (digProgress > 0){ // Increase the dirt pile and widen the hole
            calculateDigPileHeight();
            IncreaseDigSiteRadius();
        }

        // Check if the site is fully dug
        if (digProgress == digsNeeded){
            revealItem();
            digProgress = -1;
        }
    }

    void revealItem(){
        GameObject item = Instantiate(itemToInstantiate, itemInstantiateSpot.position, itemInstantiateSpot.rotation);
        item.GetComponent<DroppedItem>().enableSilentDrop(true);
    }

    void createFirstDig(){
        D.Log("createFirstDig", gameObject, "Dig");
        digProgress = 1;

        MeshRenderer digSpotMeshRender = digSpotTransform.gameObject.GetComponent<MeshRenderer>();
        digSpotMeshRender.enabled = true;
        float x = digSiteRadiusIncreaseAmount;
        float y = digSpotTransform.localScale.y;
        float z = digSiteRadiusIncreaseAmount;
        digSpotTransform.localScale = new Vector3(x, y, z);
    }
    void IncreaseDigSiteRadius(){
        float x = digSpotTransform.localScale.x + digSiteRadiusIncreaseAmount;
        float y = digSpotTransform.localScale.y;
        float z = digSpotTransform.localScale.z + digSiteRadiusIncreaseAmount;
        digSpotTransform.localScale = new Vector3(x, y, z);
    }

    void calculateDigPileHeight(){
        if (digProgress < 1) return;

        // We add one to the current pile
        digProgress++;
        digsDoneInCurrentStage++;

        if (digsDoneInCurrentStage > digsNeededInCurrentStage){ // All this happens when you move onto the next stage.
            digPileStage++; // increment stage
            digsNeededInCurrentStage += digsNeededPerStage; // increase to new requirement in next stage
            digsDoneInCurrentStage = 1; // reset this

            currentPile = pileTransforms[digPileStage - 1];
            currentPile.gameObject.SetActive(true);
            currentPile.localScale = new Vector3(currentPile.localScale.x, dirtPileHeight, currentPile.localScale.z);
        }
        else{
            currentPile = pileTransforms[digPileStage - 1];
            float dirtStackHeight = dirtPileHeight * digsDoneInCurrentStage; // The height of the stack in the current pile
            currentPile.localScale = new Vector3(currentPile.localScale.x, dirtStackHeight, currentPile.localScale.z);
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
