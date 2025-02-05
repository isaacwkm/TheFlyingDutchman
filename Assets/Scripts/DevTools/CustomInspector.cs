using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Interactable))]
public class MyComponentEditor : Editor
{
    public override void OnInspectorGUI()
    {
        Interactable myComponent = (Interactable)target;

        myComponent.actionText = EditorGUILayout.TextField("Action Text", myComponent.actionText);
        myComponent.interactSound = (ActionSound)EditorGUILayout.ObjectField("Interact Sound", myComponent.interactSound, typeof(ActionSound), true);
        myComponent.doCooldown = EditorGUILayout.Toggle("Do Cooldowns", myComponent.doCooldown);

        EditorGUI.BeginDisabledGroup(!myComponent.doCooldown);
        myComponent.interactCooldownSeconds = EditorGUILayout.FloatField("Interact Cooldown", myComponent.interactCooldownSeconds);
        myComponent.CooldownReturnSound = (ActionSound)EditorGUILayout.ObjectField("Cooldown Return Sound", myComponent.CooldownReturnSound, typeof(ActionSound), true);
        EditorGUI.EndDisabledGroup();
        
        myComponent.requirements = (InteractRequirements)EditorGUILayout.ObjectField("Interact Prerequisites", myComponent.requirements, typeof(InteractRequirements), true);

        serializedObject.ApplyModifiedProperties();
    }
}
