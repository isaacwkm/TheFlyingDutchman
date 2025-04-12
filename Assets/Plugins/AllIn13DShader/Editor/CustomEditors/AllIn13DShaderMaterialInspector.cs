using UnityEditor;
using UnityEngine;

namespace AllIn13DShader
{
	[CanEditMultipleObjects]
	public class AllIn13DShaderMaterialInspector : ShaderGUI
	{
		private PropertiesConfigCollection propertiesConfigCollection;
		private PropertiesConfig currentPropertiesConfig;

		private AllIn13DShaderInspectorReferences inspectorReferences;
		private AbstractEffectDrawer[] drawers;
		private GlobalPropertiesDrawer globalPropertiesDrawer;
		private AdvancedPropertiesDrawer advancedPropertiesDrawer;

		private MaterialPresetCollection blendingModeCollection;

		private MaterialProperty matPropertyRenderPreset;
		private MaterialProperty matPropertyBlendSrc;
		private MaterialProperty matPropertyBlendDst;
		private MaterialProperty matPropertyZWrite;
		
		private int lastRenderQueue;
		private float lasTimeRebuilt;

		private void RefreshReferences(MaterialEditor materialEditor, MaterialProperty[] properties)
		{
			if (inspectorReferences == null)
			{
				inspectorReferences = new AllIn13DShaderInspectorReferences();
				inspectorReferences.Setup(materialEditor, properties);

				if (lastRenderQueue > 0)
				{
					inspectorReferences.targetMat.renderQueue = lastRenderQueue;
				}
			}

			if (propertiesConfigCollection == null)
			{
				string[] guids = AssetDatabase.FindAssets("PropertiesConfigCollection t:PropertiesConfigCollection");
				if(guids.Length == 0)
				{
					Debug.LogWarning("PropertiesConfigCollection not found in the project. Configuring...");
					this.propertiesConfigCollection = PropertiesConfigCreator.CreateConfig();
					Debug.LogWarning("AllIn13DShader configured");
				}
				else
				{
					string path = AssetDatabase.GUIDToAssetPath(guids[0]);
					propertiesConfigCollection = AssetDatabase.LoadAssetAtPath<PropertiesConfigCollection>(path);
				}

				RefreshPropertiesConfig();

				lasTimeRebuilt = (float)EditorApplication.timeSinceStartup;
			}

			if (blendingModeCollection == null)
			{
				blendingModeCollection = (MaterialPresetCollection)EditorUtils.FindAsset<ScriptableObject>("BlendingModeCollection");
			}

			CreateDrawers();

			matPropertyRenderPreset = inspectorReferences.matProperties[currentPropertiesConfig.renderPreset];
			matPropertyBlendSrc = inspectorReferences.matProperties[currentPropertiesConfig.blendSrcIdx];
			matPropertyBlendDst = inspectorReferences.matProperties[currentPropertiesConfig.blendDstIdx];
			matPropertyZWrite = inspectorReferences.matProperties[currentPropertiesConfig.zWriteIndex];

			//We ensure that data is refreshed. Sometimes objects are not null but we need to refresh the references
			inspectorReferences.Setup(materialEditor, properties);
			RefreshDrawers();
		}

		private void ResetReferences()
		{
			this.propertiesConfigCollection = null;
			this.currentPropertiesConfig = null;

			inspectorReferences = null;
			drawers = null;

			globalPropertiesDrawer = null;
			advancedPropertiesDrawer = null;
		}

		private void CreateDrawers()
		{
			if (drawers == null)
			{
				drawers = new AbstractEffectDrawer[0];

				GeneralEffectDrawer generalEffectDrawer = new GeneralEffectDrawer(inspectorReferences, currentPropertiesConfig);


				EffectProperty mainNormalMapProperty = currentPropertiesConfig.FindEffectProperty("NORMAL_MAP", "_NormalMap");

				TriplanarEffectDrawer triplanarEffectDrawer = new TriplanarEffectDrawer(mainNormalMapProperty, inspectorReferences, currentPropertiesConfig);
				ColorRampEffectDrawer colorRampEffectDrawer = new ColorRampEffectDrawer(inspectorReferences, currentPropertiesConfig);
				OutlineEffectDrawer outlineEffectDrawer = new OutlineEffectDrawer(inspectorReferences, currentPropertiesConfig);
				TextureBlendingEffectDrawer vertexColorEffectDrawer = new TextureBlendingEffectDrawer(mainNormalMapProperty, inspectorReferences, currentPropertiesConfig);

				ArrayUtility.Add(ref drawers, generalEffectDrawer);
				ArrayUtility.Add(ref drawers, triplanarEffectDrawer);
				ArrayUtility.Add(ref drawers, colorRampEffectDrawer);
				ArrayUtility.Add(ref drawers, outlineEffectDrawer);
				ArrayUtility.Add(ref drawers, vertexColorEffectDrawer);

				advancedPropertiesDrawer = new AdvancedPropertiesDrawer(currentPropertiesConfig.advancedProperties, currentPropertiesConfig.blendSrcIdx, currentPropertiesConfig.blendDstIdx, inspectorReferences);
			}

			if (globalPropertiesDrawer == null)
			{
				globalPropertiesDrawer = new GlobalPropertiesDrawer();
			}
		}

