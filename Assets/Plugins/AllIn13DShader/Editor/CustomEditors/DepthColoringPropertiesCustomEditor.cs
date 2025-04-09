using UnityEditor;
using UnityEngine;

namespace AllIn13DShader
{
	[CustomEditor(typeof(AllIn1DepthColoringProperties))]
	public class DepthColoringPropertiesCustomEditor : Editor
	{
		private SerializedProperty spDepthColoringMinDepth;
		private SerializedProperty spDepthZoneLength;
		private SerializedProperty spFallOff;
		private SerializedProperty spDepthColoringGradientTex;

		private Texture gradientTex;

		private bool createGradientToggle;

		private GradientEditorDrawer gradientEditorDrawer;

		private AllIn1DepthColoringProperties depthColoringProperties;

		private void RefreshRerences()
		{
			if(spDepthColoringMinDepth == null)
			{
				spDepthColoringMinDepth = serializedObject.FindProperty("depthColoringMinDepth");
			}

			if(spDepthZoneLength == null)
			{
				spDepthZoneLength = serializedObject.FindProperty("depthZoneLength");
			}

			if(spDepthColoringGradientTex == null)
			{
				spDepthColoringGradientTex = serializedObject.FindProperty("depthColoringGradientTex");
			}

			if (spFallOff == null)
			{
				spFallOff = serializedObject.FindProperty("fallOff");
			}

			if(gradientEditorDrawer == null)
			{
				gradientEditorDrawer = new GradientEditorDrawer();
			}

			depthColoringProperties = (AllIn1DepthColoringProperties)target;
		}

		public override void OnInspectorGUI()
		{
			RefreshRerences();

			serializedObject.Update();

			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(spDepthColoringMinDepth);
			EditorGUILayout.PropertyField(spDepthZoneLength);
			EditorGUILayout.PropertyField(spFallOff);

			Rect rect = EditorGUILayout.GetControlRect(true, 500f);


			if(spDepthColoringGradientTex.objectReferenceValue != null)
			{
				gradientTex = (Texture)spDepthColoringGradientTex.objectReferenceValue;
			}

			gradientTex = gradientEditorDrawer.Draw(rect, gradientTex);
			spDepthColoringGradientTex.objectReferenceValue = gradientTex;

			bool changes = EditorGUI.EndChangeCheck();

			serializedObject.ApplyModifiedProperties();

			if (changes)
			{
				Texture gradientTexToApply = spDepthColoringGradientTex.objectReferenceValue == null ? gradientTex : (Texture)spDepthColoringGradientTex.objectReferenceValue;
				depthColoringProperties.ApplyValues(gradientTexToApply);
			}
		}

		private void GradientChangedCallback()
		{
			depthColoringProperties.ApplyValues();
		}
	}
}