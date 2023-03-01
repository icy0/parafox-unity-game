using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public class Level {

    // TODO:
    // -    Finish method "CreateDirectedEdge"                                              (DONE)
    // -    create lists for assets and their file paths (materials, game object, etc.)     (DONE)
    // -    Write these lists to the .xml file                                              (DONE)
    // -    complete writing to file                                                        (DONE)
    // -    read from file                                                                  (DONE)
    // -    implement write and read of level objects                                       (DONE)  

    public static string LEVELS_FOLDER_NAME = "Levels";

    public static string LEVEL_TAG = "level", NAME_TAG = "name", MATERIALS_PATH_TAG = "materials", BACKGROUNDS_PATH_TAG = "backgrounds",
        LEVEL_OBJECTS_PATH_TAG = "levelobjects", DE_TAG = "directed_edge", VERT_TAG = "vertices", EDGE_TAG = "edges",
        SECTIONS_TAG = "sections", SECTION_ID_TAG = "section_", TYPEINFO_TAG = "typeinfo", LEVEL_OBJECTS_TAG = "placedlevelobjects",
        LEVEL_OBJECT_ID_TAG = "lvl_obj_", EVENT_MANAGEMENT_SYSTEM_TAG = "ems", EVENT_GROUP_TAG = "eg_", EVENT_GROUP_INFORMATION_TAG = "egi",
        EVENT_GROUP_NODES_TAG = "egn", NODE_TAG = "node_";

    private static string COUNT_ATTR = "count";

    public string name { get; set; }

    public GameObject levelObject { get; set; }

    public DirectedEdgeDataStructure de { get; set; }

    public EventManagementSystem ems { get; set; }

    public List<Section> sections { get; set; }

    public List<Material> materials { get; set; }
    public List<string> materialsPaths { get; set; }

    public List<Texture2D> backgrounds { get; set; }
    public List<string> backgroundsPaths { get; set; }

    public List<Object> levelObjectAssets { get; set; }
    public List<string> levelObjectAssetsPaths { get; set; }
    public List<GameObject> levelObjects { get; set; }

    public Level(string name) {
        this.name = name;
        this.de = new DirectedEdgeDataStructure();
        this.sections = new List<Section>();
        this.levelObjects = new List<GameObject>();
        this.ems = new EventManagementSystem();
    }

    public void writeLevelToFile() {

        Debug.Log("Started writing level to .xml file. This may take a few seconds.");

        // Cleanup
         // TEST!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        this.levelObjects.RemoveAll((o) => { return o == null; });

        // Write to XML

        XMLWriter xmlHandler = new XMLWriter();

        xmlHandler.openPath(LEVEL_TAG);
            xmlHandler.openPath(NAME_TAG);
                xmlHandler.write(this.name);
            xmlHandler.closePath();
            xmlHandler.openPath(MATERIALS_PATH_TAG);
                xmlHandler.writeLines(this.materialsPaths.ToArray());
            xmlHandler.closePath();
            xmlHandler.openPath(BACKGROUNDS_PATH_TAG);
                xmlHandler.writeLines(this.backgroundsPaths.ToArray());
            xmlHandler.closePath(); 
            xmlHandler.openPath(LEVEL_OBJECTS_PATH_TAG);
                xmlHandler.writeLines(this.levelObjectAssetsPaths.ToArray());
            xmlHandler.closePath();
            xmlHandler.openPath(DE_TAG);
                xmlHandler.openPath(VERT_TAG);
                    xmlHandler.writeLines(this.de.serializeVertices());
                xmlHandler.closePath();
                xmlHandler.openPath(EDGE_TAG);
                    xmlHandler.writeLines(this.de.serializeEdges());
                xmlHandler.closePath();
            xmlHandler.closePath();
            xmlHandler.openPath(SECTIONS_TAG, new Tuple<string, string>(COUNT_ATTR, this.sections.Count.ToString()));
                for (int i = 0; i < this.sections.Count; ++i) {
                    this.writeSection(xmlHandler, this.sections[i], i);
                }
            xmlHandler.closePath();
            xmlHandler.openPath(LEVEL_OBJECTS_TAG, new Tuple<string, string>(COUNT_ATTR, this.levelObjects.Count.ToString()));
                this.writeLevelObjects(xmlHandler);
            xmlHandler.closePath();
            xmlHandler.openPath(EVENT_MANAGEMENT_SYSTEM_TAG, new Tuple<string, string>(COUNT_ATTR, this.ems.eventGroups.Count.ToString()));
                for (int i = 0; i < this.ems.eventGroups.Count; ++i) {
                    this.writeEventGroup(xmlHandler, this.ems.eventGroups[i], i);
                }
            xmlHandler.closePath();
        xmlHandler.closePath();
        string text = xmlHandler.retrieve();

        StreamWriter file;
        try {
            string fileSystemPath = Application.dataPath.Replace('/', '\\');
            fileSystemPath = fileSystemPath.Insert(2, "\\");
            file = new StreamWriter(fileSystemPath + "\\" + LEVELS_FOLDER_NAME + "\\" + this.name + ".xml", false);
        } catch {
            Debug.LogError("Could not create the levels .xml file.");
            return;
        }

        file.Write(text);
        file.Close();

        Debug.Log("Finished writing to file.");
    }

    private void writeSection(XMLWriter xmlHandler, Section s, int index) {
        xmlHandler.openPath(SECTION_ID_TAG + index.ToString()); 
            xmlHandler.write(s.name);
            xmlHandler.write(s.layer.ToString());
            xmlHandler.write(((int) s.type).ToString());
            xmlHandler.write(((int) s.id).ToString());
            this.writeSectionsTypeInformation(xmlHandler, s);
        xmlHandler.closePath();
    }

    private void writeSectionsTypeInformation(XMLWriter xmlHandler, Section s) {

        xmlHandler.openPath(TYPEINFO_TAG);
        if (s.type == SectionType.IMAGE) {
            xmlHandler.write(this.backgrounds.IndexOf(s.image).ToString());
            xmlHandler.write(s.gameObject.transform.position.x.ToString());
            xmlHandler.write(s.gameObject.transform.position.y.ToString());
            xmlHandler.write(s.gameObject.transform.localScale.x.ToString());
            xmlHandler.write(s.gameObject.transform.localScale.y.ToString());
            xmlHandler.write(s.gameObject.transform.rotation.eulerAngles.z.ToString());
        } else if (s.type == SectionType.MESH) {
            xmlHandler.write(this.materials.IndexOf(s.material).ToString());
            foreach (HalfEdge e in s.halfEdges) {
                xmlHandler.write(this.de.edges.IndexOf(e).ToString());
            }
        } else {
            Debug.LogError("A section type was not handled!");
        }
        xmlHandler.closePath();
    }

    private void writeLevelObjects(XMLWriter xmlHandler) {
        for (int i = 0; i < this.levelObjects.Count; ++i) {
            GameObject o = this.levelObjects[i];
            xmlHandler.openPath(LEVEL_OBJECT_ID_TAG + i);
                xmlHandler.write(o.GetComponent<LevelObjectScript>().serialize(this));
            xmlHandler.closePath();
        }
    }

    private void writeEventGroup(XMLWriter xmlHandler, EventGroup eg, int index) {
        xmlHandler.openPath(EVENT_GROUP_TAG + index.ToString());
            xmlHandler.openPath(EVENT_GROUP_INFORMATION_TAG);
                xmlHandler.write(eg.serialize());
            xmlHandler.closePath();
            xmlHandler.openPath(EVENT_GROUP_NODES_TAG, new Tuple<string, string>(COUNT_ATTR, eg.nodes.Count.ToString()));
                for (int i = 0; i < eg.nodes.Count; ++i) {
                    xmlHandler.openPath(NODE_TAG + i);
                        xmlHandler.write(eg.nodes[i].serialize());
                    xmlHandler.closePath();
                }
            xmlHandler.closePath();
        xmlHandler.closePath();
    }

    public static Level CreateLevelFromFile(string fileName, GameObject terrainObject, GameObject verticesGameObject, GameObject levelObjectsGameObject) {

        string fileSystemPath = Application.dataPath.Replace('/', '\\');
        fileSystemPath = fileSystemPath.Insert(2, "\\") + "\\" + LEVELS_FOLDER_NAME + "\\" + fileName + ".xml";
        Debug.Log("Loading level from file " + fileSystemPath + ". This may take a few seconds.");

        XMLReader xmlReader = new XMLReader(fileSystemPath);

        xmlReader.evaluate();

        string[] levelName = xmlReader.values[XMLReader.AssembleXPath(LEVEL_TAG, NAME_TAG)];
        Level level = new Level(levelName[0]);

        level.materialsPaths = new List<string>(xmlReader.values[XMLReader.AssembleXPath(LEVEL_TAG, MATERIALS_PATH_TAG)]);
        level.materials = GetMaterials(xmlReader.values[XMLReader.AssembleXPath(LEVEL_TAG, MATERIALS_PATH_TAG)]);
        level.backgroundsPaths = new List<string>(xmlReader.values[XMLReader.AssembleXPath(LEVEL_TAG, BACKGROUNDS_PATH_TAG)]);
        level.backgrounds= GetBackgrounds(xmlReader.values[XMLReader.AssembleXPath(LEVEL_TAG, BACKGROUNDS_PATH_TAG)]);
        level.levelObjectAssetsPaths = new List<string>(xmlReader.values[XMLReader.AssembleXPath(LEVEL_TAG, LEVEL_OBJECTS_PATH_TAG)]);
        level.levelObjectAssets = GetLevelObjects(xmlReader.values[XMLReader.AssembleXPath(LEVEL_TAG, LEVEL_OBJECTS_PATH_TAG)]);

        DEDebugger.printList("material paths: ", level.materialsPaths);
        DEDebugger.printList("background paths: ", level.backgroundsPaths);
        DEDebugger.printList("level object paths: ", level.levelObjectAssetsPaths);

        string[] vertices = xmlReader.values[XMLReader.AssembleXPath(LEVEL_TAG, DE_TAG, VERT_TAG)];
        string[] edges = xmlReader.values[XMLReader.AssembleXPath(LEVEL_TAG, DE_TAG, EDGE_TAG)];
        level.de = Level.CreateDirectedEdge(verticesGameObject, vertices, edges);

        // Sections

        if (xmlReader.attributes.ContainsKey(XMLReader.AssembleXPath(LEVEL_TAG, SECTIONS_TAG))) {
            CreateSections(terrainObject, xmlReader, level);
        } else {
            Debug.LogWarning("Level file has no section tag. Your file must be corrupt.");
        }

        // Level Objects

        if (xmlReader.attributes.ContainsKey(XMLReader.AssembleXPath(LEVEL_TAG, LEVEL_OBJECTS_TAG))) {
            CreateLevelObjects(levelObjectsGameObject, xmlReader, level);
        } else {
            Debug.LogWarning("Level file has no level objects tag. Either the file is corrupt, or was created before saving level objects to levels was implemented.");
        }

        // Event Management System

        if (xmlReader.attributes.ContainsKey(XMLReader.AssembleXPath(LEVEL_TAG, EVENT_MANAGEMENT_SYSTEM_TAG))) {
            CreateEventManagementSystem(xmlReader, level);
        } else {
            Debug.LogWarning("Level file has no event management system tag. Either the file is corrupt, or was created before the implementation of event management systems.");
        }

        Debug.Log("Finished loading level from file.");

        return level;
    }

    private static void CreateSections(GameObject terrainObject, XMLReader xmlReader, Level level) {

        Tuple<string, string>[] sectionsAttr = xmlReader.attributes[XMLReader.AssembleXPath(LEVEL_TAG, SECTIONS_TAG)];
        int sectionsCount = int.Parse(sectionsAttr[0].Item2);

        int highestSectionID = -1;

        for (int i = 0; i < sectionsCount; ++i) {

            string sectionTag = SECTION_ID_TAG + i.ToString();
            string[] sectionInformation = xmlReader.values[XMLReader.AssembleXPath(LEVEL_TAG, SECTIONS_TAG, sectionTag)];

            string sectionName = sectionInformation[0];
            int sectionLayer = int.Parse(sectionInformation[1]);
            SectionType sectionType = (SectionType) int.Parse(sectionInformation[2]);
            int sectionID = int.Parse(sectionInformation[3]);

            if (sectionID > highestSectionID) {
                highestSectionID = sectionID;
            }

            string[] typeInfo = xmlReader.values[XMLReader.AssembleXPath(LEVEL_TAG, SECTIONS_TAG, sectionTag, TYPEINFO_TAG)];

            int index = int.Parse(typeInfo[0]);
            Section s;

            if (sectionType == SectionType.IMAGE) {
                s = new Section(sectionName, terrainObject, sectionLayer, level.backgrounds[index], false);

                float xPos = float.Parse(typeInfo[1]);
                float yPos = float.Parse(typeInfo[2]);

                s.gameObject.transform.position = new Vector3(xPos, yPos, s.getLayerPos());

                try {

                    float xScale = float.Parse(typeInfo[3]);
                    float yScale = float.Parse(typeInfo[4]);

                    s.gameObject.transform.localScale = new Vector3(xScale, yScale, 1.0F);

                    float rotation = float.Parse(typeInfo[5]);

                    s.gameObject.transform.rotation = Quaternion.AngleAxis(rotation, Vector3.forward);
                } catch(Exception) {
                    Debug.LogWarning("A section of the type image did not contain information about its scale or rotation. Either the level file is corrupt, or it was created before this feature was added.");
                }

            } else if (sectionType == SectionType.MESH) {
                s = new Section(sectionName, terrainObject, sectionLayer, level.materials[index], false);

                for (int j = 1; j < typeInfo.Length; ++j) {
                    int halfEdgeIndex = int.Parse(typeInfo[j]);
                    s.halfEdges.Add(level.de.edges[halfEdgeIndex]);
                }

            } else {
                Debug.LogError("A section type was not handled!");
                continue;
            }

            s.id = sectionID;
            level.sections.Add(s);
        }

        Section.ID = highestSectionID + 1;
    }

    private static void CreateLevelObjects(GameObject levelObjectsGameObject, XMLReader xmlReader, Level level) {

        Tuple<string, string>[] levelObjectsAttr = xmlReader.attributes[XMLReader.AssembleXPath(LEVEL_TAG, LEVEL_OBJECTS_TAG)];
        int levelObjectsCount = int.Parse(levelObjectsAttr[0].Item2);
        string[] levelObjectPaths = xmlReader.values[XMLReader.AssembleXPath(LEVEL_TAG, LEVEL_OBJECTS_PATH_TAG)];

        int highestLevelObjectID = -1;

        for (int i = 0; i < levelObjectsCount; ++i) {

            string levelObjectTag = LEVEL_OBJECT_ID_TAG + i.ToString();
            string levelObjInformationLine = xmlReader.values[XMLReader.AssembleXPath(LEVEL_TAG, LEVEL_OBJECTS_TAG, levelObjectTag)][0];
            string[] levelObjectInformation = CSVHandler.read(levelObjInformationLine);

            // Create the level object game object from the given asset path
            string path = levelObjectPaths[int.Parse(levelObjectInformation[0])];

            Object levelObjectAsset = AssetDatabase.LoadAssetAtPath<Object>(path);
            GameObject levelObjectGameObject = (GameObject) GameObject.Instantiate(levelObjectAsset, Vector3.zero, Quaternion.identity, levelObjectsGameObject.transform);

            // Configure the level object based on the given information
            var levelObject = levelObjectGameObject.GetComponent(Type.GetType(levelObjectInformation[1])) as IConfigurator;
            levelObject.configureLevelObject(levelObjectInformation, level);

            int levelObjectID = int.Parse(levelObjectInformation[2]);

            level.levelObjects.Add(levelObjectGameObject);

            if (levelObjectID > highestLevelObjectID) {
                highestLevelObjectID = levelObjectID;
            }
        }

        LevelObjectScript.ID = highestLevelObjectID + 1;
    }

    private static void CreateEventManagementSystem(XMLReader xmlReader, Level level) {

        Tuple<string, string>[] emsAttr = xmlReader.attributes[XMLReader.AssembleXPath(LEVEL_TAG, EVENT_MANAGEMENT_SYSTEM_TAG)];
        int egCount = int.Parse(emsAttr[0].Item2);

        for (int i = 0; i < egCount; ++i) {
            level.ems.eventGroups.Add(CreateEventGroup(xmlReader, i, level));
        }

    }

    private static EventGroup CreateEventGroup(XMLReader xmlReader, int index, Level level) {

        string eventGroupTag = EVENT_GROUP_TAG + index.ToString();
        string eventGroupInformationLine = xmlReader.values[XMLReader.AssembleXPath(LEVEL_TAG, EVENT_MANAGEMENT_SYSTEM_TAG, eventGroupTag, EVENT_GROUP_INFORMATION_TAG)][0];
        string[] eventGroupInformation = CSVHandler.read(eventGroupInformationLine);

        EventGroup eg = new EventGroup(eventGroupInformation[0]);

        Tuple<string, string>[] egnAttr = xmlReader.attributes[XMLReader.AssembleXPath(LEVEL_TAG, EVENT_MANAGEMENT_SYSTEM_TAG, eventGroupTag, EVENT_GROUP_NODES_TAG)];
        int egnCount = int.Parse(egnAttr[0].Item2);

        for (int i = 0; i < egnCount; ++i) {
            string nodeTag = NODE_TAG + i.ToString();
            string nodeLine = xmlReader.values[XMLReader.AssembleXPath(LEVEL_TAG, EVENT_MANAGEMENT_SYSTEM_TAG, eventGroupTag, EVENT_GROUP_NODES_TAG, nodeTag)][0];

            string[] nodeInformation = CSVHandler.read(nodeLine);
            int levelObjectID = int.Parse(nodeInformation[0]);
            
            GameObject responsibleObject = null;
            foreach (GameObject o in level.levelObjects) {
                int id = o.GetComponent<LevelObjectScript>().id;
                if (id == levelObjectID) {
                    responsibleObject = o;
                    break;
                }
            }

            if (responsibleObject == null) {
                continue;
            }

            responsibleObject.GetComponent<Node>().configureNode(nodeInformation);
            Node n = responsibleObject.GetComponent<Node>();
            eg.nodes.Add(n);
        }

        for (int i = 0; i < egnCount; ++i) {
            string nodeTag = NODE_TAG + i.ToString();
            string nodeLine = xmlReader.values[XMLReader.AssembleXPath(LEVEL_TAG, EVENT_MANAGEMENT_SYSTEM_TAG, eventGroupTag, EVENT_GROUP_NODES_TAG, nodeTag)][0];

            string[] nodeInformation = CSVHandler.read(nodeLine);
            int predecessorNodeCount = int.Parse(nodeInformation[5]);
            for (int j = 0; j < predecessorNodeCount; ++j) {
                int predecessorID = int.Parse(nodeInformation[6 + j]);
                foreach (Node n in eg.nodes) {
                    if (n.GetComponent<Node>().id == predecessorID) {
                        eg.nodes[i].predecessors.Add(n);
                        break;
                    }
                }
            }
        }

        return eg;
    }

    private static List<Material> GetMaterials(string[] paths) {
        List<Material> materials = new List<Material>();

        foreach (string fileName in paths) {
            Material m = AssetDatabase.LoadAssetAtPath<Material>(fileName);
            if (m != null) {
                materials.Add(m);
            }
        }

        return materials;
    }

    private static List<Texture2D> GetBackgrounds(string[] paths) {
        List<Texture2D> backgrounds = new List<Texture2D>();

        foreach (string fileName in paths) {
            Texture2D t = AssetDatabase.LoadAssetAtPath<Texture2D>(fileName);
            if (t != null) {
                backgrounds.Add(t);
            }
        }

        return backgrounds;
    }

    private static List<Object> GetLevelObjects(string[] paths) {
        List<Object> levelObjects = new List<Object>();

        foreach (string fileName in paths) {
            GameObject o = AssetDatabase.LoadAssetAtPath<GameObject>(fileName);
            if (o != null) {
                levelObjects.Add(o);
            }
        }

        return levelObjects;
    }

    private static DirectedEdgeDataStructure CreateDirectedEdge(GameObject verticesGameObject, string[] vertices, string[] edges) {
        DirectedEdgeDataStructure de = new DirectedEdgeDataStructure();

        // Create the vertices
        List<Tuple<Vertex, int>> vertexList = new List<Tuple<Vertex, int>>();
        foreach (string vertex in vertices) {
            float[] values = CSVHandler.readFloat(vertex);
            Vector3 position = new Vector3(values[0], values[1], values[2]);
            Vertex v = de.createVertex(position, verticesGameObject);
            vertexList.Add(new Tuple<Vertex, int>(v, (int) values[3]));
        }

        // Create the half edges and initialize its start vertices
        List<Tuple<HalfEdge, int>> edgeList = new List<Tuple<HalfEdge, int>>();
        foreach (string edge in edges) {
            int[] values = CSVHandler.readInt(edge);
            HalfEdge e = new HalfEdge(vertexList[values[0]].Item1);
            edgeList.Add(new Tuple<HalfEdge, int>(e, values[1]));
            de.edges.Add(e);
        }

        // Initialize the opposites
        foreach (Tuple<HalfEdge, int> edge in edgeList) {
            if (edge.Item2 == -1) {
                continue;
            }
            edge.Item1.opposite = edgeList[edge.Item2].Item1;
        }

        // Initialize the vertices half edges
        foreach (Tuple<Vertex, int> vertex in vertexList)
        {
            vertex.Item1.halfEdge = edgeList[vertex.Item2].Item1;
        }

        return de;
    }
}

