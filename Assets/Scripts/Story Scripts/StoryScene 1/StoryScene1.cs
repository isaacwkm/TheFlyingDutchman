using UnityEngine;

public class StoryScene1 : StoryClass
{

    // Start of: "Data to be sent out as story data":
    private int jumpCount = 0;
    private int objectsInteractedWith = 0;
    // End
    
    // Other class variables:
    public override int[] receiveDataFrom { get; } = { }; // List of scenes to receive data from
    public override void startStoryScene() // Operations to do when starting the scene
    {
    }

    public override void cleanupStoryScene() // Operations to do to clean up what was done in the playthrough of the scene
    {
    }

    public override void finalizeData() // Publish data to be read by other scenes
    {
        storyData.Set("JumpCount", jumpCount);
        storyData.Set("ObjectsInteractedWith", objectsInteractedWith);
    }
}
