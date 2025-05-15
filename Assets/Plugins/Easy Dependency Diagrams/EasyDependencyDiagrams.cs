using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
#if UNITY_EDITOR
namespace EasyDependencyDiagrams
{
    /// <summary>
    /// The class that handles the parsing of code files
    /// </summary>
    public class EasyDependencyDiagrams : ScriptableObject
    {

        /// <summary>
        /// Stores the data for folders, files, structures and attributes
        /// </summary>
        [SerializeField]
        public class DataElement
        {
            public string name, description, type;
            public string objectType;
            public string prefix;
            public string attributeField;
            public string postFix;
            public int page;
            public int yPos;
            public List<string> parameters;
            public List<DataElement> children;
            public List<string> dependencies;
            public List<string> usings;
            public string path; //Will be used by the editor and not directly modified by the parser itself
            public string fullLine; //Used to find the line number of the element

            public DataElement(string _name = "", string _description = "", string _type = "", string _objectType = "", string _prefix = "", string _braceField = "", string _postFix = "", string _fullLine = "", List<string> _parameters = null, List<DataElement> _children = null, List<string> _dependencies = null, List<string> _usings = null)
            {
                name = _name;
                description = _description;
                type = _type;
                objectType = _objectType;
                prefix = _prefix;
                attributeField = _braceField;
                postFix = _postFix;
                parameters = _parameters;
                fullLine = _fullLine;
                page = 0;
                yPos = 0;
                if (parameters == null)
                    parameters = new List<string>();
                children = _children;
                if (children == null)
                    children = new List<DataElement>();
                dependencies = _dependencies;
                if (dependencies == null)
                    dependencies = new List<string>();
                usings = _usings;
                if (usings == null)
                    usings = new List<string>();

                path = "";
            }

            public DataElement GetCopy()
            {
                DataElement de = new DataElement(name, description, type, objectType, prefix, attributeField, postFix, fullLine, parameters, children);
                return de;
            }

            /// <summary>
            /// Calculates the dimensions of this DataElement tree, assuming it is a folder
            /// </summary>
            /// <returns>Dimensions of the tree in terms of width and depth</returns>
            public Vector2Int GetFolderDimensions()
            {
                Vector2Int fileDimensions = new Vector2Int(1, 1);
                Vector2Int folderDimensions = new Vector2Int(1, 1);
                for (int i = 0; i < children.Count; ++i)
                {
                    if (children[i].type.Equals("Folder"))
                    {
                        Vector2Int childDimensions = children[i].GetFolderDimensions();
                        folderDimensions.y += childDimensions.y;
                        folderDimensions.x = Mathf.Max(folderDimensions.x, childDimensions.x + 1);
                    }
                    else if (children[i].type.Equals("File"))
                    {
                        fileDimensions += new Vector2Int(1, 0);
                    }
                }

                Vector2Int maxDimension = new Vector2Int(Mathf.Max(fileDimensions.x, folderDimensions.x), Mathf.Max(fileDimensions.y, folderDimensions.y));

                return maxDimension;
            }

            /// <summary>
            /// Calculates the dimensions of this DataElement tree
            /// </summary>
            /// <returns>Dimensions of the tree in terms of width and depth</returns>
            public Vector2Int GetDataDimensions()
            {
                Vector2Int propertyDimensions = new Vector2Int(0, 0);
                Vector2Int structureDimensions = new Vector2Int(1, 1);
                bool hasConstructor = false;
                bool hasDestructor = false;
                bool hasOperator = false;
                bool hasEnum = false;
                bool hasMethod = false;
                bool hasProperty = false;
                bool hasVariable = false;
                for (int i = 0; i < children.Count; ++i)
                {
                    if (children[i].type.Equals("Namespace") || children[i].type.Equals("Class"))
                    {
                        Vector2Int childDimensions = children[i].GetDataDimensions();
                        structureDimensions.y += childDimensions.y;
                        structureDimensions.x = Mathf.Max(structureDimensions.x, childDimensions.x + 1);
                    }
                    if (children[i].type.Equals("Constructor"))
                        hasConstructor = true;
                    if (children[i].type.Equals("Destructor"))
                        hasDestructor = true;
                    if (children[i].type.Equals("Operator"))
                        hasOperator = true;
                    if (children[i].type.Equals("Enum"))
                        hasEnum = true;
                    if (children[i].type.Equals("Method"))
                        hasMethod = true;
                    if (children[i].type.Equals("Property"))
                        hasProperty = true;
                    if (children[i].type.Equals("Variable"))
                        hasVariable = true;
                }
                if (hasConstructor)
                    propertyDimensions += new Vector2Int(0, 1);
                if (hasDestructor)
                    propertyDimensions += new Vector2Int(0, 1);
                if (hasOperator)
                    propertyDimensions += new Vector2Int(0, 1);
                if (hasEnum)
                    propertyDimensions += new Vector2Int(0, 1);
                if (hasMethod)
                    propertyDimensions += new Vector2Int(0, 1);
                if (hasProperty)
                    propertyDimensions += new Vector2Int(0, 1);
                if (hasVariable)
                    propertyDimensions += new Vector2Int(0, 1);

                Vector2Int maxDimension = propertyDimensions + structureDimensions;

                return maxDimension;
            }

            public int FileFolderChildCount()
            {
                int childCount = 0;
                if (children != null)
                    childCount += children.Count;

                for (int i = 0; i < children.Count; ++i)
                {
                    if (children[i].type.Equals("Folder"))
                        childCount += children[i].FileFolderChildCount();
                }

                return childCount;
            }
        }

        /// <summary>
        /// The most recent job of the parser
        /// </summary>
        public DataElement latestJob;

        private Thread creatorThread;
        private readonly string[] modifiers = new string[18] { "abstract", "partial", "delegate", "async", "const", "event", "extern", "override", "readonly", "sealed", "static", "unsafe", "virtual", "volatile", "public", "private", "internal", "protected" };
        private string targetFilePath = "";
        private bool noStructures = false;
        private readonly char[] separators = new char[14] { ',', '?', ':', '{', '}', '[', ']', '(', ')', '<', '>', '&', '|', ' ' };
        private readonly char[] intCheck = new char[10] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
        private long dataParsed = 0;
        private long totalData = 0;

        /// <summary>
        /// Progress of the data parsing process.
        /// </summary>
        public float Progress
        {
            get
            {
                if (!activeJob)
                    return 1;
                if (totalData > 0 && totalData < float.MaxValue)
                    return (dataParsed * 1.0f) / (totalData * 1.0f);
                else
                    return 0;
            }
        }

        private bool activeJob = false;
        /// <summary>
        /// Is there an active job going on
        /// </summary>
        public bool ActiveJob
        {
            get
            {
                return activeJob;
            }
        }


        /// <summary>
        /// Starts parsing the folder/file/structure/attribute indicated by 'path'. The progress can be tracked through the 'Progress' property.
        /// </summary>
        public void ParseData(string path, bool onlyFiles)
        {
            if (!activeJob)
            {
                latestJob = null;
                targetFilePath = path;
                dataParsed = 0;
                totalData = 1;
                noStructures = onlyFiles;

                if (creatorThread != null && creatorThread.IsAlive)
                    Stop();

                activeJob = true;

                creatorThread = new Thread(new ThreadStart(StartCreatingDiagram));
                creatorThread.Start();
                WaitForJobDone(creatorThread);
            }
        }

        /// <summary>
        /// Stops the current active job.
        /// </summary>
        public void Stop()
        {
            if (creatorThread != null)
            {
                if (creatorThread.IsAlive)
                    creatorThread.Abort();
                creatorThread = null;
                activeJob = false;
            }
        }

        /// <summary>
        /// Finds all references and dependencies to other code elements based on the given line of code
        /// </summary>
        public List<string> DependenciesFromCodeLine(string line)
        {
            string originalLine = line;
            if (line.Trim().Length <= 0)
                return new List<string>();
            //Removing possible modifiers from the start (and any other unnecessary parts like semicolon or strings)
            string nextWord = NextWord(line, 0);
            int wordIndex = 0;
            while (IsModifier(nextWord))
            {
                wordIndex += line.IndexOf(nextWord, wordIndex) - wordIndex + nextWord.Length + 1;
                if (wordIndex >= line.Length)
                    return new List<string>();
                nextWord = NextWord(line, wordIndex);
            }
            line = line[wordIndex..].Trim();
            line = RemoveQuotationMarks(line);
            line = line.Replace(";", " ").Trim();
            line = line.Replace("!", " ").Trim();
            line = line.Replace("+", " ").Trim();
            line = line.Replace("-", " ").Trim();
            line = line.Replace("/", " ").Trim();
            line = line.Replace("*", " ").Trim();
            line = line.Replace("^", " ").Trim();

            List<string> dependencies = new List<string>();
            string rightSide = "";
            int equalIndex = line.IndexOf('=');
            if (equalIndex >= 0)
            {
                rightSide = line[(equalIndex + 1)..].Trim();
                line = line.Substring(0, equalIndex).Trim();
                if (rightSide.Length > 0)
                {
                    dependencies.AddRange(DependenciesFromCodeLine(rightSide));
                }
            }

            while (line.Contains('{') && line.Contains('}') && line.IndexOf('{') < line.IndexOf('}'))
            {
                string bracketContent = line.Substring(line.IndexOf('{') + 1, ClosingBrace(line, '{', '}', line.IndexOf('{') + 1) - 2 - line.IndexOf('{')).Trim();
                if (bracketContent.Length > 0)
                    dependencies.AddRange(DependenciesFromCodeLine(bracketContent));
                string tmp = line.Substring(0, line.IndexOf('{')) + line[(line.IndexOf('{') + 1)..][(line[(line.IndexOf('{') + 1)..].IndexOf('}') + 1)..];
                line = tmp;

            }

            line = line.Replace("{", " ").Trim();
            line = line.Replace("}", " ").Trim();

            while (line.Contains('(') && line.Contains(')') && line.IndexOf('(') < line.IndexOf(')'))
            {
                string bracketContent = line.Substring(line.IndexOf('(') + 1, ClosingBrace(line, '(', ')', line.IndexOf('(') + 1) - 2 - line.IndexOf('(')).Trim();
                if (bracketContent.Length > 0)
                {
                    dependencies.AddRange(DependenciesFromCodeLine(bracketContent));
                }
                string tmp = line.Substring(0, line.IndexOf('(')) + line[(line.IndexOf('(') + 1)..][(line[(line.IndexOf('(') + 1)..].IndexOf(')') + 1)..];
                line = tmp;

            }
            line = line.Replace("(", " ").Trim();
            line = line.Replace(")", " ").Trim();

            while (line.Contains('<') && line.Contains('>') && line.IndexOf('<') < line.IndexOf('>'))
            {
                string bracketContent = line.Substring(line.IndexOf('<') + 1, ClosingBrace(line, '<', '>', line.IndexOf('<') + 1) - 2 - line.IndexOf('<')).Trim();
                if (bracketContent.Length > 0)
                    dependencies.AddRange(DependenciesFromCodeLine(bracketContent));
                string tmp = line.Substring(0, line.IndexOf('<')) + line[(line.IndexOf('<') + 1)..][(line[(line.IndexOf('<') + 1)..].IndexOf('>') + 1)..];
                line = tmp;

            }
            line = line.Replace("<", " ").Trim();
            line = line.Replace(">", " ").Trim();

            while (line.Contains('[') && line.Contains(']') && line.IndexOf('[') < line.IndexOf(']'))
            {
                string bracketContent = line.Substring(line.IndexOf('[') + 1, ClosingBrace(line, '[', ']', line.IndexOf('[') + 1) - 2 - line.IndexOf('[')).Trim();
                if (bracketContent.Length > 0)
                    dependencies.AddRange(DependenciesFromCodeLine(bracketContent));
                string tmp = line.Substring(0, line.IndexOf('[')) + line[(line.IndexOf('[') + 1)..][(line[(line.IndexOf('[') + 1)..].IndexOf(']') + 1)..];
                line = tmp;

            }
            line = line.Replace("[", " ").Trim();
            line = line.Replace("]", " ").Trim();

            //making sure there's no spaces between related words
            for (int i = 0; i < line.Length - 1; ++i)
            {
                if (line[i].Equals('.') && (line[i + 1].Equals(' ') || line[i + 1].Equals('\n')))
                {
                    line = line.Remove(i + 1, 1);
                    --i;
                }
            }

            //splitting the line to independent parts
            string[] splitLine = line.Split(separators);

            for (int i = 0; i < splitLine.Length; ++i)
            {
                string trimmed = splitLine[i].Trim();

                if (trimmed.Length > 0 && !ArrayUtility.Contains<char>(intCheck, trimmed[0]))
                {
                    string[] dotSplit = trimmed.Split('.');
                    string combined = "";
                    for (int k = 0; k < dotSplit.Length; ++k)
                    {
                        if (combined.Length > 0)
                            combined += ".";
                        combined += dotSplit[k];
                        if (combined.Length > 0)
                            dependencies.Add(combined);
                    }
                }
            }

            return dependencies;
        }

