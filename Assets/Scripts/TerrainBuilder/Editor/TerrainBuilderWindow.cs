using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using Object = UnityEngine.Object;

public class TerrainBuilderWindow : EditorWindow {

    // TODO:
    //  - DOCUMENTATION!!!!!!!!
    //  - Placing objects into levels                                   (DONE)
    //  - Implement Effector Affected Framework                         (DONE)
    //  - UI: Asset Picker for materials/textures/objects               (DONE)
    //  - Move image sections from window                               (DONE)
    //  - Make split vertex multi section compatible
    //  - Implement join vertex event
    //  - Maybe rename levelObjects related stuff to disambigue

    public static TerrainBuilderWindow instance;
    public static string MATERIALS_FOLDER_NAME = "LevelAssets/Materials";
    public static string BACKGROUNDS_FOLDER_NAME = "LevelAssets/Backgrounds";
    public static string LEVEL_OBJECTS_FOLDER_NAME = "LevelAssets/LevelObjects";

    private TerrainBuilderState state;

    private List<string> levelNames;

    private Level level;
    private string levelName;
    private List<Material> materials;
    private List<string> materialsPaths;
    private List<Texture2D> backgrounds;
    private List<string> backgroundsPaths;
    private List<Object> levelObjectAssets;
    private List<string> levelObjectAssetsPaths;
    private List<GameObject> levelObjects;

    private Vector2 materialsScrollPos;
    private Vector2 backgroundsScrollPos;
    private Vector2 levelObjectsScrollPos;

    private int levelSelectedIndex;

    private GameObject levelObject;
    private GameObject verticesParent;
    private GameObject levelObjectsParent;

    private List<Section> sections;
    public DirectedEdgeDataStructure de;

    public Section currentSection { get; set; }

    private string sectionName;
    private SectionType sectionType;
    private Material sectionMaterial;
    private Texture2D sectionImage;
    private int sectionLayer;

    private int sectionSelectedIndex;

    private bool easyInstancing;
    private bool hideOtherSections;
    private bool debugVerticesActive;
    private bool debugEdgesActive;
    private bool debugOutlinesActive;

    private Object selectedLevelObject;
    private int objectLayer;
    private bool objectPlacementMode;

    int eventGroupSelectedIndex;
    string eventGroupName;
    EventGroup selectedEventGroup;

    [MenuItem("Window/Terrain Builder")]
    public static void ShowWindow() {
        TerrainBuilderWindow.instance = GetWindow<TerrainBuilderWindow>("Terrain Builder");
    }

    public void OnEnable() {

        this.resetAll();
        VertexScript.init();

        this.levelNames = this.searchForExistingLevels();
        this.searchForMaterials(out this.materials, out this.materialsPaths);
        this.searchForBackgrounds(out this.backgrounds, out this.backgroundsPaths);
        this.searchForLevelObjects(out this.levelObjectAssets, out this.levelObjectAssetsPaths);
    }

    #region RESETS & CLEANUP

    private void resetAll() {

        LevelObjectScript.ID = 0;
        Node.ID = 0;

        this.level = null;
        this.levelObject = null;
        this.levelName = "";
        this.levelSelectedIndex = -1;
        this.eventGroupSelectedIndex = -1;

        this.materialsScrollPos = new Vector2(0, 0);
        this.backgroundsScrollPos = new Vector2(0, 0);
        this.levelObjectsScrollPos = new Vector2(0, 0);

        this.state = TerrainBuilderState.INITIAL;
        this.sections = null;
        this.currentSection = null;

        this.resetSectionRelated();

        this.resetSectionBuildingFlags();

        this.resetLevelObjectEditingRelated();

        this.resetEventSystemRelated();

        this.levelNames = this.searchForExistingLevels();
    }

    private void resetSectionBuildingFlags() {
        this.easyInstancing = false;
        this.debugVerticesActive = false;
        this.debugEdgesActive = false;
        this.hideOtherSections = false;
    }

    private void resetLevelObjectEditingRelated() {

        this.selectedLevelObject = null;
        this.objectLayer = 0;
        this.objectPlacementMode = false;
    }

    private void resetSectionRelated() {

        this.sectionName = "";
        this.sectionSelectedIndex = 0;
        this.sectionMaterial = null;
        this.sectionImage = null;
        this.sectionLayer = 0;
    }

    private void resetEventSystemRelated() {
        this.eventGroupName = "";
        this.selectedEventGroup = null;
        this.nodeName = "";
    }

    private void resetEventGroupRelated() {
        this.eventGroupName = "";
    }

    private void cleanup() {

        Section.ID = 0;

        if (this.levelObject != null) {
            GameObject.DestroyImmediate(this.levelObject);
        }
    }

    #endregion

    #region INITIALIZATION_FUNCTIONS

    private void searchForMaterials(out List<Material> materials, out List<string> materialsPaths) {

        materials = new List<Material>();
        materialsPaths = new List<string>();

        string materialsPath = Application.dataPath + "/" + MATERIALS_FOLDER_NAME + "/";
        string[] fileNames = Directory.GetFiles(materialsPath);
        foreach (string absolutefileName in fileNames) {
            string fileName = "Assets/" + MATERIALS_FOLDER_NAME + "/" + absolutefileName.Substring(materialsPath.Length);
            Material m = AssetDatabase.LoadAssetAtPath<Material>(fileName);
            if (m != null) {
                materialsPaths.Add(fileName);
                materials.Add(m);
            }
        }
    }

    private void searchForBackgrounds(out List<Texture2D> backgrounds, out List<string> backgroundsPaths) {
        backgrounds = new List<Texture2D>();
        backgroundsPaths = new List<string>();

        string backgroundsPath = Application.dataPath + "/" + BACKGROUNDS_FOLDER_NAME + "/";
        string[] fileNames = Directory.GetFiles(backgroundsPath);
        foreach (string absolutefileName in fileNames) {
            string fileName = "Assets/" + BACKGROUNDS_FOLDER_NAME + "/" + absolutefileName.Substring(backgroundsPath.Length);
            Texture2D t = AssetDatabase.LoadAssetAtPath<Texture2D>(fileName);
            if (t != null) {
                backgroundsPaths.Add(fileName);
                backgrounds.Add(t);
            }
        }
    }

