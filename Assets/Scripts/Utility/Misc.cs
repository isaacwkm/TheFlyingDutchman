using UnityEngine;
using System;
using System.Reflection;

public static class Misc
{
    public static void Shuffle<T>(T[] items) {
        for (int i = 0; i < items.Length; i++) {
            int j = ((int) (UnityEngine.Random.value*items.Length))%items.Length;
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

    /* Copies all public non-inherited writable instance properties
     * from one object to another already existing object of the same type.
     * Excludes inherited properties.
     * To also transfer inherited properties,
     * call this method multiple times,
     * specializing on each ancestor type in succession
     * as the type parameter. */
    public static void TransferTypeProperties<T, U, V>(U src, V dst)
        where U : T where V : T
    {
        TransferTypeProperties(typeof(T), src, dst);
    }
    public static void TransferTypeProperties(Type type, object src, object dst)
    {
        foreach (var prop in type.GetProperties(
            BindingFlags.DeclaredOnly |
            BindingFlags.Instance |
            BindingFlags.Public
        )) {
            if (prop.CanRead && prop.CanWrite)
            {
                prop.SetValue(dst, prop.GetValue(src));
            }
        }
    }

    /* Creates a partial imitation of the given GameObject
     * that shares only a select subset of its components.
     * You *must not* use this method to copy any components
     * that hold public writable object references to child objects.
     * Builtin Instantiate would change those references in the copy
     * to point to the corresponding child objects in the copy.
     * **Imitate does not.** */
    public static GameObject Imitate(
        GameObject original,
        params Type[] componentTypes
    ) {
        var copy = new GameObject(original.name + " (imitation)");
        copy.transform.SetParent(original.transform.parent);
        copy.transform.localPosition = original.transform.localPosition;
        copy.transform.localRotation = original.transform.localRotation;
        copy.transform.localScale = original.transform.localScale;
        foreach (var componentType in componentTypes)
        {
            var origComponent = original.GetComponent(componentType);
            if (origComponent != null)
            {
                var copyComponent = copy.AddComponent(componentType);
                for (
                    var ancestor = componentType;
                    ancestor != null &&
                    ancestor != typeof(MonoBehaviour) &&
                    ancestor != typeof(Component);
                    ancestor = ancestor.BaseType
                ) {
                    TransferTypeProperties(
                        componentType, origComponent, copyComponent
                    );
                }
            }
        }
        for (int i = 0; i < original.transform.childCount; i++)
        {
            Imitate(original.transform.GetChild(i).gameObject, componentTypes)
                .transform.SetParent(copy.transform, false);
        }
        return copy;
    }
    public static T Imitate<T>(T original, params Type[] componentTypes)
        where T : Component
    {
        var actualComponentTypes = new Type[componentTypes.Length + 1];
        actualComponentTypes[0] = typeof(T);
        for (int i = 0; i < componentTypes.Length; i++)
        {
            actualComponentTypes[i + 1] = componentTypes[i];
        }
        return Imitate(original.gameObject, componentTypes).GetComponent<T>();
    }
}