        /// <summary>
        /// Finds the line number of the given code line 'fullLine' from the file indicated by 'path'
        /// </summary>
        public int FindLineNumber(string path, string fullLine)
        {
            StreamReader reader = new StreamReader(path);
            string fullFile = reader.ReadToEnd();
            reader.Close();

            if (fullLine.Length < 1)
                return 1;

            if (fullFile.Contains(fullLine))
            {
                int lines = 1;
                bool lineStart = true;
                for (int i = 0; i < fullFile.Length; ++i)
                {
                    if (lineStart && fullFile.Substring(i, fullLine.Length).Equals(fullLine))
                    {
                        return lines;
                    }
                    lineStart = false;
                    if (fullFile[i].Equals('\n'))
                    {
                        lineStart = true;
                        lines++;
                    }
                }
            }

            return 1;
        }

        /// <summary>
        /// Helper method. Clones the given list.
        /// </summary>
        public static List<T> CloneList<T>(List<T> original)
        {
            List<T> clone = new List<T>();
            for (int i = 0; i < original.Count; ++i)
            {
                clone.Add(original[i]);
            }

            return clone;
        }

        private void StartCreatingDiagram()
        {
            string rootPath = targetFilePath;

            DataElement rootData = null;
            //file
            if (rootPath.Length > 2 && rootPath.EndsWith(".cs"))
            {
                if (File.Exists(rootPath))
                {
                    totalData = DataSize(rootPath);
                    rootData = ParseCSFile(rootPath);
                }
            }
            //code element
            else if (rootPath.Length > 2 && rootPath.Contains(".cs"))
            {
                totalData = DataSize(rootPath.Substring(0, rootPath.IndexOf(".cs") + 3));
                rootData = ParseCSFile(rootPath.Substring(0, rootPath.IndexOf(".cs") + 3));
                int pathindex = rootPath.IndexOf(".cs") + 4;
                string[] splitPath = rootPath[pathindex..].Split('/');
                for (int i = 0; i < splitPath.Length; ++i)
                {
                    for (int k = 0; k < rootData.children.Count; ++k)
                    {
                        if (rootData.children[k].name.Equals(splitPath[i]))
                        {
                            rootData = rootData.children[k];
                            break;
                        }
                    }
                }
            }
            //folder
            else
            {
                if (Directory.Exists(rootPath))
                {
                    totalData = DataSize(rootPath);

                    rootData = ParseFolder(rootPath, noStructures);
                }
            }

            latestJob = rootData;

            activeJob = false;
        }

        private long DataSize(string f_path)
        {
            //file
            if (File.Exists(f_path))
            {
                FileInfo file = new FileInfo(f_path);
                return file.Length;
            }
            //folder
            else if (Directory.Exists(f_path))
            {
                DirectoryInfo dir = new DirectoryInfo(f_path);
                long size = 0;


                FileInfo[] files = dir.GetFiles("*.cs", SearchOption.TopDirectoryOnly);
                for (int i = 0; i < files.Length; ++i)
                {
                    size += files[i].Length;
                }

                //Go through all folders recursively
                DirectoryInfo[] folders = dir.GetDirectories();
                for (int i = 0; i < folders.Length; ++i)
                {
                    size += DataSize(folders[i].FullName);
                }

                return size;
            }
            return 0;
        }

        private async void WaitForJobDone(Thread _t)
        {
            while (_t.IsAlive)
            {
                await Task.Yield();
            }
#if UNITY_EDITOR
            AssetDatabase.Refresh();
#endif
        }

        private List<string> FindUsings(string fullFile)
        {
            List<string> dependencies = new List<string>();
            int index = 0;
            while (index < fullFile.Length)
            {
                if (!activeJob) throw new System.Exception("DataElement creation stopped manually.");

                string nextWord = NextWord(fullFile, index);

                //using declarations must be before any classes, enumerations or namespaces
                if (nextWord.Equals("class") || nextWord.Equals("enum") || nextWord.Equals("namespace"))
                    break;

                if (nextWord.Length < 1 || fullFile.IndexOf(nextWord, index) < index)
                    break;

                if (nextWord.Equals("using"))
                {
                    int dependencyStart = fullFile.IndexOf(nextWord, index) + nextWord.Length;
                    int dependencyEnd = NextResult(fullFile, ';', index);
                    if (dependencyEnd - dependencyStart - 1 < 0) 
                        break;
                    string dependency = fullFile.Substring(dependencyStart, dependencyEnd - dependencyStart-1).Replace(" ", "").Replace("\n", "");
                    dependencies.Add(dependency);

                    //skip until next ';'
                    index = NextResult(fullFile, ';', index)+1;
                    continue;
                }

                index = fullFile.IndexOf(nextWord, index) + nextWord.Length;

            }

            return dependencies;
        }

        private DataElement ParseFolder(string rootFolder, bool onlyFiles)
        {
            DirectoryInfo root = new DirectoryInfo(rootFolder);
            DataElement rootElement = new DataElement(root.Name, "", "Folder", "", "", "", "", "", null, new List<DataElement>());

            //Go through all files

            FileInfo[] files = root.GetFiles("*.cs", SearchOption.TopDirectoryOnly);

            for (int i = 0; i < files.Length; ++i)
            {
                try
                {
                    if (onlyFiles)
                    {
                        rootElement.children.Add(new DataElement(new FileInfo(files[i].FullName).Name, "", "File", "", "", "", "", "", null, new List<DataElement>()));

                    }
                    else
                    {
                        DataElement child = ParseCSFile(files[i].FullName);
                        rootElement.children.Add(child);
                        rootElement.usings.AddRange(child.usings);
                        rootElement.dependencies.AddRange(child.dependencies);
                    }
                }
                catch (System.Exception e)
                {
                    if (!e.GetType().Equals(typeof(ThreadAbortException)))
                        Debug.LogWarning("Couldn't parse file: " + files[i].FullName + ":\n" + e);
                }
            }

            //Go through all folders recursively
            DirectoryInfo[] folders = root.GetDirectories();
            for (int i = 0; i < folders.Length; ++i)
            {
                DataElement folder = ParseFolder(folders[i].FullName, onlyFiles);

                if (folder.type == "Folder")
                {
                    rootElement.children.Add(folder);
                    rootElement.usings.AddRange(folder.usings);
                    rootElement.dependencies.AddRange(folder.dependencies);
                }
            }

            //hide this folder if it has no relevant children
            if (rootElement.children.Count == 0)
                rootElement.type = "";

            return rootElement;
        }

        private DataElement ParseCSFile(string path)
        {
            StreamReader reader = new StreamReader(path);
            string fullFile = reader.ReadToEnd();
            reader.Close();

            //remove comments
            fullFile = RemoveComments(fullFile);

            DataElement fileElement = new DataElement(new FileInfo(path).Name, "", "File", "", "", "", "", "", null, new List<DataElement>());

            fileElement.usings.AddRange(FindUsings(fullFile));
            List<string> fileUsings = CloneList(fileElement.usings);

            System.DateTime parseStartTime = System.DateTime.UtcNow;

            //look for structures, delegates and namespaces
            int index = 0;
            while (index < fullFile.Length)
            {
                if (System.DateTime.UtcNow - parseStartTime > new System.TimeSpan(0, 0, 10))
                    throw new System.Exception("File is too big or syntax is unknown.");

                if (!activeJob) throw new System.Exception("Data parsing stopped manually.");

                string nextWord = NextWord(fullFile, index);

                if (nextWord.Length < 1 || fullFile.IndexOf(nextWord, index) < index)
                    break;

                if (nextWord.Equals("using"))
                {
                    //skip until next ';'
                    index = NextResult(fullFile, ';', index);
                    continue;
                }

                if (nextWord.Contains("///") || nextWord.Equals("namespace") || nextWord.Equals("class") || nextWord.Equals("struct") || nextWord.Equals("interface") || nextWord.Equals("enum") || nextWord.Equals("delegate"))
                {
                    DataElement classElement = ParseStructure(fullFile, index, fileUsings, "", parseStartTime);

                    if (classElement != null)
                    {
                        fileElement.children.Add(classElement);
                        fileElement.dependencies.AddRange(classElement.dependencies);
                        fileElement.usings.AddRange(classElement.usings);
                        if (classElement.type.Equals("Namespace"))
                        {
                            fileElement.usings.Add(classElement.name);
                        }
                    }
                    int altEnd = NextResult(fullFile, ';', index);
                    int start = NextResult(fullFile, '{', index);
                    if (altEnd >= 0 && (altEnd < start || start < 0))
                    {
                        index = altEnd;
                    }
                    else
                    {
                        int end = ClosingBrace(fullFile, '{', '}', start);
                        index = end;
                    }
                }
                else
                {
                    index = fullFile.IndexOf(nextWord, index) + nextWord.Length;
                }
            }

            return fileElement;
        }