public class XMLReader {

    public List<string> lines { get; set; }

    private List<string> currentXPath;

    public Dictionary<string, string[]> values { get; set; }

    public Dictionary<string, Tuple<string, string>[]> attributes { get; set; }

    public XMLReader(string fileSystemPath) {
        this.lines = new List<string>();
        this.currentXPath = new List<string>();
        this.values = new Dictionary<string, string[]>();
        this.attributes = new Dictionary<string, Tuple<string, string>[]>();

        StreamReader streamReader = new StreamReader(fileSystemPath);

        string line;
        while ((line = streamReader.ReadLine()) != null) {
            this.lines.Add(line);
        }
        streamReader.Close();
    }


    public void evaluate() {

        List<string> currValues = new List<string>();

        foreach (string currLine in this.lines) {

            string line = currLine.Trim();

            // Is the line a start or end tag?
            if (line[0] == '<') {

                // Is it an end tag?
                if (line[1] == '/') {

                    // Retrieve all previously stored values from our values dirctionary,
                    // add all of our current values to the to the already exisitng ones,
                    // store all of them with our current XPath as our key and finally
                    // remove the last tag in our XPath list
                    List<string> localValues = new List<string>();

                    if (this.values.ContainsKey(XMLReader.AssembleXPath(this.currentXPath.ToArray()))) {
                        localValues.AddRange(this.values[XMLReader.AssembleXPath(this.currentXPath.ToArray())]);
                        this.values.Remove(XMLReader.AssembleXPath(this.currentXPath.ToArray()));
                    }

                    localValues.AddRange(currValues);
                    this.values.Add(XMLReader.AssembleXPath(this.currentXPath.ToArray()), localValues.ToArray());

                    currValues.Clear();
                    this.currentXPath.RemoveAt(this.currentXPath.Count - 1);

                } else {

                    // Store all previously acquired values in our values dictionary,
                    // add the tag to our XPath list, search for attributes and add
                    // them to the attributes dictionary
                    List<string> localValues = new List<string>();

                    if (this.values.ContainsKey(XMLReader.AssembleXPath(this.currentXPath.ToArray()))) {
                        localValues.AddRange(this.values[XMLReader.AssembleXPath(this.currentXPath.ToArray())]);
                        this.values.Remove(XMLReader.AssembleXPath(this.currentXPath.ToArray()));
                    }

                    localValues.AddRange(currValues);
                    this.values.Add(XMLReader.AssembleXPath(this.currentXPath.ToArray()), localValues.ToArray());

                    this.currentXPath.Add(this.readTag(line, 1));
                    Tuple<string, string>[] currAttributes = this.readAttributes(line);
                    this.attributes.Add(XMLReader.AssembleXPath(this.currentXPath.ToArray()), currAttributes);
                    currValues.Clear();
                }

            } else {

                // Not a tag so we can just add it to our current values list
                currValues.Add(line);
            }
        }
    }

