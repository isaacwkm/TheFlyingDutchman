using Needle.Console;
using NUnit.Framework;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(DroppedItem))]
public class ActiveItem : MonoBehaviour
{
    [Tooltip("Do not re-assign during runtime.")]
    public int itemIDPleaseDoNotChange = 1; // Do not re-assign during runtime. The weird variable naming is only to discourage ID re-assignment after it has been correctly set to the right item id.
    public Vector3 heldPositionOffset;
    public Vector3 heldRotationOffset;
    public bool hasInteractAnim = false;
    public string[] interactAnimNames;
    public bool hasAttack = false;
    public string[] attackAnimNames;
    private Animator handAnim;
    private Quaternion defaultRotation;

    void Awake()
    {
        defaultRotation = gameObject.GetComponent<DroppedItem>().defaultRotation;
    }

    void OnEnable()
    {

    }

    void OnDisable()
    {

    }
    public void setPlayerHandAnim(Animator animator)
    {
        handAnim = animator;
    }
    public void SnapToHand(Transform item, Transform handPosition)
    {
        // Parent the item to the hand position
        item.SetParent(handPosition);

        // Reset the local position and rotation to align it perfectly with the hand
        item.localScale = Vector3.one; // Reset local scale
        item.localPosition = Vector3.zero + heldPositionOffset;
        item.localRotation = defaultRotation * Quaternion.Euler(heldRotationOffset);
    }

    public void doAttack(bool forcePlayAnim = false)
    {
        doAttackAnim(forcePlayAnim);
    }

    public void doAttackAnim(bool forcePlayAnim = false)
    {
        if (!hasAttack) return;
        Debug.Assert(attackAnimNames.Length > 0, $"Add attack animation names to item ID {itemIDPleaseDoNotChange} !");

        string atkName;
        int randomAtkIndex = 0;

        if (attackAnimNames.Length == 1)
        {
            atkName = attackAnimNames[0];
        }
        else
        {
            randomAtkIndex = UnityEngine.Random.Range(0, attackAnimNames.Length);
            atkName = attackAnimNames[randomAtkIndex];
        }


        if (forcePlayAnim == true)
        {
            handAnim.Play(atkName, -1, 0);
        }

        if (handAnim.GetCurrentAnimatorStateInfo(0).normalizedTime > 1 && !handAnim.IsInTransition(0))
        {
            handAnim.Play(atkName, -1, 0);
            D.Log($"Attacked with animation #{randomAtkIndex}! Name: {attackAnimNames[randomAtkIndex]}", this, "Item");
        }

        StartCoroutine(Hit(attackAnimNames[randomAtkIndex])); //throws out a hitbox, right now animation names are hardcoded
    }

     IEnumerator Hit(string animName)
    {
        yield return new WaitForSeconds(0.5f); //sync with animation swing

        var hits = new Collider[10]; //can hold 10 colliders, maybe increase if there is a ton of entities
        Vector3 boxCenter = Camera.main.transform.position + Camera.main.transform.forward * 1.5f;
        float swingAngle = 0f;
        if (animName == "Left Swing")
        {
            swingAngle = -45f;
        }
        else if (animName == "Right Swing")
        {
            swingAngle = 45f;
        }
        Physics.OverlapBoxNonAlloc(boxCenter, new Vector3(0.5f, 0.5f, 1.5f), hits, Quaternion.LookRotation(Camera.main.transform.forward) * Quaternion.Euler(0f, 0f, swingAngle)); //throw out a box collider diagonally in front of the camera
        foreach (var hit in hits)
        {
            if (hit.GetComponent<Enemy>())
            {
                Enemy enemy = hit.GetComponent<Enemy>();
                enemy.TakeHit();
            }
        }
    }



    public void doInteractionAnim(bool forcePlayAnim = false)
    {
        if (!hasInteractAnim) return;
        Debug.Assert(interactAnimNames.Length > 0, $"Add interaction animation names to item ID {itemIDPleaseDoNotChange} !");

        string interactName;
        int randominteractIndex = 0;

        if (interactAnimNames.Length == 1)
        {
            interactName = interactAnimNames[0];
        }
        else
        {
            randominteractIndex = UnityEngine.Random.Range(0, interactAnimNames.Length);
            interactName = interactAnimNames[randominteractIndex];
        }


        if (forcePlayAnim == true)
        {
            handAnim.Play(interactName, -1, 0);
        }

        if (handAnim.GetCurrentAnimatorStateInfo(0).normalizedTime > 1 && !handAnim.IsInTransition(0))
        {
            handAnim.Play(interactName, -1, 0);
            D.Log($"Interacted with animation #{randominteractIndex}! Name: {interactAnimNames[randominteractIndex]}", this, "Item");
        }
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
