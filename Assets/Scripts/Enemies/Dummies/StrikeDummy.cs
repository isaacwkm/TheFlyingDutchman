// Attribution: Aidan Higgins from CMPM 172, Contract work. 4/23/25
// Modifications made by Isaac Kim to incorporate more than just enemies, but also trees.

using System;
using Needle.Console;
using UnityEngine;

// ANY ENEMY OR DESTRUCTIBLE OBJECT THAT CAN BE ATTACKED BY A WEAPON OR TOOL CONTAINS THIS SCRIPT
// This script allows hit interactions from the player's weapon/handheld item and play a death/destruction animation when hit enough times.
public class StrikeDummy : MonoBehaviour
{
    public ActionSound hitSound;
    private Animator deathAnimator;
    public int health = 3;
    public int healthRandomizeIntensity = 1; // Dictates the range of allowed health
    public int damageTakenPerHit = 1;
    private bool alive = true;

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
        D.Log($"{gameObject.name} took a hit", this, "Combat");

        if (!HealthIsPositive())
        {
            Die();
        }
        
    }

    private void Die()
    {
        //call death animation
    }

    private bool HealthIsPositive()
    {
        if (health <= 0)
        {
            return false;
        }
        return true;
    }
}