    private void searchForLevelObjects(out List<Object> levelObjects, out List<string> levelObjectsPaths) {
        levelObjects = new List<Object>();
        levelObjectsPaths = new List<string>();

        string levelObjectsPath = Application.dataPath + "/" + LEVEL_OBJECTS_FOLDER_NAME + "/";
        string[] fileNames = Directory.GetFiles(levelObjectsPath);
        foreach (string absolutefileName in fileNames) {
            string fileName = "Assets/" + LEVEL_OBJECTS_FOLDER_NAME + "/" + absolutefileName.Substring(levelObjectsPath.Length);
            Object o = AssetDatabase.LoadAssetAtPath<Object>(fileName);
            if (o != null) {
                levelObjectsPaths.Add(fileName);
                levelObjects.Add(o);
            }
        }
    }

    private List<string> searchForExistingLevels() {
        List<string> names = new List<string>();

        string levelsPath = Application.dataPath + "/" + Level.LEVELS_FOLDER_NAME + "/";
        string[] fileNames = Directory.GetFiles(levelsPath);
        foreach (string absolutefileName in fileNames) {

            string ending = absolutefileName.Substring(absolutefileName.Length - 3);
            if (!ending.Equals("xml")) {
                continue;
            }

            string fileName = absolutefileName.Substring(levelsPath.Length);
            fileName = fileName.Substring(0, fileName.Length - 4);
            names.Add(fileName.ToLower());
        }

        return names;
    }

    #endregion

    #region GUI EVENTS AND CALLBACKS

    void OnGUI() {

        switch (this.state) {
            case TerrainBuilderState.INITIAL:
                this.buildInitialGUI();
                break;
            case TerrainBuilderState.STARTED:
                this.buildStartedGUI();
                break;
            case TerrainBuilderState.SECTION_CONFIGURATION:
                this.buildSectionConfigurationGUI();
                break;
            case TerrainBuilderState.SECTION_STARTED:
                this.buildSectionStartedGUI();
                break;
            case TerrainBuilderState.LEVEL_OBJECT_EDITING:
                this.buildLevelObjectEditingGUI();
                break;
            case TerrainBuilderState.LEVEL_EVENT_SYSTEM_EDITING:
                this.buildEventSystemConfigurationGUI();
                break;
            case TerrainBuilderState.EVENT_GROUP_EDITING:
                this.buildEventGroupEditingGUI();
                break;
            default:
                Debug.Log("Untreated state called");
                break;
        }

    }

    void OnFocus() {
        SceneView.duringSceneGui -= this.OnSceneGUI; // Just in case
        SceneView.duringSceneGui += this.OnSceneGUI;
    }

    void OnDestroy() {
        SceneView.duringSceneGui -= this.OnSceneGUI;
    }

    private void OnSceneGUI(SceneView scene) {

        if (this.state == TerrainBuilderState.INITIAL) {
            return;
        }

        foreach (Section s in this.sections) {
            s.gameObject.SetActive(!this.hideOtherSections || s.Equals(this.currentSection));
        }

        this.updateMesh();

        if (this.state == TerrainBuilderState.SECTION_STARTED || this.state == TerrainBuilderState.LEVEL_OBJECT_EDITING) {

            Vector2 position2 = Grid.getSelectedCell(Event.current.mousePosition, true);
            Vector3 position = new Vector3(position2.x, position2.y, 0.0F);

            if (this.state == TerrainBuilderState.SECTION_STARTED) {
                this.handleSectionStartedScene(position);
            } else if (this.state == TerrainBuilderState.LEVEL_OBJECT_EDITING) {
                this.handleLevelObjectEditingScene(position);
            }
        }

    }

    private void handleSectionStartedScene(Vector3 position) {

        this.drawPlacementRect(position);
        this.sectionStartedHotkeyHandler(position);

        if (this.debugVerticesActive) {
            this.debugVertices();
        }

        if (this.debugEdgesActive) {
            this.debugEdges();
        }

        if (this.debugOutlinesActive) {
            this.debugOutlines();
        }
    }

    private void handleLevelObjectEditingScene(Vector3 position) {

        this.drawPlacementRect(position);

        if (this.objectPlacementMode) {

            if (Event.current.type == EventType.Layout) {
                HandleUtility.AddDefaultControl(0);
            }

            if (this.selectedLevelObject != null && Event.current.type == EventType.MouseDown && Event.current.button == 0) {

                position.z = Section.GetLayerPos(this.objectLayer);
                GameObject levelObjectGameObject = (GameObject) Instantiate(this.selectedLevelObject, position, Quaternion.identity, this.levelObjectsParent.gameObject.transform);
                levelObjectGameObject.GetComponent<LevelObjectScript>().initialize();
                levelObjectGameObject.GetComponent<LevelObjectScript>().assetPath = this.levelObjectAssetsPaths[this.levelObjectAssets.IndexOf(this.selectedLevelObject)];
                this.levelObjects.Add(levelObjectGameObject);
            }
        }

    }

    private void updateMesh() {
        foreach (Section s in this.sections) {
            s.generateMesh();
        }
    }

    private void sectionStartedHotkeyHandler(Vector3 position) {

        Event e = Event.current;
        if (e.type == EventType.KeyDown) {
            switch (e.keyCode) {
                case KeyCode.N:
                    this.creationEvent(position);
                    break;
                case KeyCode.D:
                    Debug.Log("D");
                    this.destroyVertex();
                    break;
                case KeyCode.S:
                    this.splitEvent();
                    break;
                case KeyCode.J:
                    Debug.Log("J");
                    break;
            }
        }
    }

    private void debugVertices() {

        foreach (MeshRenderer mr in this.verticesParent.GetComponentsInChildren<MeshRenderer>()) {
            mr.sharedMaterial.color = VertexScript.DEFAULT_COLOR;
        }

        List<Vertex> verticesSelected = this.getVerticesSelected();
        int vertexCount = verticesSelected.Count;

        if (vertexCount != 1) {
            return;
        }

        Vertex vertex = verticesSelected[0];

        if (vertex.halfEdge != null) {
            Vector3 prev = this.de.prev(vertex.halfEdge).startVertex.position;
            if (prev != null) {
                this.de.checkForVertexObject(prev).GetComponent<MeshRenderer>().sharedMaterial.color = Color.blue;
            }

            Vector3 next = this.de.next(vertex.halfEdge).startVertex.position;
            if (next != null) {
                this.de.checkForVertexObject(next).GetComponent<MeshRenderer>().sharedMaterial.color = Color.green;
            }
        }

        Handles.color = Color.magenta;
        HalfEdge[] edges = this.de.getEdgesStartingWith(vertex);
        foreach (HalfEdge edge in edges) {
            Handles.DrawLine(vertex.position, this.de.next(edge).startVertex.position);
        }
    }

