using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SectionType {
    MESH,
    IMAGE
}

// TODO:
//  - Changing a sections material / Image
//  - Placing level objects

/**
 * A section is a layer of the terrain made up by a mesh or an image
 * that is displayed in the world
 */
public class Section {

    //  ----------------------------------------------------
    //  |   Static Attributes
    //  ----------------------------------------------------

    public static int ID = 0;

    //  ----------------------------------------------------
    //  |   Attributes
    //  ----------------------------------------------------

    // The sections unique id
    public int id { get; set; }

    // The type of the section (Mesh or Image)
    public SectionType type;

    // The sections material if it is a mesh section
    public Material material { get; set; }

    // The sections image if it is an image section
    public Texture2D image { get; set; }

    // The corresponding game object housing the mesh
    // renderer and filter / sprite renderer
    public GameObject gameObject { get; set; }

    // The indices of the first half edges that make
    // up each triangle that belong to this section
    public List<HalfEdge> halfEdges { get; set; }

    // The layer of the section (100 nearest, -100 farthest)
    public int layer { get; set; }

    // The sections name
    public string name { get; set; }

    // If the sections topology was changed we have to create
    // a new mesh corresponding to it
    public bool dirty { get; set; }

    //  ----------------------------------------------------
    //  |   Most basic constructor
    //  ----------------------------------------------------

    private Section(int layer) {

        this.id = ID++;
        this.layer = layer;
        this.halfEdges = new List<HalfEdge>();
        this.dirty = true;
    }

    //  ----------------------------------------------------
    //  |   Constructor for a section of type mesh
    //  ----------------------------------------------------
    public Section(string name, GameObject terrain, int layer, Material material, bool addSectionID = true) : this(layer) {

        this.material = material;
        this.type = SectionType.MESH;

        if (name == "") {
            name = "Section";
        }

        if (addSectionID) {
            name += "_" + this.id;
        }

        this.name = name;
        this.gameObject = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
        this.gameObject.transform.position = new Vector3(0, 0, this.getLayerPos());
        this.gameObject.transform.parent = terrain.transform;
        this.gameObject.GetComponent<MeshRenderer>().sharedMaterial = material;
        this.gameObject.hideFlags = HideFlags.NotEditable;

        if (layer == 0) {
            this.gameObject.AddComponent<MeshCollider>();
        }
    }

    //  ----------------------------------------------------
    //  |   Constructor for a section of type image
    //  ----------------------------------------------------
    public Section(string name, GameObject terrain, int layer, Texture2D image, bool addSectionID = true) : this(layer) {

        this.image = image;
        this.type = SectionType.IMAGE;

        if (name == "") {
            name = "Section";
        }

        if (addSectionID) {
            name += "_" + this.id;
        }

        this.name = name;
        this.gameObject = new GameObject(name, typeof(SpriteRenderer));
        this.gameObject.transform.position = new Vector3(0, 0, this.getLayerPos());
        this.gameObject.transform.parent = terrain.transform;
        Sprite sprite = Sprite.Create(image, new Rect(0, 0, image.width, image.height), new Vector2(0, 0));
        sprite.name = image.name;

        this.gameObject.GetComponent<SpriteRenderer>().sprite = sprite;
        this.gameObject.hideFlags = HideFlags.NotEditable;

        if (layer == 0) {
            this.gameObject.AddComponent<MeshCollider>();
        }
    }

    //  ----------------------------------------------------
    //  |   Calculate the z position of the section in the
    //  |   world
    //  ----------------------------------------------------
    public float getLayerPos() {
        return Section.GetLayerPos(this.layer);
    }

    //  ----------------------------------------------------
    //  |   If the section was marked dirty we have to
    //  |   create a new mesh that fits the current topology
    //  ----------------------------------------------------
    public void generateMesh() {
        if (this.type != SectionType.MESH || !this.dirty) {
            return;
        }

        // Instantiating

        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> indices = new List<int>();

        // Foreach triangle belonging to this section
        for (int i = 0; i < this.halfEdges.Count; i += 3) {

            // Get all the vertices that make up the current triangle
            Vertex vertex0 = this.halfEdges[i].startVertex;
            Vertex vertex1 = this.halfEdges[i + 1].startVertex;
            Vertex vertex2 = this.halfEdges[i + 2].startVertex;

            Vector3 v0Pos = new Vector3(vertex0.position.x, vertex0.position.y, 0.0F);
            Vector3 v1Pos = new Vector3(vertex1.position.x, vertex1.position.y, 0.0F);
            Vector3 v2Pos = new Vector3(vertex2.position.x, vertex2.position.y, 0.0F);

            // Add the vertices' positions
            vertices.Add(v0Pos);
            vertices.Add(v1Pos);
            vertices.Add(v2Pos);

            // Generate the vertices' normals
            for (int j = 0; j < 3; ++j) {
                normals.Add(new Vector3(0, 0, 1));
            }

            // Add the vertices' uv coordinates
            uvs.Add(vertex0.position);
            uvs.Add(vertex1.position);
            uvs.Add(vertex2.position);

            // Generate the indices
            indices.Add(indices.Count);
            indices.Add(indices.Count);
            indices.Add(indices.Count);
        }

        // Set up the Mesh
        Mesh m = new Mesh();
        m.name = this.name + "_Mesh";
        m.vertices = vertices.ToArray();
        m.uv = uvs.ToArray();
        m.normals = normals.ToArray();
        m.triangles = indices.ToArray();

        // Put it in the renderer
        this.gameObject.GetComponent<MeshFilter>().sharedMesh = m;

        if (this.layer == 0) {
            this.gameObject.GetComponent<MeshCollider>().sharedMesh = m;
        }
    }
    
    //  ----------------------------------------------------
    //  |   Add the three half edges, given in the Triangle
    //  |   struct to our half edge list
    //  ----------------------------------------------------
    public void addTriangle(Triangle triangle) {
        this.halfEdges.Add(triangle.e0);
        this.halfEdges.Add(triangle.e1);
        this.halfEdges.Add(triangle.e2);
    }

    //  ----------------------------------------------------
    //  |   Check if our half edge list contains the first
    //  |   half edge that makes up the given triangle
    //  ----------------------------------------------------
    public bool containsTriangle(Triangle triangle) {
        return this.halfEdges.Contains(triangle.e0);
    }

    //  ----------------------------------------------------
    //  |   Removes all triangles given in the array of
    //  |   triangles from this section
    //  ----------------------------------------------------
    public void removeTriangles(Triangle[] triangles) {
        foreach (Triangle triangle in triangles) {
            this.removeTriangle(triangle);
        }
    }

    //  ----------------------------------------------------
    //  |   Removes all of the half edges, that make up the
    //  |   Triangle "triangle" from our half edge list
    //  ----------------------------------------------------
    public void removeTriangle(Triangle triangle) {

        if (triangle.e0.opposite != null) {
            triangle.e0.opposite.opposite = null;
        }

        if (triangle.e1.opposite != null) {
            triangle.e1.opposite.opposite = null;
        }

        if (triangle.e1.opposite != null) {
            triangle.e1.opposite.opposite = null;
        }

        this.halfEdges.Remove(triangle.e0);
        this.halfEdges.Remove(triangle.e1);
        this.halfEdges.Remove(triangle.e2);
    }


    //  ----------------------------------------------------
    //  |   Calculate the z position of a layer has in the
    //  |   world
    //  ----------------------------------------------------
    public static float GetLayerPos(int layer) {
        return (-1.0F / 100) * layer;
    }
}
