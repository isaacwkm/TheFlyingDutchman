using UnityEngine;

public class Enemy : MonoBehaviour
{
    public ActionSound hitSound;

    void makeHitSound() // Call this method when hit, to use as a hit-confirm feedback to player
    {
        hitSound.PlaySingleRandom(); // Plays a sound effect
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