        private DataElement ParseStructure(string s, int ind, List<string> usings, string pathTrail, System.DateTime parseStartTime)
        {
            if (ind < 0)
                ind = 0;

            DataElement element = new DataElement
            {
                usings = CloneList(usings)
            };
            string beginningWord = NextWord(s, ind);

            //checking if there's a summary comment
            if (beginningWord.Contains("///"))
            {
                element.description = ParseComment(s, s.IndexOf("///", ind));
                //removing the comment to avoid confusion with prefixes
                string next = NextWord(s, ind);
                while (next.Contains("///"))
                {
                    if (System.DateTime.UtcNow - parseStartTime > new System.TimeSpan(0, 0, 10))
                        throw new System.Exception("File is too big or syntax is unknown.");
                    if (!activeJob) throw new System.Exception("Data parsing stopped manually.");

                    int cstart = s.IndexOf("///", ind);
                    int cend = s.IndexOf('\n', cstart);
                    s = s.Remove(cstart, cend - cstart);
                    next = NextWord(s, ind);
                }
            }

            string typeWord = NextWord(s, ind);


            while (!typeWord.Equals("class") && !typeWord.Equals("struct") && !typeWord.Equals("interface") && !typeWord.Equals("enum") && !typeWord.Equals("delegate") && !typeWord.Equals("namespace"))
            {
                if (System.DateTime.UtcNow - parseStartTime > new System.TimeSpan(0, 0, 10))
                    throw new System.Exception("File is too big or syntax is unknown.");
                if (!activeJob) throw new System.Exception("Data parsing stopped manually.");

                ind = s.IndexOf(typeWord, ind) + typeWord.Length;
                typeWord = NextWord(s, ind);

                if (typeWord.Contains("///"))
                {
                    int cstart = s.IndexOf("///", ind);
                    int cend = s.IndexOf('\n', cstart);
                    s = s.Remove(cstart, cend - cstart);
                    typeWord = NextWord(s, ind);
                }
            }

            ind = s.IndexOf(typeWord, ind);

            element.fullLine = LineFromIndex(s, ind);

            if (typeWord == "enum")
            {
                element.type = "Enum";
                element.objectType = typeWord;
            }
            else if (typeWord == "namespace")
            {
                element.type = "Namespace";
                element.objectType = typeWord;
            }
            else if (typeWord == "delegate")
            {
                DataElement deleg = ParseVarFunc(s, Mathf.Max(ind, 0), element.name, element.usings, parseStartTime);
                deleg.description = element.description;
                return deleg;
            }
            else
            {
                element.type = "Class";
                element.objectType = typeWord;
            }

            //prefix ("accessibility")
            string prefix = "";
            int lastSemi = LastResult(s, ';', ind);
            int lastBrE = LastResult(s, '}', ind);
            int lastBrS = LastResult(s, '{', ind);
            int prefixStart = Mathf.Max(lastSemi, lastBrE, lastBrS, 0);
            prefix = s[prefixStart..Mathf.Max(ind, 0)].Trim();

            //prefix includes [...] attributes
            while (prefix.Length > 0 && prefix[0].Equals('['))
            {
                if (System.DateTime.UtcNow - parseStartTime > new System.TimeSpan(0, 0, 10))
                    throw new System.Exception("File is too big or syntax is unknown.");
                if (!activeJob) throw new System.Exception("Data parsing stopped manually.");

                int attrEnd = ClosingBrace(prefix, '[', ']', 1);
                if (attrEnd > 0)
                {
                    element.attributeField += prefix.Substring(0, attrEnd).Trim() + "\n";
                    prefix = prefix[attrEnd..].Trim();
                }
                else
                {
                    break;
                }
            }
            if (element.attributeField.Length > 0)
                element.attributeField = element.attributeField[0..^1];

            element.prefix = prefix;
            element.dependencies.AddRange(DependenciesFromCodeLine(element.prefix));

            ind = s.IndexOf(typeWord, Mathf.Max(ind, 0)) + typeWord.Length;
            ind = s.IndexOf(NextWord(s, Mathf.Max(ind, 0)), Mathf.Max(ind, 0));

            //name
            string name = "";
            for (int i = Mathf.Max(ind, 0); i < s.Length; ++i)
            {
                if (char.IsWhiteSpace(s[i]) || char.IsSeparator(s[i]) || s[i] == ':' || s[i] == '(' || s[i] == '{' || s[i] == ';' || s[i] == '/' || i == s.Length - 1)
                {
                    name = s[ind..i].Trim();
                    break;
                }
            }
            element.name = DropAllBrackets(name);

            ind = s.IndexOf(name, ind) + name.Length;

            int dotdot = s.IndexOf(':', ind);
            int altEnd = s.IndexOf(';', ind);
            int start = s.IndexOf('{', ind);
            int parenthesis = s.IndexOf('(', ind);

            //class with parameters
            if (parenthesis >= 0 && (parenthesis < dotdot || dotdot < 0) && (parenthesis < altEnd || altEnd < 0) && (parenthesis < start || start < 0))
            {
                int parenthesisEnd = ClosingBrace(s, '(', ')', parenthesis + 1) - 1;
                if (parenthesisEnd > parenthesis)
                {
                    string paramString = s.Substring(parenthesis + 1, parenthesisEnd - parenthesis - 1).Trim();
                    element.dependencies.AddRange(DependenciesFromCodeLine(paramString));
                    element.parameters = ParseParameters(paramString);
                    ind = parenthesisEnd;
                    dotdot = s.IndexOf(':', ind);
                    altEnd = s.IndexOf(';', ind);
                    start = s.IndexOf('{', ind);
                }
            }

            //inheritances
            if (dotdot >= 0 && (dotdot < altEnd || altEnd < 0) && (dotdot < start || start < 0))
            {
                int postfixEnd = altEnd < 0 || start < altEnd ? start : altEnd;
                string postfix = s.Substring(dotdot + 1, postfixEnd - dotdot - 1);
                element.postFix = " " + postfix.Trim();

                element.dependencies.AddRange(DependenciesFromCodeLine(element.postFix));
            }

            if (altEnd >= 0 && altEnd < start)
                return element;

            //finding the scope of the structure
            string sc = "";
            int sci = 0;
            if (altEnd >= 0 && (altEnd < start || start < 0))
            {
                //C# version 10 namespace for the whole filecan be declared with just ;
                if (element.type == "Namespace")
                {
                    sc = s[(altEnd + 1)..];
                    sci = 0;
                }
                else
                {
                    return element;
                }
            }
            else
            {
                int end = ClosingBrace(s, '{', '}', start + 1);
                sc = s.Substring(start + 1, end - start - 1);
                sci = 0;
            }


            dataParsed += sc.Length /2;

            string newTrail = element.name;
            if (pathTrail.Length > 0)
                newTrail = pathTrail + "." + newTrail;

            if (element.type.Equals("Namespace") || element.type.Equals("Class"))
            {
                element.usings.AddRange(FindUsings(sc));
                element.usings.Add(element.name);
                string[] splitTrail = newTrail.Split(".");
                string tmpTrail = "";
                for (int i = 0; i < splitTrail.Length; ++i)
                {
                    if (tmpTrail.Length > 0)
                        tmpTrail += ".";
                    tmpTrail += splitTrail[i];
                    element.usings.Add(tmpTrail);
                }
            }

            string comment = "";
            //go through the scope
            if (element.type.Equals("Enum"))
            {
                //parse the enums like parameters
                sc = sc.Trim();
                if (sc.Length > 1)
                {
                    List<string> enumList = new List<string>();
                    int enumStart = 0;
                    string enumComment = "";
                    for (int i = 0; i < sc.Length; ++i)
                    {
                        //skip comments
                        if (i < sc.Length - 2 && sc.Substring(i, 3).Equals("///"))
                        {
                            if (enumComment.Length == 0)
                            {
                                enumComment = ParseComment(sc, i);
                            }
                            sc = sc.Remove(i, sc.IndexOf('\n', i) - i);

                            --i;
                            continue;
                        }

                        if (sc[i].Equals(','))
                        {
                            enumList.Add(sc[enumStart..i].Trim());
                            enumStart = i + 1;
                            if (enumComment.Length > 0)
                            {
                                element.description += "\n\n" + NextWord(enumList[^1], 0).Trim() + ": " + enumComment;
                                enumComment = "";
                            }
                        }
                    }
                    //last parameter
                    if (sc.Length > 0)
                    {
                        enumList.Add(sc.Substring(enumStart, sc.Length - enumStart - 1).Trim());
                        if (enumComment.Length > 0)
                        {
                            element.description += "\n\n" + NextWord(enumList[^1], 0).Trim() + ": " + enumComment + "\n";
                            enumComment = "";
                        }
                    }
                    element.parameters = enumList;
                }
            }
            else
            {
                while (sci >= 0 && sci < sc.Length)
                {
                    if (System.DateTime.UtcNow - parseStartTime > new System.TimeSpan(0, 0, 10))
                        throw new System.Exception("File is too big or syntax is unknown.");
                    if (!activeJob) throw new System.Exception("DataElement generation stopped manually.");

                    //children
                    int nextSqPar = sc.IndexOf('[', sci);
                    int nextEqual = sc.IndexOf('=', sci);
                    int nextSemiC = sc.IndexOf(';', sci);
                    int nextNPar = sc.IndexOf('<') < 0 ? -1 : NextResult(sc, '(', sci, true);
                    int nextBrace = sc.IndexOf('{', sci);
                    int nextComm = sc.IndexOf("///", sci);

                    int nextEnd = nextEqual < 0 || (nextEqual > nextSemiC && nextSemiC >= 0) ? nextSemiC : nextEqual;
                    nextEnd = nextEnd < 0 || (nextEnd > nextNPar && nextNPar >= 0) ? nextNPar : nextEnd;
                    nextEnd = nextEnd < 0 || (nextEnd > nextBrace && nextBrace >= 0) ? nextBrace : nextEnd;
                    if (nextEnd >= 0)
                    {
                        if (nextComm >= 0 && nextComm < nextEnd && (nextSqPar < 0 || nextComm < nextSqPar) && comment == "")
                        {
                            //the child has a comment
                            comment = ParseComment(sc, sc.IndexOf("///", sci));
                            //removing the comment to avoid confusion with prefixes
                            string nextw = NextWord(sc, sci);
                            while (nextw.Contains("///"))
                            {
                                if (System.DateTime.UtcNow - parseStartTime > new System.TimeSpan(0, 0, 10))
                                    throw new System.Exception("File is too big or syntax is unknown.");
                                if (!activeJob) throw new System.Exception("Dependency parsing stopped manually.");

                                int cstart = sc.IndexOf("///", sci);
                                int cend = sc.IndexOf('\n', cstart);
                                sc = sc.Remove(cstart, cend - cstart);
                                nextw = NextWord(sc, sci);
                            }
                            continue;
                        }


                        //checking if there's attributes
                        if (nextSqPar < 0 || nextEnd < nextSqPar || !NextWord(sc, sci)[0].Equals('['))
                        {
                            //no attribute on the way
                            sc = TrimBrackets(sc, sci, nextEnd);

                            string nextWord = NextWord(sc, sci);
                            while (IsModifier(nextWord))
                            {
                                if (System.DateTime.UtcNow - parseStartTime > new System.TimeSpan(0, 0, 10))
                                    throw new System.Exception("File is too big or syntax is unknown.");
                                if (!activeJob) throw new System.Exception("Dependency parsing stopped manually.");

                                sci += sc.IndexOf(nextWord, sci) - sci + nextWord.Length + 1;
                                nextWord = NextWord(sc, sci);
                            }
                        }
                        else
                        {
                            //[...]-fields before the actual data
                            while (NextWord(sc, sci)[0].Equals('['))
                            {
                                if (System.DateTime.UtcNow - parseStartTime > new System.TimeSpan(0, 0, 10))
                                    throw new System.Exception("File is too big or syntax is unknown.");
                                if (!activeJob) throw new System.Exception("Dependency parsing stopped manually.");

                                nextSqPar = NextResult(sc, '[', sci);
                                sci = ClosingBrace(sc, '[', ']', nextSqPar);
                            }
                            continue;
                        }

                        //now NextWord should be the type of the variable/function/class
                        int jumpTo = sci;
                        string next = NextWord(sc, Mathf.Max(sci, 0));

                        //simply go further if next "word" is just unnecessary characters
                        if (next.StartsWith(";") || next.StartsWith(")"))
                        {
                            sci = NextResult(sc, ';', sci);
                            continue;
                        }

                        if (next.StartsWith(";"))
                        {
                            sci = NextResult(sc, ';', sci);
                            continue;
                        }

                        if (next.Equals("class") || next.Equals("enum") || next.Equals("interface") || next.Equals("struct") || next.Equals("namespace"))
                        {
                            DataElement child = ParseStructure(sc, Mathf.Max(sci, 0), usings, newTrail, parseStartTime);

                            if (child != null)
                            {
                                if (comment != "")
                                {
                                    child.description = comment + child.description;
                                    comment = "";
                                }

                                element.children.Add(child);
                                element.dependencies.AddRange(child.dependencies);
                                element.usings.AddRange(child.usings);

                                if (child.type.Equals("Namespace"))
                                {
                                    if (element.type.Equals("Namespace"))
                                        element.usings.Add(element.name + "." + child.name);
                                    else
                                        element.usings.Add(child.name);
                                }
                            }


                            int braceStart = NextResult(sc, '{', sci);
                            jumpTo = ClosingBrace(sc, '{', '}', braceStart);
                        }
                        else
                        {
                            DataElement child = ParseVarFunc(sc, Mathf.Max(sci, 0), element.name, usings, parseStartTime);
                            if (comment != "")
                            {
                                child.description = comment;
                                comment = "";
                            }
                            //skipping all using-statements
                            if (child.type.Equals("using"))
                            {
                                int semiColon = NextResult(sc, ';', sci);
                                jumpTo = semiColon;
                            }
                            else if (child.type.Equals("Variable"))
                            {
                                int comma = NextResult(sc, ',', sci);
                                int semiColon = NextResult(sc, ';', sci, true);
                                int equal = NextResult(sc, '=', sci);
                                jumpTo = semiColon;

                                if (comma >= 0 && comma < equal)
                                {
                                    comma = 0;
                                    //if there's multiple variables declared, separate them
                                    while (comma >= 0 && comma < child.name.Length)
                                    {
                                        if (System.DateTime.UtcNow - parseStartTime > new System.TimeSpan(0, 0, 10))
                                            throw new System.Exception("File is too big or syntax is unknown.");
                                        if (!activeJob) throw new System.Exception("Dependency parsing stopped manually.");

                                        DataElement copy = child.GetCopy();
                                        //find next comma, excluding braces
                                        int nextComma = NextResult(child.name, ',', comma);
                                        if (nextComma >= 0 && nextComma < child.name.Length)
                                            copy.name = child.name.Substring(comma, nextComma - comma - 1).Trim();
                                        else
                                        {
                                            copy.name = child.name[comma..copy.name.Length].Trim();
                                        }
                                        element.children.Add(copy);
                                        comma = nextComma;
                                    }
                                }
                                else
                                {
                                    element.children.Add(child);
                                }

                                if (jumpTo < 0)
                                    jumpTo = sc.Length;
                            }
                            else if (child.type.Equals("Method") || child.type.Equals("Operator"))
                            {
                                //lambda expression might not have parenthesis
                                int parStart = NextResult(sc, ')', sci);
                                int firstSemi = NextResult(sc, ';', sci);
                                if (firstSemi > 0 && (parStart < 0 || firstSemi < parStart))
                                {
                                    jumpTo = firstSemi;
                                }
                                else
                                {
                                    int tmpScopeIndex = parStart;
                                    if (tmpScopeIndex < 0)
                                        tmpScopeIndex = sci;
                                    int nextB = NextResult(sc, '{', tmpScopeIndex);
                                    int nextSemi = NextResult(sc, ';', tmpScopeIndex);

                                    if ((nextB < 0 || nextSemi < nextB) && nextSemi >= 0)
                                    {
                                        jumpTo = nextSemi;
                                    }
                                    else
                                    {
                                        jumpTo = ClosingBrace(sc, '{', '}', nextB);
                                    }
                                }
                                if (child.prefix != "empty")
                                    element.children.Add(child);
                            }
                            else if (child.type.Equals("Property"))
                            {
                                int braceStart = NextResult(sc, '{', sci);
                                jumpTo = ClosingBrace(sc, '{', '}', braceStart);
                                if (NextWord(sc, jumpTo).StartsWith("="))
                                {
                                    jumpTo = NextResult(sc, ';', jumpTo);
                                }
                                element.children.Add(child);
                            }
                            else if (child.type.Equals("Constructor") || child.type.Equals("Destructor"))
                            {
                                jumpTo = ClosingBrace(sc, '{', '}', NextResult(sc, '{', NextResult(sc, ')', sci)));
                                element.children.Add(child);
                            }
                            else if (child.type.Equals("skip"))
                            {
                                //faulty string encountered, skipping ahead.
                                jumpTo++;
                            }

                            //parsing the child for dependencies
                            element.dependencies.AddRange(child.dependencies);
                            element.dependencies.AddRange(DependenciesFromCodeLine(child.objectType));
                            if (child.parameters != null && child.parameters.Count > 0)
                            {
                                for (int i = 0; i < child.parameters.Count; ++i)
                                {
                                    if (child.type.Equals("Property") && child.parameters[i].Trim().Length > 0)
                                        element.dependencies.AddRange(DependenciesFromCodeLine(child.parameters[i].Trim()[1..]));
                                    else
                                        element.dependencies.AddRange(DependenciesFromCodeLine(child.parameters[i]));
                                }
                            }
                        }

                        sci = jumpTo;
                    }
                    else
                        break;
                }
            }

            return element;
        }