    private void debugEdges() {

        foreach (Section s in this.sections) {

            if (this.hideOtherSections && !s.Equals(this.currentSection)) {
                continue;
            }

            for (int i = 0; i < s.halfEdges.Count; i += 3) {
                Vector3 v0p = s.halfEdges[i].startVertex.position;
                Vector3 v1p = s.halfEdges[i + 1].startVertex.position;
                Vector3 v2p = s.halfEdges[i + 2].startVertex.position;
                Vector3 mid = (v0p + v1p + v2p) / 3;

                v0p = v0p + 0.1F * (mid - v0p);
                v1p = v1p + 0.1F * (mid - v1p);
                v2p = v2p + 0.1F * (mid - v2p);

                Handles.color = Color.red;
                Handles.DrawLine(v0p, v1p);
                Handles.DrawLine(v1p, v2p);
                Handles.DrawLine(v2p, v0p);

                Handles.color = Color.green;
                Handles.DrawAAPolyLine(4.0F, new Vector3[] { v1p, v1p + 0.2F * (v0p - v1p) });
                Handles.DrawAAPolyLine(4.0F, new Vector3[] { v2p, v2p + 0.2F * (v1p - v2p) });
                Handles.DrawAAPolyLine(4.0F, new Vector3[] { v0p, v0p + 0.2F * (v2p - v0p) });
            }
        }
    }

    private void debugOutlines() {


        foreach (Section s in this.sections) {

            if (this.hideOtherSections && !s.Equals(this.currentSection)) {
                continue;
            }

            for (int i = 0; i < s.halfEdges.Count; ++i) {
                
                if (s.halfEdges[i].opposite == null) {
                    Vector3 start = s.halfEdges[i].startVertex.position;
                    Vector3 end = this.de.next(s.halfEdges[i]).startVertex.position;

                    Handles.color = Color.blue;
                    Handles.DrawAAPolyLine(4.0F, new Vector3[] { start, end });
                }
            }
        }

    }

    private void creationEvent(Vector3 position) {

        List<Vertex> verticesSelected = this.getVerticesSelected();
        int vertexCount = verticesSelected.Count;

        if (vertexCount < 2) {
            if (this.de.checkForVertex(position) == null) {
                this.de.createVertex(position, this.verticesParent);
            }
        } else if (vertexCount == 2) {
            if (this.de.checkForVertex(position) == null) {
                Vertex newVertex = this.de.createVertex(position, this.verticesParent);
                Triangle triangle = this.de.createTriangle(verticesSelected[0], verticesSelected[1], newVertex);
                this.currentSection.addTriangle(triangle);
            }
        } else if (vertexCount == 3) {
            if (!this.de.doesTriangleExist(verticesSelected[0], verticesSelected[1], verticesSelected[2])) {
                Triangle triangle = this.de.createTriangle(verticesSelected[0], verticesSelected[1], verticesSelected[2]);
                this.currentSection.addTriangle(triangle);
            }
        } else {
            Debug.Log("Too many vertices selected");
        }

        Selection.activeTransform = null;
    }

    public void splitEvent() {

        List<Vertex> verticesSelected = this.getVerticesSelected();
        int vertexCount = verticesSelected.Count;

        if (vertexCount == 2) {
            TriangleReplacement? optTriangles = this.de.splitEdge(verticesSelected[0], verticesSelected[1], this.verticesParent);
            if (optTriangles == null) {
                Debug.Log("The selected vertices don't share an edge!");
                return;
            }

            TriangleReplacement triangles = optTriangles.Value;

            for (int i = 0; i < triangles.oldTriangles.Length; ++i) {
                bool containsIndex = this.currentSection.containsTriangle(triangles.oldTriangles[i]);

                if (containsIndex) {

                    // Add the new indices first
                    this.currentSection.addTriangle(triangles.newTriangles[i * 2]);
                    this.currentSection.addTriangle(triangles.newTriangles[i * 2 + 1]);

                    this.currentSection.removeTriangle(triangles.oldTriangles[i]);
                }
            }

        } else {
            Debug.Log("Too many vertices selected");
        }
    }

    #endregion

    #region UI BUILDING

    private void buildInitialGUI() {
        this.searchForExistingLevels();
        GUILayout.Label("To start building a level enter a name and press the button below");
        this.levelName = EditorGUILayout.TextField("Level Name: ", this.levelName);
        if (!this.levelName.Equals("") && !this.levelNames.Contains(this.levelName.ToLower())) {
            if (GUILayout.Button("Start")) {
                this.createLevel();
                this.state = TerrainBuilderState.STARTED;
                GUI.FocusControl("");
            }
        }

        this.levelSelectedIndex = EditorGUILayout.Popup("Level:", this.levelSelectedIndex, this.levelNames.ToArray());
        if (this.levelNames.Count > this.levelSelectedIndex && this.levelSelectedIndex >= 0) {
            if (GUILayout.Button("Activate \"" + this.levelNames[this.levelSelectedIndex] + "\"")) {
                this.createLevelFromFile(this.levelNames[this.levelSelectedIndex]);
                this.state = TerrainBuilderState.STARTED;
                GUI.FocusControl("");
            }
        }
    }

    private void buildStartedGUI() {
        GUILayout.Label("To configure a new section press the button below, otherwise select an existing section and activate it");
        if (GUILayout.Button("Start section configuration")) {
            this.resetSectionRelated();
            this.state = TerrainBuilderState.SECTION_CONFIGURATION;
            GUI.FocusControl("");
        }

        List<string> sectionNames = new List<string>();
        foreach (Section s in this.sections) {
            sectionNames.Add(s.gameObject.name);
        }

        this.sectionSelectedIndex = EditorGUILayout.Popup("Section:", this.sectionSelectedIndex, sectionNames.ToArray());
        if (this.sections.Count > this.sectionSelectedIndex && this.sectionSelectedIndex >= 0) {
            Section s = this.sections[this.sectionSelectedIndex];
            if (GUILayout.Button("Activate \"" + s.name + "\"")) {
                this.resetSectionRelated();
                this.currentSection = s;
                this.state = TerrainBuilderState.SECTION_STARTED;
                GUI.FocusControl("");
            }

            if (GUILayout.Button("Delete \"" + s.name + "\"")) {
                GameObject.DestroyImmediate(s.gameObject);
                this.sections.RemoveAt(this.sectionSelectedIndex);
            }
        }

        if (GUILayout.Button("Edit level objects")) {

            this.state = TerrainBuilderState.LEVEL_OBJECT_EDITING;
            GUI.FocusControl("");
        }

        if (GUILayout.Button("Edit event system")) {

            this.state = TerrainBuilderState.LEVEL_EVENT_SYSTEM_EDITING;
            GUI.FocusControl("");
        }

        if (GUILayout.Button("Generate prefab")) {
            PrefabGenerator.GeneratePrefab(this.level);
        }

        if (GUILayout.Button("End terrain creation")) {
            this.level.writeLevelToFile();
            this.cleanup();
            this.resetAll();
        }
    }

