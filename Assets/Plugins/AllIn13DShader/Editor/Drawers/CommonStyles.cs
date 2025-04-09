using UnityEditor;
using UnityEngine;

namespace AllIn13DShader
{
	public class CommonStyles
	{
		public const int BUTTON_WIDTH = 600;
		public const int BIG_FONT_SIZE = 16;

		public GUIStyle style;
		public GUIStyle bigLabel;

		public void InitStyles()
		{
			if(style == null)
			{
				style = new GUIStyle(EditorStyles.helpBox);
				style.margin = new RectOffset(0, 0, 0, 0);
			}

			if(bigLabel == null)
			{
				bigLabel = new GUIStyle(EditorStyles.boldLabel);
				bigLabel.fontSize = BIG_FONT_SIZE;
			}
		}
	}
}