        private string RemoveQuotationMarks(string s)
        {
            System.DateTime parseStartTime = System.DateTime.UtcNow;

            while (s.Contains('"') || s.Contains("'"))
            {
                if (System.DateTime.UtcNow - parseStartTime > new System.TimeSpan(0, 0, 5))
                    throw new System.Exception("File is too big or syntax is unknown.");
                if (!activeJob) throw new System.Exception("Data parsing stopped manually.");

                int qIndex = s.IndexOf('"');
                int hIndex = s.IndexOf("'");
                if (qIndex > 0 && qIndex > hIndex && s.LastIndexOf('"') > qIndex)
                {
                    string tmp = s.Substring(0, qIndex) + s[(qIndex + 1)..][(s[(qIndex + 1)..].IndexOf('"') + 1)..];
                    s = tmp;
                }
                else if (hIndex > 0 && s.LastIndexOf("'") > hIndex)
                {
                    string tmp = s.Substring(0, hIndex) + s[(hIndex + 1)..][(s[(hIndex + 1)..].IndexOf("'") + 1)..];
                    s = tmp;
                }
                else
                    return s;
            }

            return s;
        }

        private DataElement ParseVarFunc(string s, int ind, string cn, List<string> usings, System.DateTime parseStartTime)
        {
            DataElement element = new DataElement
            {
                usings = CloneList(usings)
            };

            //lambda functions handle parameter parsing separately
            bool handledParameters = false;

            //objectType
            string objectTypeWord = NextWord(s, ind);
            int nameInd = ind;
            element.fullLine = LineFromIndex(s, s.IndexOf(objectTypeWord, ind));

            if (objectTypeWord.Equals("using"))
            {
                element.type = "using";
                return element;
            }
            if ((objectTypeWord.StartsWith("~" + cn) && (objectTypeWord.Length == cn.Length + 1 || objectTypeWord[cn.Length + 1].Equals('('))))
            {
                element.name = "~" + cn;
                element.type = "Destructor";

            }
            else if (objectTypeWord.StartsWith(cn) && (NextWord(s, s.IndexOf(objectTypeWord, ind) + objectTypeWord.Length).StartsWith("(") || (objectTypeWord.Length > cn.Length && objectTypeWord[cn.Length].Equals('('))))
            {
                element.name = cn;
                element.type = "Constructor";
            }
            else
            {
                element.objectType = objectTypeWord;
                int typeIndex = s.IndexOf(objectTypeWord, ind) + objectTypeWord.Length;
                while (element.objectType.EndsWith("."))
                {
                    if (System.DateTime.UtcNow - parseStartTime > new System.TimeSpan(0, 0, 10))
                        throw new System.Exception("File is too big or syntax is unknown.");
                    if (!activeJob) throw new System.Exception("Data parsing stopped manually.");
                    objectTypeWord = NextWord(s, typeIndex);
                    element.objectType += objectTypeWord;
                    typeIndex = s.IndexOf(objectTypeWord, typeIndex) + objectTypeWord.Length;

                }

                //name
                nameInd = s.IndexOf(objectTypeWord, ind) + objectTypeWord.Length + 1;
                int nameEnd = s.IndexOfAny(new char[4] { '(', '=', ';', '{' }, nameInd);


                if (nameEnd - nameInd < 0) //not actually a variable or function. possibly some string of special characters
                {
                    element.name = "";
                    element.type = "skip";
                    element.objectType = "";
                    return element;
                }

                string nameWord = s[nameInd..nameEnd].Trim();
                element.name = nameWord;

                if (element.name.StartsWith("operator") && (element.name.Length == 8 || char.IsWhiteSpace(element.name[8]) || char.IsSeparator(element.name[8]) || "+-!~*/%&|^<>=".Contains(element.name[8].ToString())))
                {
                    //operator
                    element.type = "Operator";
                    string operatorName = s.Substring(nameInd, NextResult(s, '(', nameInd) - nameInd - 1).Trim();
                    element.name = operatorName;
                }
                else
                {
                    element.name = DropAllBrackets(element.name);
                    //type
                    int braInd = NextResult(s, '{', nameInd);
                    int parInd = NextResult(s, '(', nameInd);
                    int nextEqual = NextResult(s, '=', nameInd);
                    int nextSemiC = NextResult(s, ';', nameInd);
                    int nextEnd = nextEqual < 0 || (nextEqual > nextSemiC && nextSemiC >= 0) ? nextSemiC : nextEqual;
                    int nextExpression = s.IndexOf("=>") > 0 ? NextResult(s, "=>", nameInd, true) : -1;


                    //lambda expression
                    if (nextExpression > nameInd && nextExpression < nextSemiC && nextEqual > 0 && nextEqual < nextSemiC)
                    {
                        int parEnd = -1;
                        if (parInd >= 0 && parInd < nextExpression)
                        {
                            parEnd = ClosingBrace(s, '(', ')', parInd);
                        }

                        if (parEnd < nextSemiC && parEnd > parInd)
                        {
                            element.parameters = ParseParameters(s.Substring(parInd, parEnd - parInd - 1));
                        }
                        else if (nextEqual < nextExpression)
                        {
                            element.parameters = new List<string>() { NextWord(s, nextEqual + 1) };
                        }

                        //finding dependencies from the content itself
                        if (braInd < nextSemiC && braInd > 0)
                        {
                            int contentEnd = ClosingBrace(s, '{', '}', braInd);
                            if (braInd < contentEnd)
                            {
                                string content = RemoveQuotationMarks(s[braInd..contentEnd]).Trim();
                                string[] contentLines = content.Split(';');
                                for (int i = 0; i < contentLines.Length; ++i)
                                {
                                    element.dependencies.AddRange(DependenciesFromCodeLine(contentLines[i].Trim()));
                                }
                            }
                        }

                        handledParameters = true;
                        element.type = "Method";
                    }

                    //variable
                    if (element.type.Length < 1 && nextEnd >= 0 && (nextEnd < parInd || parInd < 0) && (nextEnd < braInd || braInd < 0))
                    {
                        element.type = "Variable";
                    }
                    //method
                    else if (element.type.Length < 1 && parInd >= 0 && (parInd < braInd || braInd < 0))
                    {
                        element.type = "Method";

                        int scopeStart = braInd;
                        if ((nextSemiC > scopeStart || nextSemiC < 0) && scopeStart >= 0)
                        {
                            //not an abstract method
                            int scopeEnd = ClosingBrace(s, '{', '}', braInd) - 1;
                            if (scopeEnd < scopeStart)
                                throw new System.Exception("Couldn't find end of method " + element.name);
                        }

                    }
                    //Property
                    else if (element.type.Length < 1)
                    {
                        element.type = "Property";

                        //gets and sets
                        int scopeStart = braInd + 1;
                        int scopeEnd = ClosingBrace(s, '{', '}', braInd) - 1;
                        if (scopeEnd < scopeStart)
                            throw new System.Exception("Couldn't find end of property " + element.name);
                        string scope = s[scopeStart..scopeEnd];


                        //finding get or set
                        int getInd = scope.IndexOf("get");
                        int setInd = scope.IndexOf("set");
                        int initInd = scope.IndexOf("init");
                        if ((getInd < setInd || setInd < 0) && (getInd < initInd || initInd < 0) && getInd >= 0)
                        {
                            element.parameters.Add("{ " + scope.Substring(0, getInd + 3).Trim() + ";");
                            int getBrace = scope.IndexOf('{', scope.IndexOf("get"));
                            int getSemi = scope.IndexOf(';', scope.IndexOf("get"));
                            string afterGet = "";
                            if (getSemi >= 0 && (getBrace < 0 || getSemi < getBrace))
                            {
                                afterGet = scope[(getSemi + 1)..];
                            }
                            else
                            {
                                afterGet = scope[ClosingBrace(scope, '{', '}', getBrace + 1)..];
                            }

                            setInd = afterGet.IndexOf("set");
                            initInd = afterGet.IndexOf("init");
                            if (setInd < 0 && initInd < 0)
                            {
                                element.parameters.Add(" }");
                            }
                            else if (setInd >= 0 && (setInd < initInd || initInd < 0))
                            {
                                element.parameters.Add(" " + afterGet.Substring(0, setInd + 3).Trim() + "; }");
                            }
                            else
                            {
                                element.parameters.Add(" " + afterGet.Substring(0, initInd + 4).Trim() + "; }");
                            }

                        }
                        else if ((setInd < getInd || getInd < 0) && (setInd < initInd || initInd < 0) && setInd >= 0)
                        {
                            element.parameters.Add("{ " + scope.Substring(0, setInd + 3).Trim() + ";");
                            int getBrace = scope.IndexOf('{', scope.IndexOf("set"));
                            int getSemi = scope.IndexOf(';', scope.IndexOf("set"));
                            string afterSet = "";
                            if (getSemi >= 0 && (getBrace < 0 || getSemi < getBrace))
                            {
                                afterSet = scope[(getSemi + 1)..];
                            }
                            else
                            {
                                afterSet = scope[ClosingBrace(scope, '{', '}', getBrace + 1)..];
                            }

                            getInd = afterSet.IndexOf("get");
                            if (getInd < 0)
                            {
                                element.parameters.Add(" }");
                            }
                            else
                            {
                                element.parameters.Add(" " + afterSet.Substring(0, getInd + 3).Trim() + "; }");
                            }

                        }
                        else if (initInd >= 0)
                        {
                            element.parameters.Add("{ " + scope.Substring(0, initInd + 4).Trim() + ";");
                            int getBrace = scope.IndexOf('{', scope.IndexOf("init"));
                            int getSemi = scope.IndexOf(';', scope.IndexOf("init"));
                            string afterInit = "";
                            if (getSemi >= 0 && (getBrace < 0 || getSemi < getBrace))
                            {
                                afterInit = scope[(getSemi + 1)..];
                            }
                            else
                            {
                                afterInit = scope[ClosingBrace(scope, '{', '}', getBrace + 1)..];
                            }

                            getInd = afterInit.IndexOf("get");
                            if (getInd < 0)
                            {
                                element.parameters.Add(" }");
                            }
                            else
                            {
                                element.parameters.Add(" " + afterInit.Substring(0, getInd + 3).Trim() + "; }");
                            }

                        }

                    }
                }

            }

            if (element.type.Equals("Method") || element.type.Equals("Operator") || element.type.Equals("Constructor") || element.type.Equals("Destructor"))
            {

                //parameters (lambda expressions are already handled)
                if (!handledParameters)
                {
                    int parInd = s.IndexOf('(', nameInd);
                    string parameters = s.Substring(parInd + 1, ClosingBrace(s, '(', ')', parInd + 1) - parInd - 2);
                    if (parameters.Length > 1)
                        element.parameters = ParseParameters(parameters);

                    //finding dependencies from the content itself
                    int contentStart = NextResult(s, '{', ClosingBrace(s, '(', ')', parInd + 1));
                    if (contentStart > 0)
                    {
                        int contentEnd = ClosingBrace(s, '{', '}', contentStart);
                        if (contentStart < contentEnd)
                        {
                            string content = RemoveQuotationMarks(s[contentStart..contentEnd]).Trim();
                            string[] contentLines = content.Split(';');
                            for (int i = 0; i < contentLines.Length; ++i)
                            {
                                element.dependencies.AddRange(DependenciesFromCodeLine(contentLines[i].Trim()));
                            }
                        }
                    }
                }

            }

            //prefix ("accessibility")
            string prefix = "";
            int lastSemi = LastResult(s, ';', ind);
            int lastBrE = LastResult(s, '}', ind);
            int lastBrS = LastResult(s, '{', ind);
            int prefixStart = Mathf.Max(lastSemi, lastBrE, lastBrS, 0);
            prefix = s[prefixStart..ind].Trim();

            //prefix includes [...] attributes
            while (prefix.Length > 0 && prefix[0].Equals('['))
            {
                if (System.DateTime.UtcNow - parseStartTime > new System.TimeSpan(0, 0, 10))
                    throw new System.Exception("File is too big or syntax is unknown.");
                if (!activeJob) throw new System.Exception("Dependency parsing stopped manually.");

                int attrEnd = ClosingBrace(prefix, '[', ']', 1);
                if (attrEnd > 0)
                {
                    element.attributeField += prefix.Substring(0, attrEnd).Trim() + "\n";
                    prefix = prefix[attrEnd..].Trim();
                }
                else
                {
                    break;
                }
            }
            if (element.attributeField.Length > 0)
                element.attributeField = element.attributeField[0..^1];

            element.prefix = prefix;

            return element;
        }