    private void buildSectionConfigurationGUI() {

        GUILayout.Label("Configure the new section.");
        GUILayout.Label("Select a name for your section (can be empty).");
        this.sectionName = EditorGUILayout.TextField("Section Name: ", this.sectionName);
        GUILayout.Label("Select a sections layer (100 = nearest, -100 = furthest).");
        GUILayout.Label("For Layers -100 to 11 and 11 to 100 a parallax effect will be created.");
        this.sectionLayer = EditorGUILayout.IntSlider(this.sectionLayer, -100, 100);
        GUILayout.Label("Select a the sections type (a creatable mesh, or simple image).");
        this.sectionType = (SectionType) EditorGUILayout.EnumPopup("Section type: ", this.sectionType);
        if (this.sectionType == SectionType.MESH) {
            GUILayout.Label("Select a the material the section should be made of.");
            this.sectionMaterial = TerrainBuilderGUIHelper.ObjectPicker(this.materials, this.position.width, this.sectionMaterial, this.materialsScrollPos, out this.materialsScrollPos);
        } else {
            GUILayout.Label("Select the image the section should display.");
            this.sectionImage = TerrainBuilderGUIHelper.ObjectPicker(this.backgrounds, this.position.width, this.sectionImage, this.backgroundsScrollPos, out this.backgroundsScrollPos);
        }

        if (this.sectionMaterial != null || this.sectionImage != null) {
            if (GUILayout.Button("New section")) {
                Section s;
                if (this.sectionType == SectionType.MESH) {
                    s = new Section(this.sectionName, this.levelObject, this.sectionLayer, this.sectionMaterial);
                } else {
                    s = new Section(this.sectionName, this.levelObject, this.sectionLayer, this.sectionImage);
                }

                this.currentSection = s;
                this.state = TerrainBuilderState.SECTION_STARTED;

                this.sections.Add(s);
                GUI.FocusControl("");
            }
        }

        if (GUILayout.Button("Abort!")) {
            this.state = TerrainBuilderState.STARTED;
            GUI.FocusControl("");
        }
    }

    private void buildSectionStartedGUI() {

        this.hideOtherSections = EditorGUILayout.Toggle("Hide other sections:", this.hideOtherSections);

        this.currentSection.layer = EditorGUILayout.IntSlider(this.currentSection.layer, -100, 100);
        this.currentSection.gameObject.transform.position = new Vector3(this.currentSection.gameObject.transform.position.x, this.currentSection.gameObject.transform.position.y, this.currentSection.getLayerPos());

        if (this.currentSection.type == SectionType.MESH) {
            this.buildMeshSectionStartedGUI();
        } else if (this.currentSection.type == SectionType.IMAGE) {
            this.buildImageSectionStartedGUI();
        } else {
            Debug.LogError("Unknown section type!");
        }

        if (GUILayout.Button("End section")) {
            this.currentSection = null;
            this.resetSectionBuildingFlags();
            this.state = TerrainBuilderState.STARTED;
            GUI.FocusControl("");
        }
    }

    private void buildMeshSectionStartedGUI() {

        this.easyInstancing = EditorGUILayout.Toggle("Activate Easy Instancing:", this.easyInstancing);
        this.debugVerticesActive = EditorGUILayout.Toggle("Activate Vertex Debugging:", this.debugVerticesActive);
        this.debugEdgesActive = EditorGUILayout.Toggle("Activate Edge Debugging:", this.debugEdgesActive);
        this.debugOutlinesActive = EditorGUILayout.Toggle("Activate Outline Debugging:", this.debugOutlinesActive);
    }

    private void buildImageSectionStartedGUI() {

        Vector3 currPos = this.currentSection.gameObject.transform.position;
        Vector3 currScale = this.currentSection.gameObject.transform.localScale;

        int currRotation = (int) this.currentSection.gameObject.transform.rotation.eulerAngles.z;

        float xPos = EditorGUILayout.FloatField("X Position: ", currPos.x);
        float yPos = EditorGUILayout.FloatField("Y Position: ", currPos.y);

        float xScale = EditorGUILayout.FloatField("X Scale: ", currScale.x);
        float yScale = EditorGUILayout.FloatField("Y Scale: ", currScale.y);

        float rotation = EditorGUILayout.IntSlider("Rotation: ", currRotation, 0, 360);

        this.currentSection.gameObject.transform.position = new Vector3(xPos, yPos, this.currentSection.getLayerPos());
        this.currentSection.gameObject.transform.localScale = new Vector3(xScale, yScale, 1.0F);
        this.currentSection.gameObject.transform.rotation = Quaternion.AngleAxis(rotation, Vector3.forward);
    }

