using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
#if UNITY_EDITOR
namespace EasyDependencyDiagrams
{
    /// <summary>
    /// All diagram display mode options
    /// </summary>
    public enum DiagramDisplayMode
    {
        tree = 0,
        dependencies = 1,
        compositeStructure = 2,
        packageDiagram = 3,
        classDiagram = 4,
        componentDiagram = 5,
        folderDependencies = 6
    }

    /// <summary>
    /// Diagram display mode options for folders
    /// </summary>
    public enum FolderDisplayMode
    {
        tree = 0,
        folderDependencies = 6
    }

    /// <summary>
    /// Diagram display mode options for files
    /// </summary>
    public enum FileDisplayMode
    {
        tree = 0,
        componentDiagram = 5
    }

    /// <summary>
    /// Diagram display mode options for namespaces
    /// </summary>
    public enum NamespaceDisplayMode
    {
        tree = 0,
        dependencies = 1,
        packageDiagram = 3
    }

    /// <summary>
    /// Diagram display mode options for classes
    /// </summary>
    public enum ClassDisplayMode
    {
        tree = 0,
        dependencies = 1,
        compositeStructure = 2,
        classDiagram = 4
    }

    public enum FilterType
    {
        [Description("Strict (recommended)")]
        strict = 0,
        [Description("Loose")]
        loose = 1,
        [Description("None (not recommended)")]
        none = 2,
        [Description("Only included")]
        onlyIncluded = 3
    }

    /// <summary>
    /// Settings window for Easy Dependency Diagram
    /// </summary>
    public class EasyDependencyDiagramsSettings : EditorWindow
    {
        public static bool settingsUpdated = true;
        FilterType filterType;
        string includedPaths;
        string excludedPaths;
        Color[] colors;
        GUIStyle boldFont;

        [MenuItem("Tools/Easy Dependency Diagrams/Settings")]
        public static void ShowWindow()
        {
            EditorWindow w = GetWindow(typeof(EasyDependencyDiagramsSettings), false, "Easy Dependency Diagrams Settings");
            w.titleContent = new GUIContent("Easy Dependency Diagrams Settings", "Easy Dependency Diagrams Settings");
            w.minSize = new Vector2(400, 500);
            w.maxSize = new Vector2(400, 500);
            w.Show();
        }

        private void InitSettings()
        {
            boldFont = new GUIStyle(GUI.skin.label)
            {
                fontStyle = FontStyle.Bold
            };
            filterType = (FilterType)PlayerPrefs.GetInt("DiagramFilterType", 0);
            includedPaths = PlayerPrefs.GetString("DiagramIncludedPaths", "");
            excludedPaths = PlayerPrefs.GetString("DiagramExcludedPaths", "");

            colors = new Color[12];

            string colorString = "#" + PlayerPrefs.GetString("DiagramInfoBoxColor", "");
            if (!ColorUtility.TryParseHtmlString(colorString, out colors[0]))
            {
                colors[0] = new Color(96f / 255f, 96f / 255f, 96f / 255f, 1f);
                PlayerPrefs.SetString("DiagramInfoBoxColor", ColorUtility.ToHtmlStringRGBA(colors[0]));
            }

            colorString = "#" + PlayerPrefs.GetString("DiagramHighlightedColor", "");
            if (!ColorUtility.TryParseHtmlString(colorString, out colors[1]))
            {
                colors[1] = new Color(251f / 255f, 118f / 255f, 167f / 255f, 1f);
                PlayerPrefs.SetString("DiagramHighlightedColor", ColorUtility.ToHtmlStringRGBA(colors[1]));
            }

            colorString = "#" + PlayerPrefs.GetString("DiagramBoxColor", "");
            if (!ColorUtility.TryParseHtmlString(colorString, out colors[2]))
            {
                colors[2] = new Color(70f / 255f, 66f / 255f, 89f / 255f, 1f);
                PlayerPrefs.SetString("DiagramBoxColor", ColorUtility.ToHtmlStringRGBA(colors[2]));
            }

            colorString = "#" + PlayerPrefs.GetString("DiagramBoxBorderColor", "");
            if (!ColorUtility.TryParseHtmlString(colorString, out colors[3]))
            {
                colors[3] = new Color(227f / 255f, 234f / 255f, 251f / 255f, 1f);
                PlayerPrefs.SetString("DiagramBoxBorderColor", ColorUtility.ToHtmlStringRGBA(colors[3]));
            }

            colorString = "#" + PlayerPrefs.GetString("DiagramDependencyIncomingColor", "");
            if (!ColorUtility.TryParseHtmlString(colorString, out colors[4]))
            {
                colors[4] = new Color(149f / 255f, 70f / 255f, 99f / 255f, 1f);
                PlayerPrefs.SetString("DiagramDependencyIncomingColor", ColorUtility.ToHtmlStringRGBA(colors[4]));
            }

            colorString = "#" + PlayerPrefs.GetString("DiagramDependencyOutgoingColor", "");
            if (!ColorUtility.TryParseHtmlString(colorString, out colors[5]))
            {
                colors[5] = new Color(149f / 255f, 70f / 255f, 99f / 255f, 1f);
                PlayerPrefs.SetString("DiagramDependencyOutgoingColor", ColorUtility.ToHtmlStringRGBA(colors[5]));
            }

            colorString = "#" + PlayerPrefs.GetString("DiagramDependencyTwoWayColor", "");
            if (!ColorUtility.TryParseHtmlString(colorString, out colors[6]))
            {
                colors[6] = new Color(149f / 255f, 70f / 255f, 99f / 255f, 1f);
                PlayerPrefs.SetString("DiagramDependencyTwoWayColor", ColorUtility.ToHtmlStringRGBA(colors[6]));
            }

            colorString = "#" + PlayerPrefs.GetString("DiagramDependencyColor", "");
            if (!ColorUtility.TryParseHtmlString(colorString, out colors[7]))
            {
                colors[7] = new Color(149f / 255f, 70f / 255f, 99f / 255f, 1f);
                PlayerPrefs.SetString("DiagramDependencyColor", ColorUtility.ToHtmlStringRGBA(colors[7]));
            }

            colorString = "#" + PlayerPrefs.GetString("DiagramAssociationColor", "");
            if (!ColorUtility.TryParseHtmlString(colorString, out colors[8]))
            {
                colors[8] = new Color(149f / 255f, 70f / 255f, 99f / 255f, 1f);
                PlayerPrefs.SetString("DiagramAssociationColor", ColorUtility.ToHtmlStringRGBA(colors[8]));
            }

            colorString = "#" + PlayerPrefs.GetString("DiagramAggregationColor", "");
            if (!ColorUtility.TryParseHtmlString(colorString, out colors[9]))
            {
                colors[9] = new Color(149f / 255f, 70f / 255f, 99f / 255f, 1f);
                PlayerPrefs.SetString("DiagramAggregationColor", ColorUtility.ToHtmlStringRGBA(colors[9]));
            }

            colorString = "#" + PlayerPrefs.GetString("DiagramInheritanceColor", "");
            if (!ColorUtility.TryParseHtmlString(colorString, out colors[10]))
            {
                colors[10] = new Color(149f / 255f, 70f / 255f, 99f / 255f, 1f);
                PlayerPrefs.SetString("DiagramInheritanceColor", ColorUtility.ToHtmlStringRGBA(colors[10]));
            }

            colorString = "#" + PlayerPrefs.GetString("DiagramCompositionColor", "");
            if (!ColorUtility.TryParseHtmlString(colorString, out colors[11]))
            {
                colors[11] = new Color(149f / 255f, 70f / 255f, 99f / 255f, 1f);
                PlayerPrefs.SetString("DiagramCompositionColor", ColorUtility.ToHtmlStringRGBA(colors[11]));
            }
        }

        private void OnGUI()
        {
            if (boldFont == null)
                InitSettings();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField(new GUIContent("General", ""), boldFont);

            EditorGUILayout.Space();
            FilterType fType = (FilterType)EditorGUILayout.EnumPopup(new GUIContent("Filter type", "Type of the filter\nStrict: Only include entities that are directly dependant of main target or entities that main target is dependant on through some connections.\nLoose: Include are entities that are dependant on main through any connection or vice versa.\nNone: All entities in the project will be included (can cause extreme lag on larger projects).\nOnly included: Only include the main target and any separately whitelisted paths."), filterType);
            if (fType != filterType)
            {
                filterType = fType;
                PlayerPrefs.SetInt("DiagramFilterType", (int)filterType);
                settingsUpdated = true;
            }

            EditorGUILayout.Space();
            string iPaths = EditorGUILayout.TextField(new GUIContent("Included paths", "Paths that will be always included in diagrams, no matter which filter is selected. For example: \"Assets/Plugins\". Separate with commas (',')."), includedPaths);
            if (iPaths != includedPaths)
            {
                includedPaths = iPaths;
                while (includedPaths.Contains('\\'))
                    includedPaths = includedPaths.Replace('\\', '/');
                while (includedPaths.Contains("/,"))
                    includedPaths = includedPaths.Replace("/,", ",");
                if (includedPaths.EndsWith('/'))
                    includedPaths = includedPaths[0..^1];
                PlayerPrefs.SetString("DiagramIncludedPaths", includedPaths);
                settingsUpdated = true;
            }

            EditorGUILayout.Space();
            string ePaths = EditorGUILayout.TextField(new GUIContent("Excluded paths", "Paths that will be excluded from any diagrams. For example: \"Assets/Plugins\". Separate with commas (',')."), excludedPaths);
            if (ePaths != excludedPaths)
            {
                excludedPaths = ePaths;
                while (excludedPaths.Contains('\\'))
                    excludedPaths = excludedPaths.Replace('\\', '/');
                while (excludedPaths.Contains("/,"))
                    excludedPaths = excludedPaths.Replace("/,", ",");
                if (excludedPaths.EndsWith('/'))
                    excludedPaths = excludedPaths[0..^1];
                PlayerPrefs.SetString("DiagramExcludedPaths", excludedPaths);
                settingsUpdated = true;
            }

            DrawColorPicker(0, "Info box", "", "DiagramInfoBoxColor", new Color(96f / 255f, 96f / 255f, 96f / 255f, 1f));
            DrawColorPicker(1, "Highlighted connection", "", "DiagramHighlightedColor", new Color(251f / 255f, 118f / 255f, 167f / 255f, 1f));
            DrawColorPicker(2, "Diagram box", "", "DiagramBoxColor", new Color(70f / 255f, 66f / 255f, 89f / 255f, 1f));
            DrawColorPicker(3, "Diagram box border", "", "DiagramBoxBorderColor", new Color(227f / 255f, 234f / 255f, 251f / 255f, 1f));
            DrawColorPicker(7, "General dependency", "", "DiagramDependencyColor", new Color(149f / 255f, 70f / 255f, 99f / 255f, 1f));
            DrawColorPicker(8, "Association", "", "DiagramAssociationColor", new Color(149f / 255f, 70f / 255f, 99f / 255f, 1f));
            DrawColorPicker(9, "Aggregation", "", "DiagramAggregationColor", new Color(149f / 255f, 70f / 255f, 99f / 255f, 1f));
            DrawColorPicker(10, "Inheritance", "", "DiagramInheritanceColor", new Color(149f / 255f, 70f / 255f, 99f / 255f, 1f));
            DrawColorPicker(11, "Composition", "", "DiagramCompositionColor", new Color(149f / 255f, 70f / 255f, 99f / 255f, 1f));

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(new GUIContent("Dependency diagram", ""), boldFont);

            DrawColorPicker(4, "Incoming dependencies", "", "DiagramDependencyIncomingColor", new Color(149f / 255f, 70f / 255f, 99f / 255f, 1f));
            DrawColorPicker(5, "Outgoing dependencies", "", "DiagramDependencyOutgoingColor", new Color(149f / 255f, 70f / 255f, 99f / 255f, 1f));
            DrawColorPicker(6, "Two-way dependencies", "", "DiagramDependencyTwoWayColor", new Color(149f / 255f, 70f / 255f, 99f / 255f, 1f));

        }

        void DrawColorPicker(int colorIndex, string name, string tooltip, string playerPref, Color defaultColor)
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            Color tmpColor = EditorGUILayout.ColorField(new GUIContent(name, tooltip), colors[colorIndex]);
            if (tmpColor != colors[colorIndex])
            {
                colors[colorIndex] = tmpColor;
                PlayerPrefs.SetString(playerPref, ColorUtility.ToHtmlStringRGBA(colors[colorIndex]));
                settingsUpdated = true;
            }
            EditorGUILayout.Space();
            if (GUILayout.Button("Default"))
            {
                colors[colorIndex] = defaultColor;
                PlayerPrefs.SetString(playerPref, ColorUtility.ToHtmlStringRGBA(colors[colorIndex]));
                settingsUpdated = true;
            }
            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();
        }
    }

    /// <summary>
    /// Editor class for Easy Dependency Diagrams
    /// </summary>
    public class EasyDependencyDiagramsView : EditorWindow
    {
        private static EasyDependencyDiagrams.DataElement displayedData;
        private static EasyDependencyDiagrams ecd;
        private static EasyDependencyDiagrams ecdScope;
        private static EasyDependencyDiagrams.DataElement scopeData;
        private static List<EasyDependencyDiagrams.DataElement> allScopeElements = new List<EasyDependencyDiagrams.DataElement>();
        private static List<EasyDependencyDiagrams.DataElement> relevantScopeElements = new List<EasyDependencyDiagrams.DataElement>();
        private static float dpiScaleFactor;
        private static Vector2 scrollPos = new Vector2(0, 0);
        private static string projectRoot = "";
        private static Texture2D folderIcon;
        private static Texture2D smallFolderIcon;
        private static Texture2D minimizeIcon;
        private static Texture2D fileIcon;
        private static Texture2D openFolderIcon;
        private static Texture2D namespaceIcon;
        private static Texture2D classIcon;
        private static Texture2D methodIcon;
        private static Texture2D varIcon;
        private static Texture2D enumIcon;
        private static Texture2D prevIcon;
        private static Texture2D parentIcon;
        private static Texture2D collapseIcon;
        private static Texture2D expandIcon;
        private static Texture2D editorIcon;
        private static Texture2D stopIcon;
        private static string targetRoot = "";
        private static string activeTarget = "";
        private static string activeScopeTarget = "";
        private static string scopeTargetRoot = " ";
        private static List<string> targets = new List<string>();
        private static List<string> collapsed = new List<string>();
        private static List<string> uncollapsed = new List<string>();
        private static bool collapseDefault = false;
        private static DiagramDisplayMode mode = DiagramDisplayMode.tree;
        private static bool groupNamespaces = false;
        private static bool showInwardDependencies = true;
        private static bool showOutwardDependencies = true;
        private static Material lineMat;
        private static Color[] colorList;
        private static DiagramArrow highlightedArrow = null;
        private static List<DiagramStructureInfo> diagramStructureInfos = new List<DiagramStructureInfo>();
        private static List<KeyValuePair<DiagramStructureInfo, Rect>> diagramStructureRects = new List<KeyValuePair<DiagramStructureInfo, Rect>>();
        private static List<DiagramArrow> diagramArrows = new List<DiagramArrow>();
        private static DiagramStructureInfo diagramMainElement;
        private static readonly float diagramRadius = 150;
        private static Vector2Int scrollAreaDimensions = new Vector2Int(460, 460);
        private static Vector2Int diagramCenterPos = new Vector2Int(10, 10);
        private static GUIStyle infoBoxStyle = null;
        private static GUIStyle diagramBoxStyle = null;
        private static GUIStyle dashedBoxStyle = null;
        private static GUIStyle smallBoxStyle = null;
        private static GUIStyle smallDashedBoxStyle = null;
        private static GUIStyle labelStyle = null;
        private static GUIStyle leftAlignLabelStyle = null;
        private static GUIStyle toggleStyle = null;
        private static GUIStyle intFieldStyle = null;
        private static int diagramInwardDependencyCount = 0;
        private static int diagramOutwardDependencyCount = 0;
        private static bool onlyShowConnectedElements = false;
        private static Rect infoboxRect = new Rect();
        private static bool movingInfoBox = false;
        private static DiagramStructureInfo movedTarget = null;
        private static Vector2 movingCursorOffset = new Vector2();
        private static bool movableBoxes = false;
        private static bool recalcArrows = false;
        private static int classDiagramDependencyDepth = 1;
        private static int classDiagramInheritanceDepth = 1;
        private static int classDiagramAssociationDepth = 1;
        private static int classDiagramAggregationDepth = 1;
        private static int classDiagramCompositionDepth = 1;
        private static int classDiagramAttributeDepth = 1;
        private static Texture2D borderPixelTexture;
        private static float zoomScale = 1f;
        private static readonly int boxMargin = 173;
        private static bool dragOn = false;
        private static int previousScreenWidth;
        private static Vector2 diagramOffset = new Vector2(10, 70);
        private static FilterType filterType = FilterType.strict;
        private static List<string> includedPaths = new List<string>();
        private static List<string> excludedPaths = new List<string>();
        private static Thread cacheThread;
        private static bool caching = false;
        private static int loadingProgress = 0;
        private static string loadingDescription = "";
        private static bool callRepaint = false;

        private class DiagramStructureInfo
        {
            public string name;
            public string identifier;
            public string path;
            public string type;
            public string objectType;
            public string prefix;
            public string postfix;
            public string fullLine;
            public List<DiagramStructureInfo> children;
            public List<string> usings;
            public List<string> dependencies;
            public int angle;

            public DiagramStructureInfo()
            {
                name = "";
                identifier = "";
                path = "";
                type = "";
                objectType = "";
                prefix = "";
                postfix = "";
                fullLine = "";
                children = new List<DiagramStructureInfo>();
                usings = new List<string>();
                dependencies = new List<string>();
                angle = 0;
            }

            public DiagramStructureInfo(DiagramStructureInfo original)
            {
                name = original.name;
                identifier = original.identifier;
                path = original.path;
                type = original.type;
                objectType = original.objectType;
                prefix = original.prefix;
                postfix = original.postfix;
                fullLine = original.fullLine;

                children = new List<DiagramStructureInfo>();
                for (int i = 0; i < original.children.Count; ++i)
                {
                    children.Add(new DiagramStructureInfo(original.children[i]));
                }

                usings = EasyDependencyDiagrams.CloneList(original.usings);

                dependencies = EasyDependencyDiagrams.CloneList(original.dependencies);

                angle = original.angle;
            }

            public DiagramStructureInfo(string _name, string _identifier, string _path, string _type, string _objectType, string _prefix, string _postfix, string _fullLine, List<DiagramStructureInfo> _children, List<string> _usings, List<string> _dependencies, int _angle)
            {
                name = _name;
                identifier = _identifier;
                path = _path;
                type = _type;
                objectType = _objectType;
                prefix = _prefix;
                postfix = _postfix;
                fullLine = _fullLine;
                children = _children;
                usings = _usings;
                dependencies = _dependencies;
                angle = _angle;
            }

            public bool IsParentTo(string identifier, bool direct)
            {
                if (this.identifier.Equals(identifier))
                    return true;
                for (int i = 0; i < children.Count; ++i)
                {
                    if (children[i].identifier.Equals(identifier))
                        return true;
                    if (!direct && children[i].IsParentTo(identifier, direct))
                        return true;
                }

                return false;
            }
        }

        private class DiagramArrow
        {
            public Rect start;
            public Rect end;
            public List<Vector2> wayPoints;
            public bool endArrow;
            public bool startArrow;
            public float width;
            public int type; //-1 = default, 0 = dependency (access or undefined), 1 = association, 2 = aggregation, 3 = inheritance, 4 = composition, 5 = import
            public string startName;
            public string endName;

            public DiagramArrow()
            {
                start = new Rect();
                end = new Rect();
                wayPoints = new List<Vector2>();
                endArrow = false;
                startArrow = false;
                startName = "";
                endName = "";
                width = 1;
                type = -1;
            }

            public DiagramArrow(Rect _start, Rect _end, bool _startArrow, bool _endArrow, float _width, string _startName, string _endName, List<Vector2> _wayPoints, int _type)
            {
                start = _start;
                end = _end;
                startName = _startName;
                endName = _endName;
                wayPoints = _wayPoints;
                startArrow = _startArrow;
                endArrow = _endArrow;
                width = _width;
                type = _type;
            }
        }

        private struct ArrowNode
        {
            public Vector2 pos;
            public int dir; //0 = down, 1 = up, 2 = left, 3 = right

            public ArrowNode(Vector2 _pos, int _dir)
            {
                pos = _pos;
                dir = _dir;
            }

            public ArrowNode(ArrowNode original)
            {
                pos = original.pos;
                dir = original.dir;
            }

            public static bool operator ==(ArrowNode a, ArrowNode b)
            {
                return a.pos == b.pos && a.dir == b.dir;
            }

            public static bool operator !=(ArrowNode a, ArrowNode b)
            {
                return a.pos != b.pos || a.dir != b.dir;
            }

            public override bool Equals(object obj)
            {
                if (obj.GetType().Equals(typeof(ArrowNode)))
                    return (ArrowNode)obj == this;

                return false;
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

        }

        [MenuItem("Tools/Easy Dependency Diagrams/Show Window")]
        public static void ShowWindow()
        {
            EditorWindow w = GetWindow(typeof(EasyDependencyDiagramsView), false, "Easy Dependency Diagrams");
            w.titleContent = new GUIContent("Easy Dependency Diagrams", "Easy Dependency Diagrams");
            w.minSize = new Vector2(670, 500);
            w.Show();
        }

        [MenuItem("Tools/Easy Dependency Diagrams/Export as .svg")]
        public static void ExportAsSVG()
        {
            if (ecd == null || ecd.ActiveJob || ecdScope == null || ecdScope.ActiveJob)
            {
                Debug.LogWarning("Cannot export diagram as svg while the content is loading or does not exist.");
                return;
            }

            string defaultName;
            if ((targetRoot.EndsWith(".cs") || !targetRoot.Contains(".cs")) && targetRoot.LastIndexOf('/') > 0)
            {
                defaultName = targetRoot[(targetRoot.LastIndexOf('/') + 1)..];
                if (defaultName.Contains(".cs"))
                    defaultName = defaultName[0..^3];
                defaultName += " ";
            }
            else
            {
                defaultName = FullName(targetRoot).Replace('.', '_') + " ";
            }
            switch (mode)
            {
                case DiagramDisplayMode.tree:
                    defaultName += "Content tree";
                    break;

                case DiagramDisplayMode.dependencies:
                    defaultName += "Dependency diagram";
                    break;

                case DiagramDisplayMode.compositeStructure:
                    defaultName += "Composite structure diagram";
                    break;

                case DiagramDisplayMode.packageDiagram:
                    defaultName += "Package diagram";
                    break;

                case DiagramDisplayMode.classDiagram:
                    defaultName += "Class diagram";
                    break;

                case DiagramDisplayMode.componentDiagram:
                    defaultName += "Component diagram";
                    break;

                case DiagramDisplayMode.folderDependencies:
                    defaultName += "Folder dependency diagram";
                    break;
            }    
            string saveLocation = EditorUtility.SaveFilePanel("Where to export the diagram as SVG", projectRoot, defaultName, "svg");

            if (saveLocation.Length < 1)
                return;

            if (!saveLocation.EndsWith(".svg"))
                saveLocation += ".svg";

            int slashPosition = saveLocation.LastIndexOf('/');
            if (slashPosition < 0 || !Directory.Exists(saveLocation.Substring(0, slashPosition)))
            {
                Debug.LogWarning("SVG save folder doesn't exist.");
                return;
            }

            if (File.Exists(saveLocation))
            {
                File.Delete(saveLocation);
            }

            string boxColorString = "#" + ColorUtility.ToHtmlStringRGB(colorList[2]);
            string borderColorString = "#" + ColorUtility.ToHtmlStringRGB(colorList[3]);
            Vector2Int offset = diagramCenterPos - new Vector2Int(40, 90);
            offset = new Vector2Int(Mathf.Max(offset.x, 40), Mathf.Max(offset.y, 70));
            Vector2Int canvasSize = new Vector2Int(scrollAreaDimensions.x - diagramCenterPos.x + offset.x, scrollAreaDimensions.y - diagramCenterPos.y + offset.y);
            if (mode == DiagramDisplayMode.tree)
            {
                offset = new Vector2Int(10, 10);
                canvasSize = new Vector2Int(scrollAreaDimensions.x, scrollAreaDimensions.y);
            }

            string textStyle = "style=\"dominant-baseline:middle;text-anchor:middle;fill:white;font-style:normal;font-variant:normal;font-weight:normal;font-stretch:normal;font-family:Arial;font-size:12px\"";
            string leftAlignTextStyle = "style=\"fill:white;font-style:normal;font-variant:normal;font-weight:normal;font-stretch:normal;font-family:Arial;font-size:12px\"";

            string documentStart = "<svg width=\"" + canvasSize.x + "\" height=\"" + canvasSize.y + "\" version=\"1.1\" xmlns=\"http://www.w3.org/2000/svg\" xmlns:xlink=\"http://www.w3.org/1999/xlink\" >\n";
            string documentEnd = "</svg>";

            string boxRects = "";
            string lines = "";

            if (mode == DiagramDisplayMode.dependencies || mode == DiagramDisplayMode.folderDependencies)
            {
                //background for dependency view
                boxRects += "<rect x=\"0\" y=\"0\" width=\"" + (scrollAreaDimensions.x - diagramCenterPos.x + offset.x) + "\" height=\"" + (scrollAreaDimensions.y - diagramCenterPos.y + offset.y) + "\" fill=\"" + boxColorString + "\" stroke-width=\"2\" stroke=\"" + borderColorString + "\"/>\n";
            }
            else if (mode == DiagramDisplayMode.tree)
            {
                //background for dependency view
                boxRects += "<rect x=\"0\" y=\"0\" width=\"" + (scrollAreaDimensions.x) + "\" height=\"" + (scrollAreaDimensions.y) + "\" fill=\"" + boxColorString + "\" stroke-width=\"2\" stroke=\"" + borderColorString + "\"/>\n";

            }

            if (mode != DiagramDisplayMode.tree)
            {
                for (int i = 0; i < diagramStructureRects.Count; ++i)
                {
                    Vector2Int boxPos = new Vector2Int((int)(diagramStructureRects[i].Value.x + offset.x), (int)(diagramStructureRects[i].Value.y + offset.y));
                    //box itself
                    if (mode == DiagramDisplayMode.compositeStructure && diagramStructureRects[i].Key.angle != 0)
                    {
                        boxRects += "<rect x=\"" + boxPos.x + "\" y=\"" + boxPos.y + "\" width=\"" + diagramStructureRects[i].Value.width + "\" height=\"" + diagramStructureRects[i].Value.height + "\" fill=\"" + boxColorString + "\" stroke-width=\"2\" stroke-dasharray=\"10,5\" stroke=\"" + borderColorString + "\"/>\n";
                    }
                    else if (mode == DiagramDisplayMode.dependencies || mode == DiagramDisplayMode.folderDependencies)
                    {
                        boxPos = new Vector2Int((int)(diagramStructureRects[i].Value.x + offset.x - diagramCenterPos.x), (int)(diagramStructureRects[i].Value.y + offset.y - diagramCenterPos.y));
                        if (diagramStructureRects[i].Key.type.Equals("Namespace"))
                            boxRects += "<rect x=\"" + (boxPos.x - 15) + "\" y=\"" + boxPos.y + "\" width=\"" + diagramStructureRects[i].Value.width + "\" height=\"" + diagramStructureRects[i].Value.height + "\" fill=\"" + boxColorString + "\" stroke-width=\"2\" stroke=\"" + borderColorString + "\"/>\n";
                        else
                        {

                            if (mode == DiagramDisplayMode.dependencies)
                            {
                                boxRects += IconAsSVG("class", new Vector2Int(boxPos.x, boxPos.y), new Vector2Int((int)diagramStructureRects[i].Value.width, (int)diagramStructureRects[i].Value.height));
                            }
                            else if (mode == DiagramDisplayMode.folderDependencies)
                            {
                                if (i == 0)
                                    boxRects += IconAsSVG("folderOpen", new Vector2Int(boxPos.x, boxPos.y), new Vector2Int((int)diagramStructureRects[i].Value.width, (int)diagramStructureRects[i].Value.height));
                                else
                                    boxRects += IconAsSVG("folder", new Vector2Int(boxPos.x, boxPos.y), new Vector2Int((int)diagramStructureRects[i].Value.width, (int)diagramStructureRects[i].Value.height));
                            }
                        }
                    }
                    else
                    {
                        boxRects += "<rect x=\"" + boxPos.x + "\" y=\"" + boxPos.y + "\" width=\"" + diagramStructureRects[i].Value.width + "\" height=\"" + diagramStructureRects[i].Value.height + "\" fill=\"" + boxColorString + "\" stroke-width=\"2\" stroke=\"" + borderColorString + "\"/>\n";
                    }

                    //box contents
                    if (mode == DiagramDisplayMode.classDiagram)
                    {
                        string prefix = "&lt;&lt;entity&gt;&gt;";
                        if (diagramStructureRects[i].Key.prefix.Contains("static"))
                            prefix = "&lt;&lt;control&gt;&gt;";
                        if (classDiagramAttributeDepth >= diagramStructureRects[i].Key.angle && diagramStructureRects[i].Value.height > 60)
                        {
                            Vector2 titleLine = new Vector2(boxPos.x + diagramStructureRects[i].Value.width / 2, boxPos.y + 15);
                            boxRects += "<text x=\"" + titleLine.x + "\" y=\"" + titleLine.y + "\" " + textStyle + ">\n" +
                                "<tspan x=\"" + titleLine.x + "\" dy=\"0\">" + prefix + "</tspan>\n" +
                                "<tspan x=\"" + titleLine.x + "\" dy=\"1.2em\">" + diagramStructureRects[i].Key.name.Replace("<", "&lt;").Replace("<", "&gt;") + "</tspan>\n" +
                                "</text>\n";


                            Rect nextLine = new Rect(boxPos.x + 5, boxPos.y + 62, diagramStructureRects[i].Value.width - 10, 20);

                            //drawing variable/property list
                            for (int k = 0; k < diagramStructureRects[i].Key.children.Count; ++k)
                            {
                                if (diagramStructureRects[i].Key.children[k].type.Equals("Variable") || diagramStructureRects[i].Key.children[k].type.Equals("Property"))
                                {
                                    char prefixCharacter = PrefixCharacter(diagramStructureRects[i].Key.children[k]);

                                    boxRects += "<text x=\"" + nextLine.x + "\" y=\"" + nextLine.y + "\" " + leftAlignTextStyle + ">" + (prefixCharacter + diagramStructureRects[i].Key.children[k].name).Replace("<", "&lt;").Replace("<", "&gt;") + "</text>\n";
                                    nextLine.y += 20;
                                }
                            }

                            if (nextLine.y > boxPos.y + 63)
                            {
                                //Line separator
                                boxRects += "<rect x=\"" + (boxPos.x+1) + "\" y=\"" + (boxPos.y + 40) + "\" width=\"" + (diagramStructureRects[i].Value.width-2) + "\" height=\"" + 2 + "\" fill=\"" + borderColorString + "\" stroke-width=\"0\"/>\n";
                                nextLine.y += 15;
                            }

                            float separatorLinePos = nextLine.y - 15;

                            //drawing constructor/destructor/method list
                            for (int k = 0; k < diagramStructureRects[i].Key.children.Count; ++k)
                            {
                                if (diagramStructureRects[i].Key.children[k].type.Equals("Constructor") || diagramStructureRects[i].Key.children[k].type.Equals("Destructor") || diagramStructureRects[i].Key.children[k].type.Equals("Method"))
                                {
                                    string returnValue = "";
                                    if (!diagramStructureRects[i].Key.children[k].objectType.Equals("void"))
                                        returnValue = " : " + diagramStructureRects[i].Key.children[k].objectType;
                                    char prefixCharacter = PrefixCharacter(diagramStructureRects[i].Key.children[k]);
                                    boxRects += "<text x=\"" + nextLine.x + "\" y=\"" + nextLine.y + "\" " + leftAlignTextStyle + ">" + (prefixCharacter + diagramStructureRects[i].Key.children[k].name + "()" + returnValue).Replace("<", "&lt;").Replace("<", "&gt;") + "</text>\n";
                                    nextLine.y += 20;
                                }
                            }

                            if (nextLine.y > separatorLinePos + 15)
                            {
                                //Line separator
                                boxRects += "<rect x=\"" + (boxPos.x+1) + "\" y=\"" + (separatorLinePos - 5) + "\" width=\"" + (diagramStructureRects[i].Value.width-2) + "\" height=\"" + 2 + "\" fill=\"" + borderColorString + "\" stroke-width=\"0\"/>\n";
                            }

                        }
                        else
                        {
                            Vector2 titleLine = new Vector2(boxPos.x + diagramStructureRects[i].Value.width / 2, boxPos.y + diagramStructureRects[i].Value.height / 2 - 10);
                            boxRects += "<text x=\"" + titleLine.x + "\" y=\"" + titleLine.y + "\" " + textStyle + ">\n" +
                                "<tspan x=\"" + titleLine.x + "\" dy=\"0\">" + prefix + "</tspan>\n" +
                                "<tspan x=\"" + titleLine.x + "\" dy=\"1.2em\">" + diagramStructureRects[i].Key.name.Replace("<", "&lt;").Replace("<", "&gt;") + "</tspan>\n" +
                                "</text>\n";
                        }
                    }

                    else if (mode == DiagramDisplayMode.compositeStructure)
                    {
                        if (diagramStructureRects[i].Value.height > 60)
                        {
                            Vector2Int titleLine = new Vector2Int((int)(boxPos.x + diagramStructureRects[i].Value.width / 2), (int)(boxPos.y + 15));
                            boxRects += "<text x=\"" + titleLine.x + "\" y=\"" + titleLine.y + "\" " + textStyle + ">" + diagramStructureRects[i].Key.postfix.Replace("<", "&lt;").Replace("<", "&gt;") + "</text>\n";

                        }
                        else
                        {
                            Vector2Int titleLine = new Vector2Int((int)(boxPos.x + diagramStructureRects[i].Value.width / 2), (int)(boxPos.y + diagramStructureRects[i].Value.height / 2));
                            boxRects += "<text x=\"" + titleLine.x + "\" y=\"" + titleLine.y + "\" " + textStyle + ">" + diagramStructureRects[i].Key.postfix.Replace("<", "&lt;").Replace("<", "&gt;") + "</text>\n";
                        }
                    }

                    else if (mode == DiagramDisplayMode.packageDiagram)
                    {
                        if (diagramStructureRects[i].Value.height > 60)
                        {
                            Vector2Int titleLine = new Vector2Int((int)(boxPos.x + diagramStructureRects[i].Value.width / 2), (int)(boxPos.y + 15));
                            boxRects += "<text x=\"" + titleLine.x + "\" y=\"" + titleLine.y + "\" " + textStyle + ">" + diagramStructureRects[i].Key.name.Replace("<", "&lt;").Replace("<", "&gt;") + "</text>\n";

                        }
                        else
                        {
                            Vector2Int titleLine = new Vector2Int((int)(boxPos.x + diagramStructureRects[i].Value.width / 2), (int)(boxPos.y + diagramStructureRects[i].Value.height / 2));
                            boxRects += "<text x=\"" + titleLine.x + "\" y=\"" + titleLine.y + "\" " + textStyle + ">" + diagramStructureRects[i].Key.name.Replace("<", "&lt;").Replace("<", "&gt;") + "</text>\n";
                        }
                    }

                    else if (mode == DiagramDisplayMode.componentDiagram)
                    {
                        string prefix = "&lt;&lt;component&gt;&gt;";
                        if (diagramStructureRects[i].Value.height > 60)
                        {
                            Vector2Int titleLine = new Vector2Int((int)(boxPos.x + diagramStructureRects[i].Value.width / 2), (int)(boxPos.y + 15f));
                            boxRects += "<text x=\"" + titleLine.x + "\" y=\"" + titleLine.y + "\" " + textStyle + ">\n" +
                                "<tspan x=\"" + titleLine.x + "\" dy=\"0\">" + prefix + "</tspan>\n" +
                                "<tspan x=\"" + titleLine.x + "\" dy=\"1.2em\">" + diagramStructureRects[i].Key.name.Replace("<", "&lt;").Replace("<", "&gt;") + "</tspan>\n" +
                                "</text>\n";
                        }
                        else
                        {
                            Vector2Int titleLine = new Vector2Int((int)(boxPos.x + diagramStructureRects[i].Value.width / 2), (int)(boxPos.y + diagramStructureRects[i].Value.height / 2 - 5f));
                            boxRects += "<text x=\"" + titleLine.x + "\" y=\"" + titleLine.y + "\" " + textStyle + ">\n" +
                                "<tspan x=\"" + titleLine.x + "\" dy=\"0\">" + prefix + "</tspan>\n" +
                                "<tspan x=\"" + titleLine.x + "\" dy=\"1.2em\">" + diagramStructureRects[i].Key.name.Replace("<", "&lt;").Replace("<", "&gt;") + "</tspan>\n" +
                                "</text>\n";
                        }
                    }

                    else if (mode == DiagramDisplayMode.dependencies || mode == DiagramDisplayMode.folderDependencies)
                    {
                        if (diagramStructureRects[i].Key.type.Equals("Namespace"))
                        {
                            Vector2Int titleLine = new Vector2Int((int)(boxPos.x + diagramStructureRects[i].Value.width / 2 - 15), (int)(boxPos.y + 15));
                            boxRects += "<text x=\"" + titleLine.x + "\" y=\"" + titleLine.y + "\" " + textStyle + ">" + diagramStructureRects[i].Key.name.Replace("<", "&lt;").Replace("<", "&gt;") + "</text>\n";
                        }
                        else
                        {
                            Vector2Int titleLine = new Vector2Int((int)(boxPos.x + diagramStructureRects[i].Value.width / 2), (int)(boxPos.y + diagramStructureRects[i].Value.height + 10));
                            boxRects += "<text x=\"" + titleLine.x + "\" y=\"" + titleLine.y + "\" " + textStyle + ">" + diagramStructureRects[i].Key.name.Replace("<", "&lt;").Replace("<", "&gt;") + "</text>\n";
                        }
                    }
                }

                for (int i = 0; i < diagramArrows.Count; ++i)
                {
                    Color color = GetArrowColor(diagramArrows[i]);

                    string colorString = "#" + ColorUtility.ToHtmlStringRGB(color);

                    float angleStart = Vector2.SignedAngle(new Vector2(0, 1), new Vector2(diagramArrows[i].wayPoints[1].x - diagramArrows[i].wayPoints[0].x, diagramArrows[i].wayPoints[0].y - diagramArrows[i].wayPoints[1].y));
                    Vector2 startCornerVector = Quaternion.Euler(0, 0, -angleStart) * new Vector3(1, 0, 0);
                    Vector2 startLineVector = Quaternion.Euler(0, 0, -angleStart - 90) * new Vector3(1, 0, 0);

                    float angleEnd = Vector2.SignedAngle(new Vector2(0, 1), new Vector2(diagramArrows[i].wayPoints[^1].x - diagramArrows[i].wayPoints[^2].x, diagramArrows[i].wayPoints[^2].y - diagramArrows[i].wayPoints[^1].y));
                    Vector2 endCornerVector = Quaternion.Euler(0, 0, -angleEnd) * new Vector3(1, 0, 0);
                    Vector2 endLineVector = Quaternion.Euler(0, 0, -angleEnd - 90) * new Vector3(1, 0, 0);

                    for (int k = 0; k < diagramArrows[i].wayPoints.Count - 1; ++k)
                    {
                        if ((diagramArrows[i].type == 0 || diagramArrows[i].type == 5) && k % 2 != 0 && k < diagramArrows[i].wayPoints.Count - 2)
                            continue;

                        float angle = Vector2.SignedAngle(new Vector2(0, 1), new Vector2(diagramArrows[i].wayPoints[k + 1].x - diagramArrows[i].wayPoints[k].x, diagramArrows[i].wayPoints[k].y - diagramArrows[i].wayPoints[k + 1].y));
                        Vector2 lineVector = Quaternion.Euler(0, 0, -angle - 90) * new Vector3(1, 0, 0);

                        Vector2 start = new Vector2(diagramArrows[i].wayPoints[k].x - diagramCenterPos.x + offset.x, diagramArrows[i].wayPoints[k].y - diagramCenterPos.y + offset.y);
                        Vector2 end = new Vector2(diagramArrows[i].wayPoints[k + 1].x - diagramCenterPos.x + offset.x, diagramArrows[i].wayPoints[k + 1].y - diagramCenterPos.y + offset.y);

                        int endMargin = 4;

                        if (diagramArrows[i].endArrow)
                        {
                            endMargin = 5;
                            if (diagramArrows[i].type == 0)
                            {
                                endMargin = 1;
                            }
                            else if (diagramArrows[i].type == 2)
                            {
                                endMargin = 12;
                            }
                            else if (diagramArrows[i].type == 3)
                            {
                                endMargin = 7;
                            }
                            else if (diagramArrows[i].type == 4)
                            {
                                endMargin = 10;
                            }
                        }

                        if (k > 0)
                            start -= lineVector * 2;
                        else if (diagramArrows[i].startArrow)
                            start += startLineVector * endMargin;

                        if (k < diagramArrows[i].wayPoints.Count - 2)
                            end += lineVector * 2;
                        else if (diagramArrows[i].endArrow)
                            end -= endLineVector * endMargin;


                        lines += "<line x1=\"" + (int)start.x + "\" y1=\"" + (int)start.y + "\" x2=\"" + (int)end.x + "\" y2=\"" + (int)end.y + "\"  stroke-width=\"" + (int)(diagramArrows[i].width * 2) + "\" stroke=\"" + colorString + "\"></line>\n";
                    }

                    //arrowheads
                    if (diagramArrows[i].startArrow)
                    {

                        Vector2 startCorner1 = new Vector2(diagramArrows[i].wayPoints[0].x - diagramCenterPos.x + offset.x, diagramArrows[i].wayPoints[0].y - diagramCenterPos.y + offset.y) + startCornerVector - startLineVector;
                        Vector2 startCorner2 = new Vector2(diagramArrows[i].wayPoints[0].x - diagramCenterPos.x + offset.x, diagramArrows[i].wayPoints[0].y - diagramCenterPos.y + offset.y) - startCornerVector - startLineVector;

                        //startarrow is always the basic type
                        Vector2[] startArrowPoints = new Vector2[3];
                        startArrowPoints[0] = (startCorner1 + startCorner2) / 2f;
                        startArrowPoints[1] = startCorner1 + 6 * startCornerVector + 12 * startLineVector;
                        startArrowPoints[2] = startCorner2 - 6 * startCornerVector + 12 * startLineVector;

                        string arrowPoints = "";
                        for (int k = 0; k < startArrowPoints.Length; ++k)
                        {
                            arrowPoints += (int)startArrowPoints[k].x + "," + (int)startArrowPoints[k].y + " ";
                        }
                        lines += "<polygon points=\"" + arrowPoints + "\" style=\"" + "fill:" + colorString + ";stroke-width:0\" />\n";
                    }

                    if (diagramArrows[i].endArrow && diagramArrows[i].type != 1)
                    {

                        Vector2 endCorner1 = new Vector2(diagramArrows[i].wayPoints[^1].x - diagramCenterPos.x + offset.x, diagramArrows[i].wayPoints[^1].y - diagramCenterPos.y + offset.y) + endCornerVector - endLineVector;
                        Vector2 endCorner2 = new Vector2(diagramArrows[i].wayPoints[^1].x - diagramCenterPos.x + offset.x, diagramArrows[i].wayPoints[^1].y - diagramCenterPos.y + offset.y) - endCornerVector - endLineVector;
                        Vector2[] endArrowPoints;
                        bool fill;
                        if (diagramArrows[i].type == 0 || diagramArrows[i].type == 5)
                        {
                            // dependency: dashed line, open hollow triangle arrow
                            endArrowPoints = new Vector2[3];
                            endArrowPoints[0] = (endCorner1 - 10 * (Vector2)(endCornerVector) - 10 * (Vector2)(endLineVector));
                            endArrowPoints[1] = ((endCorner1 + endCorner2) / 2f);
                            endArrowPoints[2] = (endCorner2 + 10 * (Vector2)(endCornerVector) - 10 * (Vector2)(endLineVector));
                            //line, not a polygon
                            lines += "<polyline points=\"" + (int)endArrowPoints[0].x + "," + (int)endArrowPoints[0].y + " " + (int)endArrowPoints[1].x + "," + (int)endArrowPoints[1].y + " " + (int)endArrowPoints[2].x + "," + (int)endArrowPoints[2].y + "\" style=\"fill:none;stroke-width:" + (diagramArrows[i].width * 1.5f) + ";stroke:" + colorString + "\"/>\n";
                            continue;
                        }
                        else if (diagramArrows[i].type == 2)
                        {
                            fill = false;
                            // aggregation: solid line, hollow diamond arrow
                            endArrowPoints = new Vector2[4];
                            endArrowPoints[0] = (endCorner2 + 5 * (Vector2)(endCornerVector) - 7 * (Vector2)(endLineVector));
                            endArrowPoints[1] = ((endCorner1 + endCorner2) / 2f);
                            endArrowPoints[2] = (endCorner1 - 5 * (Vector2)(endCornerVector) - 7 * (Vector2)(endLineVector));
                            endArrowPoints[3] = ((endCorner1 + endCorner2) / 2f - 14 * (Vector2)(endLineVector));
                        }
                        else if (diagramArrows[i].type == 3)
                        {
                            fill = false;
                            // inheritance: solid line, hollow triange arrow
                            endArrowPoints = new Vector2[4];
                            endArrowPoints[0] = (endCorner2 + 6 * (Vector2)(endCornerVector) - 9 * (Vector2)(endLineVector));
                            endArrowPoints[1] = ((endCorner1 + endCorner2) / 2f);
                            endArrowPoints[2] = (endCorner1 - 6 * (Vector2)(endCornerVector) - 9 * (Vector2)(endLineVector));
                            endArrowPoints[3] = (endCorner2 + 6 * (Vector2)(endCornerVector) - 9 * (Vector2)(endLineVector));
                        }
                        else if (diagramArrows[i].type == 4)
                        {
                            fill = true;
                            // composition: solid line, solid diamond arrow
                            endArrowPoints = new Vector2[4];
                            endArrowPoints[0] = (endCorner2 + 6 * (Vector2)(endCornerVector) - 8 * (Vector2)(endLineVector));
                            endArrowPoints[1] = ((endCorner1 + endCorner2) / 2f);
                            endArrowPoints[2] = (endCorner1 - 6 * (Vector2)(endCornerVector) - 8 * (Vector2)(endLineVector));
                            endArrowPoints[3] = ((endCorner1 + endCorner2) / 2f - 16 * (Vector2)(endLineVector));

                        }
                        else
                        {
                            fill = true;
                            endArrowPoints = new Vector2[3];
                            endArrowPoints[0] = (endCorner1 + endCorner2) / 2f;
                            endArrowPoints[1] = endCorner1 + 6 * endCornerVector - 12 * endLineVector;
                            endArrowPoints[2] = endCorner2 - 6 * endCornerVector - 12 * endLineVector;
                        }

                        string arrowPoints = "";
                        for (int k = 0; k < endArrowPoints.Length; ++k)
                        {
                            arrowPoints += (int)endArrowPoints[k].x + "," + (int)endArrowPoints[k].y + " ";
                        }
                        string styleText;
                        if (fill)
                            styleText = "fill:" + colorString + ";stroke-width:0";
                        else
                            styleText = "fill:none;stroke:" + colorString + ";stroke-width:" + (diagramArrows[i].width * 1.5f);
                        lines += "<polygon points=\"" + arrowPoints + "\" style=\"" + styleText + "\" />\n";
                    }
                }
            }
            else
            {
                boxRects += SvgTree(0, 0, displayedData, targetRoot, displayedData.type.Equals("Folder")).Key;
            }

            string fullDocument = documentStart + boxRects + lines + documentEnd;

            FileStream fs = File.Create(saveLocation);
            // Add some text to file
            byte[] title = new UTF8Encoding(true).GetBytes(fullDocument);
            fs.Write(title, 0, title.Length);
            fs.Close();
            AssetDatabase.Refresh();

            Debug.Log("Exported the diagram as svg to '" + saveLocation + "'");
        }

        private static void InitWindow()
        {
            dpiScaleFactor = 1f / EditorGUIUtility.pixelsPerPoint;

            classDiagramDependencyDepth = PlayerPrefs.GetInt("DiagramDependencyDepth", 1);
            classDiagramInheritanceDepth = PlayerPrefs.GetInt("DiagramInheritanceDepth", 1);
            classDiagramAssociationDepth = PlayerPrefs.GetInt("DiagramAssociationDepth", 1);
            classDiagramAggregationDepth = PlayerPrefs.GetInt("DiagramAggregationDepth", 1);
            classDiagramCompositionDepth = PlayerPrefs.GetInt("DiagramCompositionDepth", 1);
            classDiagramAttributeDepth = PlayerPrefs.GetInt("DiagramAttributeDepth", 1);
            groupNamespaces = PlayerPrefs.GetInt("DiagramGroupNamespaces", 0) == 1;
            showInwardDependencies = PlayerPrefs.GetInt("DiagramInwardDependencies", 1) == 1;
            showOutwardDependencies = PlayerPrefs.GetInt("DiagramOutwardDependencies", 1) == 1;
            onlyShowConnectedElements = PlayerPrefs.GetInt("DiagramShowConnected", 0) == 1;
            mode = (DiagramDisplayMode)PlayerPrefs.GetInt("DiagramDisplayMode", 0);
            int targetCount = PlayerPrefs.GetInt("DiagramTargetCount", 0);
            targets = new List<string>();
            for (int i = 0; i < targetCount; ++i)
            {
                string target = PlayerPrefs.GetString("DiagramTarget" + i, "");
                if (target.Length > 0)
                    targets.Add(target);
            }
            int collapsedCount = PlayerPrefs.GetInt("DiagramCollapsedCount", 0);
            collapsed = new List<string>();
            for (int i = 0; i < collapsedCount; ++i)
            {
                string coll = PlayerPrefs.GetString("DiagramCollapsed" + i, "");
                if (coll.Length > 0)
                    collapsed.Add(coll);
            }
            int uncollapsedCount = PlayerPrefs.GetInt("DiagramUncollapsedCount", 0);
            uncollapsed = new List<string>();
            for (int i = 0; i < uncollapsedCount; ++i)
            {
                string coll = PlayerPrefs.GetString("DiagramUncollapsed" + i, "");
                if (coll.Length > 0)
                    uncollapsed.Add(coll);
            }

            projectRoot = PlayerPrefs.GetString("DiagramProjectRoot", Application.dataPath);
            //defaulting to application datapath if folder names have been changed
            if (!Directory.Exists(projectRoot))
                projectRoot = Application.dataPath;
            ecd = (EasyDependencyDiagrams)CreateInstance(typeof(EasyDependencyDiagrams));
            ecdScope = (EasyDependencyDiagrams)CreateInstance(typeof(EasyDependencyDiagrams));
            targetRoot = projectRoot;
            targetRoot = PlayerPrefs.GetString("DiagramTargetRoot", projectRoot);

            //defaulting to project root if folder/file have been changed
            if ((targetRoot.Contains(".cs") && !File.Exists(targetRoot.Substring(0, targetRoot.IndexOf(".cs") + 3))) || (!targetRoot.Contains(".cs") && !Directory.Exists(targetRoot)))
            {
                targetRoot = projectRoot;
            }

            activeTarget = targetRoot;
            if (targets.Count < 1 || !targets[^1].Equals(activeTarget))
            {
                targets.Add(activeTarget);
                PlayerPrefs.SetInt("DiagramTargetCount", targets.Count);
                PlayerPrefs.SetString("DiagramTarget" + (targets.Count - 1), activeTarget);
            }

            ecd.ParseData(targetRoot, mode != DiagramDisplayMode.folderDependencies);
            previousScreenWidth = (int)(Screen.width * dpiScaleFactor);
            folderIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Plugins/Easy Dependency Diagrams/Editor/folder.png");
            smallFolderIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Plugins/Easy Dependency Diagrams/Editor/folder-icon.png");
            minimizeIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Plugins/Easy Dependency Diagrams/Editor/minimize-icon.png");
            fileIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Plugins/Easy Dependency Diagrams/Editor/cs.png");
            openFolderIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Plugins/Easy Dependency Diagrams/Editor/folderOpen.png");
            namespaceIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Plugins/Easy Dependency Diagrams/Editor/namespace.png");
            classIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Plugins/Easy Dependency Diagrams/Editor/class.png");
            methodIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Plugins/Easy Dependency Diagrams/Editor/method.png");
            varIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Plugins/Easy Dependency Diagrams/Editor/variable.png");
            enumIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Plugins/Easy Dependency Diagrams/Editor/enum.png");
            prevIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Plugins/Easy Dependency Diagrams/Editor/arrow-back-icon.png");
            parentIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Plugins/Easy Dependency Diagrams/Editor/arrow-parent-icon.png");
            stopIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Plugins/Easy Dependency Diagrams/Editor/stop-icon.png");
            collapseIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Plugins/Easy Dependency Diagrams/Editor/collapse-icon.png");
            expandIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Plugins/Easy Dependency Diagrams/Editor/expand-icon.png");
            editorIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Plugins/Easy Dependency Diagrams/Editor/open-editor-icon.png");
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            lineMat = new Material(shader)
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            // Turn on alpha blending
            lineMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            lineMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            // Turn backface culling off
            lineMat.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            // Turn off depth writes
            lineMat.SetInt("_ZWrite", 0);

            borderPixelTexture = new Texture2D(1, 1);
            borderPixelTexture.SetPixels32(new Color32[1] { colorList[3] });
            borderPixelTexture.Apply();

            GUIStyle baseBoxStyle = new GUIStyle(GUI.skin.box)
            {
                border = new RectOffset(2, 2, 2, 2),
                alignment = TextAnchor.MiddleCenter,
                clipping = TextClipping.Clip,
                contentOffset = new Vector2(0, 0),
                fixedHeight = 0,
                fixedWidth = 0,
                fontSize = 12,
                fontStyle = FontStyle.Normal,
                imagePosition = ImagePosition.TextOnly,
                margin = new RectOffset(0, 0, 0, 0),
                overflow = new RectOffset(0, 0, 0, 0),
                padding = new RectOffset(0, 0, 0, 0),
                richText = false,
                stretchHeight = false,
                stretchWidth = false,
                wordWrap = false
            };
            baseBoxStyle.active.textColor = baseBoxStyle.focused.textColor = baseBoxStyle.hover.textColor = baseBoxStyle.onActive.textColor = baseBoxStyle.onFocused.textColor = baseBoxStyle.onHover.textColor = baseBoxStyle.onNormal.textColor = baseBoxStyle.normal.textColor = Color.white;

            infoBoxStyle = new GUIStyle(baseBoxStyle)
            {
                wordWrap = true,
                alignment = TextAnchor.MiddleLeft
            };
            infoBoxStyle.normal.background = new Texture2D(1, 1);
            infoBoxStyle.normal.background.SetPixels32(new Color32[1] { colorList[0] });
            infoBoxStyle.normal.background.Apply();
            infoBoxStyle.normal.scaledBackgrounds = new Texture2D[1] { infoBoxStyle.normal.background };
            infoBoxStyle.active = infoBoxStyle.focused = infoBoxStyle.hover = infoBoxStyle.onActive = infoBoxStyle.onFocused = infoBoxStyle.onHover = infoBoxStyle.onNormal = infoBoxStyle.normal;

            diagramBoxStyle = new GUIStyle(baseBoxStyle)
            {
                wordWrap = true,
                alignment = TextAnchor.UpperCenter,
                border = new RectOffset(2, 2, 2, 2)
            };
            diagramBoxStyle.normal.background = CreateBoxWithBorders(colorList[2], colorList[3], 128, 2, false, 2);
            diagramBoxStyle.normal.scaledBackgrounds = new Texture2D[1] { CreateBoxWithBorders(colorList[2], colorList[3], 256, 4, false, 4) };
            diagramBoxStyle.active = diagramBoxStyle.focused = diagramBoxStyle.hover = diagramBoxStyle.onActive = diagramBoxStyle.onFocused = diagramBoxStyle.onHover = diagramBoxStyle.onNormal = diagramBoxStyle.normal;
            
            dashedBoxStyle = new GUIStyle(diagramBoxStyle);
            dashedBoxStyle.normal.background = CreateBoxWithBorders(colorList[2], colorList[3], 128, 2, true, 8);
            dashedBoxStyle.normal.scaledBackgrounds = new Texture2D[1] { CreateBoxWithBorders(colorList[2], colorList[3], 256, 4, true, 16) };
            dashedBoxStyle.active = dashedBoxStyle.focused = dashedBoxStyle.hover = dashedBoxStyle.onActive = dashedBoxStyle.onFocused = dashedBoxStyle.onHover = dashedBoxStyle.onNormal = dashedBoxStyle.normal;

            smallBoxStyle = new GUIStyle(diagramBoxStyle)
            {
                alignment = TextAnchor.MiddleCenter
            };

            smallDashedBoxStyle = new GUIStyle(dashedBoxStyle)
            {
                alignment = TextAnchor.MiddleCenter
            };

            labelStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter
            };
            labelStyle.normal.textColor = Color.white;
            labelStyle.active = labelStyle.focused = labelStyle.hover = labelStyle.onActive = labelStyle.onFocused = labelStyle.onHover = labelStyle.onNormal = labelStyle.normal;


            leftAlignLabelStyle = new GUIStyle(GUI.skin.label)
            {
                wordWrap = true
            };
            leftAlignLabelStyle.normal.textColor = Color.white;
            leftAlignLabelStyle.active = leftAlignLabelStyle.focused = leftAlignLabelStyle.hover = leftAlignLabelStyle.onActive = leftAlignLabelStyle.onFocused = leftAlignLabelStyle.onHover = leftAlignLabelStyle.onNormal = leftAlignLabelStyle.normal;

            toggleStyle = new GUIStyle(GUI.skin.toggle);
            toggleStyle.normal.textColor = Color.white;
            toggleStyle.active = toggleStyle.focused = toggleStyle.hover = toggleStyle.onActive = toggleStyle.onFocused = toggleStyle.onHover = toggleStyle.onNormal = toggleStyle.normal;

            intFieldStyle = new GUIStyle(GUI.skin.textField);
            intFieldStyle.normal.textColor = Color.black;
            intFieldStyle.active = intFieldStyle.focused = intFieldStyle.hover = intFieldStyle.onActive = intFieldStyle.onFocused = intFieldStyle.onHover = intFieldStyle.onNormal = intFieldStyle.normal;

            infoboxRect = new Rect((Screen.width * dpiScaleFactor) - 270, 10, 250, 250);
            movingInfoBox = false;

        }

        private void DPIChanged()
        {

            dpiScaleFactor = 1f / EditorGUIUtility.pixelsPerPoint;
        }

        private static async void WaitForJobDone(Thread _t)
        {
            while (_t.IsAlive)
            {
                await Task.Yield();
            }

            loadingDescription = "";
            loadingProgress = 0;
            callRepaint = true;
        }

        private void RefreshSettings()
        {
            filterType = (FilterType)PlayerPrefs.GetInt("DiagramFilterType", 0);
            string iPaths = PlayerPrefs.GetString("DiagramIncludedPaths", "");
            string ePaths = PlayerPrefs.GetString("DiagramExcludedPaths", "");
            includedPaths = new List<string>();
            includedPaths.AddRange(iPaths.Split(','));
            excludedPaths = new List<string>();
            excludedPaths.AddRange(ePaths.Split(','));
            for (int i = 0; i < includedPaths.Count; ++i)
            {
                includedPaths[i] = includedPaths[i].Trim();
                if (includedPaths[i].Length == 0)
                {
                    includedPaths.RemoveAt(i);
                    --i;
                }

            }
            for (int i = 0; i < excludedPaths.Count; ++i)
            {
                excludedPaths[i] = excludedPaths[i].Trim();
                if (excludedPaths[i].Length == 0)
                {
                    excludedPaths.RemoveAt(i);
                    --i;
                }
            }

            EasyDependencyDiagramsSettings.settingsUpdated = false;

            ResetCachedDiagram();

            //updating colors
            colorList = new Color[12];
            string colorString = "#" + PlayerPrefs.GetString("DiagramInfoBoxColor", "");
            if (!ColorUtility.TryParseHtmlString(colorString, out colorList[0]))
            {
                colorList[0] = new Color(96f / 255f, 96f / 255f, 96f / 255f, 1f);
                PlayerPrefs.SetString("DiagramInfoBoxColor", ColorUtility.ToHtmlStringRGBA(colorList[0]));
            }

            colorString = "#" + PlayerPrefs.GetString("DiagramHighlightedColor", "");
            if (!ColorUtility.TryParseHtmlString(colorString, out colorList[1]))
            {
                colorList[1] = new Color(251f / 255f, 118f / 255f, 167f / 255f, 1f);
                PlayerPrefs.SetString("DiagramHighlightedColor", ColorUtility.ToHtmlStringRGBA(colorList[1]));
            }

            colorString = "#" + PlayerPrefs.GetString("DiagramBoxColor", "");
            if (!ColorUtility.TryParseHtmlString(colorString, out colorList[2]))
            {
                colorList[2] = new Color(70f / 255f, 66f / 255f, 89f / 255f, 1f);
                PlayerPrefs.SetString("DiagramBoxColor", ColorUtility.ToHtmlStringRGBA(colorList[2]));
            }

            colorString = "#" + PlayerPrefs.GetString("DiagramBoxBorderColor", "");
            if (!ColorUtility.TryParseHtmlString(colorString, out colorList[3]))
            {
                colorList[3] = new Color(227f / 255f, 234f / 255f, 251f / 255f, 1f);
                PlayerPrefs.SetString("DiagramBoxBorderColor", ColorUtility.ToHtmlStringRGBA(colorList[3]));
            }

            colorString = "#" + PlayerPrefs.GetString("DiagramDependencyIncomingColor", "");
            if (!ColorUtility.TryParseHtmlString(colorString, out colorList[4]))
            {
                colorList[4] = new Color(149f / 255f, 70f / 255f, 99f / 255f, 1f);
                PlayerPrefs.SetString("DiagramDependencyIncomingColor", ColorUtility.ToHtmlStringRGBA(colorList[4]));
            }

            colorString = "#" + PlayerPrefs.GetString("DiagramDependencyOutgoingColor", "");
            if (!ColorUtility.TryParseHtmlString(colorString, out colorList[5]))
            {
                colorList[5] = new Color(149f / 255f, 70f / 255f, 99f / 255f, 1f);
                PlayerPrefs.SetString("DiagramDependencyOutgoingColor", ColorUtility.ToHtmlStringRGBA(colorList[5]));
            }

            colorString = "#" + PlayerPrefs.GetString("DiagramDependencyTwoWayColor", "");
            if (!ColorUtility.TryParseHtmlString(colorString, out colorList[6]))
            {
                colorList[6] = new Color(149f / 255f, 70f / 255f, 99f / 255f, 1f);
                PlayerPrefs.SetString("DiagramDependencyTwoWayColor", ColorUtility.ToHtmlStringRGBA(colorList[6]));
            }

            colorString = "#" + PlayerPrefs.GetString("DiagramDependencyColor", "");
            if (!ColorUtility.TryParseHtmlString(colorString, out colorList[7]))
            {
                colorList[7] = new Color(149f / 255f, 70f / 255f, 99f / 255f, 1f);
                PlayerPrefs.SetString("DiagramDependencyColor", ColorUtility.ToHtmlStringRGBA(colorList[7]));
            }

            colorString = "#" + PlayerPrefs.GetString("DiagramAssociationColor", "");
            if (!ColorUtility.TryParseHtmlString(colorString, out colorList[8]))
            {
                colorList[8] = new Color(149f / 255f, 70f / 255f, 99f / 255f, 1f);
                PlayerPrefs.SetString("DiagramAssociationColor", ColorUtility.ToHtmlStringRGBA(colorList[8]));
            }

            colorString = "#" + PlayerPrefs.GetString("DiagramAggregationColor", "");
            if (!ColorUtility.TryParseHtmlString(colorString, out colorList[9]))
            {
                colorList[9] = new Color(149f / 255f, 70f / 255f, 99f / 255f, 1f);
                PlayerPrefs.SetString("DiagramAggregationColor", ColorUtility.ToHtmlStringRGBA(colorList[9]));
            }

            colorString = "#" + PlayerPrefs.GetString("DiagramInheritanceColor", "");
            if (!ColorUtility.TryParseHtmlString(colorString, out colorList[10]))
            {
                colorList[10] = new Color(149f / 255f, 70f / 255f, 99f / 255f, 1f);
                PlayerPrefs.SetString("DiagramInheritanceColor", ColorUtility.ToHtmlStringRGBA(colorList[10]));
            }

            colorString = "#" + PlayerPrefs.GetString("DiagramCompositionColor", "");
            if (!ColorUtility.TryParseHtmlString(colorString, out colorList[11]))
            {
                colorList[11] = new Color(149f / 255f, 70f / 255f, 99f / 255f, 1f);
                PlayerPrefs.SetString("DiagramCompositionColor", ColorUtility.ToHtmlStringRGBA(colorList[11]));
            }

            //updating relevant skins
            if (infoBoxStyle != null)
            {
                infoBoxStyle.normal.background = new Texture2D(1, 1);
                infoBoxStyle.normal.background.SetPixels32(new Color32[1] { colorList[0] });
                infoBoxStyle.normal.background.Apply();
                infoBoxStyle.normal.scaledBackgrounds = new Texture2D[1] { infoBoxStyle.normal.background };
                infoBoxStyle.active = infoBoxStyle.focused = infoBoxStyle.hover = infoBoxStyle.onActive = infoBoxStyle.onFocused = infoBoxStyle.onHover = infoBoxStyle.onNormal = infoBoxStyle.normal;

                diagramBoxStyle.normal.background = CreateBoxWithBorders(colorList[2], colorList[3], 128, 2, false, 2);
                diagramBoxStyle.normal.scaledBackgrounds = new Texture2D[1] { CreateBoxWithBorders(colorList[2], colorList[3], 256, 4, false, 4) };
                diagramBoxStyle.active = diagramBoxStyle.focused = diagramBoxStyle.hover = diagramBoxStyle.onActive = diagramBoxStyle.onFocused = diagramBoxStyle.onHover = diagramBoxStyle.onNormal = diagramBoxStyle.normal;

                dashedBoxStyle.normal.background = CreateBoxWithBorders(colorList[2], colorList[3], 128, 2, true, 8);
                dashedBoxStyle.normal.scaledBackgrounds = new Texture2D[1] { CreateBoxWithBorders(colorList[2], colorList[3], 256, 4, true, 16) };
                dashedBoxStyle.active = dashedBoxStyle.focused = dashedBoxStyle.hover = dashedBoxStyle.onActive = dashedBoxStyle.onFocused = dashedBoxStyle.onHover = dashedBoxStyle.onNormal = dashedBoxStyle.normal;

                smallBoxStyle = new GUIStyle(diagramBoxStyle)
                {
                    alignment = TextAnchor.MiddleCenter
                };

                smallDashedBoxStyle = new GUIStyle(dashedBoxStyle)
                {
                    alignment = TextAnchor.MiddleCenter
                };
            }
        }

        private void Update()
        {
            if (callRepaint)
            {
                callRepaint = false;
                Repaint();
            }
        }

        void OnGUI()
        {

            if (EasyDependencyDiagramsSettings.settingsUpdated)
            {
                RefreshSettings();
            }

            if (ecd == null)
            {
                wantsMouseMove = false;
                ResetCachedDiagram();
                InitWindow();
            }

            if (dpiScaleFactor != (1f / EditorGUIUtility.pixelsPerPoint))
            {
                DPIChanged();
            }

            if (ecd.ActiveJob)
            {
                callRepaint = true;
            }

            if (!ecd.ActiveJob && displayedData != ecd.latestJob && ecd.latestJob != null)
            {
                displayedData = ecd.latestJob;
                displayedData.path = targetRoot;

                if (displayedData.type.Equals("Folder"))
                    scrollAreaDimensions = displayedData.GetFolderDimensions();
                else
                    scrollAreaDimensions = displayedData.GetDataDimensions() + new Vector2Int(1, 0);

                scrollAreaDimensions.x = 60 * scrollAreaDimensions.x + 100;
                scrollAreaDimensions.y = 100 * scrollAreaDimensions.y + 10;

                //collapsing everything by default in big trees
                if (displayedData.FileFolderChildCount() > 100)
                    collapseDefault = true;
                else
                    collapseDefault = false;

                callRepaint = true;
            }

            if (targetRoot != activeTarget)
            {
                if (targetRoot.Length > 2 && targetRoot.Contains(".cs"))
                {
                    //File
                    displayedData = null;
                    activeTarget = targetRoot;
                    ecd.ParseData(targetRoot, false);
                }
                else
                {
                    if (Directory.Exists(targetRoot))
                    {
                        //Folder
                        displayedData = null;
                        activeTarget = targetRoot;
                        ecd.ParseData(targetRoot, mode != DiagramDisplayMode.folderDependencies);
                    }
                }

                callRepaint = true;
                return;
            }

            InfoBoxClickHandler();
             
            #region header

            bool compactMode = (Screen.width * dpiScaleFactor) < 750;
            int buttonY = compactMode ? 70 : 20;
            int settingsX = compactMode ? 10 : 90;
            diagramOffset = new Vector2(1, buttonY + 60);

            //previous
            EditorGUI.BeginDisabledGroup(targets == null || targets.Count <= 1);
            if (GUI.Button(new Rect(10, buttonY, 30, 30), new GUIContent(prevIcon, "Previous")) && !movingInfoBox)
            {
                if (targets.Count > 1)
                {
                    scopeTargetRoot = " ";
                    relevantScopeElements = new List<EasyDependencyDiagrams.DataElement>();
                    allScopeElements = new List<EasyDependencyDiagrams.DataElement>();
                    targets.RemoveAt(targets.Count - 1);
                    targetRoot = targets[^1];
                    PlayerPrefs.SetInt("DiagramTargetCount", targets.Count);
                    PlayerPrefs.SetString("DiagramTarget" + (targets.Count - 1), targets[^1]);
                    PlayerPrefs.SetString("DiagramTargetRoot", targetRoot);

                    FetchDiagramScope(true);
                    return;
                }
            }
            EditorGUI.EndDisabledGroup();
            if (targets != null && targets.Count > 1 && !movingInfoBox)
            EditorGUIUtility.AddCursorRect(new Rect(10, buttonY, 30, 30), MouseCursor.Link);

            //parent
            EditorGUI.BeginDisabledGroup(targetRoot.Equals(projectRoot));
            if (GUI.Button(new Rect(50, buttonY, 30, 30), new GUIContent(parentIcon, "Parent")) && !movingInfoBox)
            {
                string parent = "";
                if (targetRoot.LastIndexOf('/') > 0)
                    parent = targetRoot.Substring(0, targetRoot.LastIndexOf('/'));
                if (parent.Length == 0)
                {
                    DirectoryInfo parentFolder = Directory.GetParent(targetRoot);
                    parent = parentFolder.FullName;
                }
                scopeTargetRoot = " ";
                targetRoot = parent;
                PlayerPrefs.SetString("DiagramTargetRoot", targetRoot);
                targets.Add(parent);
                PlayerPrefs.SetInt("DiagramTargetCount", targets.Count);
                PlayerPrefs.SetString("DiagramTarget" + (targets.Count - 1), targets[^1]);
                FetchDiagramScope(true);
                return;
            }
            EditorGUI.EndDisabledGroup();
            if (!targetRoot.Equals(projectRoot) && !movingInfoBox)
                EditorGUIUtility.AddCursorRect(new Rect(50, buttonY, 30, 30), MouseCursor.Link);

            //display mode
            DiagramDisplayMode oldMode = mode;
            EditorGUI.BeginDisabledGroup(displayedData == null);
            string targetType = "None";
            if (displayedData != null)
                targetType = displayedData.type;
            EditorGUI.LabelField(new Rect(settingsX, 10, 200, 20), new GUIContent("Display mode (" + targetType + ")", "Diagram display mode"));
            switch (targetType)
            {
                case "Folder":
                    FolderDisplayMode folderValue = FolderDisplayMode.tree;
                    if (System.Enum.IsDefined(typeof(FolderDisplayMode), (int)mode))
                        folderValue = (FolderDisplayMode)mode;

                    mode = (DiagramDisplayMode)EditorGUI.EnumPopup(new Rect(settingsX, 30, 200, 30), folderValue);
                    break;

                case "File":
                    FileDisplayMode fileValue = FileDisplayMode.tree;
                    if (System.Enum.IsDefined(typeof(FileDisplayMode), (int)mode))
                        fileValue = (FileDisplayMode)mode;

                    mode = (DiagramDisplayMode)EditorGUI.EnumPopup(new Rect(settingsX, 30, 200, 30), fileValue);
                    break;

                case "Namespace":
                    NamespaceDisplayMode namespaceValue = NamespaceDisplayMode.tree;
                    if (System.Enum.IsDefined(typeof(NamespaceDisplayMode), (int)mode))
                        namespaceValue = (NamespaceDisplayMode)mode;

                    mode = (DiagramDisplayMode)EditorGUI.EnumPopup(new Rect(settingsX, 30, 200, 30), namespaceValue);
                    break;

                case "Class":
                    ClassDisplayMode classValue = ClassDisplayMode.tree;
                    if (System.Enum.IsDefined(typeof(ClassDisplayMode), (int)mode))
                        classValue = (ClassDisplayMode)mode;

                    mode = (DiagramDisplayMode)EditorGUI.EnumPopup(new Rect(settingsX, 30, 200, 30), classValue);
                    break;

                case "None":
                    mode = (DiagramDisplayMode)EditorGUI.EnumPopup(new Rect(settingsX, 30, 200, 30), mode);
                    break;
            }
            EditorGUI.EndDisabledGroup();

            if (!movingInfoBox)
                EditorGUIUtility.AddCursorRect(new Rect(settingsX, 30, 200, 30), MouseCursor.Arrow);

            if (oldMode != mode)
            {
                PlayerPrefs.SetInt("DiagramDisplayMode", (int)mode);
                FetchDiagramScope(true);
                if (mode == DiagramDisplayMode.folderDependencies)
                {
                    displayedData = null;
                    ecd.ParseData(targetRoot, false);
                }
                return;
            }


            //project root
            EditorGUI.BeginDisabledGroup(displayedData == null || (ecd != null && ecd.ActiveJob) || (ecdScope != null && ecdScope.ActiveJob));
            EditorGUI.LabelField(new Rect(210 + settingsX, 10, 100, 20), new GUIContent("Project root", "The root folder of the relevant project. The tool will not search anything beyond this folder."));
            EditorGUI.BeginDisabledGroup(true);
            string visualProjectRoot = projectRoot;
            if (projectRoot.StartsWith(Application.dataPath))
                visualProjectRoot = "Assets" + projectRoot[Application.dataPath.Length..];
            EditorGUI.TextField(new Rect(210 + settingsX, 30, 120, 20), visualProjectRoot);
            EditorGUI.EndDisabledGroup();
            if (GUI.Button(new Rect(340 + settingsX, 20, 30, 30), new GUIContent(smallFolderIcon, "Change project root")))
            {
                if (!movingInfoBox)
                {
                    string newRoot = EditorUtility.OpenFolderPanel("Select project root folder", projectRoot, "");
                    if (newRoot.Length > 0 && newRoot.StartsWith(Application.dataPath))
                    {
                        projectRoot = newRoot;
                        PlayerPrefs.SetString("DiagramProjectRoot", projectRoot);
                        if (!targetRoot.StartsWith(projectRoot))
                        {
                            targetRoot = projectRoot;
                            PlayerPrefs.SetString("DiagramTargetRoot", targetRoot);
                            mode = DiagramDisplayMode.tree;
                            targets = new List<string>();
                            PlayerPrefs.SetInt("DiagramTargetCount", 0);
                            PlayerPrefs.SetString("DiagramTarget" + 0, "");
                            ResetCachedDiagram();
                            return;
                        }
                        else
                        {
                            targets = new List<string>() { targetRoot };
                            PlayerPrefs.SetInt("DiagramTargetCount", targets.Count);
                            PlayerPrefs.SetString("DiagramTarget" + (targets.Count - 1), targets[^1]);
                            FetchDiagramScope(true);
                        }
                    }
                }
            }
            EditorGUI.EndDisabledGroup();

            if (!movingInfoBox)
                EditorGUIUtility.AddCursorRect(new Rect(340 + settingsX, buttonY, 30, 30), MouseCursor.Link);

            //loading indicator
            if ((ecd != null && ecd.ActiveJob) || (ecdScope != null && ecdScope.ActiveJob))
            {
                float percentage = 0;
                if (ecd != null && ecd.ActiveJob)
                {
                    percentage = ecd.Progress;
                    loadingDescription = "Parsing main target";
                }
                else if (ecdScope != null && ecdScope.ActiveJob)
                {
                    percentage = ecdScope.Progress;
                    loadingDescription = "Parsing project scope";
                }
                loadingProgress = (int)(percentage * 100f);
            }
            else if (loadingDescription.Equals("Parsing main target") || loadingDescription.Equals("Parsing project scope"))
            {
                loadingDescription = "";
                loadingProgress = 0;
            }

            if (loadingDescription != null && loadingDescription.Length > 0)
            {
                //parent
                if (GUI.Button(new Rect(10, buttonY + 60, 30, 30), new GUIContent(stopIcon, "Stop")) && !movingInfoBox)
                {
                    Stop();
                    ResetCachedDiagram();
                    relevantScopeElements = new List<EasyDependencyDiagrams.DataElement>();
                    allScopeElements = new List<EasyDependencyDiagrams.DataElement>();
                    mode = DiagramDisplayMode.tree;
                    PlayerPrefs.SetInt("DiagramDisplayMode", 0);
                    targetRoot = projectRoot;
                    activeTarget = targetRoot;
                    PlayerPrefs.SetString("DiagramTargetRoot", projectRoot);
                    InitWindow();
                    return;
                }
                EditorGUIUtility.AddCursorRect(new Rect(10, buttonY + 60, 30, 30), MouseCursor.Link);

                EditorGUI.ProgressBar(new Rect(50, buttonY + 60, 250, 30), loadingProgress / 100f, loadingDescription);
            }
            #endregion

            //checking for scroll event
            if (Event.current.isScrollWheel && mode != DiagramDisplayMode.tree)
            {
                zoomScale -= (Event.current.delta.y * zoomScale) / 100f;
                if (zoomScale < 0.5f)
                    zoomScale = 0.5f;
                else if (zoomScale > 1.4f)
                    zoomScale = 1.4f;

                Event.current.Use();
            }

            if (displayedData != null)
            {
                GUI.EndGroup();
                if (mode == DiagramDisplayMode.tree)
                    diagramOffset.x = 10;
                else
                    diagramOffset.x = 1;

                GUI.BeginGroup(new Rect(diagramOffset.x / zoomScale, 21f + (diagramOffset.y) / zoomScale, ((Screen.width * dpiScaleFactor) - diagramOffset.x) /zoomScale, ((Screen.height * dpiScaleFactor) - 23f - diagramOffset.y) / zoomScale), GUIStyle.none);

                //Setting the zoom matrix of the content
                Matrix4x4 oldMatrix = GUI.matrix;
                Matrix4x4 Translation = Matrix4x4.TRS(new Vector2(0, 21), Quaternion.identity, Vector3.one);
                Matrix4x4 Scale = Matrix4x4.Scale(new Vector3(zoomScale, zoomScale, 1.0f));
                GUI.matrix = Translation * Scale * Translation.inverse * GUI.matrix;

                if (mode == DiagramDisplayMode.dependencies && (displayedData.type.Equals("Namespace") || displayedData.type.Equals("Class")))
                {
                    DisplayDependencyDiagram();
                }
                else if (mode == DiagramDisplayMode.compositeStructure && displayedData.type.Equals("Class"))
                {
                    DisplayCompositeStructureDiagram();
                }
                else if (mode == DiagramDisplayMode.packageDiagram && displayedData.type.Equals("Namespace"))
                {
                    DisplayPackageDiagram();
                }
                else if (mode == DiagramDisplayMode.classDiagram && displayedData.type.Equals("Class"))
                {
                    DisplayClassDiagram();
                }
                else if (mode == DiagramDisplayMode.componentDiagram && displayedData.type.Equals("File"))
                {
                    DisplayComponentDiagram();
                }
                else if (mode == DiagramDisplayMode.folderDependencies && displayedData.type.Equals("Folder"))
                {
                    DisplayFolderDependencyDiagram();
                }
                else
                {
                    scrollPos = GUI.BeginScrollView(new Rect(0, 0, (Screen.width * dpiScaleFactor) - diagramOffset.x, (Screen.height * dpiScaleFactor) - 23f - diagramOffset.y), scrollPos, new Rect(0, 0, scrollAreaDimensions.x, scrollAreaDimensions.y));
                    //background box
                    GUI.Box(new Rect(0, 0, scrollAreaDimensions.x, scrollAreaDimensions.y), "", diagramBoxStyle);
                    Vector2 dimensions = DisplayStructure(0, 0, displayedData, targetRoot, displayedData.type.Equals("Folder"));

                    scrollAreaDimensions.x = (int)(40 * (dimensions.x + 3) + 20);
                    scrollAreaDimensions.y = (int)(100 * (dimensions.y + 1) + 20);
                    GUI.EndScrollView();
                }

                //reset the matrix
                GUI.matrix = oldMatrix;

                GUI.EndGroup();
                GUI.BeginGroup(new Rect(0, 0, (Screen.width * dpiScaleFactor), (Screen.height * dpiScaleFactor)));

            }
            else
            {
                callRepaint = true;
            }

            DrawInfoBox();
            if (ecd != null && !ecd.ActiveJob && ecdScope != null && !ecdScope.ActiveJob && !(Event.current != null && infoboxRect.Contains(Event.current.mousePosition)))
                EditorGUIUtility.AddCursorRect(new Rect(0, 21f, (Screen.width * dpiScaleFactor), (Screen.height * dpiScaleFactor)), MouseCursor.Pan);

            //Translation of the scroll view
            if (Event.current != null && Event.current.type == EventType.MouseDrag && movedTarget == null && !movingInfoBox && (Event.current.delta.magnitude > 2 || dragOn))
            {
                dragOn = true;
                scrollPos -= Event.current.delta / zoomScale;
                if (scrollPos.x < -200)
                    scrollPos.x = -200;
                if (scrollPos.y < -200)
                    scrollPos.y = -200;
                if (scrollPos.x > scrollAreaDimensions.x - 200)
                    scrollPos.x = scrollAreaDimensions.x - 200;
                if (scrollPos.y > scrollAreaDimensions.y - 200)
                    scrollPos.y = scrollAreaDimensions.y - 200;
                callRepaint = true;
            }
            if (Event.current != null && Event.current.type == EventType.MouseUp)
            {
                dragOn = false;
            }
        }

        void Stop()
        {
            if (ecd != null)
                ecd.Stop();
            if (ecdScope != null)
                ecdScope.Stop();

            if (cacheThread != null && cacheThread.IsAlive)
            {
                caching = false;
                cacheThread.Abort();
            }
        }

        void DrawInfoBox()
        {
            if (previousScreenWidth != (int)(Screen.width * dpiScaleFactor))
            {
                infoboxRect.x = (Screen.width * dpiScaleFactor) - previousScreenWidth + infoboxRect.x;
                previousScreenWidth = (int)(Screen.width * dpiScaleFactor);
            }

            GUI.Box(infoboxRect, GUIContent.none, infoBoxStyle);

            string targetName = displayedData != null ? displayedData.name + ", " + displayedData.type : "None";
            if (highlightedArrow == null)
                GUI.Label(new Rect(infoboxRect.x + 30, infoboxRect.y + 5, 200, 15), new GUIContent(targetName, targetName), labelStyle);
            else
            {
                GUI.Label(new Rect(infoboxRect.x + 30, infoboxRect.y + 5, 200, 15), "Connection", labelStyle);
            }

            if (GUI.Button(new Rect(infoboxRect.x + 225, infoboxRect.y + 5, 20, 20), new GUIContent(minimizeIcon, "Minimize")))
            {
                if (infoboxRect.height == 30)
                    infoboxRect.height = 250;
                else
                    infoboxRect.height = 30;
            }

            if (displayedData != null && highlightedArrow == null && displayedData.type != "Folder")
            {
                Rect VSButtonRect = new Rect(infoboxRect.xMin + 5, infoboxRect.yMin + 5, 20, 20);
                if (GUI.Button(VSButtonRect, new GUIContent(editorIcon, "Open in code editor")))
                {
                    if (displayedData.path.StartsWith(projectRoot) && displayedData.path.Contains(".cs"))
                    {
                        int startInd = displayedData.path.IndexOf("/Assets") + 1;
                        if (startInd > 0)
                        {
                            string filePath = displayedData.path[startInd..(displayedData.path.IndexOf(".cs") + 3)];
                            if (File.Exists(filePath))
                            {
                                int line = ecd.FindLineNumber(filePath, displayedData.fullLine);
                                AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath(filePath, typeof(Object)), line);
                            }
                        }
                    }
                }
                EditorGUIUtility.AddCursorRect(VSButtonRect, MouseCursor.Link);
            }

            EditorGUIUtility.AddCursorRect(infoboxRect, MouseCursor.Arrow);

            if (infoboxRect.height > 30)
            {
                //no matter what mode it is, if an arrow is selected, display information about that arrow
                if (displayedData != null && highlightedArrow != null)
                {
                    GUI.Label(new Rect(infoboxRect.x + 10, infoboxRect.y + 40, 230, 20), "From: " + highlightedArrow.startName, leftAlignLabelStyle);
                    GUI.Label(new Rect(infoboxRect.x + 10, infoboxRect.y + 70, 230, 20), "To: " + highlightedArrow.endName, leftAlignLabelStyle);
                    GUI.Label(new Rect(infoboxRect.x + 10, infoboxRect.y + 100, 230, 20), "Two-directional: " + ((highlightedArrow.startArrow && highlightedArrow.endArrow)? "Yes" : "No" ), leftAlignLabelStyle);
                    string typeText = "Normal dependency";
                    if (highlightedArrow.type == 0)
                    {
                        if (mode == DiagramDisplayMode.packageDiagram)
                            typeText = "Access dependency";
                        else
                            typeText = "Basic dependency";
                    }
                    else if (highlightedArrow.type == 1)
                    {
                        typeText = "Association";
                    }
                    else if (highlightedArrow.type == 2)
                    {
                        typeText = "Aggregation";
                    }
                    else if (highlightedArrow.type == 3)
                    {
                        typeText = "Inheritance";
                    }
                    else if (highlightedArrow.type == 4)
                    {
                        typeText = "Composition";
                    }
                    else if (highlightedArrow.type == 5)
                    {
                        typeText = "Import dependency";
                    }
                    GUI.Label(new Rect(infoboxRect.x + 10, infoboxRect.y + 130, 230, 20), "Type: " + typeText, leftAlignLabelStyle);


                }
                else if (displayedData != null && (mode == DiagramDisplayMode.tree))
                {
                    GUI.Label(new Rect(infoboxRect.x + 10, infoboxRect.y + 40, 230, 20), "Children: " + displayedData.children.Count, leftAlignLabelStyle);
                }
                else if (mode == DiagramDisplayMode.dependencies)
                {
                    //group namespaces
                    bool tmpGroupNamespaces = GUI.Toggle(new Rect(infoboxRect.x + 10, infoboxRect.y + 40, 230, 20), groupNamespaces, new GUIContent("Namespace together", "Group all classes in a namespace together"), toggleStyle);
                    if (tmpGroupNamespaces != groupNamespaces)
                    {
                        movingInfoBox = false;
                        groupNamespaces = tmpGroupNamespaces;
                        PlayerPrefs.SetInt("DiagramGroupNamespaces", groupNamespaces? 1 : 0);

                        if (caching) 
                            Stop();
                        ResetCachedDiagram();
                    }

                    bool tmpShowInward = GUI.Toggle(new Rect(infoboxRect.x + 10, infoboxRect.y + 70, 230, 20), showInwardDependencies, new GUIContent("Incoming dependencies: " + diagramInwardDependencyCount, "Show dependencies on the target."), toggleStyle);
                    if (tmpShowInward != showInwardDependencies)
                    {
                        movingInfoBox = false;
                        showInwardDependencies = tmpShowInward;
                        PlayerPrefs.SetInt("DiagramInwardDependencies", showInwardDependencies? 1 : 0);
                        ResetCachedDiagram();
                    }

                    bool tmpShowOutward = GUI.Toggle(new Rect(infoboxRect.x + 10, infoboxRect.y + 100, 230, 20), showOutwardDependencies, new GUIContent("Outgoing dependencies: " + diagramOutwardDependencyCount, "Show dependencies of the target."), toggleStyle);
                    if (tmpShowOutward != showOutwardDependencies)
                    {
                        movingInfoBox = false;
                        showOutwardDependencies = tmpShowOutward;
                        PlayerPrefs.SetInt("DiagramOutwardDependencies", showOutwardDependencies ? 1 : 0);
                        ResetCachedDiagram();
                    }

                    bool tmpOnlyShowConnected = GUI.Toggle(new Rect(infoboxRect.x + 10, infoboxRect.y + 130, 230, 20), onlyShowConnectedElements, new GUIContent("Only show connected elements", "Show only classes/namespaces that are related to the target with a dependency."), toggleStyle);
                    if (tmpOnlyShowConnected != onlyShowConnectedElements)
                    {
                        movingInfoBox = false;
                        onlyShowConnectedElements = tmpOnlyShowConnected;
                        PlayerPrefs.SetInt("DiagramShowConnected", onlyShowConnectedElements ? 1 : 0);
                        ResetCachedDiagram();

                    }


                    if (displayedData != null)
                    {
                        string fullName = FullName(displayedData.path);
                        GUI.Label(new Rect(infoboxRect.x + 10, infoboxRect.y + 160, 230, 80), "Full name: " + fullName, leftAlignLabelStyle);
                    }

                }
                else if (mode == DiagramDisplayMode.compositeStructure)
                {
                    bool tmpmovableBoxes = GUI.Toggle(new Rect(infoboxRect.x + 10, infoboxRect.y + 40, 230, 20), movableBoxes, new GUIContent("Allow reordering", "Allow moving of the boxes by dragging them. Arrows are not recalculated while this option is on."), toggleStyle);
                    if (tmpmovableBoxes != movableBoxes)
                    {
                        movableBoxes = tmpmovableBoxes;
                        if (!movableBoxes)
                            recalcArrows = true;
                    }

                    GUI.Label(new Rect(infoboxRect.x + 10, infoboxRect.y + 70, 230, 20), new GUIContent("Associations: " + diagramOutwardDependencyCount, diagramOutwardDependencyCount + " total associations."), leftAlignLabelStyle);
                    
                    GUI.Label(new Rect(infoboxRect.x + 10, infoboxRect.y + 100, 230, 20), new GUIContent("Parts: " + (diagramStructureRects.Count-1), (diagramStructureRects.Count-1) + " parts."), leftAlignLabelStyle);

                }
                else if (mode == DiagramDisplayMode.packageDiagram)
                {
                    bool tmpmovableBoxes = GUI.Toggle(new Rect(infoboxRect.x + 10, infoboxRect.y + 40, 230, 20), movableBoxes, new GUIContent("Allow reordering", "Allow moving of the boxes by dragging them. Arrows are not recalculated while this option is on."), toggleStyle);
                    if (tmpmovableBoxes != movableBoxes)
                    {
                        movableBoxes = tmpmovableBoxes;
                        if (!movableBoxes)
                            recalcArrows = true;
                    }

                    GUI.Label(new Rect(infoboxRect.x + 10, infoboxRect.y + 70, 230, 20), new GUIContent("Dependencies: " + diagramOutwardDependencyCount, diagramOutwardDependencyCount + " total dependencies, " + diagramInwardDependencyCount + " two-way dependencies."), leftAlignLabelStyle);

                    GUI.Label(new Rect(infoboxRect.x + 10, infoboxRect.y + 100, 230, 20), new GUIContent("Packages: " + diagramStructureRects.Count, diagramStructureRects.Count + " packages."), leftAlignLabelStyle);

                }
                else if (mode == DiagramDisplayMode.componentDiagram)
                {
                    bool tmpmovableBoxes = GUI.Toggle(new Rect(infoboxRect.x + 10, infoboxRect.y + 40, 230, 20), movableBoxes, new GUIContent("Allow reordering", "Allow moving of the boxes by dragging them. Arrows are not recalculated while this option is on."), toggleStyle);
                    if (tmpmovableBoxes != movableBoxes)
                    {
                        movableBoxes = tmpmovableBoxes;
                        if (!movableBoxes)
                            recalcArrows = true;
                    }

                    GUI.Label(new Rect(infoboxRect.x + 10, infoboxRect.y + 70, 230, 20), new GUIContent("Dependencies: " + diagramOutwardDependencyCount, diagramOutwardDependencyCount + " total dependencies, " + diagramInwardDependencyCount + " two-way dependencies."), leftAlignLabelStyle);

                    GUI.Label(new Rect(infoboxRect.x + 10, infoboxRect.y + 100, 230, 20), new GUIContent("Components: " + diagramStructureRects.Count, diagramStructureRects.Count + " components."), leftAlignLabelStyle);

                }
                else if (mode == DiagramDisplayMode.classDiagram)
                {
                    infoboxRect.height = 280;

                    bool tmpmovableBoxes = GUI.Toggle(new Rect(infoboxRect.x + 10, infoboxRect.y + 40, 230, 20), movableBoxes, new GUIContent("Allow reordering", "Allow moving of the boxes by dragging them. Arrows are not recalculated while this option is on."), toggleStyle);
                    if (tmpmovableBoxes != movableBoxes)
                    {
                        movableBoxes = tmpmovableBoxes;
                        if (!movableBoxes)
                            recalcArrows = true;
                    }

                    GUI.Label(new Rect(infoboxRect.x + 10, infoboxRect.y + 70, 150, 20), new GUIContent("Relationship depths:", "How far should each relationship be shown."), leftAlignLabelStyle);

                    GUI.Label(new Rect(infoboxRect.x + 10, infoboxRect.y + 100, 100, 20), new GUIContent("Dependencies", "How far should Dependencies be show, in terms of relationships."), leftAlignLabelStyle);
                    int oldClassDiagramDependencyDepth = EditorGUI.IntField(new Rect(infoboxRect.x + 170, infoboxRect.y + 100, 40, 20), "", classDiagramDependencyDepth);
                    if (GUI.Button(new Rect(infoboxRect.x + 140, infoboxRect.y + 100, 20, 20), new GUIContent("-", "Reduce Dependency depth by one")))
                    {
                        oldClassDiagramDependencyDepth--;
                    }
                    if (GUI.Button(new Rect(infoboxRect.x + 220, infoboxRect.y + 100, 20, 20), new GUIContent("+", "Increase Dependency depth by one")))
                    {
                        oldClassDiagramDependencyDepth++;
                    }

                    GUI.Label(new Rect(infoboxRect.x + 10, infoboxRect.y + 130, 100, 20), new GUIContent("Associations", "How far should Associations be show, in terms of relationships."), leftAlignLabelStyle);
                    int oldClassDiagramAssociationDepth = EditorGUI.IntField(new Rect(infoboxRect.x + 170, infoboxRect.y + 130, 40, 20), "", classDiagramAssociationDepth);
                    if (GUI.Button(new Rect(infoboxRect.x + 140, infoboxRect.y + 130, 20, 20), new GUIContent("-", "Reduce Association depth by one")))
                    {
                        oldClassDiagramAssociationDepth--;
                    }
                    if (GUI.Button(new Rect(infoboxRect.x + 220, infoboxRect.y + 130, 20, 20), new GUIContent("+", "Increase Association depth by one")))
                    {
                        oldClassDiagramAssociationDepth++;
                    }

                    GUI.Label(new Rect(infoboxRect.x + 10, infoboxRect.y + 160, 100, 20), new GUIContent("Aggregations", "How far should Aggregations be show, in terms of relationships."), leftAlignLabelStyle);
                    int oldClassDiagramAggregationDepth = EditorGUI.IntField(new Rect(infoboxRect.x + 170, infoboxRect.y + 160, 40, 20), "", classDiagramAggregationDepth);
                    if (GUI.Button(new Rect(infoboxRect.x + 140, infoboxRect.y + 160, 20, 20), new GUIContent("-", "Reduce Aggregation depth by one")))
                    {
                        oldClassDiagramAggregationDepth--;
                    }
                    if (GUI.Button(new Rect(infoboxRect.x + 220, infoboxRect.y + 160, 20, 20), new GUIContent("+", "Increase Aggregation depth by one")))
                    {
                        oldClassDiagramAggregationDepth++;
                    }

                    GUI.Label(new Rect(infoboxRect.x + 10, infoboxRect.y + 190, 100, 20), new GUIContent("Inheritances", "How far should Inheritances be show, in terms of relationships."), leftAlignLabelStyle);
                    int oldClassDiagramInheritanceDepth = EditorGUI.IntField(new Rect(infoboxRect.x + 170, infoboxRect.y + 190, 40, 20), "", classDiagramInheritanceDepth);
                    if (GUI.Button(new Rect(infoboxRect.x + 140, infoboxRect.y + 190, 20, 20), new GUIContent("-", "Reduce Inheritance depth by one")))
                    {
                        oldClassDiagramInheritanceDepth--;
                    }
                    if (GUI.Button(new Rect(infoboxRect.x + 220, infoboxRect.y + 190, 20, 20), new GUIContent("+", "Increase Inheritance depth by one")))
                    {
                        oldClassDiagramInheritanceDepth++;
                    }

                    GUI.Label(new Rect(infoboxRect.x + 10, infoboxRect.y + 220, 100, 20), new GUIContent("Compositions", "How far should Compositions be show, in terms of relationships."), leftAlignLabelStyle);
                    int oldClassDiagramCompositionDepth = EditorGUI.IntField(new Rect(infoboxRect.x + 170, infoboxRect.y + 220, 40, 20), "", classDiagramCompositionDepth);
                    if (GUI.Button(new Rect(infoboxRect.x + 140, infoboxRect.y + 220, 20, 20), new GUIContent("-", "Reduce Composition depth by one")))
                    {
                        oldClassDiagramCompositionDepth--;
                    }
                    if (GUI.Button(new Rect(infoboxRect.x + 220, infoboxRect.y + 220, 20, 20), new GUIContent("+", "Increase Composition depth by one")))
                    {
                        oldClassDiagramCompositionDepth++;
                    }

                    GUI.Label(new Rect(infoboxRect.x + 10, infoboxRect.y + 250, 100, 20), new GUIContent("Attribute depth", "How far should attributes and methods be show, in terms of relationships."), leftAlignLabelStyle);
                    int oldClassDiagramAttributeDepth = EditorGUI.IntField(new Rect(infoboxRect.x + 170, infoboxRect.y + 250, 40, 20), "", classDiagramAttributeDepth);
                    if (GUI.Button(new Rect(infoboxRect.x + 140, infoboxRect.y + 250, 20, 20), new GUIContent("-", "Reduce Attribute depth by one")))
                    {
                        oldClassDiagramAttributeDepth--;
                    }
                    if (GUI.Button(new Rect(infoboxRect.x + 220, infoboxRect.y + 250, 20, 20), new GUIContent("+", "Increase Attribute depth by one")))
                    {
                        oldClassDiagramAttributeDepth++;
                    }

                    if (oldClassDiagramDependencyDepth < 0)
                        oldClassDiagramDependencyDepth = 0;
                    if (oldClassDiagramAssociationDepth < 0)
                        oldClassDiagramAssociationDepth = 0;
                    if (oldClassDiagramAggregationDepth < 0)
                        oldClassDiagramAggregationDepth = 0;
                    if (oldClassDiagramInheritanceDepth < 0)
                        oldClassDiagramInheritanceDepth = 0;
                    if (oldClassDiagramCompositionDepth < 0)
                        oldClassDiagramCompositionDepth = 0;
                    if (oldClassDiagramAttributeDepth < 0)
                        oldClassDiagramAttributeDepth = 0;

                    if (oldClassDiagramDependencyDepth != classDiagramDependencyDepth)
                    {
                        classDiagramDependencyDepth = oldClassDiagramDependencyDepth;
                        PlayerPrefs.SetInt("DiagramDependencyDepth", classDiagramDependencyDepth);
                        ResetCachedDiagram();
                    }

                    if (oldClassDiagramAssociationDepth != classDiagramAssociationDepth)
                    {
                        classDiagramAssociationDepth = oldClassDiagramAssociationDepth;
                        PlayerPrefs.SetInt("DiagramAssociationDepth", classDiagramAssociationDepth);
                        ResetCachedDiagram();
                    }

                    if (oldClassDiagramAggregationDepth != classDiagramAggregationDepth)
                    {
                        classDiagramAggregationDepth = oldClassDiagramAggregationDepth;
                        PlayerPrefs.SetInt("DiagramAggregationDepth", classDiagramAggregationDepth);
                        ResetCachedDiagram();
                    }

                    if (oldClassDiagramInheritanceDepth != classDiagramInheritanceDepth)
                    {
                        classDiagramInheritanceDepth = oldClassDiagramInheritanceDepth;
                        PlayerPrefs.SetInt("DiagramInheritanceDepth", classDiagramInheritanceDepth);
                        ResetCachedDiagram();
                    }

                    if (oldClassDiagramCompositionDepth != classDiagramCompositionDepth)
                    {
                        classDiagramCompositionDepth = oldClassDiagramCompositionDepth;
                        PlayerPrefs.SetInt("DiagramCompositionDepth", classDiagramCompositionDepth);
                        ResetCachedDiagram();
                    }

                    if (oldClassDiagramAttributeDepth != classDiagramAttributeDepth)
                    {
                        classDiagramAttributeDepth = oldClassDiagramAttributeDepth;
                        PlayerPrefs.SetInt("DiagramAttributeDepth", classDiagramAttributeDepth);
                        ResetCachedDiagram();
                    }
                }
                else if (mode == DiagramDisplayMode.folderDependencies)
                {
                    bool tmpShowInward = GUI.Toggle(new Rect(infoboxRect.x + 10, infoboxRect.y + 40, 230, 20), showInwardDependencies, new GUIContent("Incoming dependencies: " + diagramInwardDependencyCount, "Show dependencies on the target."), toggleStyle);
                    if (tmpShowInward != showInwardDependencies)
                    {
                        movingInfoBox = false;
                        showInwardDependencies = tmpShowInward;
                        PlayerPrefs.SetInt("DiagramInwardDependencies", showInwardDependencies ? 1 : 0);
                        ResetCachedDiagram();
                    }

                    bool tmpShowOutward = GUI.Toggle(new Rect(infoboxRect.x + 10, infoboxRect.y + 70, 230, 20), showOutwardDependencies, new GUIContent("Outgoing dependencies: " + diagramOutwardDependencyCount, "Show dependencies of the target."), toggleStyle);
                    if (tmpShowOutward != showOutwardDependencies)
                    {
                        movingInfoBox = false;
                        showOutwardDependencies = tmpShowOutward;
                        PlayerPrefs.SetInt("DiagramOutwardDependencies", showOutwardDependencies ? 1 : 0);
                        ResetCachedDiagram();
                    }

                    bool tmpOnlyShowConnected = GUI.Toggle(new Rect(infoboxRect.x + 10, infoboxRect.y + 100, 230, 20), onlyShowConnectedElements, new GUIContent("Only show connected elements", "Show only classes/namespaces that are related to the target with a dependency."), toggleStyle);
                    if (tmpOnlyShowConnected != onlyShowConnectedElements)
                    {
                        movingInfoBox = false;
                        onlyShowConnectedElements = tmpOnlyShowConnected;
                        PlayerPrefs.SetInt("DiagramShowConnected", onlyShowConnectedElements ? 1 : 0);
                        ResetCachedDiagram();

                    }


                    if (displayedData != null)
                    {
                        string fullName = FullName(displayedData.path);
                        if (fullName.Equals(projectRoot))
                            fullName = displayedData.name;
                        else if (fullName.StartsWith(projectRoot))
                            fullName = fullName[(projectRoot.Length + 1)..];
                        GUI.Label(new Rect(infoboxRect.x + 10, infoboxRect.y + 130, 230, 80), "Full name:\n" + fullName, leftAlignLabelStyle);
                    }

                }
            }

            //handling the movement of the infobox
            if (Event.current != null && Event.current.type == EventType.MouseDrag && movingInfoBox)
            {
                infoboxRect.x += Event.current.delta.x;
                infoboxRect.y += Event.current.delta.y;
                callRepaint = true;
            }

            if (infoboxRect.x + infoboxRect.width > (Screen.width * dpiScaleFactor) - 20)
                infoboxRect.x = (Screen.width * dpiScaleFactor) - 20 - infoboxRect.width;

            if (infoboxRect.x < 10)
                infoboxRect.x = 10;

            if (infoboxRect.y + infoboxRect.height > (Screen.height * dpiScaleFactor) - 26)
                infoboxRect.y = (Screen.height * dpiScaleFactor) - 26 - infoboxRect.height;

            if (infoboxRect.y < 31)
                infoboxRect.y = 31;

            if (Event.current != null && Event.current.type == EventType.MouseUp)
            {
                movingInfoBox = false;
            }
        }

        void InfoBoxClickHandler()
        {
            if (movableBoxes)
            {
                movingInfoBox = false;
            }
            else if (Event.current != null && Event.current.type == EventType.MouseDown)
            {
                if (infoboxRect.Contains(Event.current.mousePosition + new Vector2(0, 21)))
                {
                    movingInfoBox = true;
                }
                else
                {
                    movingInfoBox = false;
                }
            }
        }

        private List<EasyDependencyDiagrams.DataElement> PruneRelevantElements(List<EasyDependencyDiagrams.DataElement> originalElements, List<EasyDependencyDiagrams.DataElement> newElements, List<EasyDependencyDiagrams.DataElement> relevantElements)
        {
            //pruning the relevant elements based on the active filter
            List<EasyDependencyDiagrams.DataElement> prunedRelevantElements = new List<EasyDependencyDiagrams.DataElement>();
            prunedRelevantElements.AddRange(originalElements);
            
            List<EasyDependencyDiagrams.DataElement> newRelevantElements = new List<EasyDependencyDiagrams.DataElement>();
            for (int i = 0; i < relevantElements.Count; ++i)
            {
                //including separately included paths
                int inclusionType = AlwaysIncludeOrExcludePath(relevantElements[i].path);


                if (inclusionType == 1)
                {
                    newRelevantElements.Add(relevantElements[i]);
                    continue;
                }
                else if (inclusionType == 2)
                {
                    //checking if this excluded element has children that are included
                    if (mode != DiagramDisplayMode.componentDiagram && mode != DiagramDisplayMode.tree && mode != DiagramDisplayMode.folderDependencies)
                    {
                        for (int k = 0; k < excludedPaths.Count; ++k)
                        {
                            if (relevantElements[i].path.Contains(excludedPaths[k]))
                            {
                                //this element should be excluded
                                //checking all its children if they overwrite the exclusion
                                List<EasyDependencyDiagrams.DataElement> elementChildren = ChildClassesOfDataElement(relevantElements[i], relevantElements[i].path);
                                for (int j = 0; j < elementChildren.Count; ++j)
                                {
                                    if (AlwaysIncludeOrExcludePath(elementChildren[j].path) < 2)
                                    {
                                        //include this child class even if the parent is excluded
                                        newRelevantElements.Add(elementChildren[j]);
                                    }
                                }
                                break;
                            }
                        }
                    }

                }
                else
                {
                    //if no filter, including the element by default
                    if (filterType == FilterType.none)
                    {
                        newRelevantElements.Add(relevantElements[i]);
                    }
                    //otherwise checking if the filter allows this element
                    else
                    {
                        for (int k = 0; k < newElements.Count; ++k)
                        {
                            bool alreadyIncluded = false;
                            //including elements that the existing elements depend on
                            if (filterType != FilterType.onlyIncluded)
                            {
                                if ((relevantElements[i].type.Equals("Folder") && IsDependantOnFolder(newElements[k], relevantElements[i]))
                                    || (relevantElements[i].type.Equals("File") && IsDependantOnFile(newElements[k].usings, newElements[k].dependencies, newElements[k].path, relevantElements[i])) 
                                    || (!relevantElements[i].type.Equals("Folder")  && !relevantElements[i].type.Equals("File") && IsDependant(newElements[k].usings, newElements[k].dependencies, newElements[k].path, relevantElements[i].path)))
                                {
                                    newRelevantElements.Add(relevantElements[i]);
                                    alreadyIncluded = true;
                                }
                            }

                            //including elements dependant on included elements
                            if (!alreadyIncluded && (filterType == FilterType.loose || (filterType == FilterType.strict && newElements[k].path.Equals(displayedData.path))))
                            {
                                if ((newElements[k].type.Equals("Folder") && IsDependantOnFolder(relevantElements[i], newElements[k]))
                                    || (newElements[k].type.Equals("File") && IsDependantOnFile(relevantElements[i].usings, relevantElements[i].dependencies, relevantElements[i].path, newElements[k])) 
                                    || (!relevantElements[i].type.Equals("Folder") && !relevantElements[i].type.Equals("File") && IsDependant(relevantElements[i].usings, relevantElements[i].dependencies, relevantElements[i].path, newElements[k].path)))
                                {
                                    newRelevantElements.Add(relevantElements[i]);
                                    continue;
                                }
                            }
                        }
                    }
                }
            }

            //taking off any duplicates from newRelevantElements
            for (int i = 0; i < newRelevantElements.Count; ++i)
            {
                bool include = true;

                for (int k = 0; k < originalElements.Count; ++k)
                {
                    if (newRelevantElements[i].path.Equals(originalElements[k].path))
                    {
                        include = false;
                        break;
                    }
                }

                if (include)
                {
                    for (int k = 0; k < newElements.Count; ++k)
                    {
                        if (newRelevantElements[i].path.Equals(newElements[k].path))
                        {
                            include = false;
                            break;
                        }
                    }
                }

                if (!include)
                {
                    newRelevantElements.RemoveAt(i);
                    --i;
                }

            }

            prunedRelevantElements.AddRange(newRelevantElements);

            //if there is no filter or only separately included paths are included, the pruning is done
            if (filterType == FilterType.none || filterType == FilterType.onlyIncluded)
                return prunedRelevantElements;

            //next iteration of pruning
            if (newRelevantElements.Count > 0)
            {
                List<EasyDependencyDiagrams.DataElement> newPrunes = PruneRelevantElements(EasyDependencyDiagrams.CloneList(prunedRelevantElements), newRelevantElements, relevantElements);
                for (int i = 0; i < newPrunes.Count; ++i)
                {
                    bool include = true;
                    for (int k = 0; k < prunedRelevantElements.Count; ++k)
                    {
                        if (newPrunes[i].path.Equals(prunedRelevantElements[k].path))
                        {
                            include = false;
                            break;
                        }
                    }
                    if (!include)
                    {
                        newPrunes.RemoveAt(i);
                        --i;
                    }

                }
                prunedRelevantElements.AddRange(newPrunes);
            } 


            return prunedRelevantElements;
        }

        private List<EasyDependencyDiagrams.DataElement> FindAllScopeElements(EasyDependencyDiagrams.DataElement data)
        {
            List<EasyDependencyDiagrams.DataElement> scopeElements = new List<EasyDependencyDiagrams.DataElement>();

            if ((mode == DiagramDisplayMode.componentDiagram && data.type.Equals("File")) || data.type.Equals("Namespace") || data.type.Equals("Class"))
            {
                scopeElements.Add(data);
            }
            else if (data.children != null && (data.type.Equals("Folder") || (mode != DiagramDisplayMode.componentDiagram && data.type.Equals("File"))))
            {
                bool includedFolder = false;
                foreach (EasyDependencyDiagrams.DataElement child in data.children)
                {
                    child.path = data.path + "/" + child.name;

                    if (!includedFolder && mode == DiagramDisplayMode.folderDependencies && child.type.Equals("File"))
                    {
                        scopeElements.Add(data);
                        includedFolder = true;
                    }
                    else if (mode != DiagramDisplayMode.folderDependencies || child.type.Equals("Folder"))
                    {
                        scopeElements.AddRange(FindAllScopeElements(child));
                    }
                }
            }

            return scopeElements;
        }

        bool FetchDiagramScope(bool forceChange)
        {
            bool targetChanged = forceChange;
            string scopeTarget = projectRoot;
            if (mode == DiagramDisplayMode.tree)
            {
                scopeTarget = targetRoot;
            }
            if (!scopeTargetRoot.Equals(scopeTarget))
            {
                targetChanged = true;
                
            }
            scopeTargetRoot = scopeTarget;

            if (targetChanged)
            {
                //reseting variables relating to the old scope
                Stop();
                ResetCachedDiagram();
                relevantScopeElements = new List<EasyDependencyDiagrams.DataElement>();
                allScopeElements = new List<EasyDependencyDiagrams.DataElement>();
                callRepaint = true;
            }
            return targetChanged;
        }

        void ResetCachedDiagram(bool keepCaching = false)
        {
            if (!keepCaching)
            {
                if (caching)
                    Stop();
            }

            diagramArrows = new List<DiagramArrow>();
            diagramStructureInfos = new List<DiagramStructureInfo>();
            diagramStructureRects = new List<KeyValuePair<DiagramStructureInfo, Rect>>();
            relevantScopeElements = PruneRelevantElements(new List<EasyDependencyDiagrams.DataElement>() { displayedData }, new List<EasyDependencyDiagrams.DataElement>() { displayedData }, allScopeElements);
            highlightedArrow = null;
            movableBoxes = false;
            zoomScale = 1;
            scrollPos = new Vector2(0, 0);

        }

        List<DiagramStructureInfo> FindRecursiveStructures(EasyDependencyDiagrams.DataElement data, bool separateChildren, bool emptyNamespaces)
        {
            List<DiagramStructureInfo> structures = new List<DiagramStructureInfo>();
            List<DiagramStructureInfo> childClassStructures = new List<DiagramStructureInfo>();

            for (int i = 0; i < data.children.Count; ++i)
            {
                data.children[i].path = data.path + "/" + data.children[i].name;

                bool includeChild = false;
                if ((data.children[i].objectType.Equals("class") || data.children[i].objectType.Equals("interface")) && (mode == DiagramDisplayMode.dependencies || mode == DiagramDisplayMode.tree || mode == DiagramDisplayMode.compositeStructure || mode == DiagramDisplayMode.classDiagram || mode == DiagramDisplayMode.componentDiagram || mode == DiagramDisplayMode.folderDependencies))
                    includeChild = true;
                else if ((data.children[i].objectType.Equals("struct") || data.children[i].type.Equals("Variable") || data.children[i].type.Equals("Property")) && (mode == DiagramDisplayMode.compositeStructure || mode == DiagramDisplayMode.classDiagram))
                    includeChild = true;
                else if (data.children[i].type.Equals("Method") && mode == DiagramDisplayMode.classDiagram)
                    includeChild = true;
                else if (data.children[i].type.Equals("Namespace") && (mode == DiagramDisplayMode.packageDiagram || mode == DiagramDisplayMode.dependencies || mode == DiagramDisplayMode.componentDiagram || mode == DiagramDisplayMode.folderDependencies))
                    includeChild = true;
                else if (data.children[i].type.Equals("File") && (mode == DiagramDisplayMode.componentDiagram || mode == DiagramDisplayMode.folderDependencies))
                    includeChild = true;
                else if (data.children[i].type.Equals("Folder") && mode == DiagramDisplayMode.folderDependencies)
                    includeChild = true;

                if (includeChild) 
                {
                    List<DiagramStructureInfo> childStructures = FindRecursiveStructures(data.children[i], separateChildren, emptyNamespaces);
                    if (separateChildren || (data.children[i].type.Equals("Namespace") && mode == DiagramDisplayMode.dependencies))
                    {
                        structures.AddRange(childStructures);
                    }
                    if (!(data.children[i].type.Equals("Namespace") && mode == DiagramDisplayMode.dependencies))
                        childClassStructures.AddRange(childStructures);
                }
                else if (data.children[i].type.Equals("Namespace"))
                {
                    structures.AddRange(FindRecursiveStructures(data.children[i], separateChildren, emptyNamespaces));
                } 
            }

            if (emptyNamespaces || childClassStructures.Count > 0 || !data.type.Equals("Namespace"))
                structures.Add(new DiagramStructureInfo(data.name, FullName(data.path), data.path, data.type, data.objectType, data.prefix.Trim(), data.postFix.Trim(), data.fullLine, childClassStructures, RemoveDuplicates(data.usings), RemoveDuplicates(data.dependencies), 0));

            structures.Reverse();
            return structures;
        }

        void CacheDependencies()
        {
            ResetCachedDiagram(true);

            loadingDescription = "Caching dependency information";
            loadingProgress = 0;

            scrollAreaDimensions = new Vector2Int(460, 460);
            diagramCenterPos = new Vector2Int(10, 10);

            //first element reserved for the main element
            bool separateChildClasses = mode == DiagramDisplayMode.dependencies && (!displayedData.type.Equals("Namespace") && !groupNamespaces);
            List <DiagramStructureInfo> mainStructure = FindRecursiveStructures(displayedData, separateChildClasses, mode == DiagramDisplayMode.dependencies|| mode == DiagramDisplayMode.packageDiagram || mode == DiagramDisplayMode.componentDiagram);
            if (mainStructure.Count > 0)
                diagramMainElement = mainStructure[0];
            else
                diagramMainElement = new DiagramStructureInfo();

            diagramStructureInfos.AddRange(mainStructure);

            List<string> alreadyIncluded = new List<string>();
            foreach (DiagramStructureInfo main in mainStructure)
            {
                if (main.type.Equals("Class"))
                    alreadyIncluded.Add(main.identifier);
                for (int i = 0; i < main.children.Count; ++i)
                {
                    if (main.children[i].type.Equals("Class"))
                        alreadyIncluded.Add(main.children[i].identifier);
                }
            }

            //Finding all the structures in the scope
            for (int i = 0; i < relevantScopeElements.Count; ++i)
            {
                loadingProgress = (int)(i*1f/relevantScopeElements.Count*100f);
                callRepaint = true;

                //skipping main element. it has been included already
                if (relevantScopeElements[i].path.Equals(diagramMainElement.path))
                    continue;

                separateChildClasses = mode == DiagramDisplayMode.dependencies && (!groupNamespaces);
                List<DiagramStructureInfo> structures = FindRecursiveStructures(relevantScopeElements[i], separateChildClasses, mode == DiagramDisplayMode.packageDiagram || mode == DiagramDisplayMode.componentDiagram);

                for (int k = 0; k < structures.Count; ++k)
                {
                    //ignoring already added classes
                    if (alreadyIncluded.Contains(structures[k].identifier) || (!diagramMainElement.type.Equals("Namespace") && diagramStructureInfos[0].identifier.Equals(structures[k].identifier)))
                        continue;

                    bool exists = false;
                    for (int j = 0; j < diagramStructureInfos.Count; ++j)
                    {
                        //Updating existing structure
                        if (diagramStructureInfos[j].identifier.Equals(structures[k].identifier))
                        {
                            diagramStructureInfos[j].name = structures[k].name;
                            foreach (DiagramStructureInfo newChild in structures[k].children)
                            {
                                bool childExists = false;
                                foreach (DiagramStructureInfo oldChild in diagramStructureInfos[j].children)
                                {
                                    if (oldChild.identifier.Equals(newChild.identifier))
                                    {
                                        childExists = true;
                                        break;
                                    }
                                }

                                if (!childExists)
                                {
                                    diagramStructureInfos[j].children.Add(newChild);
                                }
                            }
                            diagramStructureInfos[j].dependencies = RemoveDuplicates(diagramStructureInfos[j].dependencies, structures[k].dependencies);
                            diagramStructureInfos[j].usings = RemoveDuplicates(diagramStructureInfos[j].usings, structures[k].usings);

                            if (mode != DiagramDisplayMode.packageDiagram)
                            {
                                //making sure the child classes aren't added multiple times
                                //removing separate classes if they are already in the mainelement (namespace)
                                if (structures[k].identifier.Equals(structures[0].identifier))
                                {
                                    for (int l = 0; l < structures[k].children.Count; ++l)
                                    {
                                        alreadyIncluded.Add(structures[k].children[l].identifier);
                                    }
                                }
                                //removing child classes if they are already the mainelement (class)
                                for (int l = 0; l < diagramStructureInfos[j].children.Count; ++l)
                                {
                                    if (diagramStructureInfos[j].children[l].identifier.Equals(diagramStructureInfos[0].identifier))
                                    {
                                        diagramStructureInfos[j].children.RemoveAt(l);
                                        break;
                                    }
                                }
                            } 

                            exists = true;

                            break;
                        }
                    }

                    //Adding new structure to the list
                    if (!exists && (mode != DiagramDisplayMode.dependencies || !structures[k].type.Equals("Namespace") || groupNamespaces))
                    {
                        diagramStructureInfos.Add(structures[k]);

                        if (mode != DiagramDisplayMode.packageDiagram)
                        {

                            //removing child classes if they are already the mainelement (class)
                            for (int l = 0; l < diagramStructureInfos[^1].children.Count; ++l)
                            {
                                if (diagramStructureInfos[^1].children[l].identifier.Equals(diagramStructureInfos[0].identifier))
                                {
                                    diagramStructureInfos[^1].children.RemoveAt(l);
                                    break;
                                }
                            }
                            if (mode == DiagramDisplayMode.dependencies && diagramStructureInfos.Count > 1 && diagramStructureInfos[^1].children.Count == 0 && diagramStructureInfos[^1].type.Equals("Namespace"))
                                diagramStructureInfos.RemoveAt(diagramStructureInfos.Count - 1);
                        }
                    }
                }
            }

            callRepaint = true;
            caching = false;
        }

        /// <returns>True if loading is still happening. False if the loading is done.</returns>
        bool LoadScopeData()
        {
            if (loadingDescription.Length == 0)
            {
                loadingDescription = "Fetching project data";
            }
            bool loadingDiagram = true;
            if (activeScopeTarget == scopeTargetRoot && (ecdScope.ActiveJob || ecdScope.latestJob != null))
            {

                if (!ecdScope.ActiveJob)
                {
                    if (ecdScope.latestJob != scopeData)
                        scopeData = ecdScope.latestJob;

                    if (scopeData == ecdScope.latestJob && scopeData != null)
                    {
                        if (allScopeElements.Count < 1)
                        {
                            scopeData.path = activeScopeTarget;
                            allScopeElements = FindAllScopeElements(scopeData);

                            relevantScopeElements = PruneRelevantElements(new List<EasyDependencyDiagrams.DataElement>() { displayedData }, new List<EasyDependencyDiagrams.DataElement>() { displayedData }, allScopeElements);
                        }
                        else if (relevantScopeElements.Count < 1)
                        {
                            relevantScopeElements = PruneRelevantElements(new List<EasyDependencyDiagrams.DataElement>() { displayedData }, new List<EasyDependencyDiagrams.DataElement>() { displayedData }, allScopeElements);
                        }

                        loadingDiagram = false;

                        if (loadingDescription.Equals("Fetching project data"))
                        {
                            loadingDescription = "";
                        }
                    }
                }
                else
                {
                    callRepaint = true;
                }
            }
            else
            {
                FetchDiagramScope(true);
                activeScopeTarget = scopeTargetRoot;
                ecdScope.ParseData(scopeTargetRoot, false);
                callRepaint = true;
            }

            return loadingDiagram;
        }

        void CacheComponentDiagramElements()
        {
            if (!recalcArrows)
            {
                //finding main element that should always be included
                DiagramStructureInfo main = diagramStructureInfos[0];
                for (int i = 0; i < diagramStructureInfos.Count; ++i)
                {
                    if (displayedData != null && diagramStructureInfos[i].path.Equals(displayedData.path))
                    {
                        main = diagramStructureInfos[i];
                        break;
                    }
                }
                List<DiagramStructureInfo> componentDiagramContents = FindComponentDiagramContents(main, new List<string>());

                diagramStructureRects = CalcBoxRects(componentDiagramContents, boxMargin);

            }
            else
            {
                recalcArrows = false;
                movedTarget = null;
                diagramArrows = new List<DiagramArrow>();
                highlightedArrow = null;
                diagramInwardDependencyCount = 0;
                diagramOutwardDependencyCount = 0;
            }

            //finding the offset of the diagram
            diagramCenterPos = new Vector2Int(50, 100);

            for (int i = 0; i < diagramStructureRects.Count; ++i)
            {
                if (diagramStructureRects[i].Value.x < -(diagramCenterPos.x - 50))
                    diagramCenterPos.x = (int)(-diagramStructureRects[i].Value.x + 50);
                if (diagramStructureRects[i].Value.y < -(diagramCenterPos.y - 100))
                    diagramCenterPos.y = (int)(-diagramStructureRects[i].Value.y + 100);
            }
            scrollAreaDimensions = new Vector2Int(20, 20);

            for (int i = 0; i < diagramStructureRects.Count; ++i)
            {
                if (scrollAreaDimensions.x - 20 < diagramCenterPos.x + diagramStructureRects[i].Value.xMax)
                    scrollAreaDimensions.x = (int)(diagramCenterPos.x + diagramStructureRects[i].Value.xMax + 20);

                if (scrollAreaDimensions.y - 20 < diagramCenterPos.y + diagramStructureRects[i].Value.yMax)
                    scrollAreaDimensions.y = (int)(diagramCenterPos.y + diagramStructureRects[i].Value.yMax + 20);
            }

            //finding all arrows that need to be drawn
            bool[][] arrowsToDraw = new bool[diagramStructureRects.Count][];
            List<Rect> arrowObstacles = new List<Rect>();
            int arrowMargin = 15;
            int lineCount = 0;
            for (int i = 0; i < diagramStructureRects.Count; ++i)
            {
                Rect boxRect = new Rect(diagramStructureRects[i].Value.x + diagramCenterPos.x, diagramStructureRects[i].Value.y + diagramCenterPos.y, diagramStructureRects[i].Value.width, diagramStructureRects[i].Value.height);
                Rect obstacleRect = new Rect(boxRect.x - arrowMargin, boxRect.y - arrowMargin, boxRect.width + arrowMargin * 2, boxRect.height + arrowMargin * 2);
                arrowObstacles.Add(obstacleRect);
                arrowsToDraw[i] = new bool[diagramStructureRects.Count];

                //finding dependencies
                for (int k = 0; k < diagramStructureRects.Count; ++k)
                {
                    if (k != i)
                    {
                        List<DiagramStructureInfo> fileChildClasses = ChildClassesOfStructureInfo(diagramStructureRects[k].Key);

                        bool dependant = false;
                        for (int l = 0; l < fileChildClasses.Count; ++l)
                        {
                            if (IsDependant(diagramStructureRects[i].Key.usings, diagramStructureRects[i].Key.dependencies, diagramStructureRects[i].Key.path, fileChildClasses[l].path))
                            {
                                dependant = true;
                                break;
                            }
                        }

                        if (dependant)
                        {
                            arrowsToDraw[i][k] = true;
                            lineCount++;

                            diagramOutwardDependencyCount++;
                            if (i > k && arrowsToDraw[k][i])
                                diagramInwardDependencyCount++;
                        }
                    }
                }
            }

            for (int i = 0; i < arrowsToDraw.Length; ++i)
            {
                loadingProgress = (int)(i * 1f / arrowsToDraw.Length * 100);
                callRepaint = true;

                Rect boxRect = new Rect(diagramStructureRects[i].Value.x + diagramCenterPos.x, diagramStructureRects[i].Value.y + diagramCenterPos.y, diagramStructureRects[i].Value.width, diagramStructureRects[i].Value.height);

                for (int k = 0; k < arrowsToDraw[i].Length; ++k)
                {
                    Rect targetBoxRect = new Rect(diagramStructureRects[k].Value.x + diagramCenterPos.x, diagramStructureRects[k].Value.y + diagramCenterPos.y, diagramStructureRects[k].Value.width, diagramStructureRects[k].Value.height);


                    if (arrowsToDraw[i][k])
                        CreateDiagramArrow(boxRect, targetBoxRect, i, k, arrowObstacles, true, false, 2, diagramStructureRects[i].Key.name, diagramStructureRects[k].Key.name, 0, lineCount, false);
                }
            }
            caching = false;
        }

        void DisplayComponentDiagram()
        {
            if (LoadScopeData())
                return;

            //mouse up event when dragging boxes
            if (movableBoxes && Event.current != null && !movingInfoBox && Event.current.type == EventType.MouseUp && movedTarget != null)
            {
                movedTarget = null;
            }

            //caching information
            if ((diagramStructureInfos == null || diagramStructureInfos.Count < 1) && (!caching))
            {
                caching = true;
                cacheThread = new Thread(new ThreadStart(CacheDependencies));
                cacheThread.Start();
                WaitForJobDone(cacheThread);
            }
            if ((diagramStructureRects == null || diagramStructureRects.Count < 1 || recalcArrows) && (!caching))
            {
                caching = true;
                loadingDescription = "Caching component diagram";
                loadingProgress = 0;
                cacheThread = new Thread(new ThreadStart(CacheComponentDiagramElements));
                cacheThread.Start();
                WaitForJobDone(cacheThread);
            }
            else if (!caching)
            {
                //draw the boxes with relevant content included
                for (int i = 0; i < diagramStructureRects.Count; ++i)
                {
                    GUIStyle boxStyle = diagramBoxStyle;
                    if (diagramStructureRects[i].Value.height < 80)
                        boxStyle = smallBoxStyle;
                    Rect boxRect = new Rect(diagramStructureRects[i].Value.x + diagramCenterPos.x - scrollPos.x, diagramStructureRects[i].Value.y + diagramCenterPos.y - scrollPos.y, diagramStructureRects[i].Value.width, diagramStructureRects[i].Value.height);

                    //not drawing something that is not visible
                    if (boxRect.yMax < -200 / zoomScale || boxRect.yMin > ((Screen.height * dpiScaleFactor) + 200) / zoomScale || boxRect.xMax < -200 / zoomScale || boxRect.xMin > ((Screen.width * dpiScaleFactor) + 200) / zoomScale)
                    {
                        continue;
                    }

                    string titlePrefix = "<<component>>\n";
                    GUI.Box(boxRect, titlePrefix + diagramStructureRects[i].Key.name, boxStyle);
                }

                RectReordering(boxMargin);

                //Drawing the diagram arrows
                for (int i = 0; i < diagramArrows.Count; ++i)
                {
                    DrawLine(diagramArrows[i]);
                }

                //drawing the highlighted arrow on top of everything
                if (highlightedArrow != null)
                    DrawLine(highlightedArrow);
            }
        }

        void CacheClassDiagramElements()
        {
            if (!recalcArrows)
            {
                //finding main element that should always be included
                DiagramStructureInfo main = diagramStructureInfos[0];
                for (int i = 0; i < diagramStructureInfos.Count; ++i)
                {
                    if (displayedData != null && diagramStructureInfos[i].path.Equals(displayedData.path))
                    {
                        main = diagramStructureInfos[i];
                        break;
                    }
                }

                List<DiagramStructureInfo> classDiagramContents = FindClassDiagramContents(main, new List<string>(), 1);
                diagramStructureRects = CalcBoxRects(classDiagramContents, boxMargin);


            }
            else
            {
                recalcArrows = false;
                movedTarget = null;
                diagramArrows = new List<DiagramArrow>();
                highlightedArrow = null;
                diagramInwardDependencyCount = 0;
                diagramOutwardDependencyCount = 0;
            }

            //finding the offset of the diagram
            diagramCenterPos = new Vector2Int(50, 100);
            for (int i = 0; i < diagramStructureRects.Count; ++i)
            {
                if (diagramStructureRects[i].Value.x < -(diagramCenterPos.x - 50))
                    diagramCenterPos.x = (int)(-diagramStructureRects[i].Value.x + 50);
                if (diagramStructureRects[i].Value.y < -(diagramCenterPos.y - 100))
                    diagramCenterPos.y = (int)(-diagramStructureRects[i].Value.y + 100);
            }
            scrollAreaDimensions = new Vector2Int(20, 20);

            for (int i = 0; i < diagramStructureRects.Count; ++i)
            {
                if (scrollAreaDimensions.x - 20 < diagramCenterPos.x + diagramStructureRects[i].Value.xMax)
                    scrollAreaDimensions.x = (int)(diagramCenterPos.x + diagramStructureRects[i].Value.xMax + 20);

                if (scrollAreaDimensions.y - 20 < diagramCenterPos.y + diagramStructureRects[i].Value.yMax)
                    scrollAreaDimensions.y = (int)(diagramCenterPos.y + diagramStructureRects[i].Value.yMax + 20);
            }

            //finding all arrows that need to be drawn
            bool[] connectedRects = new bool[diagramStructureRects.Count];
            bool[][] arrowsToDraw = new bool[diagramStructureRects.Count][];
            int[][] arrowTypes = new int[diagramStructureRects.Count][];
            List<Rect> arrowObstacles = new List<Rect>();
            int arrowMargin = 15;
            int lineCount = 0;
            for (int i = 0; i < diagramStructureRects.Count; ++i)
            {
                Rect boxRect = new Rect(diagramStructureRects[i].Value.x + diagramCenterPos.x, diagramStructureRects[i].Value.y + diagramCenterPos.y, diagramStructureRects[i].Value.width, diagramStructureRects[i].Value.height);
                Rect obstacleRect = new Rect(boxRect.x - arrowMargin, boxRect.y - arrowMargin, boxRect.width + arrowMargin * 2, boxRect.height + arrowMargin * 2);
                arrowObstacles.Add(obstacleRect);
                arrowsToDraw[i] = new bool[diagramStructureRects.Count];
                arrowTypes[i] = new int[diagramStructureRects.Count];

                //finding dependencies, aggregations etc.
                for (int k = 0; k < diagramStructureRects.Count; ++k)
                {
                    if (k != i)
                    {

                        if (IsDependant(diagramStructureRects[i].Key.usings, diagramStructureRects[i].Key.dependencies, diagramStructureRects[i].Key.path, diagramStructureRects[k].Key.path))
                        {
                            // 	- 0 dependencies (dependency)
                            //  - 1 association(dependency to static class)
                            //  - 2 aggregations(public child class)
                            //  - 3 inheritances(postfix contains superclass)
                            //  - 4 composition(private child class)
                            //  - attributes and methods

                            //checking more specifically, what kind of dependency this is

                            //default is dependency
                            arrowTypes[i][k] = 0;

                            //association
                            if (diagramStructureRects[k].Key.prefix.Contains("static"))
                                arrowTypes[i][k] = 1;

                            //inheritance
                            if (IsSuperclass(diagramStructureRects[i].Key, diagramStructureRects[k].Key))
                                arrowTypes[i][k] = 3;

                            if (diagramStructureRects[k].Key.IsParentTo(diagramStructureRects[i].Key.identifier, true))
                            {
                                //aggregation
                                if (diagramStructureRects[i].Key.prefix.Contains("public"))
                                    arrowTypes[i][k] = 2;
                                //composition
                                else
                                    arrowTypes[i][k] = 4;
                            }

                            switch (arrowTypes[i][k])
                            {
                                case 0:
                                    if (classDiagramDependencyDepth < diagramStructureRects[i].Key.angle && classDiagramDependencyDepth < diagramStructureRects[k].Key.angle)
                                        continue;
                                    break;
                                case 1:
                                    if (classDiagramAssociationDepth < diagramStructureRects[i].Key.angle && classDiagramAssociationDepth < diagramStructureRects[k].Key.angle)
                                        continue;
                                    break;
                                case 2:
                                    if (classDiagramAggregationDepth < diagramStructureRects[i].Key.angle && classDiagramAggregationDepth < diagramStructureRects[k].Key.angle)
                                        continue;
                                    break;
                                case 3:
                                    if (classDiagramInheritanceDepth < diagramStructureRects[i].Key.angle && classDiagramInheritanceDepth < diagramStructureRects[k].Key.angle)
                                        continue;
                                    break;
                                case 4:
                                    if (classDiagramCompositionDepth < diagramStructureRects[i].Key.angle && classDiagramCompositionDepth < diagramStructureRects[k].Key.angle)
                                        continue;
                                    break;
                            }

                            lineCount++;
                            arrowsToDraw[i][k] = true;
                            diagramOutwardDependencyCount++;
                            if (arrowsToDraw[k] != null && arrowsToDraw[k][i])
                                diagramInwardDependencyCount++;
                            connectedRects[i] = true;
                            connectedRects[k] = true;
                        }
                    }
                }
            }

            for (int i = 0; i < arrowsToDraw.Length; ++i)
            {
                loadingProgress = (int)(i * 1f / arrowsToDraw.Length * 100);
                callRepaint = true;

                Rect boxRect = new Rect(diagramStructureRects[i].Value.x + diagramCenterPos.x, diagramStructureRects[i].Value.y + diagramCenterPos.y, diagramStructureRects[i].Value.width, diagramStructureRects[i].Value.height);

                for (int k = 0; k < arrowsToDraw[i].Length; ++k)
                {
                    Rect targetBoxRect = new Rect(diagramStructureRects[k].Value.x + diagramCenterPos.x, diagramStructureRects[k].Value.y + diagramCenterPos.y, diagramStructureRects[k].Value.width, diagramStructureRects[k].Value.height);

                    bool twoWay = arrowsToDraw[k][i];
                    if (twoWay && (arrowTypes[i][k] < arrowTypes[k][i] || (i > k && arrowTypes[i][k] == arrowTypes[k][i])))
                        continue;

                    if (arrowsToDraw[i][k])
                        CreateDiagramArrow(boxRect, targetBoxRect, i, k, arrowObstacles, arrowTypes[i][k] != 1, false, 2, diagramStructureRects[i].Key.name, diagramStructureRects[k].Key.name, arrowTypes[i][k], lineCount, false);
                }
            }

            int removed = 0;
            for (int i = 0; i < connectedRects.Length; ++i)
            {
                if (!connectedRects[i] && displayedData != null && diagramStructureRects[i-removed].Key.path != displayedData.path)
                {
                    diagramStructureRects.RemoveAt(i - removed);
                    removed++;
                } 
            }

            callRepaint = true;
            caching = false;
        }

        void DisplayClassDiagram()
        {

            if (LoadScopeData())
                return;

            //mouse up event when dragging boxes
            if (movableBoxes && Event.current != null && !movingInfoBox && Event.current.type == EventType.MouseUp && movedTarget != null)
            {
                movedTarget = null;
            }

            //caching information
            if ((diagramStructureInfos == null || diagramStructureInfos.Count < 1) && (!caching))
            {
                caching = true;
                cacheThread = new Thread(new ThreadStart(CacheDependencies));
                cacheThread.Start();
                WaitForJobDone(cacheThread);
                callRepaint = true;
            }
            if ((diagramStructureRects == null || diagramStructureRects.Count < 1 || recalcArrows) && (!caching))
            {
                caching = true;
                loadingDescription = "Caching class diagram";
                loadingProgress = 0;
                cacheThread = new Thread(new ThreadStart(CacheClassDiagramElements));
                cacheThread.Start();
                WaitForJobDone(cacheThread);
                callRepaint = true;
            }
            else if (!caching)
            {
                //draw the boxes with relevant content included
                for (int i = 0; i < diagramStructureRects.Count; ++i)
                {
                    // control class = static class
                    // entity class = non-static

                    GUIStyle boxStyle = diagramBoxStyle;
                    if (diagramStructureRects[i].Value.height < 80)
                        boxStyle = smallBoxStyle;
                    Rect boxRect = new Rect(diagramStructureRects[i].Value.x + diagramCenterPos.x - scrollPos.x, diagramStructureRects[i].Value.y + diagramCenterPos.y - scrollPos.y, diagramStructureRects[i].Value.width, diagramStructureRects[i].Value.height);

                    //not drawing something that is not visible
                    if (boxRect.yMax < 0 || boxRect.yMin > ((Screen.height * dpiScaleFactor) - 50) / zoomScale || boxRect.xMax < -20 / zoomScale || boxRect.xMin > ((Screen.width * dpiScaleFactor) + 20) / zoomScale)
                    {
                        continue;
                    }

                    string titlePrefix = "<<entity>>\n";
                    if (diagramStructureRects[i].Key.prefix.Contains("static"))
                        titlePrefix = "<<control>>\n";
                    GUI.Box(boxRect, titlePrefix + diagramStructureRects[i].Key.name, boxStyle);

                    if (classDiagramAttributeDepth >= diagramStructureRects[i].Key.angle && diagramStructureRects[i].Value.height > 60)
                    {
                        Rect nextLine = new Rect(boxRect.x + 5, boxRect.y + 40, boxRect.width - 10, 20);

                        //drawing variable/property list
                        for (int k = 0; k < diagramStructureRects[i].Key.children.Count; ++k)
                        {
                            if (diagramStructureRects[i].Key.children[k].type.Equals("Variable") || diagramStructureRects[i].Key.children[k].type.Equals("Property"))
                            {
                                char prefix = PrefixCharacter(diagramStructureRects[i].Key.children[k]);
                                DrawFitLabel(prefix + diagramStructureRects[i].Key.children[k].name, nextLine, leftAlignLabelStyle);
                                nextLine.y += 20;
                            }
                        }

                        if (nextLine.y > boxRect.y + 41)
                        {
                            //Line separator
                            GUI.DrawTexture(new Rect(boxRect.x, boxRect.y + 35, boxRect.width, 2), borderPixelTexture);
                            nextLine.y += 15;
                        }

                        float separatorLinePos = nextLine.y;

                        //drawing constructor/destructor/method list
                        for (int k = 0; k < diagramStructureRects[i].Key.children.Count; ++k)
                        {
                            if (diagramStructureRects[i].Key.children[k].type.Equals("Constructor") || diagramStructureRects[i].Key.children[k].type.Equals("Destructor") || diagramStructureRects[i].Key.children[k].type.Equals("Method"))
                            {
                                string returnValue = "";
                                if (!diagramStructureRects[i].Key.children[k].objectType.Equals("void"))
                                    returnValue = " : " + diagramStructureRects[i].Key.children[k].objectType;
                                DrawFitLabel(PrefixCharacter(diagramStructureRects[i].Key.children[k]) + diagramStructureRects[i].Key.children[k].name + "()" + returnValue, nextLine, leftAlignLabelStyle);
                                nextLine.y += 20;
                            }
                        }

                        if (nextLine.y > separatorLinePos)
                        {
                            //Line separator
                            GUI.DrawTexture(new Rect(boxRect.x, separatorLinePos - 5, boxRect.width, 2), borderPixelTexture);
                        }

                    }

                }

                RectReordering(boxMargin);

                //Drawing the diagram arrows
                for (int i = 0; i < diagramArrows.Count; ++i)
                {
                    DrawLine(diagramArrows[i]);
                }
                //drawing the highlighted arrow on top of everything
                if (highlightedArrow != null)
                    DrawLine(highlightedArrow);
            }
        }

        void CachePackageDiagramElements()
        {
            if (!recalcArrows)
            {
                //finding main element that should always be included
                DiagramStructureInfo main = diagramStructureInfos[0];
                for (int i = 0; i < diagramStructureInfos.Count; ++i)
                {
                    if (displayedData != null && diagramStructureInfos[i].path.Equals(displayedData.path))
                    {
                        main = diagramStructureInfos[i];
                        break;
                    }
                }
                List<DiagramStructureInfo> packageDiagramContents = FindPackageDiagramContents(main, new List<string>());

                diagramStructureRects = CalcBoxRects(packageDiagramContents, boxMargin);
            }
            else
            {
                recalcArrows = false;
                movedTarget = null;
                diagramArrows = new List<DiagramArrow>();
                highlightedArrow = null;
                diagramInwardDependencyCount = 0;
                diagramOutwardDependencyCount = 0;
            }

            //finding the offset of the diagram
            diagramCenterPos = new Vector2Int(50, 100);

            for (int i = 0; i < diagramStructureRects.Count; ++i)
            {
                if (diagramStructureRects[i].Value.x < -(diagramCenterPos.x - 50))
                    diagramCenterPos.x = (int)(-diagramStructureRects[i].Value.x + 50);
                if (diagramStructureRects[i].Value.y < -(diagramCenterPos.y - 100))
                    diagramCenterPos.y = (int)(-diagramStructureRects[i].Value.y + 100);
            }
            scrollAreaDimensions = new Vector2Int(20, 20);

            for (int i = 0; i < diagramStructureRects.Count; ++i)
            {
                if (scrollAreaDimensions.x - 20 < diagramCenterPos.x + diagramStructureRects[i].Value.xMax)
                    scrollAreaDimensions.x = (int)(diagramCenterPos.x + diagramStructureRects[i].Value.xMax + 20);

                if (scrollAreaDimensions.y - 20 < diagramCenterPos.y + diagramStructureRects[i].Value.yMax)
                    scrollAreaDimensions.y = (int)(diagramCenterPos.y + diagramStructureRects[i].Value.yMax + 20);
            }

            //finding all arrows that need to be drawn
            bool[][] arrowsToDraw = new bool[diagramStructureRects.Count][];
            bool[][] importDependency = new bool[diagramStructureRects.Count][];
            List<Rect> arrowObstacles = new List<Rect>();
            int lineCount = 0;
            int arrowMargin = 15;
            for (int i = 0; i < diagramStructureRects.Count; ++i)
            {
                Rect boxRect = new Rect(diagramStructureRects[i].Value.x + diagramCenterPos.x, diagramStructureRects[i].Value.y + diagramCenterPos.y, diagramStructureRects[i].Value.width, diagramStructureRects[i].Value.height);
                Rect obstacleRect = new Rect(boxRect.x - arrowMargin, boxRect.y - arrowMargin, boxRect.width + arrowMargin * 2, boxRect.height + arrowMargin * 2);
                arrowObstacles.Add(obstacleRect);
                arrowsToDraw[i] = new bool[diagramStructureRects.Count];
                importDependency[i] = new bool[diagramStructureRects.Count];

                //finding dependencies
                for (int k = 0; k < diagramStructureRects.Count; ++k)
                {
                    //only showing something that's not otherwise obvious 
                    string pathStart1 = FullName(diagramStructureRects[i].Key.path);
                    pathStart1 = pathStart1[..Mathf.Max(0, pathStart1.LastIndexOf('.'))];
                    string pathStart2 = FullName(diagramStructureRects[k].Key.path);
                    pathStart2 = pathStart2[..Mathf.Max(0, pathStart2.LastIndexOf('.'))];
                    if (k != i && pathStart1.Equals(pathStart2))
                    {
                        bool import = IsDependant(diagramStructureRects[i].Key.usings, new List<string>(), diagramStructureRects[i].Key.path, diagramStructureRects[k].Key.path);
                        bool access = IsDependant(diagramStructureRects[i].Key.usings, diagramStructureRects[i].Key.dependencies, diagramStructureRects[i].Key.path, diagramStructureRects[k].Key.path);
                        if (access)
                        {
                            lineCount++;
                            importDependency[i][k] = import;
                            arrowsToDraw[i][k] = true;
                            diagramOutwardDependencyCount++;
                            if (arrowsToDraw[k] != null && arrowsToDraw[k][i])
                                diagramInwardDependencyCount++;
                        }
                    }
                }
            }

            for (int i = 0; i < arrowsToDraw.Length; ++i)
            {
                loadingProgress = (int)(i * 1f / arrowsToDraw.Length * 100);
                callRepaint = true;

                Rect boxRect = new Rect(diagramStructureRects[i].Value.x + diagramCenterPos.x, diagramStructureRects[i].Value.y + diagramCenterPos.y, diagramStructureRects[i].Value.width, diagramStructureRects[i].Value.height);

                for (int k = 0; k < arrowsToDraw[i].Length; ++k)
                {
                    Rect targetBoxRect = new Rect(diagramStructureRects[k].Value.x + diagramCenterPos.x, diagramStructureRects[k].Value.y + diagramCenterPos.y, diagramStructureRects[k].Value.width, diagramStructureRects[k].Value.height);

                    if (arrowsToDraw[i][k])
                        CreateDiagramArrow(boxRect, targetBoxRect, i, k, arrowObstacles, true, false, 2, diagramStructureRects[i].Key.name, diagramStructureRects[k].Key.name, importDependency[i][k] ? 5 : 0, lineCount, false);
                }
            }
            caching = false;
        }

        void DisplayPackageDiagram()
        {
            if (LoadScopeData())
                return;

            //mouse up event when dragging boxes
            if (movableBoxes && Event.current != null && !movingInfoBox && Event.current.type == EventType.MouseUp && movedTarget != null)
            {
                movedTarget = null;
            }

            //caching information
            if ((diagramStructureInfos == null || diagramStructureInfos.Count < 1) && (!caching))
            {
                caching = true;
                cacheThread = new Thread(new ThreadStart(CacheDependencies));
                cacheThread.Start();
                WaitForJobDone(cacheThread);
            }
            if ((diagramStructureRects == null || diagramStructureRects.Count < 1 || recalcArrows) && (!caching))
            {
                caching = true;
                loadingDescription = "Caching package diagram";
                loadingProgress = 0;
                cacheThread = new Thread(new ThreadStart(CachePackageDiagramElements));
                cacheThread.Start();
                WaitForJobDone(cacheThread);
            }
            else if (!caching)
            {
                //draw the boxes with relevant content included
                for (int i = 0; i < diagramStructureRects.Count; ++i)
                {
                    GUIStyle boxStyle = diagramBoxStyle;
                    if (diagramStructureRects[i].Key.children.Count == 0)
                        boxStyle = smallBoxStyle;
                    Rect boxRect = new Rect(diagramStructureRects[i].Value.x + diagramCenterPos.x - scrollPos.x, diagramStructureRects[i].Value.y + diagramCenterPos.y - scrollPos.y, diagramStructureRects[i].Value.width, diagramStructureRects[i].Value.height);

                    //not drawing something that is not visible
                    if (boxRect.yMax < -200 / zoomScale || boxRect.yMin > ((Screen.height * dpiScaleFactor) + 200) / zoomScale || boxRect.xMax < -200 / zoomScale || boxRect.xMin > ((Screen.width * dpiScaleFactor) + 200) / zoomScale)
                    {
                        continue;
                    }

                    GUI.Box(boxRect, diagramStructureRects[i].Key.name, boxStyle);
                }

                RectReordering(boxMargin);

                //Drawing the diagram arrows
                for (int i = 0; i < diagramArrows.Count; ++i)
                {
                    DrawLine(diagramArrows[i]);
                }

                //drawing the highlighted arrow on top of everything
                if (highlightedArrow != null)
                    DrawLine(highlightedArrow);
            }
        }

        void CacheCompositeStructureDiagramElements()
        {
            if (!recalcArrows)
            {
                //finding main element that should always be included
                DiagramStructureInfo main = diagramStructureInfos[0];
                for (int i = 0; i < diagramStructureInfos.Count; ++i)
                {
                    if (displayedData != null && diagramStructureInfos[i].path.Equals(displayedData.path))
                    {
                        main = diagramStructureInfos[i];
                        break;
                    }
                }
                List<DiagramStructureInfo> diagramContents = new List<DiagramStructureInfo>() { FindCompositeStructureContents(main, "", new List<string>()) };
                diagramStructureRects = CalcBoxRects(diagramContents, boxMargin);

            }
            else
            {
                recalcArrows = false;
                movedTarget = null;
                diagramArrows = new List<DiagramArrow>();
                highlightedArrow = null;
                diagramInwardDependencyCount = 0;
                diagramOutwardDependencyCount = 0;
            }


            //finding the offset of the diagram
            diagramCenterPos = new Vector2Int(50, 100);

            for (int i = 0; i < diagramStructureRects.Count; ++i)
            {
                if (diagramStructureRects[i].Value.x < -(diagramCenterPos.x - 50))
                    diagramCenterPos.x = (int)(-diagramStructureRects[i].Value.x + 50);
                if (diagramStructureRects[i].Value.y < -(diagramCenterPos.y - 100))
                    diagramCenterPos.y = (int)(-diagramStructureRects[i].Value.y + 100);
            }
            scrollAreaDimensions = new Vector2Int(80, 80);

            for (int i = 0; i < diagramStructureRects.Count; ++i)
            {
                if (scrollAreaDimensions.x - 80 < diagramCenterPos.x + diagramStructureRects[i].Value.width)
                    scrollAreaDimensions.x = (int)(diagramCenterPos.x + diagramStructureRects[i].Value.width + 80);

                if (scrollAreaDimensions.y - 80 < diagramCenterPos.y + diagramStructureRects[i].Value.height)
                    scrollAreaDimensions.y = (int)(diagramCenterPos.y + diagramStructureRects[i].Value.height + 80);
            }

            //finding all arrows that need to be drawn
            bool[][] arrowsToDraw = new bool[diagramStructureRects.Count][];
            List<Rect> arrowObstacles = new List<Rect>();
            int arrowMargin = 15;
            int lineCount = 0;
            for (int i = 0; i < diagramStructureRects.Count; ++i)
            {
                arrowsToDraw[i] = new bool[diagramStructureRects.Count];
                Rect boxRect = new Rect(diagramStructureRects[i].Value.x + diagramCenterPos.x, diagramStructureRects[i].Value.y + diagramCenterPos.y, diagramStructureRects[i].Value.width, diagramStructureRects[i].Value.height);
                Rect obstacleRect = new Rect(boxRect.x - arrowMargin, boxRect.y - arrowMargin, boxRect.width + arrowMargin * 2, boxRect.height + arrowMargin * 2);
                if (i > 0)
                    arrowObstacles.Add(obstacleRect);

                //finding aggregations
                for (int k = 0; k < diagramStructureRects.Count; ++k)
                {
                    //only aggregating to something that's not otherwise implied
                    bool parentChild = HasTypeAsChild(diagramStructureRects[i].Key, diagramStructureRects[k].Key.objectType) || HasTypeAsChild(diagramStructureRects[k].Key, diagramStructureRects[i].Key.objectType);

                    if (diagramStructureRects[i].Key.path.Contains(diagramStructureRects[k].Key.path) || diagramStructureRects[k].Key.path.Contains(diagramStructureRects[i].Key.path))
                        parentChild = true;

                    if (k != i && !parentChild)
                    {
                        if (IsDependant(diagramStructureRects[i].Key.usings, diagramStructureRects[i].Key.dependencies, diagramStructureRects[i].Key.path, diagramStructureRects[k].Key.identifier))
                        {
                            lineCount++;
                            arrowsToDraw[i][k] = true;
                            diagramOutwardDependencyCount++;
                            if (arrowsToDraw[k] != null && arrowsToDraw[k][i])
                                diagramInwardDependencyCount++;
                        }
                    }
                }
            }

            for (int i = 0; i < arrowsToDraw.Length; ++i)
            {
                loadingProgress = (int)(i * 1f / arrowsToDraw.Length * 100);
                callRepaint = true;

                Rect boxRect = new Rect(diagramStructureRects[i].Value.x + diagramCenterPos.x + 5, diagramStructureRects[i].Value.y + diagramCenterPos.y + 5, diagramStructureRects[i].Value.width - 10, diagramStructureRects[i].Value.height - 10);

                for (int k = 0; k < arrowsToDraw[i].Length; ++k)
                {
                    Rect targetBoxRect = new Rect(diagramStructureRects[k].Value.x + diagramCenterPos.x + 5, diagramStructureRects[k].Value.y + diagramCenterPos.y + 5, diagramStructureRects[k].Value.width - 10, diagramStructureRects[k].Value.height - 10);

                    bool twoWay = arrowsToDraw[k][i];
                    if (twoWay && i > k)
                        continue;

                    if (arrowsToDraw[i][k])
                        CreateDiagramArrow(boxRect, targetBoxRect, i-1, k-1, arrowObstacles, true, twoWay, 2, diagramStructureRects[i].Key.name, diagramStructureRects[k].Key.name, 1, lineCount, false);
                }
            }
            caching = false;
        }

        void DisplayCompositeStructureDiagram()
        {
            if (LoadScopeData())
                return;

            //mouse up event when dragging boxes
            if (movableBoxes && Event.current != null && !movingInfoBox && Event.current.type == EventType.MouseUp && movedTarget != null)
            {
                movedTarget = null;
            }
            
            //caching information
            if ((diagramStructureInfos == null || diagramStructureInfos.Count < 1) && (!caching))
            {
                caching = true;
                cacheThread = new Thread(new ThreadStart(CacheDependencies));
                cacheThread.Start();
                WaitForJobDone(cacheThread);
            }
            if ((diagramStructureRects == null || diagramStructureRects.Count < 1 || recalcArrows) && (!caching))
            {
                caching = true;
                loadingDescription = "Caching composite structure diagram";
                loadingProgress = 0;
                cacheThread = new Thread(new ThreadStart(CacheCompositeStructureDiagramElements));
                cacheThread.Start();
                WaitForJobDone(cacheThread);
            }
            else if (!caching)
            {
                //draw the boxes with relevant content included
                for (int i = 0; i < diagramStructureRects.Count; ++i)
                {
                    GUIStyle boxStyle = dashedBoxStyle;

                    if (diagramStructureRects[i].Key.children.Count == 0)
                        boxStyle = smallDashedBoxStyle;

                    if (diagramStructureRects[i].Key.angle == 0)
                    {
                        //internal type. normal box style
                        boxStyle = diagramBoxStyle;

                        if (diagramStructureRects[i].Key.children.Count == 0)
                            boxStyle = smallBoxStyle;
                    }

                    Rect boxRect = new Rect(diagramStructureRects[i].Value.x + diagramCenterPos.x - scrollPos.x, diagramStructureRects[i].Value.y + diagramCenterPos.y - scrollPos.y, diagramStructureRects[i].Value.width, diagramStructureRects[i].Value.height);

                    //not drawing something that is not visible
                    if (boxRect.yMax < -200 / zoomScale || boxRect.yMin > ((Screen.height * dpiScaleFactor) + 200) / zoomScale || boxRect.xMax < -200 / zoomScale || boxRect.xMin > ((Screen.width * dpiScaleFactor) + 200) / zoomScale)
                    {
                        continue;
                    }

                    GUI.Box(boxRect, diagramStructureRects[i].Key.postfix, boxStyle);

                }

                RectReordering(boxMargin);

                //Drawing the diagram arrows
                for (int i = 0; i < diagramArrows.Count; ++i)
                {
                    DrawLine(diagramArrows[i]);
                }

                //drawing the highlighted arrow on top of everything
                if (highlightedArrow != null)
                    DrawLine(highlightedArrow);
            }
        }

        void CacheDependencyDiagramElements()
        {
            for (int i = 0; i < diagramStructureInfos.Count; ++i)
            {
                if (AlwaysIncludeOrExcludePath(diagramStructureInfos[i].path) == 2)
                {
                    diagramStructureInfos.RemoveAt(i);
                    --i;
                }
            }

            //calculating all angles
            List<Rect> occupiedSpaces = new List<Rect>();
            for (int i = 0; i < diagramStructureInfos.Count; ++i)
            {
                float radianAngle = diagramStructureInfos[i].angle * Mathf.PI / 180f;
                Vector2 rectPos = new Vector2(diagramCenterPos.x + Mathf.Sin(radianAngle) * diagramRadius * (2 + ((int)(radianAngle / (-2 * Mathf.PI)))), diagramCenterPos.y + Mathf.Cos(radianAngle) * diagramRadius * (2 + ((int)(radianAngle / (-2 * Mathf.PI)))));

                if (i == 0)
                {
                    rectPos = diagramCenterPos;
                }

                Rect boxRect;

                //Leave at least 10px marginal between everything
                int padding = 5;

                bool overlap;
                do
                {
                    overlap = false;
                    if (diagramStructureInfos[i].type.Equals("Namespace"))
                        boxRect = new Rect(rectPos.x - padding, rectPos.y - padding, 80 * Mathf.Min(diagramStructureInfos[i].children.Count, 5) + padding * 2, 20 + 70 * Mathf.Ceil(diagramStructureInfos[i].children.Count / 5f) + padding * 2);
                    else
                        boxRect = new Rect(rectPos.x - padding, rectPos.y - padding, 80 + padding * 2, 70 + padding * 2);
                    for (int k = 0; k < occupiedSpaces.Count; ++k)
                    {
                        if (boxRect.Overlaps(occupiedSpaces[k]))
                        {
                            overlap = true;
                            break;
                        }
                    }
                    if (overlap)
                        diagramStructureInfos[i].angle = NextDependencyDiagramAngle(diagramStructureInfos[i].angle, diagramStructureInfos[i].type.Equals("Namespace"));

                    radianAngle = diagramStructureInfos[i].angle * Mathf.PI / 180f;
                    rectPos = new Vector2(diagramCenterPos.x + Mathf.Sin(radianAngle) * diagramRadius * (2 + ((int)(radianAngle / (-2 * Mathf.PI)))), diagramCenterPos.y + Mathf.Cos(radianAngle) * diagramRadius * (2 + ((int)(radianAngle / (-2 * Mathf.PI)))));
                } while (overlap);

                occupiedSpaces.Add(boxRect);
            }

            //finding the center position of the diagram
            for (int i = 0; i < diagramStructureInfos.Count; ++i)
            {
                float radianAngle = diagramStructureInfos[i].angle * Mathf.PI / 180f;
                Vector2 topLeftPos = new Vector2(Mathf.Sin(radianAngle) * diagramRadius * (2 + ((int)(radianAngle / (-2 * Mathf.PI)))), Mathf.Cos(radianAngle) * diagramRadius * (2 + ((int)(radianAngle / (-2 * Mathf.PI)))));
                if (diagramCenterPos.x < 100 - (int)topLeftPos.x)
                    diagramCenterPos.x = 100 - (int)(topLeftPos.x);
                if (diagramCenterPos.y < 100 - (int)topLeftPos.y)
                    diagramCenterPos.y = 100 - (int)(topLeftPos.y);
            }

            //finding the arrows and rect positions
            int[] inds = new int[diagramStructureInfos.Count];
            Rect mainRect = new Rect(0, 0, 0, 0);
            diagramInwardDependencyCount = 0;
            diagramOutwardDependencyCount = 0;
            diagramStructureRects = new List<KeyValuePair<DiagramStructureInfo, Rect>>();

            if (diagramMainElement != null)
            {
                for (int i = 0; i < diagramStructureInfos.Count; ++i)
                {
                    loadingProgress = (int)(i * 1f / diagramStructureInfos.Count * 100);
                    callRepaint = true;

                    Vector2 rectPos;

                    if (i > 0)
                    {
                        float radianAngle = diagramStructureInfos[i].angle * Mathf.PI / 180f;
                        rectPos = new Vector2(diagramCenterPos.x + Mathf.Sin(radianAngle) * diagramRadius * (2 + ((int)(radianAngle / (-2 * Mathf.PI)))), diagramCenterPos.y + Mathf.Cos(radianAngle) * diagramRadius * (2 + ((int)(radianAngle / (-2 * Mathf.PI)))));
                    }
                    else
                        rectPos = diagramCenterPos;

                    if (diagramStructureInfos[i].type.Equals("Namespace"))
                    {
                        if (groupNamespaces || i == 0)
                        {
                            int childCount = Mathf.Max(diagramStructureInfos[i].children.Count, 1);
                            Rect namespaceRect = new Rect(rectPos.x, rectPos.y, 80 * Mathf.Min(childCount, 5), 20 + 70 * Mathf.Ceil(childCount / 5f));

                            if (i == 0)
                                mainRect = namespaceRect;

                            diagramStructureRects.Add(new KeyValuePair<DiagramStructureInfo, Rect>(diagramStructureInfos[i], namespaceRect));

                            if (scrollAreaDimensions.x < rectPos.x + 40 + 80 * Mathf.Min(childCount, 5))
                            {
                                scrollAreaDimensions.x = (int)(rectPos.x + 40 + 80 * Mathf.Min(childCount, 5));
                            }

                            if (scrollAreaDimensions.y < rectPos.y + 50 + 70 * Mathf.Ceil(childCount / 5f))
                            {
                                scrollAreaDimensions.y = (int)(rectPos.y + 50 + 70 * Mathf.Ceil(childCount / 5f));

                            }

                            bool drawFromMain = false;
                            if (i > 0 && IsDependant(diagramStructureInfos[0].usings, diagramStructureInfos[0].dependencies, diagramStructureInfos[0].path, diagramStructureInfos[i].path))
                            {
                                if (showOutwardDependencies)
                                    drawFromMain = true;
                                diagramOutwardDependencyCount++;
                            }

                            bool drawToMain = false;
                            if (i > 0 && IsDependant(diagramStructureInfos[i].usings, diagramStructureInfos[i].dependencies, diagramStructureInfos[i].path, diagramMainElement.path))
                            {
                                if (showInwardDependencies)
                                    drawToMain = true;

                                diagramInwardDependencyCount++;
                            }

                            if (drawFromMain)
                            {
                                CreateDiagramArrow(mainRect, namespaceRect, 0, 0, new List<Rect>(), true, drawToMain, 2, diagramMainElement.name, diagramStructureInfos[i].name, -1, 1, true);
                            }
                            else if (drawToMain)
                            {
                                CreateDiagramArrow(mainRect, namespaceRect, 0, 0, new List<Rect>(), false, true, 2, diagramStructureInfos[i].name, diagramMainElement.name, -2, 1, true);
                            }

                            if (i != 0 && !drawFromMain && !drawToMain && onlyShowConnectedElements)
                                diagramStructureRects.RemoveAt(diagramStructureRects.Count - 1);

                            //Drawing child classes
                            for (int k = 0; k < diagramStructureInfos[i].children.Count; ++k)
                            {
                                Rect classRect = new Rect(rectPos.x + 25 + (inds[i] % 5) * 80, rectPos.y + 30 + 70 * (inds[i] / 5), 30, 30);

                                diagramStructureRects.Add(new KeyValuePair<DiagramStructureInfo, Rect>(diagramStructureInfos[i].children[k], classRect));

                                if (!groupNamespaces || (i == 0))
                                {
                                    if (scrollAreaDimensions.x < rectPos.x + 80)
                                    {
                                        scrollAreaDimensions.x = (int)(rectPos.x + 80);
                                    }

                                    if (scrollAreaDimensions.y < rectPos.y + 80)
                                    {
                                        scrollAreaDimensions.y = (int)(rectPos.y + 80);
                                    }
                                }

                                inds[i]++;
                            }
                        }
                    }
                    else if (diagramStructureInfos[i].type.Equals("Class"))
                    {
                        //draw direct class
                        Rect classRect = new Rect(rectPos.x, rectPos.y, 30, 30);
                        diagramStructureRects.Add(new KeyValuePair<DiagramStructureInfo, Rect>(diagramStructureInfos[i], classRect));

                        if (i == 0)
                            mainRect = classRect;

                        bool drawFromMain = false;
                        if (i != 0 && IsDependant(diagramStructureInfos[0].usings, diagramStructureInfos[0].dependencies, diagramStructureInfos[0].path, diagramStructureInfos[i].path))
                        {
                            if (showOutwardDependencies)
                                drawFromMain = true;
                            diagramOutwardDependencyCount++;
                        }

                        bool drawToMain = false;
                        if (i != 0 && IsDependant(diagramStructureInfos[i].usings, diagramStructureInfos[i].dependencies, diagramStructureInfos[i].path, diagramMainElement.path))
                        {
                            if (showInwardDependencies)
                                drawToMain = true;
                            diagramInwardDependencyCount++;
                        }


                        if (drawFromMain)
                        {
                            CreateDiagramArrow(mainRect, classRect, 0, 0, new List<Rect>(), true, drawToMain, 2, diagramMainElement.name, diagramStructureInfos[i].name, -1, 1, true);
                        }
                        else if (drawToMain)
                        {
                            CreateDiagramArrow(mainRect, classRect, 0, 0, new List<Rect>(), false, true, 2, diagramStructureInfos[i].name, diagramMainElement.name, -2, 1, true);
                        }

                        if (scrollAreaDimensions.x < rectPos.x + 90)
                        {
                            scrollAreaDimensions.x = (int)(rectPos.x + 90);
                        }

                        if (scrollAreaDimensions.y < rectPos.y + 50)
                        {
                            scrollAreaDimensions.y = (int)(rectPos.y + 50);
                        }

                        if (i != 0 && !drawFromMain && !drawToMain && onlyShowConnectedElements)
                            diagramStructureRects.RemoveAt(diagramStructureRects.Count - 1);

                        inds[i]++;
                    }
                }
            }
            caching = false;
        }

        void DisplayDependencyDiagram()
        {
            if (LoadScopeData())
                return;

            if ((diagramStructureInfos == null || diagramStructureInfos.Count < 1) && (!caching))
            {
                caching = true;
                cacheThread = new Thread(new ThreadStart(CacheDependencies));
                cacheThread.Start();
                WaitForJobDone(cacheThread);
            }
            else if ((!caching) && (diagramStructureRects == null || diagramStructureRects.Count == 0))
            {
                caching = true;
                loadingDescription = "Caching dependency diagram";
                loadingProgress = 0;
                cacheThread = new Thread(new ThreadStart(CacheDependencyDiagramElements));
                cacheThread.Start();
                WaitForJobDone(cacheThread);
            }
            else if (!caching)
            {
                //drawing the background
                GUI.Box(new Rect(Mathf.Min(40, diagramCenterPos.x - 20) - scrollPos.x, Mathf.Min(70, diagramCenterPos.y - 20) - scrollPos.y, scrollAreaDimensions.x, scrollAreaDimensions.y), "", diagramBoxStyle);

                //Drawing the diagram arrows
                for (int i = 0; i < diagramArrows.Count; ++i)
                {
                    DrawLine(diagramArrows[i]);
                }

                //drawing the highlighted arrow on top of everything
                if (highlightedArrow != null)
                    DrawLine(highlightedArrow);

                //drawing all the namespaces and classes
                for (int i = 0; i < diagramStructureRects.Count; ++i)
                {
                    Rect boxRect = new Rect(diagramStructureRects[i].Value.x - scrollPos.x, diagramStructureRects[i].Value.y - scrollPos.y, diagramStructureRects[i].Value.width, diagramStructureRects[i].Value.height);

                    if (diagramStructureRects[i].Key.type.Equals("Namespace"))
                    {
                        string trueText = FullName(diagramStructureRects[i].Key.path);
                        string labelText = trueText;
                        Vector2 size = diagramBoxStyle.CalcSize(new GUIContent(labelText));
                        if (size.x > diagramStructureRects[i].Value.xMax - diagramStructureRects[i].Value.xMin)
                        {
                            do
                            {
                                if (trueText.Length == 0)
                                {
                                    labelText = diagramStructureRects[i].Key.name;
                                    break;
                                }
                                trueText = trueText[0..^1];
                                labelText = trueText + "...";
                                size = diagramBoxStyle.CalcSize(new GUIContent(labelText));
                            } while (size.x > diagramStructureRects[i].Value.xMax - diagramStructureRects[i].Value.xMin);
                        }
                        GUI.Box(boxRect, labelText, diagramBoxStyle);
                    }
                    else if (diagramStructureRects[i].Key.type.Equals("Class"))
                    {
                        DrawCodeElement(boxRect, diagramStructureRects[i].Key.name, FullName(diagramStructureRects[i].Key.path), diagramStructureRects[i].Key.type, diagramStructureRects[i].Key.objectType, diagramStructureRects[i].Key.path, diagramStructureRects[i].Key.children.Count, (diagramStructureRects[i].Value.y >= diagramCenterPos.y || groupNamespaces));

                        //in case the target has been changed
                        if (allScopeElements.Count == 0 || relevantScopeElements.Count == 0)
                            return;
                    }
                }
            }
        }

        void CacheFolderDependencyDiagramElements()
        {
            for (int i = 0; i < diagramStructureInfos.Count; ++i)
            {
                if (AlwaysIncludeOrExcludePath(diagramStructureInfos[i].path) == 2)
                {
                    diagramStructureInfos.RemoveAt(i);
                    --i;
                }
            }

            //calculating all angles
            List<Rect> occupiedSpaces = new List<Rect>();
            for (int i = 0; i < diagramStructureInfos.Count; ++i)
            {
                float radianAngle = diagramStructureInfos[i].angle * Mathf.PI / 180f;
                Vector2 rectPos = new Vector2(diagramCenterPos.x + Mathf.Sin(radianAngle) * diagramRadius * (2 + ((int)(radianAngle / (-2 * Mathf.PI)))), diagramCenterPos.y + Mathf.Cos(radianAngle) * diagramRadius * (2 + ((int)(radianAngle / (-2 * Mathf.PI)))));

                if (i == 0)
                {
                    rectPos = diagramCenterPos;
                }

                Rect boxRect;

                //Leave at least 10px marginal between everything
                int padding = 5;

                bool overlap;
                do
                {
                    overlap = false;
                    boxRect = new Rect(rectPos.x - padding, rectPos.y - padding, 80 + padding * 2, 70 + padding * 2);
                    for (int k = 0; k < occupiedSpaces.Count; ++k)
                    {
                        if (boxRect.Overlaps(occupiedSpaces[k]))
                        {
                            overlap = true;
                            break;
                        }
                    }
                    if (overlap)
                        diagramStructureInfos[i].angle = NextDependencyDiagramAngle(diagramStructureInfos[i].angle, false);

                    radianAngle = diagramStructureInfos[i].angle * Mathf.PI / 180f;
                    rectPos = new Vector2(diagramCenterPos.x + Mathf.Sin(radianAngle) * diagramRadius * (2 + ((int)(radianAngle / (-2 * Mathf.PI)))), diagramCenterPos.y + Mathf.Cos(radianAngle) * diagramRadius * (2 + ((int)(radianAngle / (-2 * Mathf.PI)))));
                } while (overlap);

                occupiedSpaces.Add(boxRect);
            }

            //finding the center position of the diagram
            for (int i = 0; i < diagramStructureInfos.Count; ++i)
            {
                float radianAngle = diagramStructureInfos[i].angle * Mathf.PI / 180f;
                Vector2 topLeftPos = new Vector2(Mathf.Sin(radianAngle) * diagramRadius * (2 + ((int)(radianAngle / (-2 * Mathf.PI)))), Mathf.Cos(radianAngle) * diagramRadius * (2 + ((int)(radianAngle / (-2 * Mathf.PI)))));
                if (diagramCenterPos.x < 100 - (int)topLeftPos.x)
                    diagramCenterPos.x = 100 - (int)(topLeftPos.x);
                if (diagramCenterPos.y < 100 - (int)topLeftPos.y)
                    diagramCenterPos.y = 100 - (int)(topLeftPos.y);
            }

            //finding the arrows and rect positions
            int[] inds = new int[diagramStructureInfos.Count];
            Rect mainRect = new Rect(0, 0, 0, 0);
            diagramInwardDependencyCount = 0;
            diagramOutwardDependencyCount = 0;
            diagramStructureRects = new List<KeyValuePair<DiagramStructureInfo, Rect>>();

            if (diagramMainElement != null)
            {
                for (int i = 0; i < diagramStructureInfos.Count; ++i)
                {
                    loadingProgress = (int)(i * 1f / diagramStructureInfos.Count * 100);
                    callRepaint = true;

                    Vector2 rectPos;

                    if (i > 0)
                    {
                        float radianAngle = diagramStructureInfos[i].angle * Mathf.PI / 180f;
                        rectPos = new Vector2(diagramCenterPos.x + Mathf.Sin(radianAngle) * diagramRadius * (2 + ((int)(radianAngle / (-2 * Mathf.PI)))), diagramCenterPos.y + Mathf.Cos(radianAngle) * diagramRadius * (2 + ((int)(radianAngle / (-2 * Mathf.PI)))));
                    }
                    else
                        rectPos = diagramCenterPos;


                    //draw direct class
                    Rect classRect = new Rect(rectPos.x, rectPos.y, 30, 30);
                    diagramStructureRects.Add(new KeyValuePair<DiagramStructureInfo, Rect>(diagramStructureInfos[i], classRect));

                    if (i == 0)
                        mainRect = classRect;

                    bool drawFromMain = false;
                    if (i != 0 && IsDependantOnFolder(diagramMainElement, diagramStructureInfos[i]))
                    {
                        if (showOutwardDependencies)
                            drawFromMain = true;
                        diagramOutwardDependencyCount++;
                    }

                    bool drawToMain = false;
                    if (i != 0 && IsDependantOnFolder(diagramStructureInfos[i], diagramMainElement))
                    {
                        if (showInwardDependencies)
                            drawToMain = true;
                        diagramInwardDependencyCount++;
                    }


                    if (drawFromMain)
                    {
                        CreateDiagramArrow(mainRect, classRect, 0, 0, new List<Rect>(), true, drawToMain, 2, diagramMainElement.name, diagramStructureInfos[i].name, -1, 1, true);
                    }
                    else if (drawToMain)
                    {
                        CreateDiagramArrow(mainRect, classRect, 0, 0, new List<Rect>(), false, true, 2, diagramStructureInfos[i].name, diagramMainElement.name, -2, 1, true);
                    }

                    if (scrollAreaDimensions.x < rectPos.x + 90)
                    {
                        scrollAreaDimensions.x = (int)(rectPos.x + 90);
                    }

                    if (scrollAreaDimensions.y < rectPos.y + 50)
                    {
                        scrollAreaDimensions.y = (int)(rectPos.y + 50);
                    }

                    if (i != 0 && !drawFromMain && !drawToMain && onlyShowConnectedElements)
                        diagramStructureRects.RemoveAt(diagramStructureRects.Count - 1);

                    inds[i]++;
                }

            }
            caching = false;
        }

        void DisplayFolderDependencyDiagram()
        {
            if (LoadScopeData())
                return;


            if ((diagramStructureInfos == null || diagramStructureInfos.Count < 1) && (!caching))
            {
                caching = true;
                cacheThread = new Thread(new ThreadStart(CacheDependencies));
                cacheThread.Start();
                WaitForJobDone(cacheThread);
            }
            else if ((!caching) && (diagramStructureRects == null || diagramStructureRects.Count == 0))
            {
                caching = true;
                loadingDescription = "Caching dependency diagram";
                loadingProgress = 0;
                cacheThread = new Thread(new ThreadStart(CacheFolderDependencyDiagramElements));
                cacheThread.Start();
                WaitForJobDone(cacheThread);
            }
            else if (!caching)
            {
                //drawing the background
                GUI.Box(new Rect(Mathf.Min(40, diagramCenterPos.x - 20) - scrollPos.x, Mathf.Min(70, diagramCenterPos.y - 20) - scrollPos.y, scrollAreaDimensions.x, scrollAreaDimensions.y), "", diagramBoxStyle);

                //Drawing the diagram arrows
                for (int i = 0; i < diagramArrows.Count; ++i)
                {
                    DrawLine(diagramArrows[i]);
                }

                //drawing the highlighted arrow on top of everything
                if (highlightedArrow != null)
                    DrawLine(highlightedArrow);

                //drawing all the folders
                for (int i = 0; i < diagramStructureRects.Count; ++i)
                {
                    Rect boxRect = new Rect(diagramStructureRects[i].Value.x - scrollPos.x, diagramStructureRects[i].Value.y - scrollPos.y, diagramStructureRects[i].Value.width, diagramStructureRects[i].Value.height);

                    DrawCodeElement(boxRect, diagramStructureRects[i].Key.name, FullName(diagramStructureRects[i].Key.path), diagramStructureRects[i].Key.type, diagramStructureRects[i].Key.objectType, diagramStructureRects[i].Key.path, diagramStructureRects[i].Key.children.Count, (diagramStructureRects[i].Value.y >= diagramCenterPos.y || groupNamespaces));

                    //in case the target has been changed
                    if (allScopeElements.Count == 0 || relevantScopeElements.Count == 0)
                        return;

                }
            }
        }

        void RectReordering(int boxMarginal)
        {
            int clickedRect = -1;
            bool mouseDown = false;
            int movedIndex = -1;
            for (int i = 0; i < diagramStructureRects.Count; ++i)
            {
                Rect boxRect = new Rect(diagramStructureRects[i].Value.x + diagramCenterPos.x - scrollPos.x, diagramStructureRects[i].Value.y + diagramCenterPos.y - scrollPos.y, diagramStructureRects[i].Value.width, diagramStructureRects[i].Value.height);

                //checking for mouse down event on the box
                if (movableBoxes && Event.current != null && !movingInfoBox && Event.current.type == EventType.MouseDown)
                {
                    if (boxRect.Contains(Event.current.mousePosition) && movedTarget == null)
                        clickedRect = i;

                    mouseDown = true;
                }

                if (clickedRect == i || (clickedRect < 0 && diagramStructureRects[i].Key == movedTarget))
                    movedIndex = i;

            }

            if (clickedRect >= 0)
            {
                movedTarget = diagramStructureRects[clickedRect].Key;
                movingCursorOffset = -Event.current.mousePosition;
                callRepaint = true;
            }
            else if (mouseDown || movedIndex < 0)
            {
                movedTarget = null;
                return;
            }

            //handling the movement of the box
            if (movableBoxes && Event.current != null && Event.current.type == EventType.MouseDrag && !movingInfoBox && movedTarget == diagramStructureRects[movedIndex].Key)
            {
                Rect boxRect = new Rect(diagramStructureRects[movedIndex].Value.x + diagramCenterPos.x - scrollPos.x, diagramStructureRects[movedIndex].Value.y + diagramCenterPos.y - scrollPos.y, diagramStructureRects[movedIndex].Value.width, diagramStructureRects[movedIndex].Value.height);

                Vector2 movement = Event.current.mousePosition + movingCursorOffset;
                Rect newRect = new Rect(movement.x + boxRect.x, movement.y + boxRect.y, boxRect.width, boxRect.height);

                //checking for overlap
                bool overlap = false;
                List<int> childrenToMove = new List<int>();
                int reAttempts = 0;
                for (int k = 0; k < diagramStructureRects.Count; ++k)
                {
                    if (movedIndex != k)
                    {
                        Rect otherRect = new Rect(diagramStructureRects[k].Value.x + diagramCenterPos.x - scrollPos.x, diagramStructureRects[k].Value.y + diagramCenterPos.y - scrollPos.y, diagramStructureRects[k].Value.width, diagramStructureRects[k].Value.height);

                        Rect otherRectMargin = new Rect(otherRect.x - boxMarginal, otherRect.y - boxMarginal, otherRect.width + boxMarginal * 2, otherRect.height + boxMarginal * 2);

                        //k is child of i
                        if (!childrenToMove.Contains(k) && RectFullyContainsOther(boxRect, otherRect))
                        {
                            childrenToMove.Add(k);
                        }

                        //i is child of k
                        bool childOfK = false;
                        if (RectFullyContainsOther(otherRect, boxRect))
                        {
                            childOfK = true;
                        }

                        if (!childrenToMove.Contains(k) && otherRectMargin.Overlaps(newRect))
                        {
                            if (!childOfK || !RectFullyContainsOther(otherRect, newRect))
                            {
                                //overlapping (or not fully overlapping) with something the rect shouldn't overlap with
                                overlap = true;
                            }
                        }
                        else if (childOfK)
                        {
                            //not overlapping with the rect's parent
                            overlap = true;
                        }

                        if (overlap)
                        {
                            //finding the closest valid spot in manhattan distance to the target and trying again
                            bool xPositive = movement.x > 0;
                            bool yPositive = movement.y > 0;

                            Vector2 newMovementXFirst = new Vector2(0, 0);
                            Vector2 newMovementYFirst = new Vector2(0, 0);

                            if (!childOfK)
                            {
                                if (yPositive && boxRect.yMax < otherRectMargin.yMin)
                                {
                                    newMovementXFirst.x = movement.x;
                                    newMovementXFirst.y = Mathf.Min(otherRectMargin.y - boxRect.height - boxRect.y, movement.y);
                                }
                                else if (!yPositive && boxRect.yMin > otherRectMargin.yMax)
                                {
                                    newMovementXFirst.x = movement.x;
                                    newMovementXFirst.y = Mathf.Max(otherRectMargin.yMax - boxRect.y + 1, movement.y);
                                }
                                else if (xPositive)
                                {
                                    newMovementXFirst.x = Mathf.Min(otherRectMargin.x - boxRect.width - boxRect.x, movement.x);
                                    newMovementXFirst.y = movement.y;
                                }
                                else
                                {
                                    newMovementXFirst.x = Mathf.Max(otherRectMargin.xMax - boxRect.x + 1, movement.x);
                                    newMovementXFirst.y = movement.y;
                                }


                                if (xPositive && boxRect.xMax < otherRectMargin.xMin)
                                {
                                    newMovementYFirst.y = movement.y;
                                    newMovementYFirst.x = Mathf.Min(otherRectMargin.x - boxRect.width - boxRect.x, movement.x);
                                }
                                else if (!xPositive && boxRect.xMin > otherRectMargin.xMax)
                                {
                                    newMovementYFirst.y = movement.y;
                                    newMovementYFirst.x = Mathf.Max(otherRectMargin.xMax + 1 - boxRect.x, movement.x);
                                }
                                else if (yPositive)
                                {
                                    newMovementYFirst.y = Mathf.Min(otherRectMargin.y - boxRect.height - boxRect.y, movement.y);
                                    newMovementYFirst.x = movement.x;
                                }
                                else
                                {
                                    newMovementYFirst.y = Mathf.Max(otherRectMargin.yMax + 1 - boxRect.y, movement.y);
                                    newMovementYFirst.x = movement.x;
                                }
                            }
                            else
                            {
                                if (xPositive)
                                    newMovementXFirst.x = Mathf.Min(otherRect.xMax - boxRect.width - boxRect.x - 1, movement.x);
                                else
                                    newMovementXFirst.x = Mathf.Max(otherRect.xMin - boxRect.x + 1, movement.x);

                                if (yPositive)
                                    newMovementXFirst.y = Mathf.Min(otherRect.yMax - boxRect.height - boxRect.y - 1, movement.y);
                                else
                                    newMovementXFirst.y = Mathf.Max(otherRect.yMin - boxRect.y + 1, movement.y);

                                newMovementYFirst = new Vector2(float.MaxValue, float.MaxValue);
                            }

                            if (reAttempts > 10)
                            {
                                return;
                            }

                            if (Vector2.Distance(newMovementXFirst, movement) < Vector2.Distance(newMovementYFirst, movement) && newMovementXFirst.magnitude > 0 && newMovementXFirst.magnitude < movement.magnitude)
                            {
                                movement = newMovementXFirst;
                                newRect = new Rect(movement.x + boxRect.x, movement.y + boxRect.y, boxRect.width, boxRect.height);
                                k = -1;
                                reAttempts++;
                                overlap = false;
                                continue;
                            }
                            else if (newMovementYFirst.magnitude > 0 && newMovementYFirst.magnitude < movement.magnitude)
                            {
                                movement = newMovementYFirst;
                                newRect = new Rect(movement.x + boxRect.x, movement.y + boxRect.y, boxRect.width, boxRect.height);
                                k = -1;
                                reAttempts++;
                                overlap = false;
                                continue;
                            }

                            //exiting if the rect is as close to the target already as possible
                            return;
                        }
                    }
                }

                if (!overlap)
                {
                    newRect = new Rect(newRect.x - diagramCenterPos.x + scrollPos.x, newRect.y - diagramCenterPos.y + scrollPos.y, newRect.width, newRect.height);
                    diagramStructureRects[movedIndex] = new KeyValuePair<DiagramStructureInfo, Rect>(diagramStructureRects[movedIndex].Key, newRect);

                    //moving children as well
                    for (int k = 0; k < childrenToMove.Count; ++k)
                    {
                        Rect newChildRect = diagramStructureRects[childrenToMove[k]].Value;
                        newChildRect = new Rect(movement.x + newChildRect.x, movement.y + newChildRect.y, newChildRect.width, newChildRect.height);
                        diagramStructureRects[childrenToMove[k]] = new KeyValuePair<DiagramStructureInfo, Rect>(diagramStructureRects[childrenToMove[k]].Key, newChildRect);

                    }

                    movingCursorOffset -= movement;
                    callRepaint = true;

                }
            }

            return;
        }

        List<Rect> FindAllRelatedStructureRects(DiagramStructureInfo main, List<KeyValuePair<DiagramStructureInfo, Rect>> allStructures)
        {
            List<Rect> allRelated = new List<Rect>();
            for (int i = 0; i < allStructures.Count; ++i)
            {
                if (main.path != allStructures[i].Key.path)
                { 
                    //if target if a file
                    if (allStructures[i].Key.type.Equals("File"))
                    {
                        //main dependant on target
                        if (IsDependantOnFile(main.usings, main.dependencies, main.path, allStructures[i].Key))
                        {
                            allRelated.Add(allStructures[i].Value);
                        }
                    }
                    else
                    {
                        //main dependant on target
                        if (IsDependant(main.usings, main.dependencies, main.path, allStructures[i].Key.path))
                        {
                            allRelated.Add(allStructures[i].Value);
                        }
                    }

                    //if main if a file
                    if (main.type.Equals("File"))
                    {
                        //target dependant on main
                        if (IsDependantOnFile(allStructures[i].Key.usings, allStructures[i].Key.dependencies, allStructures[i].Key.path, main))
                        {
                            allRelated.Add(allStructures[i].Value);
                        }
                    }
                    else
                    {
                        //target dependant on main
                        if (IsDependant(allStructures[i].Key.usings, allStructures[i].Key.dependencies, allStructures[i].Key.path, main.path))
                        {
                            allRelated.Add(allStructures[i].Value);
                        }
                    }
                }
            }

            return allRelated;
        }

        List<KeyValuePair<DiagramStructureInfo, Rect>> DiagramChildRects(DiagramStructureInfo main, string family, int margin, bool dummyMain)
        {

            List<KeyValuePair<DiagramStructureInfo, Rect>> rectList = new List<KeyValuePair<DiagramStructureInfo, Rect>>();

            if (!dummyMain && AlwaysIncludeOrExcludePath(main.path) == 2)
                return rectList;

            float maxWidth = 0;
            float maxHeight = 0;
            float xMin = 0;
            float yMin = 0;

            List<KeyValuePair<DiagramStructureInfo, Rect>> childRects = new List<KeyValuePair<DiagramStructureInfo, Rect>>();
            bool overlap = false;
            Vector2 center = new Vector2(40, 40);
            Vector2 offset = new Vector2(center.x, center.y);
            int layer = 0;

            //class and component diagram elements don't have children
            if ((mode != DiagramDisplayMode.classDiagram  && mode != DiagramDisplayMode.componentDiagram) || dummyMain)
            {
                for (int i = 0; i < main.children.Count; ++i)
                {
                    if (!overlap)
                    {
                        layer = 1;
                        List<Rect> allRelatedStructures = FindAllRelatedStructureRects(main.children[i], rectList);
                        Vector2 avgRelatedPos = new Vector2(0, 0);
                        for (int k = 0; k < allRelatedStructures.Count; ++k)
                        {
                            avgRelatedPos += allRelatedStructures[k].position;
                        }
                        if (allRelatedStructures.Count > 0)
                            avgRelatedPos /= (1f*allRelatedStructures.Count);
                        else
                            avgRelatedPos = new Vector2(40, 40);
                        center = avgRelatedPos;

                        childRects = DiagramChildRects(main.children[i], dummyMain ? main.children[i].name : family, margin, false);
                        if (childRects.Count == 0)
                            continue;
                    }

                    overlap = false;

                    if (rectList.Count > 0)
                    {
                        for (int k = 0; k < layer; ++k)
                        {
                            for (int j = 0; j < layer; ++j)
                            {
                                if (k == layer - 1 || j == layer - 1)
                                {
                                    overlap = false;
                                    int xPos = j % 2 == 0 ? j : -(j - 1);
                                    int yPos = k % 2 == 0 ? k : -(k - 1);

                                    //for actual child elements, build towards down and right, not all around
                                    if (!dummyMain)
                                    {
                                        xPos = j;
                                        yPos = k;
                                    }

                                    offset = new Vector2(center.x + 27 * xPos, center.y + 27 * yPos);


                                    Rect rectOption = new Rect(childRects[^1].Value.x + offset.x - (margin-1), childRects[^1].Value.y + offset.y - (margin - 1), childRects[^1].Value.width + (margin - 1)*2, childRects[^1].Value.height + (margin - 1)*2);
                                    for (int l = 0; l < rectList.Count; ++l)
                                    {
                                        if (rectList[l].Value.Overlaps(rectOption))
                                        {
                                            overlap = true;
                                            break;
                                        }
                                    }
                                    if (!overlap) break;
                                }
                            }
                            if (!overlap) break;
                        }
                    }

                    if (!overlap)
                    {
                        for (int k = 0; k < childRects.Count; ++k)
                        {
                            if (childRects[k].Value.xMax + offset.x > maxWidth - 40)
                                maxWidth = childRects[k].Value.xMax + offset.x + 40;
                            if (childRects[k].Value.yMax + offset.y > maxHeight - 40)
                                maxHeight = childRects[k].Value.yMax + offset.y + 40;
                            if (offset.x < xMin)
                                xMin = offset.x;
                            if (offset.y < yMin)
                                yMin = offset.y;
                            rectList.Add(new KeyValuePair<DiagramStructureInfo, Rect>(childRects[k].Key, new Rect(childRects[k].Value.x + offset.x, childRects[k].Value.y + offset.y, childRects[k].Value.width, childRects[k].Value.height)));
                        }
                    }
                    else
                    {
                        --i;
                        layer++;
                    }
                }
            }
            
            if (!dummyMain)
            {
                //default size if this one doesn't have children
                if (maxWidth == 0)
                    maxWidth = 120;
                if (maxHeight == 0)
                    maxHeight = 60;

                //classdiagram elements include a row for each child
                if (mode == DiagramDisplayMode.classDiagram && main.angle <= classDiagramAttributeDepth)
                {
                    bool variables = false;
                    bool methods = false;
                    int attributeCount = 0;
                    for (int i = 0; i < main.children.Count; ++i)
                    {
                        if (main.children[i].type.Equals("Property") || main.children[i].type.Equals("Variable"))
                        {
                            attributeCount++;
                            variables = true;
                        }
                        else if (main.children[i].type.Equals("Method") || main.children[i].type.Equals("Constructor") || main.children[i].type.Equals("Destructor"))
                        {
                            attributeCount++;
                            methods = true;
                        }
                    }


                    if (attributeCount > 0)
                    {
                        maxHeight = 60 + attributeCount * 20;
                        if (variables && methods)
                            maxHeight += 10;
                        maxWidth = 200;
                    }
                }

                if (mode == DiagramDisplayMode.packageDiagram)
                    main.objectType = family;

                //place the main rect to contain all of the other rects
                Rect thisRect = new Rect(xMin, yMin, maxWidth, maxHeight);

                rectList.Add(new KeyValuePair<DiagramStructureInfo, Rect>(main, thisRect));
            }

            return rectList;
        }

        List<KeyValuePair<DiagramStructureInfo, Rect>> CalcBoxRects(List<DiagramStructureInfo> elements, int margin)
        {
            DiagramStructureInfo dummy = new DiagramStructureInfo
            {
                children = elements
            };
            List<KeyValuePair<DiagramStructureInfo, Rect>> rects = DiagramChildRects(dummy, "", margin, true);
            rects.Reverse();
            return rects;
        }

        List<DiagramStructureInfo> FindComponentDiagramContents(DiagramStructureInfo main, List<string> included)
        {
            List<DiagramStructureInfo> relatedFiles = new List<DiagramStructureInfo>();

            if (AlwaysIncludeOrExcludePath(main.path) == 2)
            {
                return relatedFiles;
            }

            included.Add(main.identifier);
            relatedFiles.Add(main);

            for (int i = 0; i < diagramStructureInfos.Count; ++i)
            {
                List<DiagramStructureInfo> fileChildClasses = ChildClassesOfStructureInfo(diagramStructureInfos[i]);

                // files main is dependant on
                for (int k = 0; k < fileChildClasses.Count; ++k)
                {
                        if (!included.Contains(diagramStructureInfos[i].identifier) && IsDependant(main.usings, main.dependencies, main.path, fileChildClasses[k].path))
                        {
                            relatedFiles.AddRange(FindComponentDiagramContents(diagramStructureInfos[i], included));
                            break;
                        }
                }

                
                List<DiagramStructureInfo> mainChildClasses = ChildClassesOfStructureInfo(main);

                for (int k = 0; k < mainChildClasses.Count; ++k)
                {
                    //files that are dependnant on main
                    if (!included.Contains(diagramStructureInfos[i].identifier) && IsDependant(diagramStructureInfos[i].usings, diagramStructureInfos[i].dependencies, diagramStructureInfos[i].path, mainChildClasses[k].path))
                    {
                        relatedFiles.AddRange(FindComponentDiagramContents(diagramStructureInfos[i], included));
                        break;
                    }
                }
            }
            return relatedFiles;
        }

        List<DiagramStructureInfo> FindClassDiagramContents(DiagramStructureInfo main, List<string> included, int depth)
        {
            List<DiagramStructureInfo> classList = new List<DiagramStructureInfo>();

            if (main.angle == 0 || main.angle > depth)
                main.angle = depth;

            if (AlwaysIncludeOrExcludePath(main.path) == 2)
            {
                return classList;
            }

            if (!included.Contains(main.identifier))
            {
                included.Add(main.identifier);
                classList.Add(main);
            }

            //child classes. either aggregations or compositions
            for (int i = 0; i < main.children.Count; ++i)
            {
                if (main.children[i].type.Equals("Class") && (!included.Contains(main.children[i].identifier) || main.children[i].angle > depth + 1) && ((main.children[i].prefix.Contains("public") && depth <= classDiagramAggregationDepth) || depth <= classDiagramCompositionDepth))
                {
                    classList.AddRange(FindClassDiagramContents(main.children[i], included, depth + 1));
                }
            }

            //classes that are dependent on this
            for (int i = 1; i < diagramStructureInfos.Count; ++i)
            {
                if (diagramStructureInfos[i].type.Equals("Class"))
                {
                    //dependencies and associations of this class
                    if (IsDependant(main.usings, main.dependencies, main.path, diagramStructureInfos[i].path))
                    {
                        if ((!included.Contains(diagramStructureInfos[i].identifier) || diagramStructureInfos[i].angle > depth + 1) && ((!diagramStructureInfos[i].prefix.Contains("static") && depth <= classDiagramDependencyDepth) || (diagramStructureInfos[i].prefix.Contains("static") && depth <= classDiagramAssociationDepth)))
                            classList.AddRange(FindClassDiagramContents(diagramStructureInfos[i], included, depth + 1));
                    }

                    //classes dependant or associated to this class
                    if (IsDependant(diagramStructureInfos[i].usings, diagramStructureInfos[i].dependencies, diagramStructureInfos[i].path, main.path))
                    {
                        if ((!included.Contains(diagramStructureInfos[i].identifier) || diagramStructureInfos[i].angle > depth + 1) && ((main.prefix.Contains("static") && depth <= classDiagramDependencyDepth) || (main.prefix.Contains("static") && depth <= classDiagramAssociationDepth)))
                            classList.AddRange(FindClassDiagramContents(diagramStructureInfos[i], included, depth + 1));
                    }

                    //parent classes. either aggregations or compositions
                    if (diagramStructureInfos[i].IsParentTo(main.identifier, true) && (!included.Contains(diagramStructureInfos[i].identifier) || diagramStructureInfos[i].angle > depth + 1) && ((diagramStructureInfos[i].prefix.Contains("public") && depth <= classDiagramAggregationDepth) || depth <= classDiagramCompositionDepth))
                    {
                        classList.AddRange(FindClassDiagramContents(diagramStructureInfos[i], included, depth + 1));
                    }
                }


                if (diagramStructureInfos[i].type.Equals("Namespace"))
                {
                    List<DiagramStructureInfo> childClasses = ChildClassesOfStructureInfo(diagramStructureInfos[i]);
                    for (int k = 0; k < childClasses.Count; ++k)
                    {
                        //dependencies and associations of this class
                        if (IsDependant(main.usings, main.dependencies, main.path, childClasses[k].path))
                        {
                            if ((!included.Contains(childClasses[k].identifier) || childClasses[k].angle > depth + 1) && ((!childClasses[k].prefix.Contains("static") && depth <= classDiagramDependencyDepth) || (childClasses[k].prefix.Contains("static") && depth <= classDiagramAssociationDepth)))
                                classList.AddRange(FindClassDiagramContents(childClasses[k], included, depth + 1));
                        }

                        //classes dependant or associated to this class
                        if (IsDependant(childClasses[k].usings, childClasses[k].dependencies, childClasses[k].path, main.path))
                        {
                            if ((!included.Contains(childClasses[k].identifier) || childClasses[k].angle > depth + 1) && ((!main.prefix.Contains("static") && depth <= classDiagramDependencyDepth) || (main.prefix.Contains("static") && depth <= classDiagramAssociationDepth)))
                            {
                                classList.AddRange(FindClassDiagramContents(childClasses[k], included, depth + 1));
                            }
                        }

                        //parent classes. either aggregations or compositions
                        if (childClasses[k].IsParentTo(main.identifier, true) && (!included.Contains(childClasses[k].identifier) || childClasses[k].angle > depth + 1) && ((childClasses[k].prefix.Contains("public") && depth <= classDiagramAggregationDepth) || depth <= classDiagramCompositionDepth))
                        {
                            classList.AddRange(FindClassDiagramContents(childClasses[k], included, depth + 1));
                        }
                    }
                }
            }


            //inheritances of this class
            if (main.postfix.Length > 0 && classDiagramInheritanceDepth >= depth)
            {
                //the superclass of this class
                for (int i = 1; i < diagramStructureInfos.Count; ++i)
                {
                    if (diagramStructureInfos[i].type.Equals("Class"))
                    {
                        if (main.postfix.Equals(diagramStructureInfos[i].name) && (!included.Contains(diagramStructureInfos[i].identifier) || diagramStructureInfos[i].angle > depth + 1))
                            classList.AddRange(FindClassDiagramContents(diagramStructureInfos[i], included, depth + 1));
                    }
                    else if (diagramStructureInfos[i].type.Equals("Namespace"))
                    {
                        List<DiagramStructureInfo> childClasses = ChildClassesOfStructureInfo(diagramStructureInfos[i]);
                        for (int k = 0; k < childClasses.Count; ++k)
                        {
                            if (main.postfix.Equals(childClasses[k].name) && (!included.Contains(childClasses[k].identifier) || childClasses[k].angle > depth + 1))
                                classList.AddRange(FindClassDiagramContents(childClasses[k], included, depth + 1));
                        }
                    }
                }
            }

            //classes that inherit from this
            if (classDiagramInheritanceDepth >= depth)
            {
                //any classes that are inherited from this
                for (int i = 1; i < diagramStructureInfos.Count; ++i)
                {
                    if (diagramStructureInfos[i].type.Equals("Class"))
                    {
                        if (diagramStructureInfos[i].postfix.Equals(main.name) && (!included.Contains(diagramStructureInfos[i].identifier) || diagramStructureInfos[i].angle > depth + 1))
                            classList.AddRange(FindClassDiagramContents(diagramStructureInfos[i], included, depth + 1));
                    }
                    else if (diagramStructureInfos[i].type.Equals("Namespace"))
                    {
                        List<DiagramStructureInfo> childClasses = ChildClassesOfStructureInfo(diagramStructureInfos[i]);
                        for (int k = 0; k < childClasses.Count; ++k)
                        {
                            if (childClasses[k].postfix.Equals(main.name) && (!included.Contains(childClasses[k].identifier) || childClasses[k].angle > depth + 1))
                            {
                                classList.AddRange(FindClassDiagramContents(childClasses[k], included, depth + 1));
                            }
                        }
                    }
                }
            }
            

            return classList;
        }

        List<DiagramStructureInfo> FindPackageDiagramContents(DiagramStructureInfo main, List<string> included)
        {
            List<DiagramStructureInfo> packageList = new List<DiagramStructureInfo>();

            if (AlwaysIncludeOrExcludePath(main.path) == 2)
            {
                return packageList;
            }

            //actual main target
            if (included.Count == 0)
            {
                //making sure the main target is a top-level namespace and not a child itself
                for (int i = 1; i < diagramStructureInfos.Count; ++i)
                {
                    if (diagramStructureInfos[i].IsParentTo(main.identifier, true))
                    {
                        main = diagramStructureInfos[i];
                        break;
                    }
                }
            }

            included.Add(main.identifier);
            packageList.Add(main);

            //all packages that main is dependant on
            for (int i = 1; i < diagramStructureInfos.Count; ++i)
            {
                if (diagramStructureInfos[i].type.Equals("Namespace") && IsDependant(main.usings, main.dependencies, main.path, diagramStructureInfos[i].path))
                {
                    if (!included.Contains(diagramStructureInfos[i].identifier))
                        packageList.AddRange(FindPackageDiagramContents(diagramStructureInfos[i], included));
                }
            }

            //all packages that are dependant on main
            for (int i = 1; i < diagramStructureInfos.Count; ++i)
            {
                if (diagramStructureInfos[i].type.Equals("Namespace") && IsDependant(diagramStructureInfos[i].usings, diagramStructureInfos[i].dependencies, diagramStructureInfos[i].path, main.path))
                {
                    if (!included.Contains(diagramStructureInfos[i].identifier))
                        packageList.AddRange(FindPackageDiagramContents(diagramStructureInfos[i], included));
                }
            }

            return packageList;
        }

        DiagramStructureInfo FindCompositeStructureContents(DiagramStructureInfo main, string trail, List<string> referencedClasses)
        {
            DiagramStructureInfo element = new DiagramStructureInfo(main);
            element.postfix = element.name;

            List<DiagramStructureInfo> elementChildrenList = new List<DiagramStructureInfo>();

            string newTrail = element.name;
            if (trail.Length > 0)
                newTrail = trail + "." + newTrail;

            if (!trail.Contains(element.name) && (element.objectType.Equals("class") || element.objectType.Equals("struct")))
            {
                for (int i = 0; i < element.children.Count; ++i)
                {
                    if (!element.children[i].type.Equals("Variable") && !element.children[i].type.Equals("Property"))
                    {
                        continue;
                    }

                    int maxCount = 1;
                    string objectType = element.children[i].objectType;
                    if (element.children[i].objectType.StartsWith("List<") && element.children[i].objectType.EndsWith(">"))
                    {
                        objectType = element.children[i].objectType[5..^1];
                        maxCount = 2;
                    }
                    else if (element.children[i].objectType.EndsWith("[]"))
                    {
                        maxCount = 2;
                    }

                    List<string> splitType = ecd.DependenciesFromCodeLine(objectType);
                    string realType = "";

                    for (int k = 0; k < splitType.Count; ++k)
                    {
                        if (objectType.StartsWith(splitType[k]) && splitType[k].Length > realType.Length)
                        {
                            realType = splitType[k];
                        }
                    }

                    DiagramStructureInfo matchedType = LocalTypes(diagramStructureInfos, realType);
                    if (matchedType == null)
                    {
                        element.children.RemoveAt(i);
                        --i;
                        continue;
                    }

                    elementChildrenList.Add(FindCompositeStructureContents(matchedType, newTrail, referencedClasses));
                    referencedClasses.Add(matchedType.identifier);
                    elementChildrenList[^1].identifier= element.children[i].identifier;
                    elementChildrenList[^1].name = element.children[i].name;
                    elementChildrenList[^1].objectType = element.children[i].objectType;
                    elementChildrenList[^1].type = element.children[i].type;

                    // 0 if internal type, >1 if external
                    elementChildrenList[^1].angle = 1; 
                    if (element.IsParentTo(matchedType.identifier, true))
                        elementChildrenList[^1].angle = 0;

                    elementChildrenList[^1].postfix = PrefixCharacter(element.children[i]) + element.children[i].name + " : " + realType;
                    if (maxCount > 1)
                        elementChildrenList[^1].postfix += " [0.." + "*]";
                    else
                        elementChildrenList[^1].postfix += " [0.." + "1]";

                }

                for (int i = 0; i < element.children.Count; ++i)
                {
                    //checking if there's any classes that the class doesn't have direct objects of
                    if (referencedClasses.Contains(element.children[i].identifier) || !element.children[i].type.Equals("Class"))
                    {
                        element.children.RemoveAt(i);
                        --i;
                        continue;
                    }

                    //unreferenced class. adding it as a child
                    elementChildrenList.Add(FindCompositeStructureContents(element.children[i], newTrail, referencedClasses));
                    elementChildrenList[^1].postfix = PrefixCharacter(element.children[i]) +  " : " + element.children[i].name;
                }

                element.children = elementChildrenList;
            }
            else
            {
                element.children = new List<DiagramStructureInfo>();
            }

            for (int i = 0; i < element.children.Count; ++i)
            {
                if (AlwaysIncludeOrExcludePath(element.children[i].path) == 2)
                {
                    element.children.RemoveAt(i);
                    --i;
                }
            }

            return element;
        }

        /// <returns>A list of all the local types that are refenced in the given splitType objectType</returns>
        DiagramStructureInfo LocalTypes(List<DiagramStructureInfo> scope, string targetType)
        {
            DiagramStructureInfo foundType = null;
            for (int i = 0; i < scope.Count; ++i)
            {
                if (scope[i].objectType.Equals("namespace") ||  scope[i].objectType.Equals("class") || scope[i].objectType.Equals("struct"))
                {
                    //checking if this is the type
                    if (!scope[i].objectType.Equals("namespace"))
                    {
                        if ((targetType.Contains('.') && FullName(scope[i].path).EndsWith(targetType)) || (!targetType.Contains('.') && scope[i].name.Equals(targetType)))
                        {
                            foundType = scope[i];
                            break;
                        }
                    }

                    //checking children
                    DiagramStructureInfo childResult = LocalTypes(scope[i].children, targetType);
                    if (childResult != null)
                        return childResult;
                }
            }

            return foundType;
        }

        void CreateDiagramArrow(Rect start, Rect end, int startObstacle, int endObstacle, List<Rect> obstacles, bool endArrow, bool startArrow, int width, string startName, string endName, int type, int lineCount, bool nonOrthogonal)
        {
            //finding the amount of arrows that start/end from these rects
            float[] sidePos = new float[9] { 0.5f, 0.3f, 0.7f, 0.1f, 0.9f, 0.4f, 0.6f, 0.2f, 0.8f};
            int[] startConnections = new int[4] { 0, 0, 0, 0 };
            int[] endConnections = new int[4] { 0, 0, 0, 0 };
            for (int i = 0; i < diagramArrows.Count; ++i)
            {
                Vector2 firstWaypoint = diagramArrows[i].wayPoints[0];
                Vector2 lastWaypoint = diagramArrows[i].wayPoints[^1];

                if (diagramArrows[i].start.Equals(start))
                {
                    if (firstWaypoint.y == diagramArrows[i].start.yMax)
                        startConnections[0]++;
                    else if (firstWaypoint.y == diagramArrows[i].start.yMin)
                        startConnections[1]++;
                    else if (firstWaypoint.x == diagramArrows[i].start.xMin)
                        startConnections[2]++;
                    else if (firstWaypoint.x == diagramArrows[i].start.xMax)
                        startConnections[3]++;
                }

                if (diagramArrows[i].end.Equals(start))
                {
                    if (lastWaypoint.y == diagramArrows[i].end.yMax)
                        startConnections[0]++;
                    else if (lastWaypoint.y == diagramArrows[i].end.yMin)
                        startConnections[1]++;
                    else if (lastWaypoint.x == diagramArrows[i].end.xMin)
                        startConnections[2]++;
                    else if (lastWaypoint.x == diagramArrows[i].end.xMax)
                        startConnections[3]++;
                }

                if (diagramArrows[i].start.Equals(end))
                {
                    if (firstWaypoint.y == diagramArrows[i].start.yMax)
                        endConnections[0]++;
                    else if (firstWaypoint.y == diagramArrows[i].start.yMin)
                        endConnections[1]++;
                    else if (firstWaypoint.x == diagramArrows[i].start.xMin)
                        endConnections[2]++;
                    else if (firstWaypoint.x == diagramArrows[i].start.xMax)
                        endConnections[3]++;
                }

                if (diagramArrows[i].end.Equals(end))
                {
                    if (lastWaypoint.y == diagramArrows[i].end.yMax)
                        endConnections[0]++;
                    else if (lastWaypoint.y == diagramArrows[i].end.yMin)
                        endConnections[1]++;
                    else if (lastWaypoint.x == diagramArrows[i].end.xMin)
                        endConnections[2]++;
                    else if (lastWaypoint.x == diagramArrows[i].end.xMax)
                        endConnections[3]++;
                }
            }

            List<Vector2> wayPoints = new List<Vector2>();

            //finding the two closest sides of the rectangles
            int startSide = 0;
            int endSide = 0;
            float shortestDist = float.MaxValue;

            Vector2 startSidePos = new Vector2();
            Vector2 endSidePos = new Vector2();
            Vector2 shortestStartSidePos = new Vector2();
            Vector2 shortestEndSidePos = new Vector2();

            for (int i = 0; i < 4; ++i)
            {
                float startPosPercent = sidePos[startConnections[i]%9];
                if (nonOrthogonal)
                    startPosPercent = 0.5f;
                if (i == 0)
                {
                    //down
                    startSidePos = new Vector2(start.xMin * startPosPercent + start.xMax * (1-startPosPercent), start.yMax);
                }
                else if (i == 1)
                {
                    //up
                    startSidePos = new Vector2(start.xMin * startPosPercent + start.xMax * (1 - startPosPercent), start.yMin);
                }
                else if (i == 2)
                {
                    //left
                    startSidePos = new Vector2(start.xMin, start.yMin * startPosPercent + start.yMax * (1 - startPosPercent));
                }
                else
                {
                    //right
                    startSidePos = new Vector2(start.xMax, start.yMin * startPosPercent + start.yMax * (1 - startPosPercent));
                }
                for (int k = 0; k < 4; ++k)
                {
                    float endPosPercent = sidePos[endConnections[k] % 9];
                    if (nonOrthogonal)
                        endPosPercent = 0.5f;
                    if (k == 0)
                    {
                        //down
                        endSidePos = new Vector2(end.xMin * endPosPercent + end.xMax * (1 - endPosPercent), end.yMax);
                    }
                    else if (k == 1)
                    {
                        //up
                        endSidePos = new Vector2(end.xMin * endPosPercent + end.xMax * (1 - endPosPercent), end.yMin);
                    }
                    else if (k == 2)
                    {
                        //left
                        endSidePos = new Vector2(end.xMin, end.yMin * endPosPercent + end.yMax * (1 - endPosPercent));
                    }
                    else
                    {
                        //right
                        endSidePos = new Vector2(end.xMax, end.yMin * endPosPercent + end.yMax * (1 - endPosPercent));
                    }

                    if ((endSidePos - startSidePos).magnitude < shortestDist)
                    {
                        shortestDist = (endSidePos - startSidePos).magnitude;
                        shortestStartSidePos = startSidePos;
                        shortestEndSidePos = endSidePos;
                        startSide = i;
                        endSide = k;
                    }
                }
            } 

            Vector2 startDirection;
            Vector2 endDirection;
            Vector2 startSideMin;
            Vector2 startSideMax;
            Vector2 endSideMin;
            Vector2 endSideMax;

            if (startSide == 0) //down
            {
                startDirection = new Vector2(0, 1);
                startSideMin = new Vector2(start.xMin, start.yMax);
                startSideMax = new Vector2(start.xMax, start.yMax);
            }
            else if (startSide == 1)//up
            {
                startDirection = new Vector2(0, -1);
                startSideMin = new Vector2(start.xMax, start.yMin);
                startSideMax = new Vector2(start.xMin, start.yMin);
            }
            else if (startSide == 2) //left
            {
                startDirection = new Vector2(-1, 0);
                startSideMin = new Vector2(start.xMin, start.yMin);
                startSideMax = new Vector2(start.xMin, start.yMax);
            }
            else //right
            {
                startDirection = new Vector2(1, 0);
                startSideMin = new Vector2(start.xMax, start.yMax);
                startSideMax = new Vector2(start.xMax, start.yMin);
            }

            if (endSide == 0) //down
            {
                endDirection = new Vector2(0, 1);
                endSideMin = new Vector2(end.xMin, end.yMax);
                endSideMax = new Vector2(end.xMax, end.yMax);
            }
            else if (endSide == 1)//up
            {
                endDirection = new Vector2(0, -1);
                endSideMin = new Vector2(end.xMax, end.yMin);
                endSideMax = new Vector2(end.xMin, end.yMin);
            }
            else if (endSide == 2) //left
            {
                endDirection = new Vector2(-1, 0);
                endSideMin = new Vector2(end.xMin, end.yMin);
                endSideMax = new Vector2(end.xMin, end.yMax);
            }
            else //right
            {
                endDirection = new Vector2(1, 0);
                endSideMin = new Vector2(end.xMax, end.yMax);
                endSideMax = new Vector2(end.xMax, end.yMin);
            } 

            if (nonOrthogonal)
            {
                if (end.position == diagramStructureRects[0].Value.position)
                {
                    float angle = Vector2.SignedAngle(shortestEndSidePos - shortestStartSidePos, -endDirection);
                    //shifting the endposition towards the edge depending on the angle of the arrow
                    //0 degrees = middle, 60/-60 degrees on the corner
                    Vector2 angleBasedEndSidePos;
                    if (angle < 0)
                        angleBasedEndSidePos = (1 - Mathf.Max(angle, -60f) / -60f) * (endSideMin + endSideMax)/2f + (Mathf.Max(angle, -60f) / -60f) * endSideMin;
                    else
                        angleBasedEndSidePos = (1 - Mathf.Min(angle, 60f) / 60f) * (endSideMin + endSideMax) / 2f + (Mathf.Min(angle, 60f) / 60f) * endSideMax;
                    shortestEndSidePos = angleBasedEndSidePos;
                }
                else
                {
                    float angle = Vector2.SignedAngle(shortestStartSidePos - shortestEndSidePos, -startDirection);
                    //shifting the startposition towards the edge depending on the angle of the arrow
                    //0 degrees = middle, 60/-60 degrees on the corner
                    Vector2 angleBasedStartSidePos;
                    if (angle < 0)
                        angleBasedStartSidePos = (1 - Mathf.Max(angle, -60f) / -60f) * (startSideMin + startSideMax) / 2f + (Mathf.Max(angle, -60f) / -60f) * startSideMin;
                    else
                        angleBasedStartSidePos = (1 - Mathf.Min(angle, 60f) / 60f) * (startSideMin + startSideMax) / 2f + (Mathf.Min(angle, 60f) / 60f) * startSideMax;
                    shortestStartSidePos = angleBasedStartSidePos;
                }

                wayPoints.Add(shortestStartSidePos);
                wayPoints.Add(shortestEndSidePos);
                diagramArrows.Add(new DiagramArrow(start, end, startArrow, endArrow, width, startName, endName, wayPoints, type));
                return;
            }

            wayPoints.Add(shortestStartSidePos);
            int startDistance = 10;
            int endDistance = 10;
            Vector2 lineStart = shortestStartSidePos + startDirection * (startDistance * width + Mathf.Min(startConnections[startSide], 9));
            Vector2 lineEnd = shortestEndSidePos + endDirection * (endDistance * width + Mathf.Min(endConnections[endSide], 9));
            while (startDistance > 1 && ((lineEnd.x - lineStart.x) * (lineEnd.x - shortestStartSidePos.x) < 0 || (lineEnd.y - lineStart.y) * (lineEnd.y - shortestStartSidePos.y) < 0))
            {
                startDistance--;
                lineStart = shortestStartSidePos + startDirection * (startDistance * width + Mathf.Min(startConnections[startSide], 9));
            }
            while (endDistance > 1 && ((lineStart.x - lineEnd.x) * (lineStart.x - shortestEndSidePos.x) < 0 || (lineStart.y - lineEnd.y) * (lineStart.y - shortestEndSidePos.y) < 0))
            {
                endDistance--;
                lineEnd = shortestEndSidePos + endDirection * (endDistance * width + Mathf.Min(endConnections[endSide], 9));
            }

            List<Vector2> path = ArrowPath(new ArrowNode(lineStart, startSide), new ArrowNode(lineEnd, endSide), obstacles, startObstacle, endObstacle, width, type == 0 || type == 5, lineCount);
            
            if (type == 0 || type == 5)
            {
                List<Vector2> expandedPath = new List<Vector2>();
                for (int i = 0; i < path.Count-1; ++i)
                {
                    expandedPath.Add(path[i]);

                    float dist = (path[i+1] - path[i]).magnitude;
                    int steps = (int)(dist / 10);

                    for (int k = 1; k < steps; ++k)
                    {
                        float p = k * 1f / steps;
                        expandedPath.Add(path[i] * (1-p) + path[i+1]*(p));
                    }
                }

                expandedPath.Add(path[^1]);

                path = expandedPath;
            }

            for (int i = 0; i < path.Count; ++i)
            {
                wayPoints.Add(path[i]);
            }
            wayPoints.Add(shortestEndSidePos);

            diagramArrows.Add(new DiagramArrow(start, end, startArrow, endArrow, width, startName, endName, wayPoints, type));
        }

        List<Vector2> ArrowPath(ArrowNode start, ArrowNode end, List<Rect> obstacles, int startObstacleIndex, int endObstacleIndex, float width, bool fullPath, int lineCount)
        {
            float strictness = 0.7f;

            //caching rects for other lines beforehand
            List<Rect> horLines = new List<Rect>();
            List<Rect> verLines = new List<Rect>();
            float linemargin = width * 2;
            for (int k = 0; k < diagramArrows.Count; ++k)
            {
                int linestart = 0;
                int lineEnd;
                bool previousHorizontal = false;
                for (int j = 0; j < diagramArrows[k].wayPoints.Count - 1; ++j)
                {
                    bool lineHorizontal = diagramArrows[k].wayPoints[j].x != diagramArrows[k].wayPoints[j + 1].x;

                    if (lineHorizontal == previousHorizontal || linestart == j)
                    {
                        previousHorizontal = lineHorizontal;
                        continue;
                    }

                    lineEnd = j;
                    if (previousHorizontal)
                    {
                        horLines.Add(new Rect(Mathf.Min(diagramArrows[k].wayPoints[linestart].x, diagramArrows[k].wayPoints[lineEnd].x), diagramArrows[k].wayPoints[linestart].y - linemargin, Mathf.Abs(diagramArrows[k].wayPoints[linestart].x - diagramArrows[k].wayPoints[lineEnd].x), linemargin * 2));
                    }

                    else
                    {
                        verLines.Add(new Rect(diagramArrows[k].wayPoints[linestart].x - linemargin, Mathf.Min(diagramArrows[k].wayPoints[linestart].y, diagramArrows[k].wayPoints[lineEnd].y), linemargin * 2, Mathf.Abs(diagramArrows[k].wayPoints[linestart].y - diagramArrows[k].wayPoints[lineEnd].y)));
                    }


                    previousHorizontal = lineHorizontal;
                    linestart = j;
                }

                lineEnd = diagramArrows[k].wayPoints.Count - 1;
                bool lastLineHorizontal = diagramArrows[k].wayPoints[linestart].x != diagramArrows[k].wayPoints[lineEnd].x;
                if (lastLineHorizontal)
                {
                    horLines.Add(new Rect(Mathf.Min(diagramArrows[k].wayPoints[linestart].x, diagramArrows[k].wayPoints[lineEnd].x), diagramArrows[k].wayPoints[linestart].y - linemargin, Mathf.Abs(diagramArrows[k].wayPoints[linestart].x - diagramArrows[k].wayPoints[lineEnd].x), linemargin * 2));
                }
                else
                {
                    verLines.Add(new Rect(diagramArrows[k].wayPoints[linestart].x - linemargin, Mathf.Min(diagramArrows[k].wayPoints[linestart].y, diagramArrows[k].wayPoints[lineEnd].y), linemargin * 2, Mathf.Abs(diagramArrows[k].wayPoints[linestart].y - diagramArrows[k].wayPoints[lineEnd].y)));
                }

            }

            //basic A* logic
            float stepSize = 100;
            List<ArrowNode> openSet = new List<ArrowNode>() { start };
            Dictionary<ArrowNode, ArrowNode> cameFrom = new Dictionary<ArrowNode, ArrowNode>();
            Dictionary<ArrowNode, float> gScore = new Dictionary<ArrowNode, float>();
            gScore[start] = 0;
            Dictionary<ArrowNode, float> fScore = new Dictionary<ArrowNode, float>();
            fScore[start] = ArrowHeuristicEstimate(start.pos, end.pos, stepSize, strictness);
            ArrowNode current = start;

            bool skipLine = false;

            int counter = 0;
            ArrowNode bestOption = start;
            while (openSet.Count > 0)
            {
                if (openSet.Count > 1)
                    skipLine = false;
                counter++;

                int currentIndex = 0;
                float minFScore = float.MaxValue;
                for (int i = openSet.Count - 1; i >= 0; --i)
                {
                    if (fScore[openSet[i]] < minFScore)
                    {
                        currentIndex = i;
                        minFScore = fScore[openSet[i]];

                        if (minFScore < fScore[bestOption])
                            bestOption = openSet[i];
                    }
                }

                current = openSet[currentIndex];

                if (current.pos == end.pos)
                {
                    break;
                }

                if (counter > 1000)
                {
                    //Cutting line calculation if a good path cannot be found
                    Debug.LogWarning("Cutting a dependency line short. Couldn't find a usable path in a reasonable time.");
                    current = bestOption;
                    break;
                }

                openSet.RemoveAt(currentIndex);


                for (int i = 0; i < 4; ++i)
                {
                    float d = stepSize;
                    Vector2 newPos = new Vector2();
                    if (i == 0) //down
                    {
                        float step = stepSize;
                        newPos = current.pos + new Vector2(0, step);
                        if (end.pos.y - current.pos.y <= step*2f && end.pos.y - current.pos.y > 0)
                        {
                            newPos.y = end.pos.y;
                            d = end.pos.y - current.pos.y;
                            if (end.pos == newPos)
                            {
                                d = stepSize;
                                float lengthMultiplier = 30f / (lineCount + 5);
                                ArrowNode previous = current;
                                while (lengthMultiplier > 0.5f && end.dir == 1 && cameFrom.ContainsKey(previous) && previous.dir == 0)
                                {
                                    lengthMultiplier /= 1.1f;
                                    previous = cameFrom[previous];
                                }
                                d *= lengthMultiplier;
                            }
                        }

                        if (current.dir == 1) continue;
                        if (current.dir > 1)
                            d += stepSize / 5;
                    }
                    else if (i == 1) //up
                    {
                        float step = -stepSize;
                        newPos = current.pos + new Vector2(0, step);
                        if (end.pos.y - current.pos.y >= step * 2f && end.pos.y - current.pos.y < 0)
                        {
                            newPos.y = end.pos.y;
                            d = current.pos.y - end.pos.y;
                            if (end.pos == newPos)
                            {
                                d = stepSize;
                                float lengthMultiplier = 30f / (lineCount + 5);
                                ArrowNode previous = current;
                                while (lengthMultiplier > 0.5f && end.dir == 0 && cameFrom.ContainsKey(previous) && previous.dir == 1)
                                {
                                    lengthMultiplier /= 1.1f;
                                    previous = cameFrom[previous];
                                }
                                d *= lengthMultiplier;
                            }
                        }

                        if (current.dir == 0) continue;
                        if (current.dir > 1)
                            d += stepSize / 5;
                    }
                    else if (i == 2) //left
                    {
                        float step = -stepSize;
                        newPos = current.pos + new Vector2(step, 0);
                        if (end.pos.x - current.pos.x >= step * 2f && end.pos.x - current.pos.x < 0)
                        {
                            newPos.x = end.pos.x;
                            d = current.pos.x - end.pos.x;
                            if (end.pos == newPos)
                            {
                                d = stepSize;
                                float lengthMultiplier = 30f / (lineCount + 5);
                                ArrowNode previous = current;
                                while (lengthMultiplier > 0.5f && end.dir == 3 && cameFrom.ContainsKey(previous) && previous.dir == 2)
                                {
                                    lengthMultiplier /= 1.1f;
                                    previous = cameFrom[previous];
                                }
                                d *= lengthMultiplier;
                            }
                        }

                        if (current.dir == 3) continue;
                        if (current.dir < 2)
                            d += stepSize / 5;
                    }
                    else if (i == 3) //right
                    {
                        float step = stepSize;
                        newPos = current.pos + new Vector2(step, 0);
                        if (end.pos.x - current.pos.x <= step * 2f && end.pos.x - current.pos.x > 0)
                        {
                            newPos.x = end.pos.x;
                            d = end.pos.x - current.pos.x;
                            if (end.pos == newPos)
                            {
                                d = stepSize;
                                float lengthMultiplier = 30f / (lineCount + 5);
                                ArrowNode previous = current;
                                while (lengthMultiplier > 0.5f && end.dir == 2 && cameFrom.ContainsKey(previous) && previous.dir == 3)
                                {
                                    lengthMultiplier /= 1.1f;
                                    previous = cameFrom[previous];
                                }
                                d *= lengthMultiplier;
                            }
                        }

                        if (current.dir == 2) continue;
                        if (current.dir < 2)
                            d += stepSize / 5;
                    }

                    Rect lineRect;
                    if (i > 1)
                    {
                        lineRect = new Rect(Mathf.Min(current.pos.x, newPos.x), current.pos.y - width / 2f, Mathf.Abs(current.pos.x - newPos.x), width);
                    }
                    else
                    {
                        lineRect = new Rect(current.pos.x - width / 2f, Mathf.Min(current.pos.y, newPos.y), width, Mathf.Abs(current.pos.y - newPos.y));
                    }


                    //box rect obstacles
                    bool overlap = false;
                    for (int k = 0; k < obstacles.Count; ++k)
                    {
                        //not worrying about colliding with the end rect at the very end
                        if ((endObstacleIndex < 0 || endObstacleIndex >= obstacles.Count || startObstacleIndex < 0 || startObstacleIndex >= obstacles.Count) || (k == endObstacleIndex && newPos == end.pos) || (k == startObstacleIndex && current.pos == start.pos))
                        {
                            continue;
                        }

                        if (obstacles[k].Overlaps(lineRect) && (k == startObstacleIndex || !RectFullyContainsOther(obstacles[k], obstacles[startObstacleIndex])) && (k == endObstacleIndex || !RectFullyContainsOther(obstacles[k], obstacles[endObstacleIndex])))
                        {
                            overlap = true; 
                            break;
                        }
                    }
                    if (overlap)
                        continue;
                    
                    
                    /* FOR AVOIDING OVERLAPPING LINES
                    if (!skipLine && newPos != end.pos && lineCount < 25)
                    {
                        if (i < 2)
                        {
                            for (int k = 0; k < verLines.Count; ++k)
                            {
                                if (verLines[k].Overlaps(lineRect))
                                {
                                    overlap = true;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            for (int k = 0; k < horLines.Count; ++k)
                            {
                                if (horLines[k].Overlaps(lineRect))
                                {
                                    overlap = true;
                                    break;
                                }
                            }
                        }
                    }
                    */

                    if (overlap)
                    {
                        //allowing the best line to skip the check for other lines if it has no other choices
                        if (!skipLine && (i == 3 || (current.dir == 2 && i == 2)) && openSet.Count == 0)
                        {
                            openSet.Add(bestOption);
                            skipLine = true;
                        }
                        continue;
                    }

                    ArrowNode neighBor = new ArrowNode(newPos, i);


                    float tmpGScore = gScore[current] + d * strictness;
                    if (!gScore.ContainsKey(neighBor) || gScore[neighBor] > tmpGScore)
                    {
                        if (cameFrom.ContainsKey(neighBor))
                            cameFrom[neighBor] = current;
                        else
                            cameFrom.Add(neighBor, current);

                        if (gScore.ContainsKey(neighBor))
                            gScore[neighBor] = tmpGScore;
                        else
                            gScore.Add(neighBor, tmpGScore);

                        if (fScore.ContainsKey(neighBor))
                            fScore[neighBor] = tmpGScore + ArrowHeuristicEstimate(neighBor.pos, end.pos, stepSize, strictness);
                        else
                            fScore.Add(neighBor, tmpGScore + ArrowHeuristicEstimate(neighBor.pos, end.pos, stepSize, strictness));

                        if (!openSet.Contains(neighBor))
                            openSet.Add(neighBor);
                    }
                }
            }

            List<Vector2> wayPoints = new List<Vector2>();
            wayPoints.Add(current.pos);
            while (cameFrom.ContainsKey(current))
            {
                if (fullPath || cameFrom[current].dir != current.dir || cameFrom[current].pos.Equals(start.pos))
                    wayPoints.Add(cameFrom[current].pos);
                current = cameFrom[current];
            }
            wayPoints.Reverse();
            return wayPoints;
        }

        void DrawLine(DiagramArrow arrow)
        {
            float pixelWidth = arrow.width / (Screen.width * dpiScaleFactor) * zoomScale;
            float pixelHeight = arrow.width / (Screen.height * dpiScaleFactor) * zoomScale;

            bool highlightArrow = false;
            if (highlightedArrow != null && highlightedArrow.start.Equals(arrow.start) && highlightedArrow.end.Equals(arrow.end))
            {
                highlightArrow = true;
            }

            Color color = GetArrowColor(arrow);
            if (highlightArrow)
                color = colorList[1];

            bool clickHappened = false;
            bool arrowClicked = false;
            Vector3 cornerVector = new Vector3();
            Vector3 lineVector = new Vector3();
            Vector3 startCornerVector = new Vector3();
            Vector3 startLineVector = new Vector3();
            Vector2 startCorner1 = new Vector2();
            Vector2 startCorner2 = new Vector2();
            Vector2 endCorner1 = new Vector2();
            Vector2 endCorner2 = new Vector2();

            Vector2 glSpaceOffset = PosToGLSpace(diagramOffset + scrollPos * zoomScale);

            for (int i = 0; i < arrow.wayPoints.Count - 1; ++i)
            {
                //not drawing something that's not visible
                if ((arrow.wayPoints[i].y - scrollPos.y < -20 / zoomScale && arrow.wayPoints[i + 1].y - scrollPos.y < -20 / zoomScale) || (arrow.wayPoints[i].y - scrollPos.y > ((Screen.height * dpiScaleFactor) - 50) / zoomScale && arrow.wayPoints[i + 1].y - scrollPos.y > ((Screen.height * dpiScaleFactor) - 50) / zoomScale)
                    || (arrow.wayPoints[i].x - scrollPos.x < -20 / zoomScale && arrow.wayPoints[i + 1].x - scrollPos.x < -20 / zoomScale) || (arrow.wayPoints[i].x - scrollPos.x > ((Screen.width * dpiScaleFactor) + 20) / zoomScale && arrow.wayPoints[i + 1].x - scrollPos.x > ((Screen.width * dpiScaleFactor) + 20) / zoomScale))
                {
                    continue;
                }

                float angle = Vector2.SignedAngle(new Vector2(0, 1), new Vector2(arrow.wayPoints[i + 1].x * zoomScale - arrow.wayPoints[i].x * zoomScale, arrow.wayPoints[i + 1].y * zoomScale - arrow.wayPoints[i].y * zoomScale));

                cornerVector = Quaternion.Euler(0, 0, -angle) * new Vector3(1, 0, 0);
                cornerVector.x *= pixelWidth;
                cornerVector.y *= pixelHeight;

                lineVector = Quaternion.Euler(0, 0, -angle - 90) * new Vector3(1, 0, 0);
                lineVector.x *= pixelWidth;
                lineVector.y *= pixelHeight;

                if (i == 0)
                {
                    startCornerVector = cornerVector;
                    startLineVector = lineVector;
                }

                Vector2[] corners = new Vector2[4];

                corners[0] = PosToGLSpace(new Vector2((arrow.wayPoints[i].x) * zoomScale + diagramOffset.x, (arrow.wayPoints[i].y) * zoomScale + diagramOffset.y)) + cornerVector - lineVector;
                corners[1] = PosToGLSpace(new Vector2((arrow.wayPoints[i].x) * zoomScale + diagramOffset.x, (arrow.wayPoints[i].y) * zoomScale + diagramOffset.y)) - cornerVector - lineVector;
                corners[2] = PosToGLSpace(new Vector2((arrow.wayPoints[i + 1].x) * zoomScale + diagramOffset.x, (arrow.wayPoints[i + 1].y) * zoomScale + diagramOffset.y)) - cornerVector + lineVector;
                corners[3] = PosToGLSpace(new Vector2((arrow.wayPoints[i + 1].x) * zoomScale + diagramOffset.x, (arrow.wayPoints[i + 1].y) * zoomScale + diagramOffset.y)) + cornerVector + lineVector;

                Vector2[] visibleCorners = new Vector2[4];

                visibleCorners[0] = corners[0];
                visibleCorners[1] = corners[1];
                visibleCorners[2] = corners[2];
                visibleCorners[3] = corners[3];

                if (arrow.endArrow && i == arrow.wayPoints.Count - 2)
                {
                    int endMargin = 5;
                    if (arrow.type == 0 || arrow.type == 5)
                    {
                        endMargin = 0;
                    }
                    else if (arrow.type == 2)
                    {
                        endMargin = 10;
                    }
                    else if (arrow.type == 3)
                    {
                        endMargin = 7;
                    }
                    else if (arrow.type == 4)
                    {
                        endMargin = 10;
                    }

                    visibleCorners[2] = corners[2] - endMargin * (Vector2)(lineVector);
                    visibleCorners[3] = corners[3] - endMargin * (Vector2)(lineVector);
                }
                else if (arrow.type == 0 || arrow.type == 5)
                {
                    visibleCorners[0] += (Vector2)lineVector;
                    visibleCorners[1] += (Vector2)lineVector;
                }

                if (arrow.startArrow && i == 0)
                {
                    visibleCorners[0] = corners[0] + 5 * (Vector2)(lineVector);
                    visibleCorners[1] = corners[1] + 5 * (Vector2)(lineVector);
                }

                if ((arrow.type != 0 && arrow.type != 5) || i % 2 != 1 || i >= arrow.wayPoints.Count - 2)
                {
                    if (lineMat.SetPass(0))
                    {
                        GL.PushMatrix();
                        GL.LoadOrtho();
                        GL.Begin(GL.QUADS);
                        GL.Color(color);
                        for (int k = 0; k < visibleCorners.Length; ++k)
                        {
                            if (visibleCorners[k].y > glSpaceOffset.y)
                            {
                                if (visibleCorners[visibleCorners.Length - 1 - k].y < glSpaceOffset.y)
                                {
                                    float y = visibleCorners[k].y - visibleCorners[visibleCorners.Length - 1 - k].y;
                                    float cutPointY = visibleCorners[k].y - glSpaceOffset.y;
                                    float p = cutPointY / y;
                                    visibleCorners[k] = (1 - p) * visibleCorners[k] + p * visibleCorners[visibleCorners.Length - 1 - k];
                                }
                                else
                                {
                                    visibleCorners[visibleCorners.Length - 1 - k] = visibleCorners[k];
                                }
                            }
                            GL.Vertex(visibleCorners[k]);
                        }
                        GL.End();
                        GL.PopMatrix();
                    }
                }

                if (i == 0)
                {
                    startCorner1 = corners[0];
                    startCorner2 = corners[1];
                }
                if (i == arrow.wayPoints.Count - 2)
                {
                    endCorner1 = corners[2];
                    endCorner2 = corners[3];
                }

                //checking for clicks on this part of the arrow
                if (Event.current != null && Event.current.type == EventType.MouseUp && !movingInfoBox && !movableBoxes && !dragOn)
                {
                    clickHappened = true;

                    bool thisArrowClicked = false;
                    //non-orthogonal arrows
                    //Form a triangle between startpoint, endpoint and mouse position
                    //The height of the triangle is the distance from the line to the mouse position
                    if (arrow.wayPoints[i + 1].x != arrow.wayPoints[i].x && arrow.wayPoints[i + 1].y != arrow.wayPoints[i].y)
                    {
                        Vector2 hypotenusa = (Event.current.mousePosition + scrollPos) * zoomScale - (arrow.wayPoints[i]) * zoomScale;
                        Vector2 line = (arrow.wayPoints[i + 1]) * zoomScale - (arrow.wayPoints[i]) * zoomScale;
                        float triangleAngle = Vector2.Angle((arrow.wayPoints[i + 1]) * zoomScale - (arrow.wayPoints[i]) * zoomScale, hypotenusa);
                        float height = Mathf.Sin(triangleAngle / 360f * 2f * Mathf.PI) * hypotenusa.magnitude;
                        if (hypotenusa.magnitude < line.magnitude && triangleAngle < 100 && height < 15 * zoomScale)
                        {
                            thisArrowClicked = true;
                        }
                    }
                    //orthogonal arrows
                    else
                    {
                        Rect orthogonalClickBox = new Rect((Mathf.Min(arrow.wayPoints[i].x, arrow.wayPoints[i + 1].x) - 10) * zoomScale, (Mathf.Min(arrow.wayPoints[i].y, arrow.wayPoints[i + 1].y) - 10) * zoomScale, (Mathf.Abs(arrow.wayPoints[i].x - arrow.wayPoints[i + 1].x) + 20) * zoomScale, (Mathf.Abs(arrow.wayPoints[i].y - arrow.wayPoints[i + 1].y) + 20) * zoomScale);

                        if (orthogonalClickBox.Contains((Event.current.mousePosition + scrollPos) * zoomScale))
                        {
                            thisArrowClicked = true;
                        }
                    }

                    if (thisArrowClicked)
                    {
                        if (!highlightArrow)
                            callRepaint = true;
                        arrowClicked = true;
                        highlightArrow = true;
                        callRepaint = true;
                        highlightedArrow = arrow;
                    }
                }

            }

            if (!arrowClicked && clickHappened)
            {
                if (highlightArrow)
                {
                    callRepaint = true;
                }
                if (highlightedArrow != null && highlightedArrow.start.Equals(arrow.start) && highlightedArrow.end.Equals(arrow.end))
                {
                    highlightedArrow = null;
                }
            }
            List<Vector2> arrowCorners = new List<Vector2>();

            int glType = GL.TRIANGLES;

            //only drawing arrowhead if the end of the line is visible
            if (endCorner1 != new Vector2() && endCorner2 != new Vector2())
            {

                //drawing non-solid shapes in a separate method with the width taken into account
                //default arrow
                if (arrow.type < 0 || arrow.type > 5)
                {
                    arrowCorners.Add((endCorner1 + endCorner2) / 2f);
                    arrowCorners.Add(endCorner2 + 3 * (Vector2)(cornerVector) - 7 * (Vector2)(lineVector));
                    arrowCorners.Add(endCorner1 - 3 * (Vector2)(cornerVector) - 7 * (Vector2)(lineVector));
                }
                else if (arrow.type == 0 || arrow.type == 5)
                {
                    // dependency: dashed line, open hollow triangle arrow
                    arrowCorners.Add(endCorner1 - 3 * (Vector2)(cornerVector) - 7 * (Vector2)(lineVector));
                    arrowCorners.Add((endCorner1 + endCorner2) / 2f);
                    arrowCorners.Add(endCorner2 + 3 * (Vector2)(cornerVector) - 7 * (Vector2)(lineVector));

                    GLDrawEndShape(arrowCorners, color, pixelWidth, pixelHeight, glSpaceOffset);
                    return;
                }
                else if (arrow.type == 1)
                {
                    // association: solid line, no arrow
                    return;
                }
                else if (arrow.type == 2)
                {
                    // aggregation: solid line, hollow diamond arrow
                    arrowCorners.Add(endCorner2 + 2 * (Vector2)(cornerVector) - 5 * (Vector2)(lineVector));
                    arrowCorners.Add((endCorner1 + endCorner2) / 2f);
                    arrowCorners.Add(endCorner1 - 2 * (Vector2)(cornerVector) - 5 * (Vector2)(lineVector));
                    arrowCorners.Add((endCorner1 + endCorner2) / 2f - 10 * (Vector2)(lineVector));
                    arrowCorners.Add(endCorner2 + 2 * (Vector2)(cornerVector) - 5 * (Vector2)(lineVector));

                    GLDrawEndShape(arrowCorners, color, pixelWidth, pixelHeight, glSpaceOffset);
                    return;
                }
                else if (arrow.type == 3)
                {
                    // inheritance: solid line, hollow triange arrow
                    arrowCorners.Add(endCorner2 + 3 * (Vector2)(cornerVector) - 7 * (Vector2)(lineVector));
                    arrowCorners.Add((endCorner1 + endCorner2) / 2f);
                    arrowCorners.Add(endCorner1 - 3 * (Vector2)(cornerVector) - 7 * (Vector2)(lineVector));
                    arrowCorners.Add(endCorner2 + 3 * (Vector2)(cornerVector) - 7 * (Vector2)(lineVector));

                    GLDrawEndShape(arrowCorners, color, pixelWidth, pixelHeight, glSpaceOffset);
                    return;
                }
                else if (arrow.type == 4)
                {
                    // composition: solid line, solid diamond arrow
                    glType = GL.QUADS;
                    arrowCorners.Add(endCorner2 + 2 * (Vector2)(cornerVector) - 6 * (Vector2)(lineVector));
                    arrowCorners.Add((endCorner1 + endCorner2) / 2f);
                    arrowCorners.Add(endCorner1 - 2 * (Vector2)(cornerVector) - 6 * (Vector2)(lineVector));
                    arrowCorners.Add((endCorner1 + endCorner2) / 2f - 12 * (Vector2)(lineVector));
                }

                //end of the arrow
                if (arrow.endArrow)
                {
                    if (lineMat.SetPass(0))
                    {
                        GL.PushMatrix();
                        GL.LoadOrtho();
                        GL.Begin(glType);
                        GL.Color(color);
                        for (int i = 0; i < arrowCorners.Count; ++i)
                        {
                            if (arrowCorners[i].y > glSpaceOffset.y)
                                arrowCorners[i] = new Vector2(arrowCorners[i].x, glSpaceOffset.y);
                            GL.Vertex(arrowCorners[i]);
                        }
                        GL.End();
                        GL.PopMatrix();
                    }
                }
            }

            //only drawing the start arrowhead if the start of the line is visible
            if (startCorner1 != new Vector2() && startCorner2 != new Vector2())
            {
                //drawing an arrowhead to the other direction as well (only in dependency mode, meaning always arrow type < 0)
                if (arrow.startArrow)
                {

                    Vector2[] startArrowPoints = new Vector2[3];
                    startArrowPoints[0] = (startCorner1 + startCorner2) / 2f;
                    startArrowPoints[1] = startCorner1 + 3 * (Vector2)(startCornerVector) + 7 * (Vector2)(startLineVector);
                    startArrowPoints[2] = startCorner2 - 3 * (Vector2)(startCornerVector) + 7 * (Vector2)(startLineVector);

                    //end of the arrow
                    if (lineMat.SetPass(0))
                    {
                        GL.PushMatrix();
                        GL.LoadOrtho();
                        GL.Begin(GL.TRIANGLES);
                        GL.Color(color);
                        for (int i = 0; i < startArrowPoints.Length; ++i)
                        {
                            if (startArrowPoints[i].y > glSpaceOffset.y)
                                startArrowPoints[i] = new Vector2(startArrowPoints[i].x, glSpaceOffset.y);
                            GL.Vertex(startArrowPoints[i]);
                        }
                        GL.End();
                        GL.PopMatrix();
                    }
                }
            }
        }

        void GLDrawEndShape(List<Vector2> vertices, Color color, float pixelWidth, float pixelHeight, Vector2 glCutoff)
        {
            Vector2[][] lineCorners = new Vector2[vertices.Count - 1][];
            for (int i = 0; i < vertices.Count - 1; ++i)
            {
                Vector2 startPoint = PosFromGLSpace(vertices[i]);
                Vector2 endPoint = PosFromGLSpace(vertices[i+1]);
                lineCorners[i] = new Vector2[4];
                float angle = Vector2.SignedAngle(new Vector2(0, 1), new Vector2(endPoint.x - startPoint.x, endPoint.y - startPoint.y));

                Vector3 cornerVector = Quaternion.Euler(0, 0, -angle) * new Vector3(1, 0, 0);
                cornerVector.x *= pixelWidth;
                cornerVector.y *= pixelHeight;

                Vector3 lineVector = Quaternion.Euler(0, 0, -angle - 90) * new Vector3(1, 0, 0);
                lineVector.x *= pixelWidth/2f;
                lineVector.y *= pixelHeight/2f;

                lineCorners[i][0] = new Vector3(vertices[i].x, vertices[i].y) + cornerVector - lineVector;
                lineCorners[i][1] = new Vector3(vertices[i].x, vertices[i].y) - cornerVector - lineVector;
                lineCorners[i][2] = new Vector3(vertices[i + 1].x, vertices[i + 1].y) - cornerVector + lineVector;
                lineCorners[i][3] = new Vector3(vertices[i + 1].x, vertices[i + 1].y) + cornerVector + lineVector;
            }


            if (lineMat.SetPass(0))
            {
                GL.PushMatrix();
                GL.LoadOrtho();
                GL.Begin(GL.QUADS);
                GL.Color(color);
                for (int i = 0; i < lineCorners.Length; ++i)
                {
                    for (int k = 0; k < lineCorners[i].Length; ++k)
                    {
                        if (lineCorners[i][k].y > glCutoff.y)
                            lineCorners[i][k] = new Vector2(lineCorners[i][k].x, glCutoff.y);
                        GL.Vertex(lineCorners[i][k]);
                    }
                }
                GL.End();
                GL.PopMatrix();
            }
        }

        Vector3 PosToGLSpace(Vector2 pos)
        {
            Vector3 glPos = new Vector3(pos.x, scrollAreaDimensions.y - pos.y, 0);
            glPos.y += (scrollPos.y * zoomScale + (Screen.height * dpiScaleFactor) - 21f) - scrollAreaDimensions.y;
            glPos.x -= scrollPos.x * zoomScale;

            glPos.x /= (Screen.width * dpiScaleFactor);
            glPos.y /= (Screen.height * dpiScaleFactor);

            return glPos;
        }

        Vector2 PosFromGLSpace(Vector2 pos)
        {

            Vector2 normalPos = new Vector2(pos.x, pos.y);
            normalPos.x *= (Screen.width * dpiScaleFactor);
            normalPos.y *= (Screen.height * dpiScaleFactor);
            normalPos.x += scrollPos.x;
            normalPos.y = normalPos.y - (scrollPos.y + (Screen.height * dpiScaleFactor) - 21f) + scrollAreaDimensions.y;
            normalPos.y = scrollAreaDimensions.y - normalPos.y;
            return normalPos;
        }

        Vector2 DisplayStructure(float width, int depth, EasyDependencyDiagrams.DataElement data, string path, bool onlyFiles, string childType = "")
        {
            //excluding filtered paths

            if (AlwaysIncludeOrExcludePath(data.path) == 2)
            {
                return new Vector2(width - 1, depth - 1);
            }

            Vector2 finalDimensions = new Vector2(width, depth);
            int files = 0;

            if (childType.Length > 0)
            {
                Rect childRect = new Rect(10 + 40 * width, 10 + 100 * finalDimensions.y, 75, 75);
                if (!DrawChildElements(childRect, childType, data))
                {
                    return new Vector2(width-1, depth-1);
                }

            }
            else
            {
                data.path = path;

                if (data.type.Equals("Folder"))
                {
                    DrawFileFolder(new Rect(10 + 40 * width, 10 + 100 * depth, 75, 75), data, path);
                }
                if (data.type.Equals("File"))
                {
                    if (!onlyFiles)
                        DrawFileFolder(new Rect(10 + 40 * width, 10 + 100 * depth, 75, 75), data, path);
                    else
                        DrawFileFolder(new Rect(55 + 40 * width, 7.5f + 100 * depth, 60, 60), data, path);
                }
                else if (!onlyFiles && (data.type.Equals("Namespace") || data.type.Equals("Class")))
                {
                    DrawCodeElement(new Rect(10 + 40 * width, 10 + 100 * depth, 75, 75), data, path);
                }
            }


            for (int i = 0; i < data.children.Count; ++i)
            {
                if (data.children[i].type.Equals("Folder"))
                {
                    if (!((collapseDefault && !uncollapsed.Contains(path)) || (!collapseDefault && collapsed.Contains(path))) && childType.Length == 0)
                    {
                        Vector2 childDimensions = DisplayStructure(width + 1, (int)finalDimensions.y + 1, data.children[i], path + "/" + data.children[i].name, onlyFiles);
                        finalDimensions.x = Mathf.Max(childDimensions.x, finalDimensions.x);
                        finalDimensions.y = Mathf.Max(childDimensions.y, finalDimensions.y);
                    }
                }
                else if (data.children[i].type.Equals("File"))
                {
                    if (!((collapseDefault && !uncollapsed.Contains(path)) || (!collapseDefault && collapsed.Contains(path))) && childType.Length == 0)
                    {
                        Vector2 childDimensions = DisplayStructure(width + 2 + 1.7f * files, depth, data.children[i], path + "/" + data.children[i].name, onlyFiles);
                        
                        //not doing anything if the file is excluded
                        if (childDimensions.x > width + 1.5f + 1.7f * files)
                        {
                            finalDimensions.x = Mathf.Max(width + 2 + 1.7f * files, finalDimensions.x);
                            finalDimensions.y = Mathf.Max(childDimensions.y, finalDimensions.y);
                            files++;
                        }
                    }
                }
                else if (!onlyFiles)
                {
                    if (data.children[i].type.Equals("Namespace") || data.children[i].type.Equals("Class"))
                    {
                        if (!((collapseDefault && !uncollapsed.Contains(path)) || (!collapseDefault && collapsed.Contains(path))) && childType.Length == 0)
                        {
                            Vector2 childDimensions = DisplayStructure(width + 1, (int)finalDimensions.y + 1, data.children[i], path + "/" + data.children[i].name, onlyFiles);
                            finalDimensions.x = Mathf.Max(childDimensions.x, finalDimensions.x);
                            finalDimensions.y = Mathf.Max(childDimensions.y, finalDimensions.y);
                        }
                    }
                    else if (childType.Length > 0 && data.children[i].type.Equals(childType))
                    {
                        if (uncollapsed.Contains(path + "/" + childType))
                        {
                            finalDimensions.y++;
                            finalDimensions.x = Mathf.Max(width+1, finalDimensions.x);
                            DrawCodeElement(new Rect(10 + 40 * (width+1), 10 + 100 * (finalDimensions.y), 75, 75), data.children[i], path + "/" + data.children[i].name);
                        }
                    }
                }
            }

            if (!onlyFiles && !((collapseDefault && !uncollapsed.Contains(path)) || (!collapseDefault && collapsed.Contains(path))) && childType.Length == 0 && data.type.Equals("Class"))
            {
                Vector2 childDimensions = DisplayStructure(width + 1, (int)finalDimensions.y + 1, data, path, onlyFiles, "Constructor");
                finalDimensions.x = Mathf.Max(childDimensions.x, finalDimensions.x);
                finalDimensions.y = Mathf.Max(childDimensions.y, finalDimensions.y);
                childDimensions = DisplayStructure(width + 1, (int)finalDimensions.y + 1, data, path, onlyFiles, "Destructor");
                finalDimensions.x = Mathf.Max(childDimensions.x, finalDimensions.x);
                finalDimensions.y = Mathf.Max(childDimensions.y, finalDimensions.y);
                childDimensions = DisplayStructure(width + 1, (int)finalDimensions.y + 1, data, path, onlyFiles, "Operator");
                finalDimensions.x = Mathf.Max(childDimensions.x, finalDimensions.x);
                finalDimensions.y = Mathf.Max(childDimensions.y, finalDimensions.y);
                childDimensions = DisplayStructure(width + 1, (int)finalDimensions.y + 1, data, path, onlyFiles, "Enum");
                finalDimensions.x = Mathf.Max(childDimensions.x, finalDimensions.x);
                finalDimensions.y = Mathf.Max(childDimensions.y, finalDimensions.y);
                childDimensions = DisplayStructure(width + 1, (int)finalDimensions.y + 1, data, path, onlyFiles, "Method");
                finalDimensions.x = Mathf.Max(childDimensions.x, finalDimensions.x);
                finalDimensions.y = Mathf.Max(childDimensions.y, finalDimensions.y);
                childDimensions = DisplayStructure(width + 1, (int)finalDimensions.y + 1, data, path, onlyFiles, "Property");
                finalDimensions.x = Mathf.Max(childDimensions.x, finalDimensions.x);
                finalDimensions.y = Mathf.Max(childDimensions.y, finalDimensions.y);
                childDimensions = DisplayStructure(width + 1, (int)finalDimensions.y + 1, data, path, onlyFiles, "Variable");
                finalDimensions.x = Mathf.Max(childDimensions.x, finalDimensions.x);
                finalDimensions.y = Mathf.Max(childDimensions.y, finalDimensions.y);
            }


            return finalDimensions;
        }
        
        void DrawFileFolder(Rect pos, EasyDependencyDiagrams.DataElement data, string path)
        {
            if (mode == DiagramDisplayMode.tree && (pos.y < scrollPos.y - 200 || pos.y > (Screen.height * dpiScaleFactor) + pos.y + 200))
                return;

            //click event
            if (Event.current != null && Event.current.isMouse && Event.current.clickCount == 1 && Event.current.button == 0 && Event.current.type == EventType.MouseUp && !dragOn && !movingInfoBox)
            {
                //inside the rect
                if (Event.current.mousePosition.x > pos.xMin && Event.current.mousePosition.x < pos.xMax && Event.current.mousePosition.y > pos.yMin && Event.current.mousePosition.y < pos.yMax)
                {
                    if (targetRoot != path)
                    {
                        targetRoot = path;
                        PlayerPrefs.SetString("DiagramTargetRoot", targetRoot);
                        targets.Add(targetRoot);
                        PlayerPrefs.SetInt("DiagramTargetCount", targets.Count);
                        PlayerPrefs.SetString("DiagramTarget" + (targets.Count - 1), targets[^1]);
                        callRepaint = true;
                    }
                }
            }

            GUI.Box(pos, new GUIContent("", data.type + ":\n" + data.name), diagramBoxStyle);
            EditorGUIUtility.AddCursorRect(pos, MouseCursor.Link);

            Rect imgRect = new Rect(pos.xMin + 10, pos.yMin + 10, pos.width - 20, pos.height - 20);
            Rect titleRect = new Rect(pos.xMin, pos.yMax, pos.width, 20);

            if (data.type.Equals("Folder") && data.children.Count > 0)
            {
                Rect collapseButtonRect = new Rect(pos.xMax + 10, pos.yMin, 30, 30);
                if ((collapseDefault && !uncollapsed.Contains(path)) || (!collapseDefault && collapsed.Contains(path)))
                {
                    GUI.DrawTexture(imgRect, folderIcon);

                    if (GUI.Button(collapseButtonRect, new GUIContent(expandIcon, "Expand")) && !movingInfoBox)
                    {
                        ChangeCollapse(false, path);
                    }
                }
                else
                {
                    GUI.DrawTexture(imgRect, openFolderIcon);

                    if (GUI.Button(collapseButtonRect, new GUIContent(collapseIcon, "Collapse")) && !movingInfoBox)
                    {
                        ChangeCollapse(true, path);
                    }

                }

                DrawFitLabel(data.name, titleRect, labelStyle);

                EditorGUIUtility.AddCursorRect(collapseButtonRect, MouseCursor.Link);
            }
            else
            {
                GUI.DrawTexture(imgRect, fileIcon);
                DrawFitLabel(data.name, titleRect, labelStyle);
            }

            if (data.children.Count > 0)
            {
                Rect childCountRect = new Rect(pos.xMax - 20, pos.yMax - 20, 20, 20);
                GUI.Label(childCountRect, "" + Mathf.Min(data.children.Count, 99), labelStyle);
            }


        }

        void DrawCodeElement(Rect pos, EasyDependencyDiagrams.DataElement data, string path)
        {
            DrawCodeElement(pos, data.name, FullName(path), data.type, data.fullLine, path, data.children.Count);
        }

        void DrawCodeElement(Rect pos, string name, string fullName, string type, string fullLine, string path, int childCount, bool textBelow = true)
        {
            if (mode == DiagramDisplayMode.tree && (pos.y < scrollPos.y - 200 || pos.y > (Screen.height * dpiScaleFactor) + scrollPos.y + 200))
                return;

            if (mode == DiagramDisplayMode.dependencies && (pos.y < -200 / zoomScale || pos.y > ((Screen.height * dpiScaleFactor) + 200) / zoomScale || pos.x < -200 / zoomScale || pos.x > ((Screen.width * dpiScaleFactor) + 200) / zoomScale))
                return;

            //click event
            if (Event.current != null && Event.current.isMouse && Event.current.clickCount == 1 && Event.current.button == 0 && Event.current.type == EventType.MouseUp && !dragOn && !movingInfoBox)
            {
                //inside the rect
                if (Event.current.mousePosition.x > pos.xMin && Event.current.mousePosition.x < pos.xMax && Event.current.mousePosition.y > pos.yMin && Event.current.mousePosition.y < pos.yMax)
                {
                    if (type.Equals("Enum") || type.Equals("Namespace") || type.Equals("Class") || (mode == DiagramDisplayMode.folderDependencies && type.Equals("Folder")))
                    {
                        if (targetRoot != path)
                        {
                            targetRoot = path;
                            PlayerPrefs.SetString("DiagramTargetRoot", targetRoot);
                            targets.Add(targetRoot);
                            PlayerPrefs.SetInt("DiagramTargetCount", targets.Count);
                            PlayerPrefs.SetString("DiagramTarget" + (targets.Count - 1), targets[^1]);

                            if (mode == DiagramDisplayMode.dependencies || mode == DiagramDisplayMode.folderDependencies)
                            {
                                FetchDiagramScope(true);
                            }

                            callRepaint = true;
                        }
                    }
                }
            }

            if (mode == DiagramDisplayMode.tree)
                GUI.Box(pos, new GUIContent("", type + ":\n" + fullName), diagramBoxStyle);

            if (mode == DiagramDisplayMode.tree && (type.Equals("Enum") || type.Equals("Class") || type.Equals("Namespace")))
            {
                EditorGUIUtility.AddCursorRect(pos, MouseCursor.Link);
            }

            Rect imgRect;
            if (mode == DiagramDisplayMode.tree)
                imgRect = new Rect(pos.xMin + 10, pos.yMin + 10, pos.width - 20, pos.height - 20);
            else
                imgRect = new Rect(pos.xMin, pos.yMin, pos.width, pos.height);

            Vector2 titleWidth = new Vector2(pos.xMin - 10, pos.width + 40);
            if (mode != DiagramDisplayMode.tree)
                titleWidth = new Vector2(pos.xMin - 30, pos.width + 60);
            Rect titleRect;
            if (textBelow)
                titleRect = new Rect(titleWidth.x, pos.yMax, titleWidth.y, 20);
            else
                titleRect = new Rect(titleWidth.x, pos.yMin-20, titleWidth.y, 20);
            if (type.Equals("Namespace"))
                GUI.DrawTexture(imgRect, namespaceIcon);
            else if (type.Equals("Class"))
                GUI.DrawTexture(imgRect, classIcon);
            else if (type.Equals("Enum"))
                GUI.DrawTexture(imgRect, enumIcon);
            else if (type.Equals("Method") || type.Equals("Constructor") || type.Equals("Destructor") || type.Equals("Operator"))
                GUI.DrawTexture(imgRect, methodIcon);
            else if (type.Equals("Variable") || type.Equals("Property"))
                GUI.DrawTexture(imgRect, varIcon);
            else if (type.Equals("Folder") && (displayedData != null && displayedData.path.Equals(path)))
                GUI.DrawTexture(imgRect, openFolderIcon);
            else if (type.Equals("Folder"))
                GUI.DrawTexture(imgRect, folderIcon);

            string tooltip = fullName;
            if (tooltip.Equals(projectRoot))
                tooltip = name;
            else if (tooltip.StartsWith(projectRoot))
                tooltip = tooltip[(projectRoot.Length + 1)..];
            DrawFitLabel(name, titleRect, labelStyle, tooltip);

            if (childCount > 0 && mode == DiagramDisplayMode.tree)
            {

                Rect childCountRect = new Rect(pos.xMax - 20, pos.yMax - 20, 20, 20);
                GUI.Label(childCountRect, "" + Mathf.Min(childCount, 99), labelStyle);
            }

            if (childCount > 0 && (mode == DiagramDisplayMode.tree && (type.Equals("Namespace") || type.Equals("Class"))))
            {
                Rect collapseButtonRect = new Rect(pos.xMax + 5, pos.yMin + 35, 30, 30);
                if ((collapseDefault && !uncollapsed.Contains(path)) || (!collapseDefault && collapsed.Contains(path)))
                {

                    if (GUI.Button(collapseButtonRect, new GUIContent(expandIcon, "Expand")) && !movingInfoBox)
                    {
                        ChangeCollapse(false, path);
                    }
                }
                else
                {
                    if (GUI.Button(collapseButtonRect, new GUIContent(collapseIcon, "Collapse")) && !movingInfoBox)
                    {
                        ChangeCollapse(true, path);
                    }
                }

                EditorGUIUtility.AddCursorRect(collapseButtonRect, MouseCursor.Link);
            }

            if (mode == DiagramDisplayMode.tree)
            {
                Rect VSButtonRect = new Rect(pos.xMax + 5, pos.yMin, 30, 30);
                if (GUI.Button(VSButtonRect, new GUIContent(editorIcon, "Open in code editor")) && !movingInfoBox)
                {
                    if (path.StartsWith(projectRoot))
                    {
                        int startInd = path.IndexOf("/Assets") + 1;
                        if (startInd > 0)
                        {
                            string filePath = path[startInd..(path.IndexOf(".cs") + 3)];
                            if (File.Exists(filePath))
                            {
                                int line = ecd.FindLineNumber(filePath, fullLine);
                                AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath(filePath, typeof(Object)), line);
                            }
                        }
                    }
                }
                EditorGUIUtility.AddCursorRect(VSButtonRect, MouseCursor.Link);
            }
        }

        bool DrawChildElements(Rect pos, string type, EasyDependencyDiagrams.DataElement data)
        {
            if (data.children == null)
                return false;

            string elements = "";
            int elementCount = 0;
            for (int i = 0; i < data.children.Count; ++i)
            {
                if (data.children[i].type.Equals(type))
                {
                    elements += data.children[i].name + "\n";
                    elementCount++;
                }
            }

            elements = elements.Trim();

            if (elementCount == 0)
                return false;

            if (mode == DiagramDisplayMode.tree && (pos.y < scrollPos.y - 200 || pos.y > (Screen.height * dpiScaleFactor) + pos.y + 200))
                return true;

            GUI.Box(pos, new GUIContent("", type + "s:\n" + elements), diagramBoxStyle);

            Rect imgRect = new Rect(pos.xMin + 10, pos.yMin + 10, pos.width - 20, pos.height - 20);
            Rect titleRect = new Rect(pos.xMin - 10, pos.yMax, pos.width + 40, 20);
            if (type.Equals("Method") || type.Equals("Constructor") || type.Equals("Destructor") || type.Equals("Operator"))
                GUI.DrawTexture(imgRect, methodIcon);
            else if (type.Equals("Enum"))
                GUI.DrawTexture(imgRect, enumIcon);
            else
                GUI.DrawTexture(imgRect, varIcon);

            string typestring = type + "s";
            if (type.Equals("Property"))
                typestring = "Properties";
            DrawFitLabel(typestring, titleRect, labelStyle);

            Rect childCountRect = new Rect(pos.xMax - 20, pos.yMax - 20, 20, 20);
            GUI.Label(childCountRect, "" + Mathf.Min(elementCount, 99), labelStyle);
            

            Rect collapseButtonRect = new Rect(pos.xMax + 5, pos.yMin + 35, 30, 30);
            if (!uncollapsed.Contains(data.path + "/" + type))
            {

                if (GUI.Button(collapseButtonRect, new GUIContent(expandIcon, "Expand")) && !movingInfoBox)
                {
                    ChangeCollapse(false, data.path + "/" + type);
                }
            }
            else
            {
                if (GUI.Button(collapseButtonRect, new GUIContent(collapseIcon, "Collapse")) && !movingInfoBox)
                {
                    ChangeCollapse(true, data.path + "/" + type);
                }
            }
            EditorGUIUtility.AddCursorRect(collapseButtonRect, MouseCursor.Link);


            return true;
        }

        void ChangeCollapse(bool collapse, string path)
        {
            if (collapse)
            {
                //removing from uncollapsed collapsed
                if (uncollapsed.Contains(path))
                {
                    uncollapsed.Remove(path);
                    PlayerPrefs.SetInt("DiagramUncollapsedCount", uncollapsed.Count);
                    for (int c = 0; c < uncollapsed.Count; ++c)
                    {
                        PlayerPrefs.SetString("DiagramUncollapsed" + c, uncollapsed[c]);
                    }
                }

                //adding to collapsed
                if (!collapsed.Contains(path))
                {
                    collapsed.Add(path);
                    PlayerPrefs.SetInt("DiagramCollapsedCount", collapsed.Count);
                    for (int c = 0; c < collapsed.Count; ++c)
                    {
                        PlayerPrefs.SetString("DiagramCollapsed" + c, collapsed[c]);
                    }
                }
            }
            else
            {
                //removing from collapsed collapsed
                if (collapsed.Contains(path))
                {
                    collapsed.Remove(path);
                    PlayerPrefs.SetInt("DiagramCollapsedCount", collapsed.Count);
                    for (int c = 0; c < collapsed.Count; ++c)
                    {
                        PlayerPrefs.SetString("DiagramCollapsed" + c, collapsed[c]);
                    }
                }

                //adding to uncollapsed
                if (!uncollapsed.Contains(path))
                {
                    uncollapsed.Add(path);
                    PlayerPrefs.SetInt("DiagramUncollapsedCount", uncollapsed.Count);
                    for (int c = 0; c < uncollapsed.Count; ++c)
                    {
                        PlayerPrefs.SetString("DiagramUncollapsed" + c, uncollapsed[c]);
                    }
                }
            }
        }

        void DrawFitLabel(string text, Rect rect, GUIStyle style, string tooltip = "")
        {
            if (tooltip.Length == 0)
                tooltip = text;
            string trueText = text;
            string labelText = trueText;
            Vector2 size = labelStyle.CalcSize(new GUIContent(labelText));
            if (size.x > rect.xMax - rect.xMin)
            {
                do
                {
                    if (trueText.Length == 0)
                    {
                        labelText = text;
                        break;
                    }
                    trueText = trueText[0..^1];
                    labelText = trueText + "...";
                    size = labelStyle.CalcSize(new GUIContent(labelText));
                } while (size.x > rect.xMax - rect.xMin);
            }
            GUI.Label(rect, new GUIContent(labelText, tooltip), style);
        }

        static Color GetArrowColor(DiagramArrow arrow)
        {
            switch (arrow.type)
            {
                case 0:
                case 5:
                    return colorList[7];
                case 1:
                    return colorList[8];
                case 2:
                    return colorList[9];
                case 3:
                    return colorList[10];
                case 4:
                    return colorList[11];

                default:
                    if (arrow.startArrow && arrow.endArrow)
                        return colorList[6];
                    else if (arrow.startArrow)
                        return colorList[4];
                    else
                        return colorList[5];
            }
        }

        static KeyValuePair<string, int> SvgTree(float width, int depth, EasyDependencyDiagrams.DataElement data, string path, bool onlyFiles, string childType = "")
        {
            string textStyle = "style=\"dominant-baseline:middle;text-anchor:middle;fill:white;font-style:normal;font-variant:normal;font-weight:normal;font-stretch:normal;font-family:Arial;font-size:12px\"";
            Vector2Int titleLine = new Vector2Int((int)(10 + 40 * width + 37.5f), (int)(10 + 100 * depth + 75 + 10));
            if (onlyFiles && data.type.Equals("File"))
                titleLine.x += 38;
            string tree = "";

            if (childType.Length > 0)
            {
                string iconName;
                if (childType.Equals("Method") || childType.Equals("Constructor") || childType.Equals("Destructor") || childType.Equals("Operator"))
                    iconName = "method";
                else if (childType.Equals("Enum"))
                    iconName = "enum";
                else
                    iconName = "variable";
                bool childrenExist = false;
                for (int i = 0; i < data.children.Count; ++i)
                {
                    if (data.children[i].type.Equals(childType))
                    {
                        if (!childrenExist)
                        {

                            string typestring = childType + "s";
                            if (childType.Equals("Property"))
                                typestring = "Properties";
                            tree += "<rect x=\"" + (int)(10 + 40 * width) + "\" y=\"" + (10 + 100 * depth) + "\" width=\"" + 75 + "\" height=\"" + 75 + "\" fill=\"#" + ColorUtility.ToHtmlStringRGB(colorList[2]) + "\" stroke-width=\"2\" stroke=\"#" + ColorUtility.ToHtmlStringRGB(colorList[3]) + "\"/>\n";
                            tree += "<text x=\"" + titleLine.x + "\" y=\"" + titleLine.y + "\" " + textStyle + ">" + typestring.Replace("<", "&lt;").Replace("<", "&gt;") + "</text>\n";
                            tree += IconAsSVG(iconName, new Vector2Int((int)(20 + 40 * width), (20 + 100 * depth)), new Vector2Int(55, 55));
                            width++;
                            depth++;
                        }
                        childrenExist = true;
                        if (!collapsed.Contains(path + "/" + childType))
                        {
                            titleLine = new Vector2Int((int)(10 + 40 * width + 37.5f), (int)(10 + 100 * depth + 75 + 10));

                            tree += "<rect x=\"" + (int)(10 + 40 * width) + "\" y=\"" + (10 + 100 * depth) + "\" width=\"" + 75 + "\" height=\"" + 75 + "\" fill=\"#" + ColorUtility.ToHtmlStringRGB(colorList[2]) + "\" stroke-width=\"2\" stroke=\"#" + ColorUtility.ToHtmlStringRGB(colorList[3]) + "\"/>\n";
                            tree += "<text x=\"" + titleLine.x + "\" y=\"" + titleLine.y + "\" " + textStyle + ">" + data.children[i].name.Replace("<", "&lt;").Replace("<", "&gt;") + "</text>\n";
                            tree += IconAsSVG(iconName, new Vector2Int((int)(20 + 40 * width), (20 + 100 * depth)), new Vector2Int(55, 55));
                            depth++;
                        }
                    }
                }
                return new KeyValuePair<string, int>(tree, depth - 1);
            }
            else
            {
                if (data.type.Equals("Folder"))
                {
                    tree += "<rect x=\"" + (int)(10 + 40 * width) + "\" y=\"" + (10 + 100 * depth) + "\" width=\"" + 75 + "\" height=\"" + 75 + "\" fill=\"#" + ColorUtility.ToHtmlStringRGB(colorList[2]) + "\" stroke-width=\"2\" stroke=\"#" + ColorUtility.ToHtmlStringRGB(colorList[3]) + "\"/>\n";
                    tree += "<text x=\"" + titleLine.x + "\" y=\"" + titleLine.y + "\" " + textStyle + ">" + data.name.Replace("<", "&lt;").Replace("<", "&gt;") + "</text>\n";
                    string iconName = "folderOpen";
                    if (collapsed.Contains(path))
                        iconName = "folder";
                    tree += IconAsSVG(iconName, new Vector2Int((int)(20 + 40 * width), (20 + 100 * depth)), new Vector2Int(55, 55));
                }
                if (data.type.Equals("File"))
                {
                    if (!onlyFiles)
                    {
                        tree += "<rect x=\"" + (int)(10 + 40 * width) + "\" y=\"" + (10 + 100 * depth) + "\" width=\"" + 75 + "\" height=\"" + 75 + "\" fill=\"#" + ColorUtility.ToHtmlStringRGB(colorList[2]) + "\" stroke-width=\"2\" stroke=\"#" + ColorUtility.ToHtmlStringRGB(colorList[3]) + "\"/>\n";
                        tree += "<text x=\"" + titleLine.x + "\" y=\"" + titleLine.y + "\" " + textStyle + ">" + data.name.Replace("<", "&lt;").Replace("<", "&gt;") + "</text>\n";
                        tree += IconAsSVG("cs", new Vector2Int((int)(20 + 40 * width), (20 + 100 * depth)), new Vector2Int(55, 55));
                    }
                    else
                    {
                        tree += "<rect x=\"" + (int)(55 + 40 * width) + "\" y=\"" + (int)(7.5f + 100 * depth) + "\" width=\"" + 60 + "\" height=\"" + 60 + "\" fill=\"#" + ColorUtility.ToHtmlStringRGB(colorList[2]) + "\" stroke-width=\"2\" stroke=\"#" + ColorUtility.ToHtmlStringRGB(colorList[3]) + "\"/>\n";
                        tree += "<text x=\"" + titleLine.x + "\" y=\"" + titleLine.y + "\" " + textStyle + ">" + data.name.Replace("<", "&lt;").Replace("<", "&gt;") + "</text>\n";
                        tree += IconAsSVG("cs", new Vector2Int((int)(60 + 40 * width), (int)(12.5f + 100 * depth)), new Vector2Int(50, 50));
                    }
                }
                else if (!onlyFiles && (data.type.Equals("Namespace") || data.type.Equals("Class")))
                {
                    tree += "<rect x=\"" + (int)(10 + 40 * width) + "\" y=\"" + (10 + 100 * depth) + "\" width=\"" + 75 + "\" height=\"" + 75 + "\" fill=\"#" + ColorUtility.ToHtmlStringRGB(colorList[2]) + "\" stroke-width=\"2\" stroke=\"#" + ColorUtility.ToHtmlStringRGB(colorList[3]) + "\"/>\n";
                    tree += "<text x=\"" + titleLine.x + "\" y=\"" + titleLine.y + "\" " + textStyle + ">" + data.name.Replace("<", "&lt;").Replace("<", "&gt;") + "</text>\n";
                    string iconName = "namespace";
                    if (data.type.Equals("Class"))
                        iconName = "class";
                    tree += IconAsSVG(iconName, new Vector2Int((int)(20 + 40 * width), (20 + 100 * depth)), new Vector2Int(55, 55));
                }
            }

            int finalDepth = depth;
            int files = 0;

            for (int i = 0; i < data.children.Count; ++i)
            {
                if (data.children[i].type.Equals("Folder"))
                {
                    if (!collapsed.Contains(path) && childType.Length == 0)
                    {
                        KeyValuePair<string, int> child = SvgTree(width + 1, finalDepth + 1, data.children[i], path + "/" + data.children[i].name, onlyFiles);
                        finalDepth = child.Value;
                        tree += child.Key;
                    }
                }
                else if (data.children[i].type.Equals("File"))
                {
                    if (!collapsed.Contains(path) && childType.Length == 0)
                    {
                        KeyValuePair<string, int> child = SvgTree(width + 2 + 1.7f * files, depth, data.children[i], path + "/" + data.children[i].name, onlyFiles);
                        tree += child.Key;
                    }
                    files++;
                }
                else if (!onlyFiles)
                {
                    if (data.children[i].type.Equals("Namespace") || data.children[i].type.Equals("Class"))
                    {
                        if (!collapsed.Contains(path) && childType.Length == 0)
                        {
                            KeyValuePair<string, int> child = SvgTree(width + 1, finalDepth + 1, data.children[i], path + "/" + data.children[i].name, onlyFiles);
                            finalDepth = child.Value;
                            tree += child.Key;
                        }
                    }
                }
            }

            if (!onlyFiles && !collapsed.Contains(path) && childType.Length == 0 && data.type.Equals("Class"))
            {
                KeyValuePair<string, int> child = SvgTree(width + 1, finalDepth + 1, data, path, onlyFiles, "Constructor");
                finalDepth = child.Value;
                tree += child.Key;
                child = SvgTree(width + 1, finalDepth + 1, data, path, onlyFiles, "Destructor");
                finalDepth = child.Value;
                tree += child.Key;
                child = SvgTree(width + 1, finalDepth + 1, data, path, onlyFiles, "Operator");
                finalDepth = child.Value;
                tree += child.Key;
                child = SvgTree(width + 1, finalDepth + 1, data, path, onlyFiles, "Enum");
                finalDepth = child.Value;
                tree += child.Key;
                child = SvgTree(width + 1, finalDepth + 1, data, path, onlyFiles, "Method");
                finalDepth = child.Value;
                tree += child.Key;
                child = SvgTree(width + 1, finalDepth + 1, data, path, onlyFiles, "Property");
                finalDepth = child.Value;
                tree += child.Key;
                child = SvgTree(width + 1, finalDepth + 1, data, path, onlyFiles, "Variable");
                finalDepth = child.Value;
                tree += child.Key;
            }

            return new KeyValuePair<string, int>(tree, finalDepth);
        }

        static List<string> RemoveDuplicates(params List<string>[] lists)
        {
            List<string> uniques = new List<string>();
            for (int i = 0; i < lists.Length; ++i)
            {
                for (int k = 0; k < lists[i].Count; ++k)
                {
                    if (!uniques.Contains(lists[i][k]))
                        uniques.Add(lists[i][k]);
                }
            }

            return uniques;
        }

        static Texture2D CreateBoxWithBorders(Color32 boxColor, Color32 borderColor, int size, int borderSize, bool dashed, int dashSize)
        {
            Texture2D tex = new Texture2D(size, size);
            Color32[] blueBorderBg = new Color32[size * size];
            for (int r = 0; r < size; ++r)
            {
                for (int c = 0; c < size; ++c)
                {
                    if (((r < borderSize || r >= size - borderSize) && (!dashed || c % (dashSize * 2) < dashSize)) || ((c < borderSize || c >= size - borderSize) && (!dashed || r % (dashSize * 2) < dashSize)))
                        blueBorderBg[r * size + c] = borderColor;
                    else
                        blueBorderBg[r * size + c] = boxColor;
                }
            }

            tex.SetPixels32(blueBorderBg);
            tex.Apply();
            return tex;
        }

        static bool HasTypeAsChild(DiagramStructureInfo obj, string type)
        {
            if (obj.objectType.Equals(type))
                return true;
            for (int i = 0; i < obj.children.Count; ++i)
            {
                if (HasTypeAsChild(obj.children[i], type))
                    return true;
            }
            return false;
        }

        static int NextDependencyDiagramAngle(int current, bool ns)
        {
            float oldRadians = current * Mathf.PI / 180f;
            int nextAngle = current;
            if (ns)
                nextAngle -= 48;
            else
                nextAngle -= 24;
            if (oldRadians > -2 * Mathf.PI && nextAngle * Mathf.PI / 180f <= -2 * Mathf.PI)
                nextAngle -= 12;
            else if (oldRadians > -4 * Mathf.PI && nextAngle * Mathf.PI / 180f <= -4 * Mathf.PI)
                nextAngle -= 6;
            else if (oldRadians > -6 * Mathf.PI && nextAngle * Mathf.PI / 180f <= -6 * Mathf.PI)
                nextAngle -= 12;
            else if (oldRadians > -8 * Mathf.PI && nextAngle * Mathf.PI / 180f <= -8 * Mathf.PI)
                nextAngle -= 3;
            else if (oldRadians > -10 * Mathf.PI && nextAngle * Mathf.PI / 180f <= -10 * Mathf.PI)
                nextAngle -= 6;
            else if (oldRadians > -12 * Mathf.PI && nextAngle * Mathf.PI / 180f <= -12 * Mathf.PI)
                nextAngle -= 6;
            else if (oldRadians > -14 * Mathf.PI && nextAngle * Mathf.PI / 180f <= -14 * Mathf.PI)
                nextAngle -= 6;
            else if (oldRadians < -14 * Mathf.PI)
                nextAngle -= 3;

            return nextAngle;
        }

        static bool IsDependant(List<string> usings, List<string> dependencies, string path, string targetPath)
        {
            string targetFullName = FullName(targetPath);
            string fullName = FullName(path);

            //direct reference to the target
            if (dependencies.Contains(targetFullName))
                return true;

            //direct using-directive to the target
            if (usings.Contains(targetFullName))
                return true;

            //Basic cases of "using A" -> "A.B"
            for (int k = 0; k < usings.Count; ++k)
            {
                if (targetFullName.Length > usings[k].Length + 1 && targetFullName.StartsWith(usings[k] + ".") && dependencies.Contains(targetFullName[(usings[k].Length + 1)..]))
                {
                    return true;
                }
            }

            //special case where referenching to something that is directly under the parent of this structure (A.B referencing to A.C directly by C)
            int separatorIndex = fullName.IndexOf('.');
            while (separatorIndex > 0)
            {
                string prefix = fullName[..(separatorIndex + 1)];
                if (targetFullName.Length > prefix.Length && targetFullName.StartsWith(prefix) && (usings.Contains(targetFullName[prefix.Length..]) || dependencies.Contains(targetFullName[prefix.Length..])))
                {
                    return true;
                }
                separatorIndex = fullName.IndexOf('.', separatorIndex + 1);
            }

            return false;
        }

        static bool IsDependantOnFile(List<string> usings, List<string> dependencies, string path, EasyDependencyDiagrams.DataElement target)
        {
            if (path.StartsWith(target.path))
            {
                return true; 
            }

            List<EasyDependencyDiagrams.DataElement> fileChildElements = ChildClassesOfDataElement(target, target.path);

            // classes in the file that are referenced in dependenices
            for (int k = 0; k < fileChildElements.Count; ++k)
            {

                if (IsDependant(usings, dependencies, path, fileChildElements[k].path))
                {
                    return true;
                }
            }

            return false;
        }

        static bool IsDependantOnFile(List<string> usings, List<string> dependencies, string path, DiagramStructureInfo target)
        {
            if (path.StartsWith(target.path))
            {
                return true;
            }

            List<DiagramStructureInfo> fileChildElements = ChildClassesOfStructureInfo(target);

            // classes in the file that are referenced in dependenices
            for (int k = 0; k < fileChildElements.Count; ++k)
            {

                if (IsDependant(usings, dependencies, path, fileChildElements[k].path))
                {
                    return true;
                }
            }

            return false;
        }

        static bool IsDependantOnFolder(DiagramStructureInfo main, DiagramStructureInfo target)
        {
            if (!target.type.Equals("Folder"))
                return false;

            if (main.path.StartsWith(target.path))
                return true;

            //special case for when the target folder is a subfolder of the main one
            if (target.path.StartsWith(main.path))
            {
                //if the main folder has files that are dependant on the subfolder, it counts
                for (int i = 0; i < main.children.Count; ++i)
                {
                    if (main.children[i].path.Length == 0)
                    {
                        main.children[i].path = main.path + "/" + main.children[i].name;
                    }

                    if (main.children[i].type.Equals("File") && IsDependantOnFolder(main.children[i], target))
                    {
                        return true;
                    }
                }
            }
            //normal logic
            else
            {
                for (int i = 0; i < target.children.Count; ++i)
                {
                    if (target.children[i].path.Length == 0)
                    {
                        target.children[i].path = target.path + "/" + target.children[i].name;
                    }


                    if (target.children[i].type.Equals("File") && IsDependantOnFile(main.usings, main.dependencies, main.path, target.children[i]))
                    {
                        return true;
                    }
                    else if (target.children[i].type.Equals("Folder") && IsDependantOnFolder(main, target.children[i]))
                    {
                        return true;
                    }

                }
            }

            return false;
        }

        static bool IsDependantOnFolder(EasyDependencyDiagrams.DataElement main, EasyDependencyDiagrams.DataElement target)
        {
            if (!target.type.Equals("Folder"))
                return false;

            if (main.path.StartsWith(target.path))
                return true;

            //special case for when the target folder is a subfolder of the main one
            if (target.path.StartsWith(main.path))
            {
                //if the main folder has files that are dependant on the subfolder, it counts
                for (int i = 0; i < main.children.Count; ++i)
                {
                    if (main.children[i].path.Length == 0)
                    {
                        main.children[i].path = main.path + "/" + main.children[i].name;
                    }

                    if (main.children[i].type.Equals("File") && IsDependantOnFolder(main.children[i], target))
                    {
                        return true;
                    }
                }
            }
            //normal logic
            else
            {
                for (int i = 0; i < target.children.Count; ++i)
                {
                    if (target.children[i].path.Length == 0)
                    {
                        target.children[i].path = target.path + "/" + target.children[i].name;
                    }


                    if (target.children[i].type.Equals("File") && IsDependantOnFile(main.usings, main.dependencies, main.path, target.children[i]))
                    {
                        return true;
                    }
                    else if (target.children[i].type.Equals("Folder") && IsDependantOnFolder(main, target.children[i]))
                    {
                        return true;
                    }

                }
            }

            return false;
        }

        static List<DiagramStructureInfo> ChildClassesOfStructureInfo(DiagramStructureInfo info)
        {
            List<DiagramStructureInfo> childClasses = new List<DiagramStructureInfo>();
            for (int i = 0; i < info.children.Count; ++i)
            {
                if (info.children[i].type.Equals("Class"))
                    childClasses.Add(info.children[i]);
                if (info.children[i].type.Equals("Namespace") || info.children[i].type.Equals("Class"))
                    childClasses.AddRange(ChildClassesOfStructureInfo(info.children[i]));
            }

            return childClasses;
        }

        static List<EasyDependencyDiagrams.DataElement> ChildClassesOfDataElement(EasyDependencyDiagrams.DataElement info, string pathStart)
        {
            List<EasyDependencyDiagrams.DataElement> childClasses = new List<EasyDependencyDiagrams.DataElement>();
            for (int i = 0; i < info.children.Count; ++i)
            {
                if (info.children[i].type.Equals("Class"))
                {
                    info.children[i].path = pathStart + "/" + info.children[i].name;
                    childClasses.Add(info.children[i]);
                }
                if (info.children[i].type.Equals("Namespace") || info.children[i].type.Equals("Class"))
                    childClasses.AddRange(ChildClassesOfDataElement(info.children[i], pathStart + "/" + info.children[i].name));
            }

            return childClasses;
        }

        static bool IsSuperclass(DiagramStructureInfo c, DiagramStructureInfo super)
        {
            string[] splitPostFix = c.postfix.Split(',');
            for (int i = 0; i < splitPostFix.Length; ++i)
            {
                if (splitPostFix[i].Trim().Equals(super.name))
                    return true;
            }

            return false;
        }

        static float ArrowHeuristicEstimate(Vector2 current, Vector2 end, float stepSize, float strictness)
        {
            int minTurns = 0;
            if (current.x != end.x)
                minTurns++;
            if (current.y != end.y)
                minTurns++;

            float turnPenalty = 0.5f;

            return (strictness + 0.1f) * (ManhattanDistance(current, end) + minTurns * stepSize * turnPenalty);
        }

        static float ManhattanDistance(Vector2 start, Vector2 end)
        {
            return Mathf.Abs(end.x - start.x) + Mathf.Abs(end.y - start.y);
        }

        static bool RectFullyContainsOther(Rect a, Rect b)
        {
            if (a.xMin < b.xMin && a.xMax > b.xMax && a.yMin < b.yMin && a.yMax > b.yMax)
                return true;

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path">Path of the target</param>
        /// <returns>0 if neither, 1 if included, 2 if excluded</returns>
        static int AlwaysIncludeOrExcludePath(string path)
        {
            //if the path is for the main target, it is always included. no matter what
            if (displayedData != null && displayedData.path.Equals(path))
                return 1; //include

            int exclusionIndex = -1;
            int inclusionIndex = -1;
            //checking if this element contains some excluded path
            for (int i = 0; i < excludedPaths.Count; ++i)
            {
                if (path.Contains(excludedPaths[i]))
                {
                    if (path.IndexOf(excludedPaths[i]) + excludedPaths[i].Length > exclusionIndex)
                        exclusionIndex = path.IndexOf(excludedPaths[i]) + excludedPaths[i].Length;

                    //Special case if something is included and excluded
                    for (int k = 0; k < includedPaths.Count; ++k)
                    {

                        if (path.Contains(includedPaths[k]))
                        {
                            //including something that is also excluded, but the inclusion is more specific
                            if (exclusionIndex < path.IndexOf(includedPaths[k]) + includedPaths[k].Length)
                            {
                                if (inclusionIndex < path.IndexOf(includedPaths[k]) + includedPaths[k].Length)
                                inclusionIndex = path.IndexOf(includedPaths[k]) + includedPaths[k].Length;
                            }
                        }
                    }
                }
            }

            if (inclusionIndex > exclusionIndex)
                return 1;
            if (exclusionIndex >= inclusionIndex && exclusionIndex >= 0)
                return 2;

            return 0; //neither
        }

        static string FullName(string path)
        {
            if (path.EndsWith(".cs"))
                return path;
            string fullName = "";
            string[] splitPath = path.Split('/');
            if (path.Contains(".cs"))
            {
                for (int i = splitPath.Length - 1; i >= 0; --i)
                {
                    if (splitPath[i].Contains(".cs"))
                        break;
                    fullName = splitPath[i] + "." + fullName;
                }
                if (fullName.Length > 0)
                    fullName = fullName[0..^1];
            }
            else
            {
                fullName = path;
            }

            return fullName;
        }

        static char PrefixCharacter(DiagramStructureInfo info)
        {
            char prefix = '-';
            if (info.prefix.Contains("internal"))
                prefix = '~';
            else if (info.prefix.Contains("protected"))
                prefix = '#';
            else if (info.prefix.Contains("public"))
                prefix = '+';

            return prefix;
        }

        static string IconAsSVG(string icon, Vector2Int pos, Vector2Int size)
        {
            return icon switch
            {
                "folder" => "<g transform=\"matrix(" + (0.0019f * size.x).ToString(System.Globalization.CultureInfo.InvariantCulture) + ",0,0," + (0.0019f * size.y).ToString(System.Globalization.CultureInfo.InvariantCulture) + "," + pos.x.ToString(System.Globalization.CultureInfo.InvariantCulture) + "," + pos.y.ToString(System.Globalization.CultureInfo.InvariantCulture) + ")\" style=\"isolation:isolate\">\n" +
                            "<path style=\"fill:#b5d97f;stroke-width:0px;\" d=\"M43.96,453.37c-10.32,0-19.23-3.8-26.72-11.41-7.49-7.61-11.24-16.46-11.24-26.54V96.59c0-10.09,3.75-18.94,11.24-26.54,7.49-7.61,16.4-11.41,26.72-11.41h164.12l43.02,43.02h216.95c10.09,0,18.94,3.8,26.54,11.41,7.61,7.61,11.41,16.46,11.41,26.54v275.81c0,10.09-3.8,18.94-11.41,26.54-7.61,7.61-16.46,11.41-26.54,11.41H43.96ZM43.96,423.51h424.09c2.36,0,4.3-.76,5.82-2.28,1.52-1.52,2.28-3.46,2.28-5.82V139.6c0-2.36-.76-4.3-2.28-5.82-1.52-1.52-3.46-2.28-5.82-2.28h-229.1l-43.02-43.02H43.96c-2.36,0-4.3.76-5.82,2.28-1.52,1.52-2.28,3.46-2.28,5.82v318.82c0,2.36.76,4.3,2.28,5.82,1.52,1.52,3.46,2.28,5.82,2.28ZM35.86,423.51V88.49v335.02Z\"/>\n" +
                            "</g>\n",

                "folderOpen" => "<g transform=\"matrix(" + (0.0019f * size.x).ToString(System.Globalization.CultureInfo.InvariantCulture) + ",0,0," + (0.0019f * size.y).ToString(System.Globalization.CultureInfo.InvariantCulture) + "," + pos.x.ToString(System.Globalization.CultureInfo.InvariantCulture) + "," + pos.y.ToString(System.Globalization.CultureInfo.InvariantCulture) + ")\" style=\"isolation:isolate\">\n" +
                            "<path style=\"fill:#b5d97f;stroke-width:0px;\" d=\"M39.47,437.31c-8.8,0-16.58-3.38-23.34-10.14-6.76-6.76-10.14-14.54-10.14-23.34V109.56c0-8.8,3.61-16.81,10.83-24.04,7.22-7.22,15.23-10.83,24.04-10.83h150.77l39.52,39.52h199.3c7.87,0,14.99,2.76,21.36,8.28,6.37,5.52,10.17,11.9,11.41,19.15h-243.84l-39.52-39.52H40.87c-2.17,0-3.95.7-5.35,2.09-1.39,1.39-2.09,3.18-2.09,5.35v292.89c0,1.7.43,3.1,1.28,4.18.85,1.08,1.98,2.01,3.37,2.79l59.09-223.62h408.83l-60.25,226.27c-2.26,8.58-6.35,14.94-12.25,19.06-5.9,4.12-13.35,6.18-22.34,6.18H39.47ZM66.85,409.88h351.28l50.77-196.65H117.62l-50.77,196.65ZM66.85,409.88l50.77-196.65-50.77,196.65ZM33.43,141.63v-39.52,39.52Z\"/>\n" +
                            "</g>\n",

                "namespace" => "<g transform=\"matrix(" + (0.002f * size.x).ToString(System.Globalization.CultureInfo.InvariantCulture) + ",0,0," + (0.002f * size.y).ToString(System.Globalization.CultureInfo.InvariantCulture) + "," + (pos.x).ToString(System.Globalization.CultureInfo.InvariantCulture) + "," + (pos.y).ToString(System.Globalization.CultureInfo.InvariantCulture) + ")\" style=\"isolation:isolate\">\n" +
                            "<path style=\"fill:#b5d97f;stroke-width:0px;\" d=\"M6,238.86c10.74-.28,19.49-3.17,26.25-8.68,6.76-5.5,11.25-13.07,13.48-22.69,2.23-9.62,3.43-26.07,3.56-49.35.14-23.28.57-38.62,1.26-46.01,1.26-11.71,3.59-21.12,7.01-28.23,3.41-7.11,7.63-12.79,12.65-17.04,5.02-4.25,11.43-7.49,19.23-9.73,5.31-1.39,13.95-2.09,25.94-2.09h11.72v32.83h-6.49c-14.51,0-24.12,2.61-28.86,7.84-4.75,5.23-7.11,16.9-7.11,35.02,0,36.53-.77,59.6-2.31,69.22-2.49,14.92-6.79,26.42-12.85,34.5-6.07,8.08-15.58,15.27-28.55,21.54,15.34,6.41,26.45,16.21,33.35,29.38,6.9,13.18,10.36,34.75,10.36,64.73,0,27.19.29,43.35.83,48.51,1.12,9.48,3.94,16.1,8.47,19.87,4.53,3.76,13.42,5.64,26.67,5.64h6.49v32.83h-11.72c-13.66,0-23.56-1.12-29.7-3.34-8.91-3.21-16.3-8.4-22.16-15.58-5.85-7.18-9.65-16.28-11.39-27.3-1.74-11.01-2.69-29.07-2.83-54.16-.13-25.1-1.32-42.45-3.56-52.07-2.23-9.62-6.72-17.21-13.48-22.79-6.76-5.57-15.51-8.5-26.25-8.78v-34.08Z\"/>\n" +
                            "<path style=\"fill:#b5d97f;stroke-width:0px;\" d=\"M506,238.86v34.08c-10.73.29-19.49,3.21-26.24,8.78-6.77,5.58-11.26,13.14-13.5,22.69-2.23,9.55-3.41,25.97-3.56,49.24-.13,23.29-.55,38.62-1.25,46.01-1.26,11.85-3.59,21.29-7.01,28.33-3.43,7.04-7.63,12.68-12.65,16.94-5.02,4.25-11.43,7.49-19.24,9.73-5.29,1.53-13.94,2.3-25.93,2.3h-11.7v-32.83h6.48c14.49,0,24.12-2.61,28.86-7.84,4.74-5.23,7.11-16.98,7.11-35.24,0-34.85.62-56.95,1.88-66.29,2.23-15.48,6.7-27.85,13.39-37.12,6.68-9.27,16.17-16.49,28.43-21.64-16.03-7.67-27.33-17.81-33.88-30.43-6.55-12.61-9.82-33.91-9.82-63.88,0-27.19-.35-43.42-1.05-48.73-.97-9.34-3.74-15.86-8.25-19.56-4.54-3.69-13.42-5.54-26.67-5.54h-6.48v-32.83h11.7c13.65,0,23.56,1.12,29.69,3.35,8.93,3.07,16.32,8.23,22.16,15.47,5.87,7.25,9.65,16.38,11.41,27.39,1.74,11.02,2.69,29.07,2.82,54.16.14,25.09,1.32,42.42,3.56,51.96,2.23,9.55,6.72,17.12,13.5,22.69,6.75,5.58,15.51,8.51,26.24,8.78Z\"/>\n" +
                            "</g>\n",

                "cs" => "<g transform=\"matrix(" + (0.002f * size.x).ToString(System.Globalization.CultureInfo.InvariantCulture) + ",0,0," + (0.002f * size.y).ToString(System.Globalization.CultureInfo.InvariantCulture) + "," + pos.x.ToString(System.Globalization.CultureInfo.InvariantCulture) + "," + (pos.y).ToString(System.Globalization.CultureInfo.InvariantCulture) + ")\" style=\"isolation:isolate\">\n" +
                            "<path style=\"fill:#b5d97f;stroke-width:0px;\" d=\"M325.23,6H96.59c-10.32,0-19.23,3.75-26.72,11.24-7.49,7.49-11.24,16.4-11.24,26.72v424.09c0,10.32,3.75,19.23,11.24,26.72,7.49,7.49,16.4,11.24,26.72,11.24h318.82c10.32,0,19.23-3.75,26.72-11.24,7.49-7.49,11.24-16.4,11.24-26.72V134.14L325.23,6ZM423.51,468.04c0,2.02-.84,3.88-2.53,5.57-1.69,1.69-3.54,2.53-5.57,2.53H96.59c-2.02,0-3.88-.84-5.57-2.53-1.69-1.69-2.53-3.54-2.53-5.57V43.96c0-2.02.84-3.88,2.53-5.57,1.69-1.69,3.54-2.53,5.57-2.53h213.71v111.74h113.21v320.45Z\"/>\n" +
                            "<text style=\"fill: #daa273;font-family:Arial-BoldMT, Arial;font-size:157.89px;font-weight:700;\" transform=\"translate(155.1 320.33)\"><tspan x=\"0\" y=\"0\">C#</tspan></text>\n" +
                            "</g>\n",

                "class" => "<g transform=\"matrix(" + (0.002f * size.x).ToString(System.Globalization.CultureInfo.InvariantCulture) + ",0,0," + (0.002f * size.y).ToString(System.Globalization.CultureInfo.InvariantCulture) + "," + pos.x.ToString(System.Globalization.CultureInfo.InvariantCulture) + "," + (pos.y).ToString(System.Globalization.CultureInfo.InvariantCulture) + ")\" style=\"isolation:isolate\">\n" +
                        "<path style=\"fill:#daa273;stroke-width:0px;\" d=\"M129.21,278.77l22.13,7.02c-3.39,12.34-9.03,21.5-16.92,27.49-7.89,5.99-17.9,8.98-30.03,8.98-15.01,0-27.34-5.13-37.01-15.38-9.66-10.25-14.49-24.27-14.49-42.06,0-18.81,4.86-33.42,14.57-43.83,9.71-10.41,22.49-15.61,38.32-15.61,13.83,0,25.06,4.09,33.69,12.26,5.14,4.83,8.99,11.77,11.56,20.82l-22.59,5.4c-1.34-5.86-4.12-10.49-8.37-13.88-4.24-3.39-9.39-5.09-15.46-5.09-8.38,0-15.18,3.01-20.39,9.02s-7.83,15.75-7.83,29.22c0,14.29,2.57,24.47,7.71,30.53s11.82,9.1,20.05,9.1c6.06,0,11.28-1.93,15.65-5.78s7.5-9.92,9.41-18.19Z\"/>\n" +
                        "<path style=\"fill:#daa273;stroke-width:0px;\" d=\"M170.77,320.33v-113.02h21.66v113.02h-21.66Z\"/>\n" +
                        "<path style=\"fill:#daa273;stroke-width:0px;\" d=\"M230.83,263.43l-19.66-3.55c2.21-7.92,6.01-13.77,11.41-17.58s13.41-5.71,24.05-5.71c9.66,0,16.86,1.14,21.59,3.43s8.06,5.19,9.98,8.71c1.93,3.52,2.89,9.98,2.89,19.39l-.23,25.29c0,7.2.35,12.5,1.04,15.92s1.99,7.08,3.89,10.99h-21.43c-.57-1.44-1.26-3.57-2.08-6.4-.36-1.28-.62-2.13-.77-2.54-3.7,3.6-7.66,6.3-11.87,8.1s-8.71,2.7-13.49,2.7c-8.43,0-15.07-2.29-19.93-6.86-4.86-4.57-7.29-10.36-7.29-17.35,0-4.63,1.11-8.75,3.32-12.37,2.21-3.62,5.31-6.4,9.29-8.33,3.98-1.93,9.73-3.61,17.23-5.05,10.13-1.9,17.14-3.67,21.05-5.32v-2.16c0-4.16-1.03-7.13-3.08-8.9s-5.94-2.66-11.64-2.66c-3.85,0-6.86.76-9.02,2.27-2.16,1.52-3.91,4.18-5.24,7.98ZM259.82,281.01c-2.78.93-7.17,2.03-13.18,3.32s-9.95,2.54-11.8,3.78c-2.83,2-4.24,4.55-4.24,7.63s1.13,5.65,3.39,7.86c2.26,2.21,5.14,3.32,8.63,3.32,3.91,0,7.63-1.28,11.18-3.85,2.62-1.95,4.34-4.34,5.17-7.17.57-1.85.85-5.37.85-10.56v-4.32Z\"/>\n" +
                        "<path style=\"fill:#daa273;stroke-width:0px;\" d=\"M294.82,296.97l21.74-3.32c.93,4.21,2.8,7.41,5.63,9.6,2.83,2.18,6.78,3.28,11.87,3.28,5.6,0,9.82-1.03,12.64-3.08,1.9-1.44,2.85-3.37,2.85-5.78,0-1.64-.51-3.01-1.54-4.09-1.08-1.03-3.5-1.98-7.25-2.85-17.48-3.85-28.55-7.38-33.23-10.56-6.48-4.42-9.71-10.56-9.71-18.43,0-7.09,2.8-13.06,8.4-17.89s14.29-7.25,26.06-7.25,19.53,1.82,24.98,5.47c5.45,3.65,9.2,9.05,11.26,16.19l-20.43,3.78c-.87-3.19-2.53-5.63-4.97-7.32s-5.92-2.54-10.45-2.54c-5.71,0-9.79.8-12.26,2.39-1.64,1.13-2.47,2.6-2.47,4.39,0,1.54.72,2.85,2.16,3.93,1.95,1.44,8.7,3.47,20.24,6.09,11.54,2.62,19.6,5.83,24.17,9.64,4.52,3.85,6.78,9.23,6.78,16.11,0,7.5-3.14,13.95-9.41,19.35s-15.55,8.1-27.83,8.1c-11.15,0-19.98-2.26-26.48-6.78s-10.76-10.67-12.76-18.43Z\"/>\n" +
                        "<g> <path style=\"fill:#b5d97f;stroke-width:0px;\" d=\"M88.57,194.4V43.96c0-2.02.84-3.88,2.53-5.57s3.54-2.53,5.57-2.53h213.71v111.74h113.21v76.01c12.21.41,21.77,2.81,28.82,7.53.38.25.68.58,1.04.84v-97.84L325.31,6H96.67c-10.32,0-19.23,3.75-26.72,11.24-7.49,7.49-11.24,16.4-11.24,26.72v167.3c8.27-8.55,18.31-14.1,29.86-16.85Z\"/>\n" +
                        "<path style=\"fill:#b5d97f;stroke-width:0px;\" d=\"M423.59,335.22v132.82c0,2.02-.84,3.88-2.53,5.57-1.69,1.69-3.54,2.53-5.57,2.53H96.67c-2.02,0-3.88-.84-5.57-2.53-1.69-1.69-2.53-3.54-2.53-5.57v-134.57c-11.44-2.65-21.47-8.2-29.86-16.79v151.36c0,10.32,3.75,19.23,11.24,26.72,7.49,7.49,16.4,11.24,26.72,11.24h318.82c10.32,0,19.23-3.75,26.72-11.24,7.49-7.49,11.24-16.4,11.24-26.72v-140.67c-7.83,4.94-17.8,7.62-29.86,7.85Z\"/>\n" +
                        "</g> <path style=\"fill:#daa273;stroke-width:0px;\" d=\"M382.63,296.97l21.74-3.32c.93,4.21,2.8,7.41,5.63,9.6,2.83,2.18,6.78,3.28,11.87,3.28,5.6,0,9.82-1.03,12.64-3.08,1.9-1.44,2.85-3.37,2.85-5.78,0-1.64-.51-3.01-1.54-4.09-1.08-1.03-3.5-1.98-7.25-2.85-17.48-3.85-28.55-7.38-33.23-10.56-6.48-4.42-9.71-10.56-9.71-18.43,0-7.09,2.8-13.06,8.4-17.89s14.29-7.25,26.06-7.25,19.53,1.82,24.98,5.47,9.2,9.05,11.26,16.19l-20.43,3.78c-.87-3.19-2.53-5.63-4.97-7.32-2.44-1.7-5.92-2.54-10.45-2.54-5.71,0-9.79.8-12.26,2.39-1.64,1.13-2.47,2.6-2.47,4.39,0,1.54.72,2.85,2.16,3.93,1.95,1.44,8.7,3.47,20.24,6.09s19.6,5.83,24.17,9.64c4.52,3.85,6.78,9.23,6.78,16.11,0,7.5-3.14,13.95-9.41,19.35-6.27,5.4-15.55,8.1-27.83,8.1-11.15,0-19.98-2.26-26.48-6.78-6.5-4.52-10.76-10.67-12.76-18.43Z\"/> </g>\n",
                
                "enum" => "<g transform=\"matrix(" + (0.002f * size.x).ToString(System.Globalization.CultureInfo.InvariantCulture) + ",0,0," + (0.002f * size.y).ToString(System.Globalization.CultureInfo.InvariantCulture) + "," + pos.x.ToString(System.Globalization.CultureInfo.InvariantCulture) + "," + (pos.y).ToString(System.Globalization.CultureInfo.InvariantCulture) + ")\" style=\"isolation:isolate\">\n" +
                        "<path style=\"fill:#daa273;stroke-width:0px;\" d=\"M106.82,294.27l21.59,3.62c-2.78,7.92-7.16,13.94-13.15,18.08-5.99,4.14-13.48,6.21-22.47,6.21-14.24,0-24.77-4.65-31.61-13.95-5.4-7.45-8.1-16.86-8.1-28.22,0-13.57,3.55-24.2,10.64-31.88,7.09-7.68,16.06-11.53,26.91-11.53,12.18,0,21.79,4.02,28.83,12.07,7.04,8.04,10.41,20.37,10.1,36.97h-54.28c.15,6.42,1.9,11.42,5.24,15s7.5,5.36,12.49,5.36c3.39,0,6.24-.93,8.56-2.78,2.31-1.85,4.06-4.83,5.24-8.94ZM108.05,272.37c-.15-6.27-1.77-11.04-4.86-14.3s-6.84-4.9-11.26-4.9c-4.73,0-8.63,1.72-11.72,5.17s-4.6,8.12-4.55,14.03h32.38Z\"/>\n" +
                        "<path style=\"fill:#daa273;stroke-width:0px;\" d=\"M221.69,320.33h-21.66v-41.79c0-8.84-.46-14.56-1.39-17.15-.93-2.6-2.43-4.61-4.51-6.05-2.08-1.44-4.59-2.16-7.52-2.16-3.75,0-7.12,1.03-10.1,3.08s-5.02,4.78-6.13,8.17c-1.11,3.39-1.66,9.66-1.66,18.81v37.08h-21.66v-81.88h20.12v12.03c7.14-9.25,16.14-13.88,26.98-13.88,4.78,0,9.15.86,13.11,2.58s6.95,3.92,8.98,6.59,3.44,5.71,4.24,9.1c.8,3.39,1.2,8.25,1.2,14.57v50.88Z\"/>\n" +
                        "<path style=\"fill:#daa273;stroke-width:0px;\" d=\"M297.56,320.33v-12.26c-2.98,4.37-6.9,7.81-11.76,10.33-4.86,2.52-9.98,3.78-15.38,3.78s-10.43-1.21-14.8-3.62-7.53-5.81-9.48-10.18-2.93-10.41-2.93-18.12v-51.81h21.66v37.62c0,11.51.4,18.57,1.2,21.16.8,2.6,2.25,4.65,4.36,6.17,2.11,1.52,4.78,2.27,8.02,2.27,3.7,0,7.02-1.02,9.95-3.05,2.93-2.03,4.93-4.55,6.01-7.56s1.62-10.37,1.62-22.09v-34.54h21.66v81.88h-20.12Z\"/>\n" +
                        "<g> <path style=\"fill:#b5d97f;stroke-width:0px;\" d=\"M90.63,223.44c.17,0,.31.04.48.04V43.96c0-2.02.84-3.88,2.53-5.57,1.69-1.69,3.54-2.53,5.57-2.53h213.71v111.74h113.21v76.55c1.98-.28,3.93-.71,5.97-.71,8.35,0,15.73,1.84,21.92,5.47.75.44,1.26,1.14,1.97,1.63v-96.4L327.85,6H99.2c-10.32,0-19.23,3.75-26.72,11.24-7.49,7.49-11.24,16.4-11.24,26.72v188.85c8.34-6.18,18.16-9.36,29.38-9.36Z\"/>\n" +
                        "<path style=\"fill:#b5d97f;stroke-width:0px;\" d=\"M426.12,333.48v134.56c0,2.02-.84,3.88-2.53,5.57-1.69,1.69-3.54,2.53-5.57,2.53H99.2c-2.02,0-3.88-.84-5.57-2.53-1.69-1.69-2.53-3.54-2.53-5.57v-132.81c-13.21-.31-22.89-4.04-29.86-8.88v141.68c0,10.32,3.75,19.23,11.24,26.72,7.49,7.49,16.4,11.24,26.72,11.24h318.82c10.32,0,19.23-3.75,26.72-11.24,7.49-7.49,11.24-16.4,11.24-26.72v-134.56h-29.86Z\"/>\n" +
                        "</g> <path style=\"fill:#daa273;stroke-width:0px;\" d=\"M338.49,238.45h19.97v11.18c7.14-8.69,15.65-13.03,25.52-13.03,5.24,0,9.79,1.08,13.65,3.24s7.02,5.42,9.48,9.79c3.6-4.37,7.48-7.63,11.64-9.79s8.61-3.24,13.34-3.24c6.01,0,11.1,1.22,15.27,3.66,4.16,2.44,7.27,6.03,9.33,10.76,1.49,3.5,2.24,9.15,2.24,16.96v52.35h-21.66v-46.8c0-8.12-.75-13.36-2.24-15.73-2-3.08-5.09-4.63-9.25-4.63-3.03,0-5.89.93-8.56,2.78-2.67,1.85-4.6,4.56-5.78,8.13-1.18,3.57-1.77,9.21-1.77,16.92v39.32h-21.66v-44.87c0-7.97-.39-13.11-1.16-15.42-.77-2.31-1.97-4.03-3.59-5.17s-3.82-1.7-6.59-1.7c-3.34,0-6.35.9-9.02,2.7-2.67,1.8-4.59,4.39-5.74,7.79-1.16,3.39-1.73,9.02-1.73,16.88v39.78h-21.66v-81.88Z\"/> </g>\n",
                
                "method" => "<g transform=\"matrix(" + (0.002f * size.x).ToString(System.Globalization.CultureInfo.InvariantCulture) + ",0,0," + (0.002f * size.y).ToString(System.Globalization.CultureInfo.InvariantCulture) + "," + pos.x.ToString(System.Globalization.CultureInfo.InvariantCulture) + "," + (pos.y).ToString(System.Globalization.CultureInfo.InvariantCulture) + ")\" style=\"isolation:isolate\">\n" +
                        "<path style=\"fill:#b5d97f;stroke-width:0px;\" d=\"M130.12,424.29l-5.96-39.64c-4.51-1.54-9.38-3.77-14.58-6.71-5.21-2.93-9.7-6.04-13.47-9.32l-36.88,16.79-26.35-46.59,33.32-24.52c-.44-2.32-.76-4.84-.95-7.58-.19-2.74-.29-5.27-.29-7.58s.1-4.72.29-7.46c.19-2.74.51-5.39.95-7.96l-33.32-24.65,26.35-46.21,36.75,16.54c4.02-3.28,8.56-6.37,13.59-9.26,5.04-2.89,9.86-5.04,14.46-6.44l6.09-39.97h52.29l5.96,39.77c4.85,1.79,9.7,4.02,14.56,6.67,4.85,2.65,9.22,5.73,13.11,9.23l37.38-16.54,26.22,46.21-33.83,24.5c.61,2.62.99,5.26,1.14,7.91.15,2.65.23,5.17.23,7.53s-.1,4.72-.29,7.3c-.19,2.59-.55,5.28-1.08,8.07l33.7,24.45-26.35,46.59-37.13-16.92c-4.11,3.45-8.54,6.63-13.3,9.55-4.76,2.92-9.55,5.08-14.37,6.48l-5.96,39.77h-52.29ZM142.14,409.32h28.13l4.74-36.8c7-1.76,13.53-4.38,19.59-7.86,6.06-3.48,11.86-7.9,17.39-13.26l34.44,14.91,13.19-23.36-30.48-22.5c.88-3.91,1.57-7.59,2.08-11.04.51-3.46.76-6.93.76-10.41s-.22-7.16-.66-10.54c-.44-3.37-1.17-6.93-2.18-10.66l30.74-22.75-13.19-23.36-34.82,14.91c-4.63-5.21-10.22-9.8-16.76-13.77-6.54-3.97-13.28-6.42-20.21-7.35l-4.49-36.8h-28.25l-4.36,36.67c-7.3,1.45-14.03,3.94-20.17,7.47-6.15,3.53-11.87,8.08-17.18,13.66l-34.57-14.79-13.19,23.36,30.48,22.37c-.96,3.48-1.7,7.02-2.21,10.6-.51,3.58-.76,7.29-.76,11.11s.25,7.23.76,10.73c.51,3.5,1.2,7.03,2.08,10.6l-30.36,22.5,13.19,23.36,34.44-14.79c5.28,5.36,11.01,9.78,17.19,13.26,6.19,3.48,12.91,6.1,20.16,7.86l4.49,36.67ZM155.88,336.16c10.35,0,19.13-3.61,26.34-10.82,7.21-7.21,10.82-15.99,10.82-26.34s-3.61-19.13-10.82-26.34c-7.21-7.21-15.99-10.82-26.34-10.82s-19.02,3.61-26.27,10.82c-7.25,7.21-10.88,15.99-10.88,26.34,0,10.35,3.63,19.13,10.88,26.34,7.25,7.21,16.01,10.82,26.27,10.82Z\"/>\n" +
                        "<path style=\"fill:#b5d97f;stroke-width:0px;\" d=\"M273.53,276.81l20.67-27.99c-1.83-3.72-3.44-8.07-4.83-13.07-1.4-5-2.24-9.66-2.55-13.99l-32.94-12.33,12.42-44.77,35.51,5.4c1.15-1.69,2.51-3.44,4.07-5.24,1.56-1.8,3.05-3.41,4.48-4.83,1.37-1.37,2.96-2.84,4.76-4.4,1.8-1.56,3.62-3,5.47-4.3l-5.32-35.59,44.54-12.19,12.41,32.71c4.48.46,9.16,1.34,14.03,2.66,4.87,1.32,9.14,2.96,12.83,4.92l28.27-20.8,32.1,32.1-20.75,28.07c1.88,4.08,3.49,8.42,4.84,13.03,1.35,4.61,2.14,9.18,2.38,13.72l33.1,12.8-12.27,44.46-35.81-5.73c-1.24,1.98-2.62,3.83-4.16,5.56-1.54,1.72-3.03,3.31-4.48,4.76s-2.96,2.84-4.66,4.3c-1.71,1.47-3.58,2.9-5.61,4.29l5.68,35.7-44.77,12.42-12.41-33.17c-4.64-.4-9.31-1.17-14.03-2.3-4.71-1.13-8.98-2.75-12.8-4.84l-28.07,20.75-32.1-32.1ZM290.09,275l17.26,17.26,25.5-19.68c5.38,3.22,10.99,5.62,16.85,7.2,5.86,1.58,12.13,2.43,18.81,2.53l11.99,30.29,22.43-6.24-4.9-32.52c2.94-1.86,5.62-3.69,8.06-5.5,2.43-1.81,4.72-3.79,6.86-5.92s4.26-4.53,6.06-6.87c1.8-2.34,3.54-4.97,5.21-7.88l32.83,4.9,6.24-22.43-30.53-12.22c.35-6.04-.26-12.29-1.84-18.74-1.58-6.46-4.21-12.1-7.89-16.92l19.83-25.34-17.34-17.34-25.19,19.83c-5.38-3.59-11.03-6.19-16.97-7.8-5.94-1.61-12.25-2.33-18.93-2.16l-12.14-30.29-22.43,6.24,4.98,32.44c-2.73,1.55-5.35,3.26-7.86,5.15-2.51,1.89-4.94,4.01-7.29,6.35-2.24,2.24-4.28,4.59-6.12,7.05-1.84,2.46-3.58,5.05-5.23,7.78l-32.44-4.83-6.24,22.43,30.22,12.06c-.05,6.53.75,12.76,2.41,18.7,1.66,5.94,4.18,11.67,7.55,17.2l-19.76,25.27ZM343.44,238.53c6.35,6.35,13.95,9.53,22.81,9.53,8.85,0,16.45-3.18,22.81-9.53,6.35-6.35,9.53-13.95,9.53-22.81,0-8.85-3.18-16.45-9.53-22.81-6.3-6.3-13.89-9.46-22.77-9.49-8.88-.03-16.49,3.14-22.85,9.49-6.35,6.35-9.51,13.97-9.49,22.85.03,8.88,3.19,16.47,9.49,22.77Z\"/>\n" +
                        "</g>\n",

                "variable" => "<g transform=\"matrix(" + (0.002f * size.x).ToString(System.Globalization.CultureInfo.InvariantCulture) + ",0,0," + (0.002f * size.y).ToString(System.Globalization.CultureInfo.InvariantCulture) + "," + pos.x.ToString(System.Globalization.CultureInfo.InvariantCulture) + "," + (pos.y).ToString(System.Globalization.CultureInfo.InvariantCulture) + ")\" style=\"isolation:isolate\">\n" +
                        "<text style=\"font-family: Arial-BoldMT, Arial;font-size: 326.16px;fill: #b5d97f;font-weight: 700;\" transform=\"translate(6 332.92)\"><tspan x=\"0\" y=\"0\">[</tspan></text>\n" +
                        "<text style=\"font-family: Arial-BoldItalicMT, Arial;font-size: 260.92px;font-style: italic;fill: #b5d97f;font-weight: 700;\" transform=\"translate(158.12 360.67)\"><tspan x=\"0\" y=\"0\">X</tspan></text>\n" +
                        "<text style=\"font-family: Arial-BoldMT, Arial;font-size: 326.16px;fill: #b5d97f;font-weight: 700;\" transform=\"translate(397.39 332.92)\"><tspan x=\"0\" y=\"0\">]</tspan></text>\n" +
                        "</g>\n",

                //fallback.
                _ => "",
            };
        }
    }
}
#endif