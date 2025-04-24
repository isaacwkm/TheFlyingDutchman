using UnityEngine;

public class StoryScene2 : StoryClass
{
    public override int storySceneID {get; } = 2; // IMPORTANT! Set story scene ID!
    public UsefulCommands commands;
    public Transform sceneStartSpawn;

    // Start of: "Data to be sent out as story data":
    private int jumpCount = 0;
    private int objectsInteractedWith = 0;
    // End
    public override int[] receiveDataFrom { get; } = {1}; // List of scenes to receive data from

    void Awake()
    {
        commands = SceneCore.commands;
    }
    public override void startStoryScene() // Operations to do when starting the scene
    {
        commands.Teleport(SceneCore.playerCharacter.gameObject, sceneStartSpawn);
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