    private void buildLevelObjectEditingGUI() {

        this.objectPlacementMode = TerrainBuilderGUIHelper.ToggleButton("Object Placement Mode", this.objectPlacementMode);

        if (this.objectPlacementMode) {

            this.objectLayer = EditorGUILayout.IntSlider(this.objectLayer, -100, 100);
            this.selectedLevelObject = TerrainBuilderGUIHelper.ObjectPicker(this.levelObjectAssets, this.position.width, this.selectedLevelObject, this.levelObjectsScrollPos, out this.levelObjectsScrollPos);
        } else {
            this.selectedLevelObject = null;
        }

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Delete selected level objects")) {

            // Delete all level objects with nodes at first
            Node[] selectedNodes = this.getSelectedNodes();
            foreach (Node n in selectedNodes) {
                this.levelObjects.Remove(n.gameObject);
                foreach (EventGroup eg in this.level.ems.eventGroups) {
                    eg.removeNode(n);
                }
                DestroyImmediate(n.gameObject);
            }

            // And after that the level objects without nodes
            LevelObjectScript[] selectedLevelObjects = this.getSelectedLevelObjects();
            foreach (LevelObjectScript los in selectedLevelObjects) {
                this.levelObjects.Remove(los.gameObject);
                DestroyImmediate(los.gameObject);
            }
        }
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("End object editing")) {
            this.state = TerrainBuilderState.STARTED;
            this.resetLevelObjectEditingRelated();
            GUI.FocusControl("");
        }

    }

    private void buildEventSystemConfigurationGUI() {

        EventManagementSystem ems = this.level.ems;
        List<string> eventGroupNames = new List<string>();
        foreach (EventGroup eg in ems.eventGroups) {
            eventGroupNames.Add(eg.name);
        }

        this.eventGroupName = EditorGUILayout.TextField("Event Group Name", this.eventGroupName);
        if (!this.eventGroupName.Equals("") && !eventGroupNames.Contains(this.eventGroupName)) {
            if (GUILayout.Button("Add event group")) {
                this.selectedEventGroup = new EventGroup(this.eventGroupName);
                ems.eventGroups.Add(this.selectedEventGroup);
                this.egf.initialize(this.selectedEventGroup);
                this.state = TerrainBuilderState.EVENT_GROUP_EDITING;
                GUI.FocusControl("");
            }
        }

        this.eventGroupSelectedIndex = EditorGUILayout.Popup("Event Group:", this.eventGroupSelectedIndex, eventGroupNames.ToArray());
        if (ems.eventGroups.Count > this.eventGroupSelectedIndex && this.eventGroupSelectedIndex >= 0) {

            if (GUILayout.Button("Edit event group " + eventGroupNames[this.eventGroupSelectedIndex])) {
                this.selectedEventGroup = ems.eventGroups[this.eventGroupSelectedIndex];
                this.eventGroupName = this.selectedEventGroup.name;
                this.egf.initialize(this.selectedEventGroup);
                this.state = TerrainBuilderState.EVENT_GROUP_EDITING;
                GUI.FocusControl("");
            }
        }

        if (GUILayout.Button("End event system configuration")) {
            this.resetEventSystemRelated();
            this.state = TerrainBuilderState.STARTED;
            GUI.FocusControl("");
        }
    }

    private EventGroupField egf = new EventGroupField();
    private string nodeName;

    private void buildEventGroupEditingGUI() {

        this.eventGroupName = EditorGUILayout.TextField("Editing: ", this.eventGroupName);
        this.selectedEventGroup.name = this.eventGroupName;

        GUILayout.BeginHorizontal();

        this.nodeName = EditorGUILayout.TextField("Node Name:", this.nodeName);
        if (GUILayout.Button("Add level objects to event group")) {
            Node[] nodes = this.getSelectedNodes();
            if (nodes.Length == 1) {

                if (this.nodeName.Equals("")) {
                    Debug.LogWarning("A node without a name is invalid.");
                } else {
                    if (this.selectedEventGroup.nodes.Contains(nodes[0])) {
                        Debug.Log("A Node can only be added once.");
                    } else {
                        nodes[0].initialize(this.nodeName, 0, 0);
                        this.selectedEventGroup.nodes.Add(nodes[0]);
                    }
                }

            } else {
                Debug.LogWarning("Only one Node can be added at a time.");
            }

        }

        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Delete selected Node")) {
            this.egf.deleteSelectedNode();
        }

        GUILayout.EndHorizontal();

        this.egf.draw();
        this.egf.captureEvent();
        this.Repaint();

        if (GUILayout.Button("End event group editing")) {
            this.state = TerrainBuilderState.LEVEL_EVENT_SYSTEM_EDITING;
            this.resetEventGroupRelated();
            GUI.FocusControl("");
        }
    }

    #endregion

    #region HELPER FUNCTIONS

        private void createLevel() {

        this.generateLevelGameObject();

        this.level = new Level(this.levelName);
        this.sections = this.level.sections;
        this.de = this.level.de;
        this.level.levelObject = this.levelObject;

        this.levelObjects = new List<GameObject>();

        this.setAssetLists();
    }

    private void createLevelFromFile(string fileName) {

        this.levelName = fileName;
        this.generateLevelGameObject();

        this.level = Level.CreateLevelFromFile(fileName, this.levelObject, this.verticesParent, this.levelObjectsParent);
        this.sections = this.level.sections;
        this.de = this.level.de;
        this.level.levelObject = this.levelObject;

        this.levelObjects = this.level.levelObjects;

        this.setAssetLists();
    }

    private void generateLevelGameObject() {

        this.levelObject = new GameObject(this.levelName);

        this.verticesParent = new GameObject("Vertices");
        this.verticesParent.hideFlags = HideFlags.NotEditable;
        this.verticesParent.transform.parent = this.levelObject.transform;

        this.levelObjectsParent = new GameObject("Level Objects");
        this.levelObjectsParent.hideFlags = HideFlags.NotEditable;
        this.levelObjectsParent.transform.parent = this.levelObject.transform;
    }

    private void setAssetLists() {

        this.level.materials = this.materials;
        this.level.materialsPaths = this.materialsPaths;
        this.level.backgrounds = this.backgrounds;
        this.level.backgroundsPaths = this.backgroundsPaths;
        this.level.levelObjectAssets = this.levelObjectAssets;
        this.level.levelObjectAssetsPaths = this.levelObjectAssetsPaths;
    }

    private void drawPlacementRect(Vector3 center) {
        Vector3 topLeft = center + Vector3.up * Grid.CELL_SIZE / 2.0F + Vector3.left * Grid.CELL_SIZE / 2.0F;
        Vector3 topRight = center + Vector3.up * Grid.CELL_SIZE / 2.0F + Vector3.right * Grid.CELL_SIZE / 2.0F;
        Vector3 bottomRight = center + Vector3.down * Grid.CELL_SIZE / 2.0F + Vector3.right * Grid.CELL_SIZE / 2.0F;
        Vector3 bottomLeft = center + Vector3.down * Grid.CELL_SIZE / 2.0F + Vector3.left * Grid.CELL_SIZE / 2.0F;

        Vector3[] lineSegments = { topLeft, topRight, topRight, bottomRight, bottomRight, bottomLeft, bottomLeft, topLeft };
        Handles.DrawLines(lineSegments);
    }

    private Node[] getSelectedNodes() {

        GameObject[] selectedObjects = Selection.gameObjects;

        List<Node> nodes = new List<Node>();

        foreach (GameObject o in selectedObjects) {
            Node node = o.GetComponent<Node>();
            if (node != null) {
                nodes.Add(node);
            }
        }

        return nodes.ToArray();
    }

    private LevelObjectScript[] getSelectedLevelObjects() {
        GameObject[] selectedObjects = Selection.gameObjects;

        List<LevelObjectScript> levelObjects = new List<LevelObjectScript>();

        foreach (GameObject o in selectedObjects) {
            LevelObjectScript los = o.GetComponent<LevelObjectScript>();
            if (los != null) {
                levelObjects.Add(los);
            }
        }

        return levelObjects.ToArray();
    }

    public bool isStarted() {
        return this.state == TerrainBuilderState.STARTED;
    }

    public bool isSectionStarted() {
        return this.state == TerrainBuilderState.SECTION_STARTED;
    }

    //  ----------------------------------------------------
    //  |   Delete a vertex and everthing connected to it
    //  |   from this data structure by using the vertex's
    //  |   data "vertexData"
    //  ----------------------------------------------------

    public void destroyVertex() {

        List<Vertex> vertices = this.getVerticesSelected();

        if (vertices.Count == 0) {
            Debug.LogWarning("No Vertex selected!");
            return;
        }

        if (vertices.Count > 1) {
            Debug.LogWarning("More than one vertex selected! Can not delete more than one vertex at a single time.");
            return;
        }

        Vertex vertexData = vertices[0];

        // Debug.Log(vertexData);

        List<int> indices = new List<int>();
        HalfEdge[] connectedHalfEdges = this.de.getEdgesStartingWith(vertexData);
        foreach (HalfEdge edge in connectedHalfEdges) {
            int firstIndex = this.de.firstIndex(edge);
            if (!indices.Contains(firstIndex)) {
                indices.Add(firstIndex);
            }
        }

        // DEDebugger.printList("Indizes: ", indices);
        indices.Sort((x, y) => { return (x < y) ? 1 : -1; });
        // DEDebugger.printList("Indizes: ", indices);
        Triangle[] removedTriangles = this.de.removeTriangles(indices);

        if (this.sections != null) {
            foreach (Section s in this.sections) {
                s.removeTriangles(removedTriangles);
            }
        }

        GameObject vertexGameObject = this.getGameObjectFromVertex(vertexData);
        this.de.vertices.Remove(vertexGameObject);
        GameObject.DestroyImmediate(vertexGameObject);

        this.de.cleanVertices();
    }

    //  ----------------------------------------------------
    //  |   Filters the list of all selected object for
    //  |   selected vertices and returns it
    //  ----------------------------------------------------
    private List<Vertex> getVerticesSelected() {
        GameObject[] selected = Selection.gameObjects;
        List<Vertex> verticesSelected = new List<Vertex>();

        foreach (GameObject o in selected) {
            if (o.GetComponent<VertexScript>() != null) {
                verticesSelected.Add(o.GetComponent<VertexScript>().vertex);
            }
        }

        return verticesSelected;
    }

    private GameObject getGameObjectFromVertex(Vertex vertexData) {

        GameObject[] selected = Selection.gameObjects;

        foreach (GameObject o in selected) {
            if (o.GetComponent<VertexScript>() != null) {
                VertexScript vertexScript = o.GetComponent<VertexScript>();
                if (vertexScript.vertex.Equals(vertexData)) {
                    return o;
                }
            }
        }

        return null;
    }

    #endregion
}

