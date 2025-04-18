// SC Post Effects
// Staggart Creations
// http://staggart.xyz

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace SCPE
{
    public class LUTExtracter : Editor
    {
        //Cached in- and outputs
        public static string InputPath
        {
            get { return EditorPrefs.GetString("LUT_INPUT_PATH", ""); }
            set { EditorPrefs.SetString("LUT_INPUT_PATH", value); }
        }

        public static string OutputPath
        {
            get { return EditorPrefs.GetString("LUT_OUTPUT_PATH", ""); }
            set { EditorPrefs.SetString("LUT_OUTPUT_PATH", value); }
        }

        public static void ExtractLUT(Texture2D screenshot, Texture2D targetLUT)
        {
            if (!screenshot || !targetLUT) return;
            
            Color[] texels = screenshot.GetPixels(0, 0, targetLUT.width, targetLUT.height);

            //Create new LUT
            Texture2D newLUT = new Texture2D(targetLUT.width, targetLUT.height, TextureFormat.RGBA32, false, true);

            newLUT.SetPixels(texels);
            newLUT.Apply();

            byte[] bytes = newLUT.EncodeToPNG();

            //Save new LUT
            string filePath = AssetDatabase.GetAssetPath(targetLUT);
            System.IO.File.WriteAllBytes(filePath, bytes);

            //AssetDatabase.Refresh();
            //AssetDatabase.SaveAssets();
            newLUT = (Texture2D)AssetDatabase.LoadAssetAtPath(filePath, typeof(Texture2D));
            EditorUtility.CopySerialized(newLUT, targetLUT);
            AssetDatabase.ImportAsset(filePath, ImportAssetOptions.ForceUpdate);
        }
    }
}