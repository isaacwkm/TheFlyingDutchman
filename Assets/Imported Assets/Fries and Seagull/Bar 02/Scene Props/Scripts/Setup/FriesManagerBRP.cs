using System;
using System.Collections.Generic;
# if UNITY_EDITOR
using Seagull.Bar_02.Inspector;
using UnityEditor;
# endif
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;

namespace Seagull.Bar_02.SceneProps.Setup {
    public class FriesManagerBRP : MonoBehaviour {

        public static void setupLight() {
# if UNITY_EDITOR
            PostProcessVolume volume = GameObject.Find("Post Process Volume (Fries)").GetComponent<PostProcessVolume>();
            if (volume == null) {
                Debug.LogWarning("Could not find PostProcessVolume Component");
                return;
            }
            AmbientOcclusion ao;
            if (!volume.profile.TryGetSettings(out ao)) 
                ao = volume.profile.AddSettings<AmbientOcclusion>();
            ao.enabled.Override(true);  // 开启 AO 效果
            ao.mode.Override(AmbientOcclusionMode.ScalableAmbientObscurance);
            ao.intensity.Override(1.49f);
            ao.radius.Override(11.6f);
            ao.quality.Override(AmbientOcclusionQuality.Lowest);
            ao.color.Override(Color.black);
            ao.ambientOnly.Override(true);
            
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(48f/255f, 50f/255f, 152f/255f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(53f/255f, 17f/255f, 58f/255f);
            RenderSettings.fogStartDistance = 0f;
            RenderSettings.fogEndDistance = 316.7f;
# endif
        }
        public static void unsetLight() {
# if UNITY_EDITOR
            PostProcessVolume volume = GameObject.Find("Post Process Volume (Fries)").GetComponent<PostProcessVolume>();
            if (volume == null) {
                Debug.LogWarning("Could not find PostProcessVolume Component");
                return;
            }
            if (volume.profile.TryGetSettings<AmbientOcclusion>(out _)) 
                volume.profile.RemoveSettings<AmbientOcclusion>();
            
            RenderSettings.ambientMode = AmbientMode.Skybox;
            RenderSettings.ambientLight = new Color(54f/255f, 58f/255f, 66f/255f);
            RenderSettings.fog = false;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(128f/255f, 128f/255f, 128f/255f);
            RenderSettings.fogStartDistance = 0f;
            RenderSettings.fogEndDistance = 300f;
# endif
        }

        public GameObject postProcessVolumePrefab;
        
        [Tooltip("Post Process effect including glowing will only show to these cameras")]
        public List<Camera> gameCameras = new();
        
        [Tooltip("Which layer should Post Processor Volume use")]
        public int postProcessLayer = -1;
        
# if UNITY_EDITOR
        [AButton("Initialize")] [IgnoreInInspector]
        public Action initialize;

        private void Reset() {
            initialize = init;
        }

        private void init() {
            if (gameCameras == null || gameCameras.Count == 0) {
                Debug.LogError("Please provide at least 1 valid camera to Game Cameras field.");
                return;
            }
            
            if (string.IsNullOrEmpty(LayerMask.LayerToName(postProcessLayer))) {
                Debug.LogError("Please provide a valid layer ID in Post Process Layer field.");
                return;
            }
            
            setupPostProcessorVlume();
            
            foreach (var camera in gameCameras) {
                PostProcessLayer ppl = camera.GetComponent<PostProcessLayer>();
                if (ppl) {
                    ppl.volumeLayer |= LayerMask.GetMask(LayerMask.LayerToName(postProcessLayer));
                    continue;
                }
                
                ppl = camera.gameObject.AddComponent<PostProcessLayer>();
                ppl.volumeLayer = LayerMask.GetMask(LayerMask.LayerToName(postProcessLayer));
            }
            Debug.Log($"Init post-processor settings for Built-in Rendering Pipeline successfully.");
        }

        private void setupPostProcessorVlume() {
            // 检查现在有没有 Post Processor
            bool hasValidPostProcessor = false;
            GameObject globalPPVGobj = GameObject.Find("Post Process Volume (Fries)");
            PostProcessVolume globalPPV = null;
            if (globalPPVGobj != null) {
                globalPPV = globalPPVGobj.GetComponent<PostProcessVolume>();
                FriesPostProcessorIdentifier yppi = globalPPVGobj.GetComponent<FriesPostProcessorIdentifier>();
                if (yppi != null) hasValidPostProcessor = true;
            }

            // 如果没有，则创建 Yurei Post Process Volume
            if (!hasValidPostProcessor) {
                GameObject postProcessor = GameObject.Instantiate(postProcessVolumePrefab);
                postProcessor.layer = postProcessLayer;
                postProcessor.name = "Post Process Volume (Fries)";
            }
            // 如果有 则检查它的完整性
            else {
                globalPPVGobj.layer = postProcessLayer;
                
                if (globalPPV.sharedProfile == null) {
                    globalPPV.sharedProfile = ScriptableObject.CreateInstance<PostProcessProfile>();
                    // 保存资产到指定路径
                    #if UNITY_EDITOR
                    AssetDatabase.CreateAsset(globalPPV.sharedProfile, "Assets/Post Process Volume (Fries).asset");
                    AssetDatabase.SaveAssets();
                    #endif
                }

                if (globalPPV.sharedProfile.GetSetting<Bloom>() == null)
                    globalPPV.sharedProfile.AddSettings<Bloom>();
                
                Bloom b = globalPPV.sharedProfile.GetSetting<Bloom>();
                if (!b.active) b.active = true;
                if (!b.enabled) b.enabled.value = true;
                
                b.intensity.value = 14.5f;
                b.intensity.overrideState = true;
                b.threshold.value = 2f;
                b.threshold.overrideState = true;
            }
        }
# endif
    }
    
    #if UNITY_EDITOR
    [CustomEditor(typeof(FriesManagerBRP))]
    public class FriesInitializerInspector : AnInspector { }
    #endif
}