        private List<string> ParseParameters(string parameters)
        {
            System.DateTime parseStartTime = System.DateTime.UtcNow;

            List<string> paramList = new List<string>();

            //no parameters
            string trimmedParams = parameters.Trim();
            if (trimmedParams.Length < 1 || (trimmedParams.Length == 1 && (trimmedParams[0] == ')' || trimmedParams[0] == '(')) || trimmedParams.Equals("()"))
            {
                return paramList;
            }

            int paramStart = 0;
            for (int i = 0; i < parameters.Length; ++i)
            {
                if (parameters[i].Equals('"'))
                {
                    bool noEscapes = false;
                    if (i - 2 >= 0 && parameters[i - 1].Equals('@'))
                        noEscapes = true;
                    ++i;
                    while (i + 1 < parameters.Length && (!parameters[i].Equals('"') || (noEscapes && parameters[i].Equals('"') && parameters[i + 1].Equals('"'))))
                    {
                        if (System.DateTime.UtcNow - parseStartTime > new System.TimeSpan(0, 0, 5))
                            throw new System.Exception("File is too big or syntax is unknown.");
                        if (!activeJob) throw new System.Exception("Dependency parsing stopped manually.");

                        if (noEscapes && parameters[i].Equals('"') && parameters[i + 1].Equals('"'))
                        {
                            i += 2;
                            continue;
                        }

                        //whatever is escaped, we skip it
                        if (!noEscapes && parameters[i].Equals('\\'))
                        {
                            i += 2;
                        }
                        else
                            i++;
                    }
                    continue;
                }
                if (parameters[i].Equals("'"[0]))
                {

                    ++i;
                    while (!parameters[i].Equals("'"[0]))
                    {
                        if (System.DateTime.UtcNow - parseStartTime > new System.TimeSpan(0, 0, 5))
                            throw new System.Exception("File is too big or syntax is unknown.");
                        if (!activeJob) throw new System.Exception("Dependency parsing stopped manually.");

                        //whatever is escaped, we skip it
                        if (parameters[i].Equals('\\'))
                        {
                            i += 2;
                        }
                        else
                            i++;
                    }
                    continue;
                }
                if (parameters[i].Equals('<'))
                {
                    int closing = ClosingBrace(parameters, '<', '>', i + 1);
                    if (closing > i)
                        i = closing;
                    else
                        return paramList;

                    --i;
                    continue;
                }
                if (parameters[i].Equals('['))
                {
                    int closing = ClosingBrace(parameters, '[', ']', i + 1);
                    if (closing > i)
                        i = closing;
                    else
                        return paramList;

                    --i;
                    continue;
                }
                if (parameters[i].Equals('{'))
                {
                    int closing = ClosingBrace(parameters, '{', '}', i + 1);
                    if (closing > i)
                        i = closing;
                    else
                        return paramList;

                    --i;
                    continue;
                }
                if (parameters[i].Equals(','))
                {
                    paramList.Add(parameters[paramStart..i].Trim());
                    paramStart = i + 1;
                }
            }
            //last parameter

            if (parameters.Length > 0)
            {
                paramList.Add(parameters[paramStart..].Trim());
            }
            return paramList;
        }

