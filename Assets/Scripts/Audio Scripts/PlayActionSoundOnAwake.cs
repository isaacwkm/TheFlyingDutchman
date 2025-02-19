using UnityEngine;

public class PlayActionSoundOnAwake : MonoBehaviour
{
    public ActionSound sound;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        sound.PlaySingleRandom();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
