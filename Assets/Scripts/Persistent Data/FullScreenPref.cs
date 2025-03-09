using UnityEngine;

public class FullScreenPref : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        LoadPref();
    }

    private static void WorkAroundBrokenBuiltInFullScreen(bool fullScreen)
    {
        if (fullScreen)
        {
            Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
        }
        else
        {
            Screen.fullScreenMode = FullScreenMode.Windowed;
        }
    }

    public static bool LoadPref()
    {
        bool fullScreen = (
            PlayerPrefs.HasKey("fullScreen") ?
            PlayerPrefs.GetInt("fullScreen") > 0 :
            true
        );
        Screen.fullScreen = fullScreen;
        WorkAroundBrokenBuiltInFullScreen(fullScreen);
        return fullScreen;
    }

    public static void SavePref(bool whether)
    {
        PlayerPrefs.SetInt("fullScreen", whether ? 1 : 0);
        Screen.fullScreen = whether;
        WorkAroundBrokenBuiltInFullScreen(whether);
    }
}
