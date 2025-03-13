using UnityEngine;

public static class Misc
{
    public static void Shuffle<T>(T[] items) {
        for (int i = 0; i < items.Length; i++) {
            int j = ((int) (Random.value*items.Length))%items.Length;
            T juggle = items[j];
            items[j] = items[i];
            items[i] = juggle;
        }
    }

    public static void Quit()
    {
        // Taken from https://discussions.unity.com/t/start-stop-playmode-from-editor-script/27701
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #elif UNITY_WEBPLAYER
            Application.OpenURL("..");
        #else
            Application.Quit();
        #endif
    }
}
