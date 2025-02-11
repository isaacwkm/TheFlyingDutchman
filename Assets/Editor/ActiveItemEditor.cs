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
        myComponent.handAnim = (Animator)EditorGUILayout.ObjectField("Hand Animator", myComponent.handAnim, typeof(Animator), true);

        myComponent.hasAttack = EditorGUILayout.Toggle("Has Attack", myComponent.hasAttack);
        EditorGUI.BeginDisabledGroup(!myComponent.hasAttack);
        myComponent.attackAnimName = EditorGUILayout.TextField("Attack Anim Name", myComponent.attackAnimName);
        EditorGUI.EndDisabledGroup();

        serializedObject.ApplyModifiedProperties();
    }
}
