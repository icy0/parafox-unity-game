using UnityEngine;
using UnityEditor;
using System.Collections;

/*
 * TODO:
 *  > Divide vertex into one vertex per connecting triangle     
 *  > Join two vertices into one                               
 *  > Create a new Vertex inbetween two selected ones          
 *  > Delete a vertex and all connecting triangles             
 *  > Easy instancing (selecting the last two created vertices)
 *  > Layers!                                                       > DONE
 *  > Parallax effect script for background
 *  > Asset / Object placement
 *  > Exporting into prefabs
 *  > ...
*/

[CustomEditor(typeof(TerrainBuilderScript))]
public class TerrainBuilderEditor : Editor {

    private TerrainBuilderScript terrainBuilder;
    private TerrainBuilderWindow window;

    private void Awake() {
        terrainBuilder = target as TerrainBuilderScript;
        window = EditorWindow.GetWindow<TerrainBuilderWindow>("Terrain Builder");
    }

    void OnSceneGUI() {

        if (!window.isSectionStarted()) {
			return;
		}

        Event e = Event.current;
        if (e.type == EventType.KeyDown) {
            switch (e.keyCode) {
                case KeyCode.N:
                    Debug.Log("N");
                    break;
                case KeyCode.D:
                    Debug.Log("D");
                    break;
                case KeyCode.S:
                    Debug.Log("S");
                    break;
                case KeyCode.J:
                    Debug.Log("J");
                    break;
            }
        }
    }

	public override void OnInspectorGUI() {

		/*DrawDefaultInspector();

		terrainBuilder = target as TerrainBuilderScript;

		if (!terrainBuilder.started) {
			if (GUILayout.Button ("Start")) {
				terrainBuilder.start ();
			}
		} else {
			if (GUILayout.Button ("Stop")) {
				terrainBuilder.stop();
			}
		}

        if (terrainBuilder.started) {


            if (!terrainBuilder.sectionActive) {

                if (GUILayout.Button("Switch Material")) {
                    terrainBuilder.switchMaterial();
                }
                if (GUILayout.Button("Create New Section")) {
                    terrainBuilder.createNewSection();
                }
                if (terrainBuilder.sections.Count > 0) {

                    if (GUILayout.Button("Switch Section")) {
                        terrainBuilder.switchSection();
                        SceneView.RepaintAll();
                    }

                    if (terrainBuilder.currentSectionIndex != -1) {
                        if (GUILayout.Button("Activate Section")) {
                            terrainBuilder.activateSection();
                            SceneView.RepaintAll();
                        }
                    }
                }
            } else {
                if (!terrainBuilder.easyInstancing) {
                    if (GUILayout.Button("Activate Easy Instancing")) {
                        terrainBuilder.enableEasyInstancing();
                    }
                } else {
                    if (GUILayout.Button("Deactivate Easy Instancing")) {
                        terrainBuilder.disableEasyInstancing();
                    }
                }

                if (GUILayout.Button("End Section")) {
                    terrainBuilder.endSection();
                    SceneView.RepaintAll();
                }
            }
        }*/
    }
}
