using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class StoryClass : MonoBehaviour
{
    protected StoryManager storyManagerReference;
    protected int storySceneID = 0;
    public virtual int[] receiveDataFrom { get; } // Array of Scene IDs of scenes to receive data from
    protected StoryData storyData = new StoryData();
    protected Dictionary<int, StoryData> foreignStoryData;


    void Awake()
    {
        storyManagerReference = SceneCore.storyManager;
    }
    public abstract void startStoryScene(); // Operations to do when starting the scene
    public abstract void cleanupStoryScene(); // Operations to do to clean up what was done in the playthrough of the scene
    public abstract void finalizeData(); // Publish data to be read by other scenes
    protected void endStoryScene() // Called to jump to next scene
    {
        finalizeData();
        cleanupStoryScene();
        storyManagerReference.playNextStoryScene();
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