		private void RefreshDrawers()
		{
			for(int i = 0; i < drawers.Length; i++)
			{
				drawers[i].Refresh(inspectorReferences);
			}
		}

		private AbstractEffectDrawer FindEffectDrawerByID(string drawerID)
		{
			AbstractEffectDrawer res = null;

			for (int i = 0; i < drawers.Length; i++)
			{
				if (drawers[i].ID == drawerID)
				{
					res = drawers[i];
					break;
				}
			}

			return res;
		}

		public override void AssignNewShaderToMaterial(Material material, Shader oldShader, Shader newShader)
		{
			base.AssignNewShaderToMaterial(material, oldShader, newShader);
		}

		private void RefreshPropertiesConfig()
		{
			currentPropertiesConfig = propertiesConfigCollection.FindPropertiesConfigByShader(inspectorReferences.targetMat.shader);

			inspectorReferences.SetOutlineEffect(currentPropertiesConfig);
			inspectorReferences.SetCastShadowsEffect(currentPropertiesConfig);
		}

		public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
		{
			if (lasTimeRebuilt <= EditorPrefs.GetFloat(Constants.LAST_TIME_SHADER_PROPERTIES_REBUILT_KEY, float.MaxValue))
			{
				ResetReferences();
				lasTimeRebuilt = (float)EditorApplication.timeSinceStartup;
			}

			if (inspectorReferences != null && drawers != null)
			{
				inspectorReferences.Setup(materialEditor, properties);
			}

			RefreshReferences(materialEditor, properties);

			DrawPresetsTabs();
			DrawAdvancedProperties();
			DrawGlobalProperties();
			DrawEffects();

			lastRenderQueue = inspectorReferences.targetMat.renderQueue;


			CheckPasses();

			CheckShader();
		}

		private void CheckShader()
		{
			bool isOutline = AllIn13DEffectConfig.IsEffectEnabled(inspectorReferences.outlineEffectConfig, inspectorReferences);
			bool castShadowsEnabled = AllIn13DEffectConfig.IsEffectEnabled(inspectorReferences.castShadowsEffectConfig, inspectorReferences);

			bool shaderChanged = false;

			Shader oldShader = null;
			Shader newShader = null;
			Shader shaderToCompare = null;

			if (isOutline)
			{
				if (castShadowsEnabled)
				{
					shaderToCompare = inspectorReferences.shOutline;
				}
				else
				{
					shaderToCompare = inspectorReferences.shOutlineNoShadowCaster;
				}
			}
			else
			{
				if (castShadowsEnabled)
				{
					shaderToCompare = inspectorReferences.shStandard;
				}
				else
				{
					shaderToCompare = inspectorReferences.shStandardNoShadowCaster;
				}
			}

			if (inspectorReferences.targetMat.shader != shaderToCompare)
			{
				oldShader = inspectorReferences.targetMat.shader;
				newShader = shaderToCompare;

				shaderChanged = true;
			}

			if (shaderChanged)
			{
				inspectorReferences.targetMat.shader = newShader;
				ResetReferences();
			}
		}

		private bool CheckOutlineShader(ref bool shaderChanged)
		{
			bool res = false;

			bool outlineEnabled = AllIn13DEffectConfig.IsEffectEnabled(inspectorReferences.outlineEffectConfig, inspectorReferences);
			shaderChanged = false;

			Shader oldShader = null;
			Shader newShader = null;

			if (outlineEnabled)
			{
				if(inspectorReferences.targetMat.shader != inspectorReferences.shOutline)
				{
					oldShader = inspectorReferences.shStandard;
					newShader = inspectorReferences.shOutline;

					shaderChanged = true;
				}
			}
			else
			{
				if (inspectorReferences.targetMat.shader != inspectorReferences.shStandard)
				{
					oldShader = inspectorReferences.shOutline;
					newShader = inspectorReferences.shStandard;

					shaderChanged = true;
				}
			}

			if (shaderChanged)
			{
				inspectorReferences.targetMat.shader = newShader;
			}

			res = outlineEnabled;

			return res;
		}

