using UnityEngine;

namespace AllIn13DShader
{
	[ExecuteInEditMode]
	public class DepthColoringCamera : MonoBehaviour
	{
		[SerializeField] private Camera cam;
		[SerializeField] private AllIn1DepthColoringProperties allIn1DepthColoringProperties;

		[ContextMenu("Enable Depth Texture")]
		public void EnableDepthTexture()
		{
			cam.depthTextureMode = DepthTextureMode.Depth;
		}

#if UNITY_EDITOR
		private void Update()
		{
			Update_Editor();
		}

		private void Reset()
		{
			cam = GetComponent<Camera>();
		}

		private void Update_Editor()
		{
			if(cam != null)
			{
				cam.depthTextureMode = DepthTextureMode.Depth;
			}
		}

		private void OnDrawGizmosSelected()
		{
			if(cam == null || allIn1DepthColoringProperties == null) { return; }

			Vector3 position = Vector3.zero;
			position.z = cam.nearClipPlane + allIn1DepthColoringProperties.depthColoringMinDepth;

			float size = allIn1DepthColoringProperties.depthZoneLength;
			position.z += size * 0.5f;

			Color gizmosColor = Color.blue;
			gizmosColor.a = 0.25f;

			Gizmos.matrix = cam.transform.localToWorldMatrix;
			Gizmos.color = gizmosColor;

			float height = Mathf.Tan(0.5f * cam.fieldOfView * Mathf.Deg2Rad) * cam.farClipPlane * 2f;
			float width = height * cam.aspect;
			Gizmos.DrawCube(position, new Vector3(width, height, size));
		}
#endif
	}
}