        private string ParseComment(string s, int ind)
        {
            System.DateTime parseStartTime = System.DateTime.UtcNow;

            string comment = "";
            string summary = "";
            string returns = "";
            //find the scope (until something else than a ///-comment)
            int i = ind;
            while (i < s.Length && i >= 0 && NextWord(s, i).StartsWith("///"))
            {
                if (System.DateTime.UtcNow - parseStartTime > new System.TimeSpan(0, 0, 5))
                    throw new System.Exception("File is too big or syntax is unknown.");
                if (!activeJob) throw new System.Exception("Dependency parsing stopped manually.");

                i = s.IndexOf('\n', i) + 1;
            }

            if (i < 0)
                return comment;

            string sc = s[ind..i];

            int nextTag = sc.IndexOf('<');
            while (nextTag >= 0 && nextTag < sc.Length - 1)
            {
                if (System.DateTime.UtcNow - parseStartTime > new System.TimeSpan(0, 0, 5))
                    throw new System.Exception("File is too big or syntax is unknown.");
                if (!activeJob) throw new System.Exception("Dependency parsing stopped manually.");

                int tagEnd = sc.IndexOf('>', nextTag);
                int tagStart = sc.IndexOf('<', nextTag + 1);
                if (tagEnd < nextTag || (tagStart >= 0 && tagStart < tagEnd))
                {
                    //broken tag, aborting
                    comment = "";
                    break;
                }

                string tagContent = sc.Substring(nextTag + 1, tagEnd - nextTag - 1).Trim();
                string tagName = NextWord(tagContent, 0).Trim();

                int quickClose = sc.IndexOf("/>", nextTag);

                string tagExtra = "";
                if (quickClose >= 0 && tagEnd > quickClose)
                {
                    //single-tag attribute. useless on it's own
                    nextTag = sc.IndexOf('<', tagEnd);
                    continue;
                }

                int longClose = sc.IndexOf("</" + tagName, nextTag);

                if (longClose < 0)
                {
                    //broken tag, couldn't find closing, aborting

                    comment = "";
                    break;
                }

                int closingTagEnd = sc.IndexOf('>', longClose);
                int nextTagStart = sc.IndexOf('<', longClose + 1);
                if (closingTagEnd < 0 || (nextTagStart >= 0 && nextTagStart < tagEnd))
                {
                    //broken tag, aborting
                    comment = "";
                    break;
                }

                string endTagContent = sc.Substring(longClose + 1, closingTagEnd - longClose - 1).Trim();

                if (!endTagContent.Equals("/" + tagName))
                {
                    //broken tag, aborting
                    comment = "";
                    break;
                }

                if (!tagName.Length.Equals(tagContent))
                    tagExtra = tagContent[(tagContent.IndexOf(tagName) + tagName.Length)..].Trim();


                int endingTag = sc.IndexOf('<', closingTagEnd);

                string content = sc.Substring(tagEnd + 1, longClose - 1 - tagEnd);

                //eliminating extra comment line beginnings
                int prevBreak = 0;
                while (content.Contains("///"))
                {
                    if (System.DateTime.UtcNow - parseStartTime > new System.TimeSpan(0, 0, 5))
                        throw new System.Exception("File is too big or syntax is unknown.");
                    if (!activeJob) throw new System.Exception("Dependency parsing stopped manually.");

                    int breakStart = content.IndexOf('\n');
                    if (breakStart < 0)
                        break;
                    int breakEnd = content.IndexOf("///", breakStart) + 3;
                    if (breakEnd > breakStart && breakStart >= 0)
                    {
                        string tmpContent = content;
                        content = "";
                        if (breakStart != 0)
                            content = tmpContent.Substring(0, breakStart) + " ";
                        if (breakEnd < tmpContent.Length - 1)
                            content += tmpContent[breakEnd..];
                    }
                    else
                    {
                        break;
                    }
                }

                while (content.Contains("\n"))
                    content = content.Replace('\n', ' ');

                while (content.Contains("  "))
                    content = content.Replace("  ", " ");

                //eliminating child tags inside the content
                List<int> paraPlaces = new List<int>();
                while (content.Contains("<"))
                {
                    if (System.DateTime.UtcNow - parseStartTime > new System.TimeSpan(0, 0, 5))
                        throw new System.Exception("File is too big or syntax is unknown.");
                    if (!activeJob) throw new System.Exception("Dependency parsing stopped manually.");

                    int breakStart = content.IndexOf('<');
                    int breakEnd = content.IndexOf(">", breakStart) + 1;
                    if (breakEnd > breakStart && breakStart >= 0)
                    {
                        prevBreak = breakStart;
                        string childTag = content.Substring(breakStart + 1, breakEnd - breakStart - 2).Trim();

                        if (childTag.Equals("/para") || childTag.Equals("para/") || childTag.Equals("para"))
                        {
                            if (!paraPlaces.Contains(breakStart))
                                paraPlaces.Add(breakStart);
                        }
                        if (childTag[^1].Equals('/') && (NextWord(childTag, 0).Equals("see") || NextWord(childTag, 0).Equals("seealso")))
                        {
                            //seealso or see -tag
                            childTag = childTag[childTag.IndexOf(" ")..];
                            childTag = childTag[0..^1];

                            string[][] attributes = ParseAttributes(childTag);
                            if (attributes != null && attributes.Length > 0 && attributes[0] != null)
                            {
                                for (int k = 0; k < attributes[0].Length; ++k)
                                {
                                    if (attributes[0][k].Equals("cref"))
                                    {
                                        string newContent = attributes[1][k];
                                        content = content.Insert(breakStart, newContent);
                                        breakStart += newContent.Length;
                                        breakEnd += newContent.Length;
                                        break;
                                    }
                                }
                            }
                        }
                        content = content.Remove(breakStart, breakEnd - breakStart);
                    }
                    else
                    {
                        break;
                    }
                }

                int lastIndex = 0;
                int placed = 0;
                for (int k = 0; k < paraPlaces.Count; ++k)
                {
                    if (content[lastIndex..(paraPlaces[k] + placed * 2)].Trim().Length > 0)
                    {
                        content = content.Insert(paraPlaces[k] + placed * 2, "\n\n");
                        lastIndex = paraPlaces[k] + placed * 2;
                        placed++;
                    }
                }

                while (content.Contains("\n "))
                {
                    content = content.Replace("\n ", "\n");
                }

                switch (tagName)
                {
                    case "summary":
                        summary = content.Trim();
                        break;

                    case "returns":
                        if (content.Trim().Length > 0)
                            returns = "Returns: " + content.Trim();
                        break;

                    case "param":
                        //extra should contain the name
                        string[][] attributes = ParseAttributes(tagExtra);
                        if (attributes != null && attributes.Length > 0 && attributes[0] != null)
                        {
                            for (int k = 0; k < attributes[0].Length; ++k)
                            {
                                if (attributes[0][k].Equals("name"))
                                {
                                    if (content.Trim().Length > 0)
                                        comment += attributes[1][k] + ": " + content.Trim() + "\n";
                                    break;
                                }
                            }
                        }
                        break;
                }

                nextTag = nextTagStart;
            }

            if (comment.Length > 0)
                comment = comment[0..^1];

            if (summary != "" && comment != "")
                comment = summary + "\n\n" + comment;
            else
                comment = summary + comment;

            if (comment != "" && returns != "")
                comment = comment + "\n\n" + returns;
            else
                comment += returns;

            return comment;
        }

        private string TrimBrackets(string s, int start, int end)
        {
            int trimmed = 0;
            int sqrCount = 0;
            int jpCount = 0;
            for (int i = start; i <= end - trimmed; ++i)
            {
                if (s[i].Equals('<'))
                    jpCount++;
                if (s[i].Equals('>'))
                    jpCount--;
                if (s[i].Equals('['))
                    sqrCount++;
                if (s[i].Equals(']'))
                    sqrCount--;

                if (sqrCount > 0 || jpCount > 0)
                {
                    if (char.IsWhiteSpace(s[i]) || char.IsSeparator(s[i]))
                    {
                        s = s.Remove(i, 1);
                        --i;
                        ++trimmed;
                    }
                }
            }

            return s;
        }