public enum TerrainBuilderState {
    INITIAL,
    STARTED,
    SECTION_CONFIGURATION,
    SECTION_STARTED,
    LEVEL_OBJECT_EDITING,
    LEVEL_EVENT_SYSTEM_EDITING,
    EVENT_GROUP_EDITING
}

public static class TerrainBuilderGUIHelper {

    private static float BUTTON_DIMENSION = 100.0F;

    public static T ObjectPicker<T>(List<T> objects, float windowWidth, T selectedObj, Vector2 scrollPos, out Vector2 newScrollPos) {

        newScrollPos = scrollPos;

        if (objects.Count == 0) {
            GUILayout.Label("No \"" + typeof(T).Name + "\" objects were found. Please check if any assets for this type exist");
            return selectedObj;
        }

        T returnObj = selectedObj;
        int maxEntriesPerRow = (int) (windowWidth / BUTTON_DIMENSION);

        newScrollPos = GUILayout.BeginScrollView(scrollPos);
        for (int i = 0; i < objects.Count; ++i) {

            if (i % maxEntriesPerRow == 0) {
                if (i != 0) {
                    GUILayout.EndHorizontal();
                }
                GUILayout.BeginHorizontal();
            }

            T o = objects[i];
            Object oObj = o as Object;

            bool objSelected = selectedObj != null && selectedObj.Equals(o);
            if (TerrainBuilderGUIHelper.ToggleButton(AssetPreview.GetAssetPreview(oObj), objSelected, GUILayout.Width(BUTTON_DIMENSION), GUILayout.Height(BUTTON_DIMENSION))) {
                returnObj = o;
            }
        }

        GUILayout.EndHorizontal();
        GUILayout.EndScrollView();

        return returnObj;
    }

    public static bool ToggleButton(Texture2D image, bool toggled, params GUILayoutOption[] options) {

        Color oldBackColor = GUI.backgroundColor;

        bool returnVal = false;

        if (toggled) {
            GUI.backgroundColor = Color.blue;
        }

        returnVal = GUILayout.Button(image, options);

        GUI.backgroundColor = oldBackColor;

        return returnVal;
    }

    public static bool ToggleButton(string text, bool toggled, params GUILayoutOption[] options) {

        Color oldBackColor = GUI.backgroundColor;

        bool returnVal = toggled;

        if (toggled) {
            GUI.backgroundColor = Color.blue;
        }

        if (GUILayout.Button(text, options)) {
            returnVal = !returnVal;
        }

        GUI.backgroundColor = oldBackColor;

        return returnVal;
    }

}

public static class PrefabGenerator {

    private static int GROUND_LAYER_INDEX = 8;

    private static string MESHES_FOLDER_NAME = "meshes";
    private static string SPRITES_FOLDER_NAME = "sprites";

