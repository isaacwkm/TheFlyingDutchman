using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ActiveItem)), CanEditMultipleObjects]
public class ActiveItemEditor : Editor
{
    private SerializedProperty itemIDPleaseDoNotChange;
    private SerializedProperty heldPositionOffset;
    private SerializedProperty heldRotationOffset;
    private SerializedProperty hasAttack;
    private SerializedProperty attackAnimName;

    private void OnEnable()
    {
        // Find all serialized properties
        itemIDPleaseDoNotChange = serializedObject.FindProperty("itemIDPleaseDoNotChange");
        heldPositionOffset = serializedObject.FindProperty("heldPositionOffset");
        heldRotationOffset = serializedObject.FindProperty("heldRotationOffset");
        hasAttack = serializedObject.FindProperty("hasAttack");
        attackAnimName = serializedObject.FindProperty("attackAnimName");
    }

    public override void OnInspectorGUI()
    {
        // Update the serialized object
        serializedObject.Update();

        // Draw fields using SerializedProperty
        EditorGUILayout.PropertyField(itemIDPleaseDoNotChange, new GUIContent("Item ID (Please do not change)"));
        EditorGUILayout.PropertyField(heldPositionOffset, new GUIContent("Held Position Offset"));
        EditorGUILayout.PropertyField(heldRotationOffset, new GUIContent("Held Rotation Offset"));

        EditorGUILayout.PropertyField(hasAttack, new GUIContent("Has Attack"));
        if (hasAttack.boolValue)
        {
            EditorGUILayout.PropertyField(attackAnimName, new GUIContent("Attack Anim Name"));
        }

        // Apply changes to the serialized object
        serializedObject.ApplyModifiedProperties();

        // Handle prefab modifications
        if (GUI.changed)
        {
            foreach (var targetObject in targets)
            {
                if (PrefabUtility.IsPartOfPrefabInstance(targetObject))
                {
                    PrefabUtility.RecordPrefabInstancePropertyModifications(targetObject);
                }
            }
        }
    }
}