using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ActiveItem)), CanEditMultipleObjects]
public class ActiveItemEditor : Editor
{
    public override void OnInspectorGUI()
    {
        ActiveItem myComponent = (ActiveItem)target;

        myComponent.itemIDPleaseDoNotChange = EditorGUILayout.IntField("Item ID (Please do not change)", myComponent.itemIDPleaseDoNotChange);
        myComponent.heldPositionOffset = EditorGUILayout.Vector3Field("Held Position Offset", myComponent.heldPositionOffset);
        myComponent.heldRotationOffset = EditorGUILayout.Vector3Field("Held Rotation Offset", myComponent.heldRotationOffset);

        myComponent.hasAttack = EditorGUILayout.Toggle("Has Attack", myComponent.hasAttack);
        EditorGUI.BeginDisabledGroup(!myComponent.hasAttack);
        myComponent.attackAnimation = (Animation)EditorGUILayout.ObjectField("Attack Animation", myComponent.attackAnimation, typeof(Animation), true);
        EditorGUI.EndDisabledGroup();

        serializedObject.ApplyModifiedProperties();
    }
}
