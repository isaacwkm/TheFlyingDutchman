// Attribution: Aidan Higgins from CMPM 172, Contract work. 4/23/25
// Modifications made by Isaac Kim to incorporate health and interaction with trees.

using System;
using Needle.Console;
using UnityEngine;

// ANY ENEMY OR DESTRUCTIBLE OBJECT THAT CAN BE ATTACKED BY A WEAPON OR TOOL CONTAINS THIS SCRIPT
// This script allows hit interactions from the player's weapon/handheld item and play a death/destruction animation when hit enough times.
public class StrikeDummy : MonoBehaviour
{
    public ActionSound hitSound;
    public int health = 3;
    public int healthRandomizeIntensity = 1; // Dictates the range of allowed health
    public int damageTakenPerHit = 1;
    private bool alive = true;
    public Transform deathDirectionTransform;
    public Animator deathAnimator;
    public string[] deathAnimNames;
    public Animator deathAnimatorExtraEffect; // supplementary animation to be played at the same time
    public string[] deathAnimExtraEffectNames;

    void OnEnable()
    {
        int healthModifier = UnityEngine.Random.Range(-healthRandomizeIntensity, healthRandomizeIntensity);
        health = health + healthModifier;
    }

    void makeHitSound() // Call this method when hit, to use as a hit-confirm feedback to player
    {
        hitSound.PlaySingleRandom(); // Plays a sound effect
    }

    public void TakeHit()
    {
        if (!alive) return;

        // Play hit sound effect
        makeHitSound();

        // Update data
        health--;
        D.Log($"{gameObject.name} took a hit. Health Remaining: {health}", this, "Combat");

        if (!HealthIsPositive())
        {
            Die();
        }

    }

    private void Die()
    {
        playDeathAnim();
        alive = false;
    }

    private bool HealthIsPositive()
    {
        if (health <= 0)
        {
            return false;
        }
        return true;
    }

    public void playDeathAnim(bool forcePlayAnim = false)
    {
        if (deathAnimNames.Length == 0)
        {
            D.Log($"Add death animation names to the strike dummy!", this, "Combat");
            return;
        }

        bool hasExtraDeathAnim = true;
        if (deathAnimExtraEffectNames.Length == 0)
        {
            hasExtraDeathAnim = false;
        }

        deathDirectionTransform.rotation = Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f);

        int randominteractIndex = 0;
        randominteractIndex = UnityEngine.Random.Range(0, deathAnimNames.Length);

        string deathName;
        deathName = deathAnimNames[randominteractIndex];

        string deathName2 = "";
        if (hasExtraDeathAnim)
        {
            deathName2 = deathAnimExtraEffectNames[randominteractIndex];
        }


        if (forcePlayAnim == true)
        {
            deathAnimator.Play(deathName, -1, 0);

            if (hasExtraDeathAnim)
            {
                deathAnimatorExtraEffect.Play(deathName2, -1, 0);
            }
        }

        if (deathAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1 && !deathAnimator.IsInTransition(0))
        {
            deathAnimator.Play(deathName, -1, 0);

            if (hasExtraDeathAnim)
            {
                deathAnimatorExtraEffect.Play(deathName2, -1, 0);
            }

            D.Log($"Interacted with animation #{randominteractIndex}! Name: {deathAnimNames[randominteractIndex]}", this, "Item");
        }
    }
}