		//TODO: Reuse code from CheckOutlineShader
		private void CheckCastShadows(bool isOutline, ref bool shaderChanged)
		{
			bool castShadowEnabled = AllIn13DEffectConfig.IsEffectEnabled(inspectorReferences.castShadowsEffectConfig, inspectorReferences);
			
			Shader referenceShaderNoShadowCaster = inspectorReferences.shStandardNoShadowCaster;
			if (isOutline)
			{
				referenceShaderNoShadowCaster = inspectorReferences.shOutlineNoShadowCaster;
			}

			Shader referenceShadowWithShadowCaster = inspectorReferences.shStandard;
			if (isOutline)
			{
				referenceShadowWithShadowCaster = inspectorReferences.shOutline;
			}

			Shader oldShader = null;
			Shader newShader = inspectorReferences.targetMat.shader;

			if (!castShadowEnabled)
			{
				if (inspectorReferences.targetMat.shader != referenceShaderNoShadowCaster)
				{
					oldShader = inspectorReferences.targetMat.shader;
					newShader = referenceShaderNoShadowCaster;

					shaderChanged = true;
				}
			}
			else
			{
				if (inspectorReferences.targetMat.shader != referenceShadowWithShadowCaster)
				{
					oldShader = inspectorReferences.targetMat.shader;
					newShader = referenceShadowWithShadowCaster;

					shaderChanged = true;
				}
			}

			if (shaderChanged)
			{
				inspectorReferences.targetMat.shader = newShader;
			}
		}

		private void CheckPasses()
		{
			if (inspectorReferences.targetMat.IsKeywordEnabled("_LIGHTMODEL_FASTLIGHTING") || inspectorReferences.targetMat.IsKeywordEnabled("_LIGHTMODEL_NONE"))
			{
				inspectorReferences.targetMat.SetShaderPassEnabled("ForwardAdd", false);
			}
			else
			{
				inspectorReferences.targetMat.SetShaderPassEnabled("ForwardAdd", true);
			}
		}

		private void DrawPresetsTabs()
		{
			EditorGUI.BeginChangeCheck();

			string[] texts = blendingModeCollection.CreateStringsArray();


			int presetIndex = (int)matPropertyRenderPreset.floatValue;
			if (presetIndex >= blendingModeCollection.presets.Length)
			{
				presetIndex = 1;
				matPropertyRenderPreset.floatValue = presetIndex;
			}

			BlendingMode previousPreset = blendingModeCollection[presetIndex];
			if(previousPreset == null)
			{
				previousPreset = blendingModeCollection[0];
			}

			int newIndex = (int)matPropertyRenderPreset.floatValue;
			newIndex = GUILayout.SelectionGrid(newIndex, texts, 3, inspectorReferences.tabButtonStyle);
			matPropertyRenderPreset.floatValue = newIndex;
			if (EditorGUI.EndChangeCheck())
			{
				BlendingMode selectedPreset = blendingModeCollection[newIndex];
				ApplyMaterialPreset(previousPreset, selectedPreset);
			}
		}

		private void DrawAdvancedProperties()
		{
			advancedPropertiesDrawer.Draw();
		}

		private void DrawGlobalProperties()
		{
			globalPropertiesDrawer.Draw(currentPropertiesConfig.singleProperties, inspectorReferences);
		}

		private void DrawEffects()
		{
			int globalEffectIndex = 0;
			for (int groupIdx = 0; groupIdx < currentPropertiesConfig.effectsGroups.Length; groupIdx++)
			{
				EffectGroup effectGroup = currentPropertiesConfig.effectsGroups[groupIdx];
				if (effectGroup.effects.Length <= 0) { continue; }

				EditorGUILayout.Separator();
				EditorUtils.DrawLine(Color.grey, 1, 3);
				GUILayout.Label(effectGroup.DisplayName, inspectorReferences.bigLabelStyle);

				for (int effectIdx = 0; effectIdx < effectGroup.effects.Length; effectIdx++)
				{
					AllIn13DEffectConfig effectConfig = effectGroup.effects[effectIdx];

					globalEffectIndex++;

					AbstractEffectDrawer drawer = FindEffectDrawerByID(effectConfig.effectDrawerID);
					drawer.Draw(currentPropertiesConfig, effectConfig, globalEffectIndex);
				}
			}
		}

		private void ApplyMaterialPreset(BlendingMode previousPresset, BlendingMode newPreset)
		{
			matPropertyBlendSrc.floatValue = (float)newPreset.blendSrc;
			matPropertyBlendDst.floatValue = (float)newPreset.blendDst;
			matPropertyZWrite.floatValue = newPreset.depthWrite ? 1.0f : 0.0f;


			lastRenderQueue = (int)newPreset.renderQueue;
			inspectorReferences.targetMat.renderQueue = lastRenderQueue;

			if (previousPresset != newPreset && previousPresset.defaultEnabledEffects != null)
			{
				for (int i = 0; i < previousPresset.defaultEnabledEffects.Length; i++)
				{
					string effectID = previousPresset.defaultEnabledEffects[i];
					AllIn13DEffectConfig effectConfig = currentPropertiesConfig.FindEffectConfigByID(effectID);

					AllIn13DEffectConfig.DisableEffectToggle(effectConfig, inspectorReferences);
				}
			}

			if (newPreset.defaultEnabledEffects != null)
			{
				for (int i = 0; i < newPreset.defaultEnabledEffects.Length; i++)
				{
					string effectID = newPreset.defaultEnabledEffects[i];
					AllIn13DEffectConfig effectConfig = currentPropertiesConfig.FindEffectConfigByID(effectID);

					AllIn13DEffectConfig.EnableEffectToggle(effectConfig, inspectorReferences);
				}
			}
		}
	}
}