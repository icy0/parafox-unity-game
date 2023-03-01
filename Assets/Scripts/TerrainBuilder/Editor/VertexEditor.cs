using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/**
 * The vertex editor is responsible for executing the grid snapping
 * behavior of a vertex game object
 */
[CustomEditor(typeof(VertexScript))]
public class VertexEditor : Editor
{

    //  ----------------------------------------------------
    //  |   Static Attributes
    //  ----------------------------------------------------

    private static Color DRAGGING_COLOR = Color.red;

    //  ----------------------------------------------------
    //  |   Attributes
    //  ----------------------------------------------------

    // Is a dragging action ongoing in the moment
    private bool isDragging = false;

    // The currently selected vertex
    private GameObject vertexObj;

    private Vertex vertexData;

    //  ----------------------------------------------------
    //  |   Once a vertex object is clicked on, it will be
    //  |   set as the current vertex and the transformation
    //  |   tools get hidden
    //  ----------------------------------------------------

    private void OnEnable() {
        VertexScript vertexScript = (VertexScript) target;
        vertexObj = vertexScript.gameObject;
        vertexData = new Vertex(vertexScript.vertex.position, vertexScript.vertex.halfEdge);
        Tools.hidden = true;
    }

    //  ----------------------------------------------------
    //  |   Handle the dragging of the mouse on the active
    //  |   vertex and set its position to the closest cell
    //  ----------------------------------------------------

    private void OnSceneGUI() {

        if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && hitVertex()) {

            int controlId = GUIUtility.GetControlID(FocusType.Passive);
            GUIUtility.hotControl = controlId;

            isDragging = true;
            vertexObj.GetComponent<MeshRenderer>().sharedMaterial.color = DRAGGING_COLOR;
        } else if (Event.current.type == EventType.MouseUp && Event.current.button == 0) {
            isDragging = false;
            vertexObj.GetComponent<MeshRenderer>().sharedMaterial.color = VertexScript.DEFAULT_COLOR;
        } else if (isDragging) {
            vertexObj.GetComponent<VertexScript>().updateCellPos();
        }
    }

    //  ----------------------------------------------------
    //  |   Check if a raycast from our mouse position hits
    //  |   our currently selected vertex
    //  ----------------------------------------------------

    private bool hitVertex() {

        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        RaycastHit hitObj;
        bool hit = Physics.Raycast(ray, out hitObj);
        if (hit && vertexObj.transform.Equals(hitObj.transform)) {
            return true;
        }

        return false;
    }

    //  ----------------------------------------------------
    //  |   Once our selected vertex gets deselected reset
    //  |   the dragging flag, and show the transform tools
    //  |   again
    //  ----------------------------------------------------

    private void OnDisable() {

        if (vertexObj == null) {
            return;
        }

        Tools.hidden = false;
        isDragging = !isDragging;
        vertexObj.GetComponent<MeshRenderer>().sharedMaterial.color = VertexScript.DEFAULT_COLOR;
    }

    //  ----------------------------------------------------
    //  |   OnDestroy gets called whenever a vertex is
    //  |   deselected. If target is null we can be sure,
    //  |   that the object was destroyed. If so, advise
    //  |   our terrain builder window to destroy the vertex
    //  |   by its vertex data
    //  ----------------------------------------------------
    private void OnDestroy() {

        if (target == null) {
            TerrainBuilderWindow window = EditorWindow.GetWindow<TerrainBuilderWindow>("Terrain Builder");
            if (window != null && !vertexData.Equals(null)) {
                // window.destroyVertex(vertexData);
            }
        }
    }
}
