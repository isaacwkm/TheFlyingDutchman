using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Interactable)), CanEditMultipleObjects]
public class InteractableEditor : Editor
{
    public override void OnInspectorGUI()
    {
        Interactable myComponent = (Interactable)target;

        myComponent.actionTooltip = EditorGUILayout.TextField("Action Text", myComponent.actionTooltip);
        myComponent.interactSound = (ActionSound)EditorGUILayout.ObjectField("Interact Sound", myComponent.interactSound, typeof(ActionSound), true);

        myComponent.doCooldown = EditorGUILayout.Toggle("Do Cooldowns", myComponent.doCooldown);
        EditorGUI.BeginDisabledGroup(!myComponent.doCooldown);
        myComponent.interactCooldownSeconds = EditorGUILayout.FloatField("Interact Cooldown", myComponent.interactCooldownSeconds);
        myComponent.CooldownReturnSound = (ActionSound)EditorGUILayout.ObjectField("Cooldown Return Sound", myComponent.CooldownReturnSound, typeof(ActionSound), true);
        EditorGUI.EndDisabledGroup();
        
        myComponent.requirements = (InteractRequirements)EditorGUILayout.ObjectField("Interact Prerequisites", myComponent.requirements, typeof(InteractRequirements), true);

        EditorGUI.BeginDisabledGroup(myComponent.requirements == null);
        myComponent.requirementTooltipText = EditorGUILayout.TextField("Requirement Tooltip Text", myComponent.requirementTooltipText);
        EditorGUI.EndDisabledGroup();

        // Mark the object as dirty to ensure changes are tracked
        EditorUtility.SetDirty(myComponent);

        serializedObject.ApplyModifiedProperties();
    }
}
