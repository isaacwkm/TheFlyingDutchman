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
}
