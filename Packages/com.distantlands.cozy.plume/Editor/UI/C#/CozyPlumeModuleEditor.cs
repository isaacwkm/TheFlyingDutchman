using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Linq;
using System.Collections.Generic;
using DistantLands.Cozy.Data;

namespace DistantLands.Cozy.EditorScripts
{
    [CustomEditor(typeof(PlumeModule))]
    public class CozyPlumeModuleEditor : CozyModuleEditor
    {

        PlumeModule module;
        public override ModuleCategory Category => ModuleCategory.atmosphere;
        public override string ModuleTitle => "Plume";
        public override string ModuleSubtitle => "Volumetric Clouds Module";
        public override string ModuleTooltip => "Manage the Plume volumetric cloud system.";

        public VisualElement SelectionContainer => root.Q<VisualElement>("selection-container");
        public VisualElement ProfileContainer => root.Q<VisualElement>("profile-container");
        public VisualElement GraphContainer => root.Q<VisualElement>("plume-graph-container");

        public Button RegenClouds => root.Q<Button>("regen-clouds");

        Button widget;
        VisualElement root;

        void OnEnable()
        {
            if (!target)
                return;

            module = (PlumeModule)target;
        }

        public override Button DisplayWidget()
        {
            widget = SmallWidget();
            Label status = widget.Q<Label>("dynamic-status");
            status.style.fontSize = 8;
            status.text = module.volumetricCloudProfile.name.Split(' ')[0];

            return widget;

        }

        public override VisualElement DisplayUI()
        {
            root = new VisualElement();

            VisualTreeAsset asset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                "Packages/com.distantlands.cozy.plume/Editor/UI/UXML/plume-module-editor.uxml"
            );

            root.Bind(serializedObject);

            asset.CloneTree(root);

            CozyProfileField<PlumeProfile> profileField = new CozyProfileField<PlumeProfile>(serializedObject.FindProperty("volumetricCloudProfile"));
            SelectionContainer.Add(profileField);

            InspectorElement inspector = new InspectorElement(module.volumetricCloudProfile);
            inspector.AddToClassList("p-0");
            ProfileContainer.Add(inspector);

            root.RegisterCallback<PointerMoveEvent>((evt) =>
            {
                RenderGraph();
            });

            RenderGraph();

            RegenClouds.RegisterCallback((ClickEvent evt) =>
            {
                module.Generate();
            });

            return root;

        }

        public void RenderGraph()
        {

            GraphContainer.Clear();

            VisualElement graphHolder = new VisualElement();
            graphHolder.AddToClassList("graph-section");

            graphHolder.generateVisualContent += (MeshGenerationContext context) =>
            {
                float width = graphHolder.contentRect.width;
                float height = graphHolder.contentRect.height;


                var painter = context.painter2D;

                float cellWidth = width / 41f;

                painter.lineWidth = 2;
                painter.strokeColor = Branding.lightGreyAccent;
                painter.BeginPath();
                painter.MoveTo(new Vector2(0, height));
                painter.LineTo(new Vector2(width, height));
                painter.MoveTo(new Vector2(0, height));
                painter.LineTo(new Vector2(0, 0));
                painter.MoveTo(new Vector2(width / 2f, height));
                painter.LineTo(new Vector2(width / 2f, 0));
                painter.MoveTo(new Vector2(width * 3f / 4f, height));
                painter.LineTo(new Vector2(width * 3f / 4f, 0));
                painter.MoveTo(new Vector2(width * 1f / 4f, height));
                painter.LineTo(new Vector2(width * 1f / 4f, 0));
                painter.MoveTo(new Vector2(width, height));
                painter.LineTo(new Vector2(width, 0));
                painter.MoveTo(new Vector2(0, 0));
                painter.LineTo(new Vector2(width, 0));
                painter.Stroke();

                painter.BeginPath();
                painter.strokeColor = Branding.green;
                painter.Arc(new Vector2(width / 2f, height - 5f), 5f, 0, 180, ArcDirection.Clockwise);
                painter.Arc(new Vector2(width / 2f, height - 12f), 5f, 180, 360, ArcDirection.Clockwise);
                painter.ClosePath();
                painter.Stroke();

                PlumeProfile profile = module.volumetricCloudProfile;

                for (int i = 0; i <= profile.renderDistance; i++)
                {
                    painter.strokeColor = Branding.red;

                    float perlinCoord = (width / 2f + cellWidth * i) / (width / profile.noiseScale);
                    float cloudSpawnHeight = 80f - (profile.cloudHeightDistrubution * (Mathf.PerlinNoise(perlinCoord, 0)) / 10f);
                    float cloudHeight = 60f * Mathf.Lerp(profile.minChunkHeight / 1000f, profile.maxChunkHeight / 1000f, Mathf.PerlinNoise(perlinCoord, 6));

                    if (Mathf.PerlinNoise(perlinCoord, 6) > 0.35f)
                    {
                        painter.BeginPath();
                        painter.Arc(new Vector2(width / 2f + cellWidth * i, cloudSpawnHeight + cloudHeight * 2f), cellWidth / 2f, 0f, 180f, ArcDirection.Clockwise);
                        painter.Arc(new Vector2(width / 2f + cellWidth * i, cloudSpawnHeight - cloudHeight), cellWidth / 2f, 180f, 360f, ArcDirection.Clockwise);
                        painter.ClosePath();
                        painter.Stroke();
                    }

                    perlinCoord = (width / 2f - cellWidth * i) / (width / profile.noiseScale);
                    cloudSpawnHeight = 80f - (profile.cloudHeightDistrubution * (Mathf.PerlinNoise(perlinCoord, 0)) / 10f);
                    cloudHeight = 60f * Mathf.Lerp(profile.minChunkHeight / 1000f, profile.maxChunkHeight / 1000f, Mathf.PerlinNoise(perlinCoord, 6));

                    if (Mathf.PerlinNoise(perlinCoord, 6) > 0.35f)
                    {
                        painter.BeginPath();
                        painter.Arc(new Vector2(width / 2f - cellWidth * i, cloudSpawnHeight + cloudHeight * 2f), cellWidth / 2f, 0f, 180f, ArcDirection.Clockwise);
                        painter.Arc(new Vector2(width / 2f - cellWidth * i, cloudSpawnHeight - cloudHeight), cellWidth / 2f, 180f, 360f, ArcDirection.Clockwise);
                        painter.ClosePath();
                        painter.Stroke();
                    }
                }




            };

            GraphContainer.Add(graphHolder);

        }


        public override void OpenDocumentationURL()
        {
            Application.OpenURL("https://distant-lands.gitbook.io/cozy-stylized-weather-documentation/how-it-works/modules/plume-module");
        }


    }
}