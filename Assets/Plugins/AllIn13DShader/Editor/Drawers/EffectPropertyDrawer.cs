using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace AllIn13DShader
{
	public static class EffectPropertyDrawer
	{
		public static void DrawMainProperty(int globalEffectIndex, AllIn13DEffectConfig effectConfig, AllIn13DShaderInspectorReferences references)
		{
			EditorGUILayout.BeginHorizontal();

			string label = $"{globalEffectIndex}. {effectConfig.displayName}";
			
			switch (effectConfig.effectConfigType)
			{
				case EffectConfigType.EFFECT_TOGGLE:
					DrawMainPropertyToggle(label, effectConfig, references);
					break;
				case EffectConfigType.EFFECT_ENUM:
					DrawMainPropertyEnum(label, effectConfig, references);
					break;
			}

			EditorGUILayout.EndHorizontal();
		}

		public static void DrawMainPropertyToggle(string label, AllIn13DEffectConfig effectConfig, AllIn13DShaderInspectorReferences references)
		{
			bool isEffectEnabled = AllIn13DEffectConfig.IsEffectEnabled(effectConfig, references);
			
			EditorGUI.BeginChangeCheck();

			string tooltip = effectConfig.keywords[0].keyword + " (C#)";
			GUIContent guiContent = new GUIContent(label, tooltip);
			isEffectEnabled = GUILayout.Toggle(isEffectEnabled, guiContent);
			if (EditorGUI.EndChangeCheck())
			{
				if (isEffectEnabled)
				{
					AllIn13DEffectConfig.EnableEffect(effectConfig, references);
				}
				else
				{
					AllIn13DEffectConfig.DisableEffect(effectConfig, references);
				}

				references.matProperties[effectConfig.keywordPropertyIndex].floatValue = isEffectEnabled ? 1f : 0f;

				EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
				EditorUtility.SetDirty(references.targetMat);
			}
		}

		public static void DrawMainPropertyEnum(string label, AllIn13DEffectConfig effectConfig, AllIn13DShaderInspectorReferences references)
		{
			int selectedIndex = 0;
			bool isEffectEnabled = AllIn13DEffectConfig.IsEffectEnabled(effectConfig, ref selectedIndex, references);

			EditorGUI.BeginChangeCheck();

			string tooltip = effectConfig.keywords[selectedIndex].keyword + " (C#)";
			GUIContent guiContent = new GUIContent(label, tooltip);
			selectedIndex = EditorGUILayout.Popup(guiContent, selectedIndex, effectConfig.keywordsDisplayNames);

			if (EditorGUI.EndChangeCheck())
			{
				if(selectedIndex >= 0)
				{
					AllIn13DEffectConfig.EnableEffectByIndex(effectConfig, selectedIndex, references);
				}
				else
				{
					AllIn13DEffectConfig.DisableEffect(effectConfig, references);
				}

				references.matProperties[effectConfig.keywordPropertyIndex].floatValue = selectedIndex;

				EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
				EditorUtility.SetDirty(references.targetMat);
			}
		}

		public static void DrawProperty(int propertyIndex, string labelPrefix, bool allowReset, AllIn13DShaderInspectorReferences references)
		{
			MaterialProperty targetProperty = references.matProperties[propertyIndex];
			DrawProperty(targetProperty, labelPrefix, allowReset, references);
		}

		public static void DrawProperty(EffectProperty effectProperty, string labelPrefix, bool allowReset, AllIn13DShaderInspectorReferences references)
		{
			DrawProperty(effectProperty.propertyIndex, labelPrefix, effectProperty.allowReset, references);
		}

		public static void DrawProperty(EffectProperty effectProperty, AllIn13DShaderInspectorReferences references)
		{
			DrawProperty(effectProperty.propertyIndex, references);
		}

		public static void DrawProperty(int propertyIndex, AllIn13DShaderInspectorReferences references)
		{
			DrawProperty(propertyIndex, string.Empty, true, references);
		}

		public static void DrawProperty(MaterialProperty materialProperty, AllIn13DShaderInspectorReferences references)
		{
			DrawProperty(materialProperty, string.Empty, true, references);
		}

		public static void DrawProperty(MaterialProperty materialProperty, bool allowReset, AllIn13DShaderInspectorReferences references)
		{
			DrawProperty(materialProperty, string.Empty, allowReset, references);
		}

		public static void DrawProperty(MaterialProperty materialProperty, string labelPrefix, bool allowReset, AllIn13DShaderInspectorReferences references)
		{
			DrawProperty(
				materialProperty: materialProperty, 
				labelPrefix: labelPrefix, 
				displayName: materialProperty.displayName,
				allowReset: allowReset, 
				references: references);
		}

		public static void DrawProperty(MaterialProperty materialProperty, string labelPrefix, string displayName, bool allowReset, AllIn13DShaderInspectorReferences references)
		{
			string label = $"{labelPrefix} {displayName}";
			string tooltip = materialProperty.name + "(C#)";


			EditorGUILayout.BeginHorizontal();

			DrawProperty(materialProperty, label, tooltip, references);
			if (allowReset)
			{
				DrawResetButton(materialProperty, references);
			}

			EditorGUILayout.EndHorizontal();
		}

		public static void DrawProperty(MaterialProperty targetProperty, string label, string tooltip, AllIn13DShaderInspectorReferences references)
		{
			GUIContent propertyLabel = new GUIContent();
			propertyLabel.text = label;
			propertyLabel.tooltip = tooltip;

			references.editorMat.ShaderProperty(targetProperty, propertyLabel);
		}

		public static void DrawResetButton(MaterialProperty targetProperty, AllIn13DShaderInspectorReferences references)
		{
			GUIContent resetButtonLabel = new GUIContent();
			resetButtonLabel.text = "R";
			resetButtonLabel.tooltip = "Resets to default value";
			if (GUILayout.Button(resetButtonLabel, GUILayout.Width(20)))
			{
				AllIn13DEffectConfig.ResetProperty(targetProperty, references);
			}
		}
	}
}