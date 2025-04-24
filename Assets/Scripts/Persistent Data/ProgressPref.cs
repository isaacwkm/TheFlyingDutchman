using System;
using UnityEngine;

public class ProgressPref : MonoBehaviour
{
    StoryManager storyManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (storyManager.isActiveAndEnabled)
        {
            LoadPref();
        }   
    }

    public static int progress
    {
        get { return SceneCore.storyManager.currentStorySceneID; }
        set { SceneCore.storyManager.playStoryScene(value); }
    }

    public static int LoadPref()
    {
        return progress =
            PlayerPrefs.HasKey("progress") ?
            PlayerPrefs.GetInt("progress") :
            1;
    }

    public static void SavePref()
    {
        PlayerPrefs.SetInt("progress", progress);
    }
}