    private string readTag(string line, int startIndex) {

        string word = "";
        int counter = startIndex;
        while(line[counter] != ' ' && line[counter] != '\n' && line[counter] != '\t' && line[counter] != '>') {
            word += line[counter];
            counter++;
        }

        return word;
    }

    private Tuple<string, string>[] readAttributes(string line) {

        List<Tuple<string, string>> attributes = new List<Tuple<string, string>>();

        // NEEDS TO BE TESTES
        string[] readWords = line.Split('<', ' ', '=', '\"', '>');
        List<string> actualWords = new List<string>();

        foreach(string word in readWords) {
            if (!word.Trim().Equals("")) {
                actualWords.Add(word.Trim());
            }
        }

        for (int i = 1; i < actualWords.Count; i += 2) {
            attributes.Add(new Tuple<string, string>(actualWords[i], actualWords[i + 1]));
        }

        return attributes.ToArray();
    }

    public static string AssembleXPath(params string[] xPathTags) {
        string xPath = "";

        foreach (string s in xPathTags) {
            xPath += s + "/";
        }

        if (xPath.Length > 0) {
            xPath = xPath.Substring(0, xPath.Length - 1);
        }
        
        return xPath;
    }
}

public class XMLWriter {

    private List<string> currentXPath;

    private string text;