    public static void GeneratePrefab(Level level) {

        string levelFolderPath = "Assets/" + Level.LEVELS_FOLDER_NAME + "/" + level.name;

        if (!AssetDatabase.IsValidFolder(levelFolderPath)) {
            AssetDatabase.CreateFolder("Assets/" + Level.LEVELS_FOLDER_NAME, level.name);
        }

        if (!AssetDatabase.IsValidFolder(levelFolderPath + "/" + MESHES_FOLDER_NAME)) {
            AssetDatabase.CreateFolder(levelFolderPath, MESHES_FOLDER_NAME);
        }

        if (!AssetDatabase.IsValidFolder(levelFolderPath + "/" + SPRITES_FOLDER_NAME)) {
            AssetDatabase.CreateFolder(levelFolderPath, SPRITES_FOLDER_NAME);
        }

        GameObject prefab = new GameObject(level.name);
        EventManagerScript ems = prefab.AddComponent<EventManagerScript>();
        ems.eventGroupSaveFilePath = Level.LEVELS_FOLDER_NAME + "/" + level.name + ".xml";

        GameObject LevelObjectParent = new GameObject("LevelObjects");
        LevelObjectParent.transform.parent = prefab.transform;

        foreach (GameObject o in level.levelObjects) {
            GameObject levelObject = GameObject.Instantiate(o);
            levelObject.transform.parent = LevelObjectParent.transform;
        }

        List<GameObject> layerGameObjects = new List<GameObject>();

        List<Section> layer0Sections = new List<Section>();
        foreach (Section s in level.sections) {

            GameObject layerObject = layerGameObjects.Find((o) => {
                return o.name.Equals(s.layer.ToString());
            });

            if (layerObject == null) {
                layerObject = new GameObject(s.layer.ToString());
                layerObject.transform.position = new Vector3(0.0F, 0.0F, s.getLayerPos());

                if (s.layer > 10 || s.layer < -10) {
                    layerObject.AddComponent<ParallaxScript>();
                }


                layerGameObjects.Add(layerObject);
                layerObject.transform.parent = prefab.transform;
            }

            GameObject sectionObj = GenerateSectionObject(level, s, levelFolderPath);
            sectionObj.transform.parent = layerObject.transform;

            if (s.type == SectionType.MESH && s.layer == 0) {
                layer0Sections.Add(s);
            }

        }

        if (layer0Sections.Count > 0) {

            GameObject layerObject = layerGameObjects.Find((o) => {
                return o.name.Equals("0");
            });

            layerObject.layer = GROUND_LAYER_INDEX;
            GenerateCollider(layer0Sections, level.de, layerObject);
        }

        PrefabUtility.SaveAsPrefabAsset(prefab, levelFolderPath + "/" + level.name + ".prefab");

        GameObject.DestroyImmediate(prefab);
    }

    private static GameObject GenerateSectionObject(Level level, Section s, string levelFolderPath) {

        GameObject sectionObject = new GameObject(s.name);

        if (s.type == SectionType.IMAGE) {

            sectionObject.transform.position = s.gameObject.transform.position;
            sectionObject.transform.localScale = s.gameObject.transform.localScale;
            sectionObject.transform.rotation = s.gameObject.transform.rotation;

            Sprite sprite = s.gameObject.GetComponent<SpriteRenderer>().sprite;
            string spriteAssetLocation = levelFolderPath + "/" + SPRITES_FOLDER_NAME + "/" + sprite.name + ".asset";

            Sprite spriteAsset = AssetDatabase.LoadAssetAtPath<Sprite>(spriteAssetLocation);

            while (spriteAsset == null) {

                Debug.Log("No Asset was found for " + spriteAssetLocation + ". Creating new asset.");

                AssetDatabase.CreateAsset(sprite, spriteAssetLocation);
                spriteAsset = AssetDatabase.LoadAssetAtPath<Sprite>(spriteAssetLocation);
            }

            SpriteRenderer sr = sectionObject.AddComponent<SpriteRenderer>();
            sr.sprite = spriteAsset;

        } else if (s.type == SectionType.MESH) {

            Material material = s.gameObject.GetComponent<MeshRenderer>().sharedMaterial;
            Mesh mesh = s.gameObject.GetComponent<MeshFilter>().sharedMesh;
            string meshAssetLocation = levelFolderPath + "/" + MESHES_FOLDER_NAME + "/" + mesh.name + ".mesh";

            AssetDatabase.DeleteAsset(meshAssetLocation);
            AssetDatabase.CreateAsset(mesh, meshAssetLocation);

            mesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshAssetLocation);

            MeshFilter mf = sectionObject.AddComponent<MeshFilter>();
            mf.sharedMesh = mesh;
            MeshRenderer mr = sectionObject.AddComponent<MeshRenderer>();
            mr.sharedMaterial = material;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        return sectionObject;

    }

    private static void GenerateCollider(List<Section> sections, DirectedEdgeDataStructure de, GameObject layerObject) {

        PolygonCollider2D pc = layerObject.AddComponent<PolygonCollider2D>();

        List<HalfEdge> allOutlineEdges = new List<HalfEdge>();
        foreach (Section s in sections) {
            foreach (HalfEdge e in s.halfEdges) {
                if (e.opposite == null) {
                    allOutlineEdges.Add(e);
                }
            }
        }

        List<List<Vector2>> loops = new List<List<Vector2>>();

        loops.Add(new List<Vector2>());
        int currLoop = 0;
        HalfEdge currEdge = allOutlineEdges[0];

        while (allOutlineEdges.Count > 0) {
            allOutlineEdges.Remove(currEdge);
            loops[currLoop].Add(currEdge.startVertex.position);
            Vertex nextStartVertex = de.next(currEdge).startVertex;

            HalfEdge nextOutline = GetConnectedHalfEdge(allOutlineEdges, nextStartVertex);
            if (nextOutline == null && allOutlineEdges.Count > 0) {
                loops.Add(new List<Vector2>());
                currLoop++;
                currEdge = allOutlineEdges[0];
            } else {
                currEdge = nextOutline;
            }
        }

        int shortestLoopIndex = -1;
        float shortestDistance = float.MaxValue;
        int largestLoopIndex = -1;
        float largestDistance = 0;

        for (int i = 0; i < loops.Count; ++i) {
            List<Vector2> points = loops[i];
            float currDistance = 0;
            for (int j = 0; j < points.Count; ++j) {
                currDistance += Vector2.Distance(points[j], points[(j + 1) % points.Count]);
            }

            if (currDistance < shortestDistance) {
                shortestDistance = currDistance;
                shortestLoopIndex = i;
            }

            if (currDistance > largestDistance) {
                largestDistance = currDistance;
                largestLoopIndex = i;
            }
        }

        loops.Sort((a, b) => {
            return (a.Count == b.Count) ? 0 : ((a.Count < b.Count) ? -1 : 1);
        });

        List<Vector2> wholeLoop = new List<Vector2>(loops[0]);

        for (int i = 1; i < loops.Count; ++i) {

            int closestPointIndex = GetClosestPoint(loops[i], loops[i - 1][0]);
            wholeLoop.Add(wholeLoop[0]);

            // Insert the bigger loop at the beginning up to the closest point
            for (int j = closestPointIndex; j >= 0; --j) {
                wholeLoop.Insert(0, loops[i][j]);
            }

            // And add the rest of the bigger loop starting from the closest point to the end
            for (int j = closestPointIndex; j < loops[i].Count; ++j) {
                wholeLoop.Add(loops[i][j]);
            }
        }

        pc.points = wholeLoop.ToArray();
    }

