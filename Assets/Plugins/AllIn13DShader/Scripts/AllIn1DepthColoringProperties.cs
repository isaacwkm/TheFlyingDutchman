using UnityEngine;

namespace AllIn13DShader
{
	[CreateAssetMenu(menuName = "AllIn13DShader/Others/Depth Coloring Properties", fileName = "DepthColoringProperties.asset")]
	public class AllIn1DepthColoringProperties : ScriptableObject
	{
		public float depthColoringMinDepth;
		public float depthZoneLength;
		public float fallOff;
		public Texture2D depthColoringGradientTex;
		
		private readonly int globalMinDepth = Shader.PropertyToID("global_MinDepth");
		private readonly int globalDepthZoneLength = Shader.PropertyToID("global_DepthZoneLength");
		private readonly int globalDepthGradient = Shader.PropertyToID("global_DepthGradient");
		private readonly int globalDepthGradientFallOff = Shader.PropertyToID("global_DepthGradientFallOff");

		private void OnEnable()
		{
			ApplyValues();
		}

		public void ApplyValues()
		{
			ApplyValues(depthColoringGradientTex);
		}

		public void ApplyValues(Texture gradientTex)
		{
			Shader.SetGlobalFloat(globalMinDepth, depthColoringMinDepth);
			Shader.SetGlobalFloat(globalDepthZoneLength, depthZoneLength);
			Shader.SetGlobalFloat(globalDepthGradientFallOff, fallOff);
			Shader.SetGlobalTexture(globalDepthGradient, gradientTex);
		}
	}
}