        private string DropAllBrackets(string s)
        {
            for (int i = 0; i < s.Length; ++i)
            {
                if (s[i].Equals('('))
                {
                    int len = ClosingBrace(s, '(', ')', i + 1) - i;
                    if (len < 0)
                        return s;
                    s = s.Remove(i, len);
                    --i;
                }

                if (s[i].Equals('<'))
                {
                    int len = ClosingBrace(s, '<', '>', i + 1) - i;
                    if (len < 0)
                        return s;
                    s = s.Remove(i, len);
                    --i;
                }

                if (s[i].Equals('['))
                {
                    int len = ClosingBrace(s, '[', ']', i + 1) - i;
                    if (len < 0)
                        return s;
                    s = s.Remove(i, len);
                    --i;
                }
            }

            return s;
        }

        private string[][] ParseAttributes(string s)
        {
            System.DateTime parseStartTime = System.DateTime.UtcNow;

            string[][] attributes = new string[2][];
            List<string> names = new List<string>();
            List<string> values = new List<string>();

            int ind = 0;
            while (ind >= 0 && ind < s.Length)
            {
                if (System.DateTime.UtcNow - parseStartTime > new System.TimeSpan(0, 0, 5))
                    throw new System.Exception("File is too big or syntax is unknown.");
                if (!activeJob) throw new System.Exception("Dependency parsing stopped manually.");

                int nameEnd = s.IndexOf('=', ind);

                if (nameEnd < 0 || nameEnd >= s.Length)
                    return null;

                string name = s[ind..nameEnd].Trim();
                int dquotInd = s.IndexOf('"', nameEnd);
                int squotInd = s.IndexOf("'", nameEnd);
                if (dquotInd < 0 && squotInd < 0)
                    return null;

                int qStart = 0;
                int qEnd = 0;
                if ((squotInd < 0 || dquotInd < squotInd) && dquotInd >= 0)
                {
                    qStart = dquotInd;
                    qEnd = s.IndexOf('"', dquotInd + 1);
                }
                else
                {
                    qStart = squotInd;
                    qEnd = s.IndexOf("'", squotInd + 1);
                }

                if (qEnd < qStart)
                    return null;

                string value = s.Substring(qStart + 1, qEnd - qStart - 1);

                names.Add(name);
                values.Add(value);
                ind = qEnd + 1;
            }

            attributes[0] = names.ToArray();
            attributes[1] = values.ToArray();

            return attributes;
        }

        private bool IsModifier(string s)
        {
            for (int i = 0; i < modifiers.Length; ++i)
            {
                if (modifiers[i].Equals(s))
                    return true;
            }
            return false;
        }

        private int LastResult(string s, char b, int ind)
        {
            System.DateTime parseStartTime = System.DateTime.UtcNow;

            if (ind < 0 || !s.Substring(0, ind).Contains(b + ""))
                return -1;

            int end = ind;
            for (int i = end; i >= 0; --i)
            {
                if (System.DateTime.UtcNow - parseStartTime > new System.TimeSpan(0, 0, 5))
                    return end;

                if (s[i].Equals(b))
                {
                    end = i;
                    break;
                }

                //skip quotes
                if (i >= 0 && s[i].Equals('"'))
                {
                    i--;
                    while (i >= 0 && s.Substring(i, 2) != "//" && (!s[i].Equals('"') || (i > 0 && (s[i - 1].Equals('\\')))))
                    {
                        if (System.DateTime.UtcNow - parseStartTime > new System.TimeSpan(0, 0, 5))
                            throw new System.Exception("File is too big or syntax is unknown.");
                        if (!activeJob) throw new System.Exception("Dependency parsing stopped manually.");

                        i--;
                    }
                    continue;
                }
                if (i >= 0 && s[i].Equals("'"[0]))
                {
                    i--;
                    while (i >= 0 && s.Substring(i, 2) != "//" && (!s[i].Equals("'"[0]) || (i > 0 && (s[i - 1].Equals('\\')))))
                    {
                        if (System.DateTime.UtcNow - parseStartTime > new System.TimeSpan(0, 0, 5))
                            throw new System.Exception("File is too big or syntax is unknown.");
                        if (!activeJob) throw new System.Exception("Dependency parsing stopped manually.");

                        i--;
                    }
                    continue;
                }
            }

            if (end == ind && s[ind] != b)
                return -1;

            return end + 1;
        }

        //returns the index AFTER the result
        private int NextResult(string s, char b, int ind, bool sb = false)
        {
            System.DateTime parseStartTime = System.DateTime.UtcNow;

            if (!s[ind..].Contains("" + b))
                return -1;

            int end = ind;
            for (int i = end; i < s.Length; ++i)
            {
                if (s[i] == b)
                {
                    end = i;
                    break;
                }

                //skip braces/brackets/parenthesis if asked
                if (sb)
                {
                    if (i < s.Length && s[i].Equals('{'))
                    {
                        i = ClosingBrace(s, '{', '}', i + 1) - 1;
                        continue;
                    }

                    if (i < s.Length && s[i].Equals('['))
                    {
                        i = ClosingBrace(s, '[', ']', i + 1) - 1;
                        continue;
                    }

                    if (i < s.Length && s[i].Equals('('))
                    {
                        i = ClosingBrace(s, '(', ')', i + 1) - 1;
                        continue;
                    }

                    if (i < s.Length && s[i].Equals('<'))
                    {
                        i = ClosingBrace(s, '<', '>', i + 1) - 1;
                        continue;
                    }
                }

                //skip comments
                if (i < s.Length - 2 && s.Substring(i, 3).Equals("///"))
                {
                    i = s.IndexOf('\n', i);
                    continue;
                }

                //skip quotes
                if (i < s.Length && s[i].Equals('"'))
                {
                    bool noEscapes = false;
                    if (i - 2 >= 0 && s[i - 1].Equals('@'))
                        noEscapes = true;
                    i++;
                    while (i + 1 < s.Length && (!s[i].Equals('"') || (noEscapes && s[i].Equals('"') && s[i + 1].Equals('"'))))
                    {
                        if (System.DateTime.UtcNow - parseStartTime > new System.TimeSpan(0, 0, 5))
                            throw new System.Exception("File is too big or syntax is unknown.");
                        if (!activeJob) throw new System.Exception("Dependency parsing stopped manually.");

                        if (noEscapes && s[i].Equals('"') && s[i + 1].Equals('"'))
                        {
                            i += 2;
                            continue;
                        }

                        //whatever is escaped, we skip it
                        if (!noEscapes && s[i].Equals('\\'))
                        {
                            i += 2;
                        }
                        else
                            i++;
                    }
                    continue;
                }
                if (i < s.Length && s[i].Equals("'"[0]))
                {
                    i++;
                    while (i < s.Length && !s[i].Equals("'"[0]))
                    {
                        if (System.DateTime.UtcNow - parseStartTime > new System.TimeSpan(0, 0, 5))
                            throw new System.Exception("File is too big or syntax is unknown.");
                        if (!activeJob) throw new System.Exception("Dependency parsing stopped manually.");

                        //whatever is escaped, we skip it
                        if (s[i].Equals('\\'))
                        {
                            i += 2;
                        }
                        else
                            i++;
                    }
                    continue;
                }
            }

            if (end == ind && s[ind] != b)
                return -1;

            return end + 1;
        }

        //returns the index AFTER the result
        private int NextResult(string s, string r, int ind, bool sb = false)
        {
            System.DateTime parseStartTime = System.DateTime.UtcNow;

            if (!s[ind..].Contains(r))
                return -1;

            int end = ind;
            for (int i = end; i < s.Length - r.Length + 1; ++i)
            {
                for (int k = 0; k < r.Length + 1; ++k)
                {
                    if (k == r.Length)
                    {
                        return i + 1;
                    }
                    if (s[i + k] != r[k])
                        break;
                }

                //skip braces/brackets/parenthesis if asked
                if (sb)
                {
                    if (i < s.Length && s[i].Equals('{'))
                    {
                        i = ClosingBrace(s, '{', '}', i + 1);
                        if (i < 0)
                            return -1;
                        continue;
                    }

                    if (i < s.Length && s[i].Equals('['))
                    {
                        i = ClosingBrace(s, '[', ']', i + 1);
                        if (i < 0)
                            return -1;
                        continue;
                    }

                    if (i < s.Length && s[i].Equals('('))
                    {
                        i = ClosingBrace(s, '(', ')', i + 1);
                        if (i < 0)
                            return -1;
                        continue;
                    }

                    if (i < s.Length && s[i].Equals('<'))
                    {
                        i = ClosingBrace(s, '<', '>', i + 1);
                        if (i < 0)
                            return -1;
                        continue;
                    }
                }

                //skip comments
                if (i < s.Length - 2 && s.Substring(i, 3).Equals("///"))
                {
                    i = s.IndexOf('\n', i);
                    continue;
                }

                //skip quotes
                if (i < s.Length && s[i].Equals('"'))
                {
                    bool noEscapes = false;
                    if (i - 2 >= 0 && s[i - 1].Equals('@'))
                        noEscapes = true;
                    i++;
                    while (i + 1 < s.Length && (!s[i].Equals('"') || (noEscapes && s[i].Equals('"') && s[i + 1].Equals('"'))))
                    {
                        if (System.DateTime.UtcNow - parseStartTime > new System.TimeSpan(0, 0, 5))
                            throw new System.Exception("File is too big or syntax is unknown.");
                        if (!activeJob) throw new System.Exception("Dependency parsing stopped manually.");

                        if (noEscapes && s[i].Equals('"') && s[i + 1].Equals('"'))
                        {
                            i += 2;
                            continue;
                        }

                        //whatever is escaped, we skip it
                        if (!noEscapes && s[i].Equals('\\'))
                        {
                            i += 2;
                        }
                        else
                            i++;
                    }
                    continue;
                }
                if (i < s.Length && s[i].Equals("'"[0]))
                {
                    i++;
                    while (i < s.Length && !s[i].Equals("'"[0]))
                    {
                        if (System.DateTime.UtcNow - parseStartTime > new System.TimeSpan(0, 0, 5))
                            throw new System.Exception("File is too big or syntax is unknown.");
                        if (!activeJob) throw new System.Exception("Dependency parsing stopped manually.");

                        //whatever is escaped, we skip it
                        if (s[i].Equals('\\'))
                        {
                            i += 2;
                        }
                        else
                            i++;
                    }
                    continue;
                }
            }

            if (end == ind && s[ind] + "" != r)
                return -1;

            return end + 1;
        }

