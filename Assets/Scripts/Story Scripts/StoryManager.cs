// Handles the flow of gameplay and story, responsible for switching scenes (not necessarily Unity scenes) through tecnhiques such as
// fade-to-black + teleporting. Responsible for enabling or disabling certain game objects depending on the current progression of the story.
// StoryManager mainly calls upon specific
using UnityEngine;

public class StoryManager : MonoBehaviour
{
    public GameObject levelComponents;
    public int i;
    private int currentStoryScene = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playNextStoryScene();
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void playNextStoryScene(){
        currentStoryScene++;
    }
}
