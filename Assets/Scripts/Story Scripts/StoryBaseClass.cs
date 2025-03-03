using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class StoryClass : MonoBehaviour
{
    protected StoryManager storyManager;
    public abstract int storySceneID { get; }
    public virtual int[] receiveDataFrom { get; } // Array of Scene IDs of scenes to receive data from
    protected StoryData storyData = new StoryData();
    protected Dictionary<int, StoryData> foreignStoryData;


    public void initStoryScene(){
        storyManager = SceneCore.storyManager;
        foreignStoryData = storyManager.requestDataFromScenes(receiveDataFrom);
        startStoryScene();
    }
    public abstract void startStoryScene(); // Operations to do when starting the scene
    public abstract void cleanupStoryScene(); // Operations to do to clean up what was done in the playthrough of the scene
    public abstract void finalizeData(); // Publish data to be read by other scenes
    protected void endStoryScene(Collider playerCol = null) // Called to jump to next scene
    {
        finalizeData();
        cleanupStoryScene();
        storyManager.playNextStoryScene();
    }

    public int getStorySceneID()
    {
        return storySceneID;
    }

    public StoryData GetStoryData()
    {
        return storyData;
    }

    public void setForeignStoryData(Dictionary<int, StoryData> data)
    {
        foreignStoryData = data;
    }
}
