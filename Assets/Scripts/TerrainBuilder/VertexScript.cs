using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/**
 * The vertex script manages all important vertex game object
 * related attributes and behaviours
 */
[ExecuteInEditMode]
public class VertexScript : MonoBehaviour {

    //  ----------------------------------------------------
    //  |   Static Attributes
    //  ----------------------------------------------------

    public static Mesh sphereMesh;
    public static Material defaultMaterial;
    public static Color DEFAULT_COLOR = Color.yellow;
    public static VertexScript destroyedVertex;

    //  ----------------------------------------------------
    //  |   Attributes
    //  ----------------------------------------------------

    // The information useful for our directed edge data structure
    public Vertex vertex { get; set; }
    
    // The overall directed edge data structure
    public DirectedEdgeDataStructure de { get; set; }

    //  ----------------------------------------------------
    //  |   Initialize all of the default assets
    //  ----------------------------------------------------

    public static void init() {

        GameObject tempSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        VertexScript.sphereMesh = tempSphere.GetComponent<MeshFilter>().sharedMesh;
        VertexScript.defaultMaterial = tempSphere.GetComponent<MeshRenderer>().sharedMaterial;
        VertexScript.defaultMaterial.color = DEFAULT_COLOR;
        GameObject.DestroyImmediate(tempSphere);
    }

    public VertexScript(VertexScript other) {

        this.vertex.halfEdge = other.vertex.halfEdge;
        this.vertex.position = new Vector3(other.vertex.position.x, other.vertex.position.y, other.vertex.position.z);
        this.de = other.de;
    }

    //  ----------------------------------------------------
    //  |   Save our first valid positon
    //  ----------------------------------------------------

    private void Start() {
        this.vertex.position = this.transform.position;
    }

    //  ----------------------------------------------------
    //  |   Set the position of our vertex to the next valid
    //  |   cell
    //  ----------------------------------------------------

    public void updateCellPos() {

        Vector2 cellPos = Grid.getSelectedCell(Event.current.mousePosition, true);

        if (this.de.vertices != null) {
            foreach (GameObject o in this.de.vertices) {
                if (!o.Equals(this) && o.transform.position.Equals(cellPos)) {
                    this.transform.position = this.vertex.position;
                    return;
                }
            }
        }

        this.transform.position = new Vector3(cellPos.x, cellPos.y, this.transform.position.z);
        this.vertex.position = this.transform.position;
    }
}
