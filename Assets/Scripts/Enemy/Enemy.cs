using System;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public ActionSound hitSound;

    void makeHitSound() // Call this method when hit, to use as a hit-confirm feedback to player
    {
        hitSound.PlaySingleRandom(); // Plays a sound effect
    }

    public void TakeHit()
    {
        makeHitSound();
        Die();
        Debug.Log(gameObject.name + " took a hit");
    }

    private void Die()
    {
        //call death animation
    }
}
