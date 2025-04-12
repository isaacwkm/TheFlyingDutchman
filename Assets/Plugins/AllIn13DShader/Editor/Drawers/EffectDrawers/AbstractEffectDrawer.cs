using System.Linq;
using UnityEditor;
using UnityEngine;

namespace AllIn13DShader
{
	public abstract class AbstractEffectDrawer
	{
		private const float HEIGHT_PER_LINE = 12.5f;

		protected string drawerID;
		protected PropertiesConfig propertiesConfig;
		protected AllIn13DEffectConfig effectConfig;
		protected AllIn13DShaderInspectorReferences references;
		protected int globalEffectIndex;

		public string ID
		{
			get
			{
				return drawerID;
			}
		}

		public AbstractEffectDrawer(AllIn13DShaderInspectorReferences references, PropertiesConfig propertiesConfig)
		{
			this.references = references;
			this.propertiesConfig = propertiesConfig;
		}

		public void Draw(PropertiesConfig propertiesConfig, AllIn13DEffectConfig effectConfig, int globalEffectIndex)
		{
			this.propertiesConfig = propertiesConfig;
			this.effectConfig = effectConfig;
			this.globalEffectIndex = globalEffectIndex;

			Draw();
		}

		protected virtual void Draw()
		{
			bool areDependenciesMet = AreDependenciesMet();

			EditorGUI.BeginDisabledGroup(!areDependenciesMet);
			EffectPropertyDrawer.DrawMainProperty(globalEffectIndex, effectConfig, references);
			bool isAnyPropertyVisible = IsAnyPropertyVisible();
			
			if (isAnyPropertyVisible)
			{
				EditorGUILayout.BeginVertical(references.propertiesStyle);
				DrawExtraData();
				DrawProperties();
				EditorGUILayout.EndVertical();
			}
			EditorGUI.EndDisabledGroup();
		}

		private void DrawExtraData()
		{
			if (IsParentPropertyEnabled())
			{
				string customMessage = effectConfig.GetCustomMessage(references.targetMat);
				if (!string.IsNullOrEmpty(customMessage))
				{
					EditorGUILayout.BeginHorizontal();

					int numLines = EditorUtils.GetNumLines(customMessage);
					float heightField = numLines * HEIGHT_PER_LINE;

					if (!string.IsNullOrEmpty(effectConfig.docURL))
					{
						if (GUILayout.Button("?", GUILayout.Width(heightField), GUILayout.Height(heightField)))
						{
							Application.OpenURL(effectConfig.docURL);
						}
					}


					EditorGUILayout.LabelField(customMessage, references.smallLabelStyle, GUILayout.Height(heightField));
					EditorGUILayout.EndHorizontal();
				}
			}
		}

		protected virtual void DrawProperties()
		{
			for (int i = 0; i < effectConfig.effectProperties.Count; i++)
			{
				EffectProperty effectProperty = effectConfig.effectProperties[i];
				DrawProperty(effectProperty, string.Empty, true);
			}
		}

		protected virtual void DrawProperty(EffectProperty effectProperty)
		{
			DrawProperty(effectProperty, true);
		}

		protected virtual void DrawProperty(EffectProperty effectProperty, bool allowReset)
		{
			DrawProperty(effectProperty, string.Empty, allowReset);
		}

		protected virtual void DrawProperty(EffectProperty effectProperty, string labelPrefix, bool allowReset)
		{
			if (IsEffectPropertyVisible(effectProperty))
			{
				EffectPropertyDrawer.DrawProperty(effectProperty, labelPrefix, allowReset, references);
			}
		}

		protected bool IsParentPropertyEnabled()
		{
			bool res = false;

			for (int i = 0; i < effectConfig.keywords.Count; i++)
			{
				string kw = effectConfig.keywords[i].keyword;
				if (references.targetMat.shaderKeywords.Contains(kw))
				{
					res = true;
					break;
				}
			}

			return res;
		}

		protected bool IsAnyPropertyVisible()
		{
			bool res = false;

			bool parentPropertyEnabled = IsParentPropertyEnabled();
			if (parentPropertyEnabled)
			{
				for (int propIdx = 0; propIdx < effectConfig.effectProperties.Count; propIdx++)
				{
					EffectProperty effectProperty = effectConfig.effectProperties[propIdx];

					res = IsEffectPropertyVisible(effectProperty);
					if (res)
					{
						break;
					}
				}
			}

			res = res || (parentPropertyEnabled && !string.IsNullOrEmpty(effectConfig.GetCustomMessage(references.targetMat)));

			return res;
		}

		protected bool IsEffectPropertyVisible(EffectProperty effectProperty)
		{
			bool res = false;

			bool anyIncompatibilities = false;
			for (int i = 0; i < effectProperty.incompatibleKeywords.Count; i++)
			{
				string incompatibleKw = effectProperty.incompatibleKeywords[i];
				if (references.targetMat.shaderKeywords.Contains(incompatibleKw))
				{
					anyIncompatibilities = true;
					break;
				}
			}

			if (!anyIncompatibilities)
			{
				if (effectProperty.keywordsOp == KeywordsOp.OR)
				{
					for (int i = 0; i < effectProperty.keywords.Count; i++)
					{
						string keyword = effectProperty.keywords[i];
						if (references.targetMat.shaderKeywords.Contains(keyword))
						{
							res = true;
							break;
						}
					}
				}
				else
				{
					res = true;
					for (int i = 0; i < effectProperty.keywords.Count; i++)
					{
						string keyword = effectProperty.keywords[i];
						if (!references.targetMat.shaderKeywords.Contains(keyword))
						{
							res = false;
							break;
						}
					}
				}
			}

			return res;
		}

		protected MaterialProperty FindPropertyByName(string propertyName)
		{
			MaterialProperty res = null;

			for (int i = 0; i < references.matProperties.Length; i++)
			{
				if (references.matProperties[i].name == propertyName)
				{
					res = references.matProperties[i];
					break;
				}
			}

			return res;
		}

		protected int FindPropertyIndex(string propertyName)
		{
			int res = -1;

			for (int i = 0; i < references.matProperties.Length; i++)
			{
				if (references.matProperties[i].name == propertyName)
				{
					res = i;
					break;
				}
			}

			return res;
		}

		protected bool AreDependenciesMet()
		{
			bool res = true;

			if (!string.IsNullOrEmpty(effectConfig.dependentOnEffect))
			{
				AllIn13DEffectConfig dependentEffect = propertiesConfig.FindEffectConfigByID(effectConfig.dependentOnEffect);
				res = res && AllIn13DEffectConfig.IsEffectEnabled(dependentEffect, references);
			}

			if (!string.IsNullOrEmpty(effectConfig.incompatibleWithEffectID))
			{
				AllIn13DEffectConfig dependentEffect = propertiesConfig.FindEffectConfigByID(effectConfig.incompatibleWithEffectID);
				res = res && !AllIn13DEffectConfig.IsEffectEnabled(dependentEffect, references);
			}

			return res;
		}

		public virtual void Refresh(AllIn13DShaderInspectorReferences references)
		{
			this.references = references;
		}
	}
}