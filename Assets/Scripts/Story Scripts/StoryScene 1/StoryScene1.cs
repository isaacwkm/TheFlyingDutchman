using Needle.Console;
using UnityEngine;

public class StoryScene1 : StoryClass
{
    public override int storySceneID {get; } = 1; // IMPORTANT! Set story scene ID!
    public Transform sceneStartSpawn;
    public TriggerZoneHandler nightmareBoundary;
    public NightmareShip nightmareShipScript;
    public GameObject wakeUpMusicZone;
    private UsefulPlayerOperations commands;

    // Start of: "Data to be sent out as story data":
    private int jumpCount = 0;
    private int objectsInteractedWith = 0;
    // End
    public override int[] receiveDataFrom { get; } = { }; // List of scenes to receive data from
    void Start()
    {
    }
    public override void startStoryScene() // Operations to do when starting the scene
    {
        UsefulPlayerOperations.Instance.TeleportPlayer(SceneCore.playerCharacter.gameObject, sceneStartSpawn);
        nightmareBoundary.OnExit += endStoryScene; // End the scene when player leaves the boundary (they fall off the island to move onto the next scene)

    }

    public override void cleanupStoryScene() // Operations to do to clean up what was done in the playthrough of the scene
    {
        nightmareShipScript.cleanUp();
        nightmareBoundary.OnExit -= endStoryScene; // Remove Listener
    }

    public override void finalizeData() // Publish data to be read by other scenes
    {
        storyData.Set("JumpCount", jumpCount);
        storyData.Set("ObjectsInteractedWith", objectsInteractedWith);
    }
}
