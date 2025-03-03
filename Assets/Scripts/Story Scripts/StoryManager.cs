// Handles the flow of gameplay and story, responsible for switching scenes (not necessarily Unity scenes) through tecnhiques such as
// fade-to-black + teleporting. Responsible for enabling or disabling certain game objects depending on the current progression of the story.
// StoryManager mainly calls upon specific
using System.Collections.Generic;
using Needle.Console;
using UnityEngine;

public class StoryManager : MonoBehaviour
{
    private StoryClass currentStoryScene;
    private Transform playerTransform;
    public GameObject gameplayBoundary;
    public StoryClass[] StoryScenesList;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerTransform = SceneCore.playerCharacter.transform;
        playStoryScene(1);
    }

    public void playNextStoryScene()
    {
        playStoryScene(currentStoryScene.getStorySceneID() + 1);
    }

    public void playStoryScene(int sceneID)
    {
        if (sceneID < 0 || sceneID >= StoryScenesList.Length)
        {
            D.LogError("Out of Bounds Access for requestToPlayNextStoryScene()", gameObject, "Story");
            return;
        }

        currentStoryScene = StoryScenesList[sceneID];
        currentStoryScene.initStoryScene();
    }

    public Dictionary<int, StoryData> requestDataFromScenes(int[] foreignStorySceneIDs)
    {
        if (foreignStorySceneIDs.Length == 0) return null; // Don't return any data if no data from other scenes are needed

        // Dictionary definition below:
        // Key = integer ID of foreign story scene
        // Value = Data from that foreign story scene
        Dictionary<int, StoryData> foreignStoryData = new Dictionary<int, StoryData>();

        for (int i = 0; i < foreignStorySceneIDs.Length; i++)
        {
            int foreignStorySceneID = foreignStorySceneIDs[i];
            foreignStoryData[foreignStorySceneID] = StoryScenesList[i].GetStoryData();
        }
        // Result: All keys in the dictionary are all scene ID's which the caller requested
        // All paired values in the dictionary are the associated data that was requested from each scene

        return foreignStoryData;
    }

    
}