        //first character of s should NOT be the starting brace
        private int ClosingBrace(string s, char b, char cb, int ind)
        {
            System.DateTime parseStartTime = System.DateTime.UtcNow;

            if (ind < 0)
                return -1;

            int end = ind;
            int extraParenthesis = 0;
            for (int i = end; i < s.Length; ++i)
            {

                //skip comments
                if (i < s.Length - 2 && s.Substring(i, 3).Equals("///"))
                {
                    i = s.IndexOf('\n', i);
                }

                //skip quotes
                if (s[i].Equals('"'))
                {
                    bool noEscapes = false;
                    if (i - 1 >= 0 && s[i - 1].Equals('@'))
                        noEscapes = true;
                    i++;
                    while (i + 1 < s.Length && (!s[i].Equals('"')) || (noEscapes && s[i].Equals('"') && s[i + 1].Equals('"')))
                    {
                        if (System.DateTime.UtcNow - parseStartTime > new System.TimeSpan(0, 0, 5))
                            throw new System.Exception("File is too big or syntax is unknown.");
                        if (!activeJob) throw new System.Exception("Dependency parsing stopped manually.");

                        //special case of no escape characters with the @ symbol, but still escaping the quotation mark with double quotations
                        if (noEscapes && s[i].Equals('"') && s[i + 1].Equals('"'))
                        {
                            i += 2;
                            continue;
                        }

                        //whatever is escaped, we skip it
                        if (!noEscapes && s[i].Equals('\\'))
                        {
                            i += 2;
                        }
                        else
                            i++;

                    }
                }
                if (i < s.Length && s[i].Equals("'"[0]))
                {
                    i++;
                    while (i < s.Length && !s[i].Equals("'"[0]))
                    {
                        if (System.DateTime.UtcNow - parseStartTime > new System.TimeSpan(0, 0, 5))
                            throw new System.Exception("File is too big or syntax is unknown.");
                        if (!activeJob) throw new System.Exception("Dependency parsing stopped manually.");

                        //whatever is escaped, we skip it
                        if (s[i].Equals('\\'))
                        {
                            i += 2;
                        }
                        else
                            i++;
                    }

                }

                if (i < s.Length && s[i].Equals(b))
                    extraParenthesis++;
                else if (i < s.Length && s[i].Equals(cb))
                {
                    extraParenthesis--;
                    if (extraParenthesis < 0)
                    {
                        //found the end
                        end = i;
                        break;
                    }
                }
            }


            if (end + 1 > s.Length)
            {
                return -1;
            }

            return end + 1;
        }

        private int PrevWordIndex(string s, int ind)
        {
            int wordStart = ind;
            int length = 0;
            for (int i = ind; i >= 0; --i)
            {
                if (length < 1 && (char.IsWhiteSpace(s[i]) || char.IsSeparator(s[i])))
                    wordStart--;
                else if (char.IsWhiteSpace(s[i]) || char.IsSeparator(s[i]))
                {
                    break;
                }
                else
                {
                    length++;
                    wordStart--;
                }
            }

            return wordStart;
        }

        private string NextWord(string s, int ind)
        {
            int wordStart = ind;
            int length = 0;

            for (int i = ind; i < s.Length; ++i)
            {
                if (length < 1 && (char.IsWhiteSpace(s[i]) || char.IsSeparator(s[i])))
                    wordStart++;
                else if (char.IsWhiteSpace(s[i]) || char.IsSeparator(s[i]))
                {
                    break;
                }
                else
                {
                    length++;
                }
            }


            return s.Substring(wordStart, length);
        }

        private string RemoveComments(string s)
        {
            System.DateTime parseStartTime = System.DateTime.UtcNow;

            s = s.Replace("\r", " ");

            int index = 1;
            string c;
            while (index < s.Length)
            {
                if (System.DateTime.UtcNow - parseStartTime > new System.TimeSpan(0, 0, 5))
                    throw new System.Exception("File is too big or syntax is unknown.");
                if (!activeJob) throw new System.Exception("Dependency parsing stopped manually.");

                c = s[index - 1] + "" + s[index];

                //skip everyting inside quotes
                if (s[index - 1].Equals('"'))
                {
                    bool noEscapes = false;
                    if (index - 2 >= 0 && s[index - 2].Equals('@'))
                        noEscapes = true;

                    index++;
                    char k = s[index - 1];
                    while (index < s.Length && (!k.Equals('"') || (noEscapes && k.Equals('"') && s[index].Equals('"'))))
                    {
                        if (System.DateTime.UtcNow - parseStartTime > new System.TimeSpan(0, 0, 5))
                            throw new System.Exception("File is too big or syntax is unknown.");
                        if (!activeJob) throw new System.Exception("Data parsing stopped manually.");

                        if (noEscapes && k.Equals('"') && s[index].Equals('"'))
                        {
                            index += 2;
                            k = s[index - 1];
                            continue;
                        }

                        //whatever is escaped, we skip it
                        if (!noEscapes && k.Equals('\\'))
                        {
                            index += 2;
                        }
                        else
                            index++;
                        if (index >= s.Length - 1)
                            return s;
                        k = s[index - 1];
                    }
                    index++;
                    continue;
                }

                if (s[index - 1].Equals("'"[0]))
                {

                    index++;
                    char k = s[index - 1];
                    while (!k.Equals("'"[0]))
                    {
                        if (System.DateTime.UtcNow - parseStartTime > new System.TimeSpan(0, 0, 5))
                            throw new System.Exception("File is too big or syntax is unknown.");
                        if (!activeJob) throw new System.Exception("Data parsing stopped manually.");

                        index++;
                        if (index >= s.Length - 1)
                            return s;
                        k = s[index - 1];
                    }
                    index++;
                    continue;
                }

                //oneliners
                if (c.Equals("//"))
                {
                    if (index < s.Length - 1 && s[index + 1].Equals('/') && (LastCharacterEndOfObject(s, index - 1) || CommentInsideEnum(s, index - 2)))
                    {
                        //summary comment. skip unless last character was not start of the file or ';' or '}'
                        index = s.IndexOf('\n', index) + 1;
                        string next = NextWord(s, index - 1);
                        while (next.StartsWith("///"))
                        {
                            if (System.DateTime.UtcNow - parseStartTime > new System.TimeSpan(0, 0, 5))
                                throw new System.Exception("File is too big or syntax is unknown.");
                            if (!activeJob) throw new System.Exception("Dependency parsing stopped manually.");

                            index = s.IndexOf('\n', index) + 1;
                            next = NextWord(s, index - 1);
                        }
                        continue;
                    }
                    else
                    {
                        //remove line
                        int lineEnd = s.IndexOf('\n', index);
                        if (lineEnd >= index)
                        {
                            s = s.Remove(index - 1, lineEnd - index + 1);
                        }
                        else
                        {
                            s = s.Remove(index - 1);
                        }
                        continue;
                    }
                }

                else if (c.Equals("/*")) //multiliners
                {
                    //remove comment 
                    int lineEnd = s.IndexOf("*/", index);
                    if (lineEnd < index)
                    {
                        throw new System.Exception("Couldn't find end of a /* comment.");
                    }
                    s = s.Remove(index - 1, lineEnd - index + 3);
                    continue;
                }

                //also remove preprocessor directives
                if (c.StartsWith("\u0023"))
                {
                    //remove line
                    int lineEnd = s.IndexOf('\n', index);
                    if (lineEnd >= index)
                    {
                        s = s.Remove(index - 1, lineEnd - index + 1);
                    }
                    else
                    {
                        s = s.Remove(index - 1);
                    }
                    continue;
                }

                ++index;
            }
            return s;
        }

        private bool LastCharacterEndOfObject(string s, int ind)
        {
            System.DateTime parseStartTime = System.DateTime.UtcNow;

            ind--;
            while (ind >= 0 && !s[ind].Equals('}') && !s[ind].Equals(';') && !s[ind].Equals('{') && !s[ind].Equals(']'))
            {
                if (System.DateTime.UtcNow - parseStartTime > new System.TimeSpan(0, 0, 5))
                    throw new System.Exception("File is too big or syntax is unknown.");
                if (!activeJob) throw new System.Exception("Dependency parsing stopped manually.");

                if (char.IsWhiteSpace(s[ind]) || char.IsSeparator(s[ind]))
                {
                    ind--;
                    continue;
                }
                return false;
            }

            // ] is a special case, only works with attributes, not arrays for example
            if (ind >= 0 && s[ind].Equals(']'))
            {
                int sqrStart = LastResult(s, '[', ind);
                if (sqrStart > 0)
                {
                    return LastCharacterEndOfObject(s, sqrStart - 1);
                }
            }

            return true;
        }

        private bool CommentInsideEnum(string s, int ind)
        {
            System.DateTime parseStartTime = System.DateTime.UtcNow;

            if (ind >= 0 && s.Length > ind)
            {
                while (ind >= 0 && !s[ind].Equals(',') && !s[ind].Equals('{'))
                {
                    if (System.DateTime.UtcNow - parseStartTime > new System.TimeSpan(0, 0, 5))
                        throw new System.Exception("File is too big or syntax is unknown.");
                    if (!activeJob) throw new System.Exception("Dependency parsing stopped manually.");

                    if (char.IsWhiteSpace(s[ind]) || char.IsSeparator(s[ind]))
                    {
                        ind--;
                        continue;
                    }
                    return false;
                }
            }
            else
            {
                return false;
            }

            if (ind >= 0 && s[ind].Equals(','))
            {
                int lastBr = LastResult(s, '{', ind);

                int lastBrEnd = LastResult(s, '}', ind);
                if (lastBrEnd < lastBr && lastBr > 0)
                    ind = lastBr - 1;


            }

            if (ind >= 1 && s[ind].Equals('{'))
            {
                ind = PrevWordIndex(s, ind - 1);
                if (ind >= 1)
                {
                    ind = PrevWordIndex(s, ind - 1);
                    if (NextWord(s, ind).Equals("enum"))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private string LineFromIndex(string file, int index)
        {
            int previousLineEnd = Mathf.Max(0, LastResult(file, '\n', index));
            int lineEnd = NextResult(file, '\n', index)-2;
            if (lineEnd > previousLineEnd) 
            {
                return file[previousLineEnd..lineEnd];
            }

            return "";
        }
    }

}
#endif