    private int tabIndex;

    public XMLWriter() {
        this.currentXPath = new List<string>();
        this.text = "";
        this.tabIndex = 0;
    }

    public void openPath(string path, params Tuple<string, string>[] attributes) {
        this.currentXPath.Add(path);
        for (int i = 0; i < this.tabIndex; ++i) {
            this.text += "\t";
        }
        this.text += "<" + path;

        foreach (Tuple<string, string> attr in attributes) {
            this.text += " " + attr.Item1 + "=\"" + attr.Item2 + "\"";
        }

        this.text += ">\n";
        this.tabIndex++;
    }

    public void write(string line) {
        for (int i = 0; i < this.tabIndex; ++i) {
            this.text += "\t";
        }
        this.text += line + "\n";
    }

    public void writeLines(string[] lines) {
        foreach (string line in lines) {
            this.write(line);
        }
    }

    public void closePath() {
        if (this.currentXPath.Count > 0) {
            this.tabIndex--;
            for (int i = 0; i < this.tabIndex; ++i) {
                this.text += "\t";
            }
            this.text += "</" + this.currentXPath[this.currentXPath.Count - 1] + ">\n";
            this.currentXPath.RemoveAt(this.currentXPath.Count - 1);
        }
    }

    public string retrieve() {
        while (this.currentXPath.Count > 0) {
            this.closePath();
        }

        return this.text;
    }
}

public class CSVHandler {

    public static string[] read(string line) {

        List<string> readValues = new List<string>();
        string word = "";
        for (int i = 0; i < line.Length; ++i) {
            if (line[i] == ';') {
                readValues.Add(word);
                word = "";
            } else {
                word += line[i];
            }
        }

        return readValues.ToArray();
    }

    public static float[] readFloat(string line) {

        List<float> readValues = new List<float>();
        string word = "";
        for (int i = 0; i < line.Length; ++i) {
            if (line[i] == ';') {
                readValues.Add(float.Parse(word, CultureInfo.InvariantCulture));
                word = "";
            } else {
                word += line[i];
            }
        }

        return readValues.ToArray();
    }

    public static int[] readInt(string line) {

        List<int> readValues = new List<int>();
        string word = "";
        for (int i = 0; i < line.Length; ++i) {
            if (line[i] == ';') {
                readValues.Add(int.Parse(word));
                word = "";
            } else {
                word += line[i];
            }
        }

        return readValues.ToArray();
    }

    public static string write<T>(T[] values) {
        string line = "";
        foreach (T value in values) {
            line += value.ToString().Trim() + ";";
        }

        return line;
    }
}
