using UnityEditor;
using UnityEngine;

namespace AllIn13DShader
{
	public class AtlasPackerTool
	{
		public int atlasXCount;
		public int atlasYCount;
		
		public TextureSizes atlasSizesX;
		public TextureSizes atlasSizesY;

		public FilterMode atlasFiltering;

		public bool squareAtlas;

		public Texture2D[] atlas;

		public Texture2D createdAtlas;

		public AtlasPackerTool()
		{
			atlasXCount = 1;
			atlasYCount = 1;

			atlasSizesX = TextureSizes._1024;
			atlasSizesY = TextureSizes._1024;

			squareAtlas = true;

			atlasFiltering = FilterMode.Bilinear;
		}

		public void CreateAtlas()
		{
			int atlasElements = atlasXCount * atlasYCount;
			int atlasWidth = (int)atlasSizesX;
			int atlasHeight = (int)atlasSizesY;

			Texture2D[] AtlasCopy = (Texture2D[])atlas.Clone();
			int textureXTargetWidth = atlasWidth / atlasXCount;
			int textureYTargetHeight = atlasHeight / atlasYCount;
			createdAtlas = new Texture2D(atlasWidth, atlasHeight);
			for (int i = 0; i < atlasYCount; i++)
			{
				for (int j = 0; j < atlasXCount; j++)
				{
					int currIndex = (i * atlasXCount) + j;
					bool hasImageForThisIndex = currIndex < AtlasCopy.Length && AtlasCopy[currIndex] != null;
					if (hasImageForThisIndex)
					{
						EditorUtils.SetTextureReadWrite(AssetDatabase.GetAssetPath(AtlasCopy[currIndex]), true);
						Texture2D copyTexture = new Texture2D(AtlasCopy[currIndex].width, AtlasCopy[currIndex].height);
						copyTexture.SetPixels(AtlasCopy[currIndex].GetPixels());
						copyTexture.Apply();
						AtlasCopy[currIndex] = copyTexture;
						AtlasCopy[currIndex] = EditorUtils.ScaleTexture(AtlasCopy[currIndex], textureXTargetWidth, textureYTargetHeight);
						AtlasCopy[currIndex].Apply();
					}

					for (int y = 0; y < textureYTargetHeight; y++)
					{
						for (int x = 0; x < textureXTargetWidth; x++)
						{
							if (hasImageForThisIndex) createdAtlas.SetPixel((j * textureXTargetWidth) + x, (i * textureYTargetHeight) + y, AtlasCopy[currIndex].GetPixel(x, y));
							else createdAtlas.SetPixel((j * textureXTargetWidth) + x, (i * textureYTargetHeight) + y, new Color(0, 0, 0, 1));
						}
					}
				}
			}

			createdAtlas.Apply();
		}

		public int GetAtlasElements()
		{
			int res = atlasXCount * atlasYCount;
			return res;
		}
	}
}