    private static HalfEdge GetConnectedHalfEdge(List<HalfEdge> remainingHalfEdges, Vertex startVertex) {
        foreach (HalfEdge e in remainingHalfEdges) {
            if (e.startVertex.Equals(startVertex)) {
                return e;
            }
        }
        return null;
    }

    private static int GetClosestPoint(List<Vector2> points, Vector2 desiredPosition) {

        float closestDistance = float.MaxValue;
        int index = -1;

        for (int i = 0; i < points.Count; ++i) {
            float currDistance = Vector2.Distance(points[i], desiredPosition);
            if (currDistance < closestDistance) {
                closestDistance = currDistance;
                index = i;
            }
        }

        return index;
    }
}

public class EventGroupField {

    private List<Node> nodes;

    private Rect background;

    public int selectedNodeIndex { get; private set; }
    private bool isLeftDragging;
    private bool isRightDragging;
    private Vector2 dragOffset;

    private int startNodeIndex;
    private Vector2 dragMousePosition;
    private Material material;

    public EventGroupField() {
        this.reset();
    }

    public void initialize(EventGroup eg) {
        this.material = new Material(Shader.Find("Hidden/Internal-Colored"));
        this.nodes = eg.nodes;
        this.reset();
    }

    private void reset() {
        this.selectedNodeIndex = -1;
        this.isLeftDragging = false;
        this.isRightDragging = false;
        this.startNodeIndex = -1;
    }

    public void draw() {

        this.background = EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
        EditorGUI.DrawRect(this.background, new Color(0.3F, 0.3F, 0.3F));
        Vector2 basicOffset = new Vector2(10.0F, 10.0F);

        for (int i = 0; i < this.nodes.Count; ++i) {

            Node n = this.nodes[i];
            
            Color color = new Color(0.6F, 0.1F, 0.1F, 0.6F);

            if (i == this.selectedNodeIndex) {
                color = new Color(0.8F, 0.2F, 0.2F, 0.8F);
            }

            Rect rect = n.getRect();
            rect.position = rect.position + this.background.position;

            EditorGUI.DrawRect(rect, color);
            EditorGUI.LabelField(rect, n.nodeName);
        }

        EditorGUILayout.EndVertical();
    }

    public void captureEvent() {

        Event e = Event.current;

        if (!e.isMouse && e.type != EventType.Repaint) {
            return;
        }

        if (e.isMouse) {
            if (e.button == 0) {
                this.handleLeftMouseButton(e);
            } else if (e.button == 1) {
                this.handleRightMouseButton(e);
            }
        } else if (e.type == EventType.Repaint) {
            this.drawLines();
        }
    }

    private void handleLeftMouseButton(Event e) {

        switch (e.type) {
            case EventType.MouseDown:

                bool anyNodeSelected = false;

                for (int i = 0; i < this.nodes.Count; ++i) {
                    if (this.isInBounds(this.nodes[i], e.mousePosition - this.background.position)) {
                        this.dragOffset = e.mousePosition - this.nodes[i].position;
                        this.selectedNodeIndex = i;
                        this.isLeftDragging = true;

                        anyNodeSelected = true;
                        break;
                    }
                }

                if (!anyNodeSelected) {
                    this.selectedNodeIndex = -1;
                }

                break;
            case EventType.MouseDrag:
                if (this.isLeftDragging) {
                    float x = Math.Max(0.0F, (Math.Min(e.mousePosition.x - this.dragOffset.x, this.background.size.x - Node.NODE_SIZE.x)));
                    float y = Math.Max(0.0F, (Math.Min(e.mousePosition.y - this.dragOffset.y, this.background.size.y - Node.NODE_SIZE.y)));
                    this.nodes[this.selectedNodeIndex].position = new Vector2(x, y);
                }
                break;
            case EventType.MouseUp:
                this.isLeftDragging = false;
                break;
        }
    }

    private void handleRightMouseButton(Event e) {

        if (e.type == EventType.MouseDown) {

            bool anyNodeSelected = false;

            for (int i = 0; i < this.nodes.Count; ++i) {
                if (this.isInBounds(this.nodes[i], e.mousePosition - this.background.position) && this.nodes[i].isOutput()) {
                    this.startNodeIndex = i;
                    anyNodeSelected = true;
                    break;
                }
            }

            if (!anyNodeSelected) {
                this.startNodeIndex = -1;
            }

        } else if (e.type == EventType.MouseDrag) {

            if (this.startNodeIndex != -1) {
                this.isRightDragging = true;
                this.dragMousePosition = e.mousePosition;
            }

        } else if (e.type == EventType.MouseUp) {

            int endNodeIndex = -1;

            for (int i = 0; i < this.nodes.Count; ++i) {
                if (i != this.startNodeIndex && this.isInBounds(this.nodes[i], e.mousePosition - this.background.position)) {
                    endNodeIndex = i;
                    break;
                }
            }

            if (this.startNodeIndex != -1 && endNodeIndex != -1) {

                this.nodes[endNodeIndex].addPredecessor(this.nodes[this.startNodeIndex]);

            } else {
                this.startNodeIndex = -1;
            }

            this.isRightDragging = false;
        }
    }

    public void drawLines() {

        if (this.isRightDragging) {
            this.drawLine(this.nodes[this.startNodeIndex].getOutputPosition(), (this.dragMousePosition - this.background.position));
        }

        foreach (Node n1 in this.nodes) {
            foreach(Node n2 in n1.predecessors) {
                this.drawLine(n2.getOutputPosition(), n1.getInputPosition());
            }
        }
    }

    private void drawLine(Vector2 start, Vector2 end) {
        Handles.DrawLine(start + this.background.position, end + this.background.position);
    }

    private bool isInBounds(Node n, Vector2 mousePosition) {
        Vector2 pos = n.position;
        return (mousePosition.x >= pos.x) &&
            (mousePosition.y >= pos.y) &&
            (mousePosition.x <= (pos.x + Node.NODE_SIZE.x)) &&
            (mousePosition.y <= (pos.y + Node.NODE_SIZE.y));
    }

    public void deleteSelectedNode() {

        if (this.selectedNodeIndex >= 0 && this.selectedNodeIndex < this.nodes.Count) {
            this.nodes.RemoveAt(this.selectedNodeIndex);
            this.selectedNodeIndex = -1;
        }
    }
}