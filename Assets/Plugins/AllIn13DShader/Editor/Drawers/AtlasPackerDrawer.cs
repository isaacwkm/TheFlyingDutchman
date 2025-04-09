using System.IO;
using UnityEditor;
using UnityEngine;

namespace AllIn13DShader
{
	public class AtlasPackerDrawer : ScriptableObject
	{
		public Texture2D[] Atlas;
		private AtlasPackerTool atlasPackerTool;
		private CommonStyles commonStyles;

		private bool SquareAtlas
		{
			get
			{
				return atlasPackerTool.squareAtlas;
			}
			set
			{
				atlasPackerTool.squareAtlas = value;
			}
		}

		private int AtlasXCount
		{
			get
			{
				return atlasPackerTool.atlasXCount;
			}
			set
			{
				atlasPackerTool.atlasXCount = value;
			}
		}

		private int AtlasYCount
		{
			get
			{
				return atlasPackerTool.atlasYCount;
			}
			set
			{
				atlasPackerTool.atlasYCount = value;
			}
		}

		private FilterMode AtlasFiltering
		{
			get
			{
				return atlasPackerTool.atlasFiltering;
			}
			set
			{
				atlasPackerTool.atlasFiltering = value;
			}
		}

		private TextureSizes AtlasSizesX
		{
			get
			{
				return atlasPackerTool.atlasSizesX;
			}
			set
			{
				atlasPackerTool.atlasSizesX = value;
			}
		}

		private TextureSizes AtlasSizesY
		{
			get
			{
				return atlasPackerTool.atlasSizesY;
			}
			set
			{
				atlasPackerTool.atlasSizesY = value;
			}
		}

		public void Setup(AtlasPackerTool atlasPackerTool, CommonStyles commonStyles)
		{
			this.atlasPackerTool = atlasPackerTool;
			this.commonStyles = commonStyles;
			Atlas = new Texture2D[0];
		}

		public void Draw()
		{
			GUILayout.Label("Texture Atlas / Spritesheet Packer", commonStyles.bigLabel);
			GUILayout.Space(20);
			GUILayout.Label("Add Textures to the Atlas array", EditorStyles.boldLabel);

			SerializedObject so = new SerializedObject(this);
			SerializedProperty atlasProperty = so.FindProperty("Atlas");
			EditorGUILayout.PropertyField(atlasProperty, true, GUILayout.MaxWidth(200));
			so.ApplyModifiedProperties();

			atlasPackerTool.atlas = Atlas;

			SquareAtlas = EditorGUILayout.Toggle("Square Atlas?", SquareAtlas, GUILayout.MaxWidth(200));
			EditorGUILayout.BeginHorizontal();
			{
				if (SquareAtlas)
				{
					AtlasXCount = EditorGUILayout.IntSlider("Column and Row Count", AtlasXCount, 1, 8, GUILayout.MaxWidth(302));
					AtlasYCount = AtlasXCount;
				}
				else
				{
					AtlasXCount = EditorGUILayout.IntSlider("Column Count", AtlasXCount, 1, 8, GUILayout.MaxWidth(302));
					GUILayout.Space(10);
					AtlasYCount = EditorGUILayout.IntSlider("Row Count", AtlasYCount, 1, 8, GUILayout.MaxWidth(302));
				}
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			{
				if (SquareAtlas)
				{
					GUILayout.Label("Atlas Size:", GUILayout.MaxWidth(100));
					AtlasSizesX = (TextureSizes)EditorGUILayout.EnumPopup(AtlasSizesX, GUILayout.MaxWidth(200));
					AtlasSizesY = AtlasSizesX;
				}
				else
				{
					GUILayout.Label("Atlas Size X:", GUILayout.MaxWidth(100));
					AtlasSizesX = (TextureSizes)EditorGUILayout.EnumPopup(AtlasSizesX, GUILayout.MaxWidth(200));
					GUILayout.Space(10);
					GUILayout.Label("Atlas Size Y:", GUILayout.MaxWidth(100));
					AtlasSizesY = (TextureSizes)EditorGUILayout.EnumPopup(AtlasSizesY, GUILayout.MaxWidth(200));
				}
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal();
			{
				GUILayout.Label("Atlas Filtering: ", GUILayout.MaxWidth(100));
				AtlasFiltering = (FilterMode)EditorGUILayout.EnumPopup(AtlasFiltering, GUILayout.MaxWidth(200));
			}
			EditorGUILayout.EndHorizontal();

			int atlasElements = atlasPackerTool.GetAtlasElements();
			int atlasWidth = (int)AtlasSizesX;
			int atlasHeight = (int)AtlasSizesY;
			GUILayout.Label("Output will be a " + AtlasXCount + " X " + AtlasYCount + " atlas, " + atlasElements + " elements in total. In a " +
							atlasWidth + "pixels X " + atlasHeight + "pixels texture", EditorStyles.boldLabel);

			int usedAtlasSlots = 0;
			for (int i = 0; i < atlasPackerTool.atlas.Length; i++)
			{
				if (atlasPackerTool.atlas[i] != null)
				{
					usedAtlasSlots++;
				}
			}
			if (usedAtlasSlots > atlasElements)
			{
				GUILayout.Label("*Please reduce the Atlas texture slots by " + Mathf.Abs(atlasElements - atlasPackerTool.atlas.Length) + " (extra textures will be ignored)", EditorStyles.boldLabel);
			}

			if (atlasElements > usedAtlasSlots)
			{
				GUILayout.Label("*" + (atlasElements - usedAtlasSlots) + " atlas slots unused or null (it will be filled with black)", EditorStyles.boldLabel);
			}

			GUILayout.Space(20);
			GUILayout.Label("Select the folder where new Atlases will be saved", EditorStyles.boldLabel);
			GlobalConfiguration.instance.AtlasesSavePath = EditorUtils.DrawSelectorFolder(GlobalConfiguration.instance.AtlasesSavePath, /*AllIn13DShaderConfig.ATLASES_SAVE_PATH_DEFAULT,*/ "Atlas");


			if (Directory.Exists(GlobalConfiguration.instance.AtlasesSavePath))
			{
				if (GUILayout.Button("Create And Save Atlas Texture", GUILayout.MaxWidth(CommonStyles.BUTTON_WIDTH)))
				{
					atlasPackerTool.CreateAtlas();
					EditorUtils.SaveTextureAsPNG(GlobalConfiguration.instance.AtlasesSavePath, "NormalMap", "Normal Map", atlasPackerTool.createdAtlas, AtlasFiltering, TextureImporterType.Default, TextureWrapMode.Clamp);
				}
			}
		}
	}
}