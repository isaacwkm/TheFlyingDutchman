using UnityEditor;
using UnityEngine;

namespace AllIn13DShader
{
	public class AllIn13DShaderInspectorReferences
	{
		public MaterialProperty[] matProperties;
		public string[] oldKeyWords;

		public Material targetMat;
		public Material originalMaterialCopy;

		public MaterialEditor editorMat;

		//Styles
		private const int bigFontSize = 16, smallFontSize = 11;
		public GUIStyle propertiesStyle, bigLabelStyle, smallLabelStyle, toggleButtonStyle, tabButtonStyle;

		//Outline Effect
		public AllIn13DEffectConfig outlineEffectConfig;

		//Cast Shadows Effect
		public AllIn13DEffectConfig castShadowsEffectConfig;

		//Shaders
		public Shader shStandard;
		public Shader shStandardNoShadowCaster;
		public Shader shOutline;
		public Shader shOutlineNoShadowCaster;

		public AllIn13DShaderInspectorReferences()
		{
			propertiesStyle = new GUIStyle(EditorStyles.helpBox);
			propertiesStyle.margin = new RectOffset(0, 0, 0, 0);

			bigLabelStyle = new GUIStyle(EditorStyles.boldLabel);
			bigLabelStyle.fontSize = bigFontSize;

			smallLabelStyle = new GUIStyle(EditorStyles.boldLabel);
			smallLabelStyle.fontSize = smallFontSize;

			toggleButtonStyle = new GUIStyle(GUI.skin.button) { alignment = TextAnchor.MiddleCenter, richText = true };

			tabButtonStyle = new GUIStyle(GUI.skin.button) { fontSize = 10 };

			shStandard = Shader.Find("AllIn13DShader/AllIn13DShader");
			shOutline = Shader.Find("AllIn13DShader/AllIn13DShaderOutline");

			shStandardNoShadowCaster = Shader.Find("AllIn13DShader/AllIn13DShader_NoShadowCaster");
			shOutlineNoShadowCaster = Shader.Find("AllIn13DShader/AllIn13DShaderOutline_NoShadowCaster");
		}

		public void Setup(MaterialEditor materialEditor, MaterialProperty[] properties)
		{
			this.editorMat = materialEditor;
			this.targetMat = (Material)materialEditor.target;
			this.matProperties = properties;

			if(this.originalMaterialCopy == null)
			{
				this.originalMaterialCopy = new Material(targetMat.shader);
			}
		}

		public void SetOutlineEffect(PropertiesConfig propertiesConfig)
		{
			this.outlineEffectConfig = propertiesConfig.FindEffectConfigByID("OUTLINETYPE");
		}

		public void SetCastShadowsEffect(PropertiesConfig propertiesConfig)
		{
			this.castShadowsEffectConfig = propertiesConfig.FindEffectConfigByID("CAST_SHADOWS_ON");
		}
	}
}