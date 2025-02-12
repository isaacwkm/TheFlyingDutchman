using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Interactable)), CanEditMultipleObjects]
public class InteractableEditor : Editor
{
    private SerializedProperty actionTooltip;
    private SerializedProperty interactSound;
    private SerializedProperty doCooldown;
    private SerializedProperty interactCooldownSeconds;
    private SerializedProperty CooldownReturnSound;
    private SerializedProperty requirements;
    private SerializedProperty requirementTooltipText;

    private void OnEnable()
    {
        actionTooltip = serializedObject.FindProperty("actionTooltip");
        interactSound = serializedObject.FindProperty("interactSound");
        doCooldown = serializedObject.FindProperty("doCooldown");
        interactCooldownSeconds = serializedObject.FindProperty("interactCooldownSeconds");
        CooldownReturnSound = serializedObject.FindProperty("CooldownReturnSound");
        requirements = serializedObject.FindProperty("requirements");
        requirementTooltipText = serializedObject.FindProperty("requirementTooltipText");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(actionTooltip, new GUIContent("Action Text"));
        EditorGUILayout.PropertyField(interactSound, new GUIContent("Interact Sound"));

        EditorGUILayout.PropertyField(doCooldown, new GUIContent("Do Cooldowns"));
        if (doCooldown.boolValue)
        {
            EditorGUILayout.PropertyField(interactCooldownSeconds, new GUIContent("Interact Cooldown"));
            EditorGUILayout.PropertyField(CooldownReturnSound, new GUIContent("Cooldown Return Sound"));
        }

        EditorGUILayout.PropertyField(requirements, new GUIContent("Interact Prerequisites"));
        if (requirements.objectReferenceValue != null)
        {
            EditorGUILayout.PropertyField(requirementTooltipText, new GUIContent("Requirement Tooltip Text"));
        }

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