using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/*
 * This data structure stores a list of half edges and a list of vertices,
 * which make up the terrain mesh. It supplies a lot of methods for changing
 * the mesh (creation of triangles, deletion, etc.)
 */
public class DirectedEdgeDataStructure
{

    // ----------------------------------------------------------
    // |    Attributes
    // ----------------------------------------------------------

    public List<GameObject> vertices;
    public List<HalfEdge> edges { get; }

    //  ----------------------------------------------------
    //  |   Constructor
    //  ----------------------------------------------------

    public DirectedEdgeDataStructure() {
        this.vertices = new List<GameObject>();
        this.edges = new List<HalfEdge>();
    }

    #region Localization functions

    //  ----------------------------------------------------
    //  |   Retrieve the first HalfEdge that makes up the
    //  |   triangle that the HalfEdge "edge" is a part of
    //  ----------------------------------------------------

    public HalfEdge first(HalfEdge edge) {

        int index = this.firstIndex(edge);
        if (index == -1) {
            return null;
        }

        return this.edges[index];
    }

    //  ----------------------------------------------------
    //  |   Retrieve the index of the first HalfEdge that 
    //  |   makes up the triangle that the HalfEdge "edge"
    //  |   is a part of
    //  ----------------------------------------------------

    public int firstIndex(HalfEdge edge) {

        if (edge == null) {
            return -1;
        }

        int currentIndex = this.edges.IndexOf(edge);
        if (currentIndex == -1) {
            return -1;
        }

        return (currentIndex / 3) * 3;
    }

    //  ----------------------------------------------------
    //  |   Calculate the index of the next half edge of
    //  |   the given HalfEdge "edge"
    //  ----------------------------------------------------

    public int nextIndex(HalfEdge edge) {

        if (edge == null) {
            return -1;
        }

        int currentIndex = this.edges.IndexOf(edge);
        if (currentIndex == -1) {
            return -1;
        }

        int startIndex = (currentIndex / 3) * 3;
        int nextIndex = startIndex + ((currentIndex + 1) % 3);

        return nextIndex;
    }

    //  ----------------------------------------------------
    //  |   Calculate the index of the half edge next to the
    //  |   half edge with the index "index"
    //  ----------------------------------------------------

    public int nextIndex(int index) {

        if (index >= this.edges.Count) {
            return -1;
        }

        int startIndex = (index / 3) * 3;
        int nextIndex = startIndex + ((index + 1) % 3);

        return nextIndex;
    }

    //  ----------------------------------------------------
    //  |   Retrieve the next HalfEdge of the given half
    //  |   edge "edge"
    //  ----------------------------------------------------

    public HalfEdge next(HalfEdge edge) {
        int index = this.nextIndex(edge);
        if (index == -1) {
            return null;
        }
        return this.edges[index];
    }

    //  ----------------------------------------------------
    //  |   Calculate the index of the previous half edge of
    //  |   the given HalfEdge "edge"
    //  ----------------------------------------------------

    public int prevIndex(HalfEdge edge) {

        if (edge == null) {
            return -1;
        }

        int currentIndex = this.edges.IndexOf(edge);
        if (currentIndex == -1) {
            return -1;
        }

        int startIndex = (currentIndex / 3) * 3;
        int nextTIndex = ((currentIndex / 3) + 1) * 3;
        int prevIndex = startIndex + ((nextTIndex + (currentIndex - 1) % 3) % 3);

        return prevIndex;
    }


    //  ----------------------------------------------------
    //  |   Calculate the index of the half edge previous to
    //  |   the half edge with the index "index"
    //  ----------------------------------------------------

    public int prevIndex(int index) {

        if (index >= this.edges.Count) {
            return -1;
        }

        int startIndex = (index / 3) * 3;
        int nextTIndex = ((index / 3) + 1) * 3;
        int prevIndex = startIndex + ((nextTIndex + (index - 1) % 3) % 3);

        return prevIndex;
    }

    //  ----------------------------------------------------
    //  |   Retrieve the previous half edge of the given
    //  |   HalfEdge "edge"
    //  ----------------------------------------------------

    public HalfEdge prev(HalfEdge edge) {
        int index = this.prevIndex(edge);
        if (index == -1) {
            return null;
        }
        return this.edges[index];
    }

    #endregion

    #region Topology functions

    //  ----------------------------------------------------
    //  |   Create a new vertex at the given Vector3 "pos"
    //  ----------------------------------------------------

    public Vertex createVertex(Vector3 pos, GameObject verticesGameObject) {

        GameObject vertex = new GameObject("vertex", typeof(MeshFilter), typeof(MeshRenderer), typeof(VertexScript), typeof(SphereCollider));
        vertex.hideFlags = HideFlags.NotEditable;
        vertex.tag = "Vertex";
        vertex.transform.parent = verticesGameObject.transform;
        vertex.transform.position = new Vector3(pos.x, pos.y, -1.0F);
        vertex.transform.localScale = new Vector3(0.2F, 0.2F, 0.2F);
        vertex.GetComponent<MeshFilter>().mesh = VertexScript.sphereMesh;
        vertex.GetComponent<MeshRenderer>().material = new Material(VertexScript.defaultMaterial);
        vertex.GetComponent<VertexScript>().vertex = new Vertex(pos);
        vertex.GetComponent<VertexScript>().de = this;
        vertex.GetComponent<SphereCollider>().radius = 0.5F;

        this.vertices.Add(vertex);

        return vertex.GetComponent<VertexScript>().vertex;
    }


    //  ----------------------------------------------------
    //  |   Return an array of clockwise orientated vertices
    //  ----------------------------------------------------

    public Vertex[] orientateVertices(Vertex v0, Vertex v1, Vertex v2) {

        Vector3 v0t = v0.position,
               v1t = v1.position,
               v2t = v2.position;

        float orientation = (v1t.y - v0t.y) * (v2t.x - v1t.x) - (v2t.y - v1t.y) * (v1t.x - v0t.x);

        if (orientation >= 0) {
            // The vertices are oriented clockwise and we don't need to change their order
            return new Vertex[] { v0, v1, v2 };
        } else {
            // The vertices are oriented counter clockwise and we need to change their order
            return new Vertex[] { v1, v0, v2 };
        }
    }

    //  ----------------------------------------------------
    //  |   Create a new triangle (three new half edges)
    //  |   between the three vertices "v0", "v1" and "v2"
    //  |   and return the index of the first half edge
    //  ----------------------------------------------------

    public Triangle createTriangle(Vertex v0, Vertex v1, Vertex v2) {

        Vertex[] vertices = this.orientateVertices(v0, v1, v2);

        // Storage for the newly created half edges
        List<HalfEdge> localEdges = new List<HalfEdge>();

        for (int i = 0; i < 3; ++i) {
            // Create the new half edge
            HalfEdge edge = new HalfEdge(vertices[i]);

            // Add it to our edges / local edges list
            this.edges.Add(edge);
            localEdges.Add(edge);

            // Set the vertices half edge to the newly created half edge
            vertices[i].halfEdge = edge;
        }

        // Foreach newly created edge
        for (int i = 0; i < 3; ++i) {
            HalfEdge edge = localEdges[i];

            HalfEdge opposite = this.getEdgeBetweenVertices(vertices[(i + 1) % 3], vertices[i]);
            edge.opposite = opposite;

            // if we actually found one, set the opposing's edge opposing edge to the newly created edge
            if (opposite != null) {
                opposite.opposite = edge;
            }
        }

        return new Triangle(localEdges);
    }

    //  ----------------------------------------------------
    //  |   Removes all half edge loops starting with the
    //  |   indices specified by the "indices" list
    //  ----------------------------------------------------

    public Triangle[] removeTriangles(List<int> indices) {

        // Create a list to store all of the removed triangles
        List<Triangle> removedTrianglesList = new List<Triangle>();

        // Foreach start index
        foreach (int index in indices) {
            // Remove the triangle that starts with the half edge at the index "index"
            Triangle removedTriangle = this.removeTriangle(index);
            // And add the returned triangle to the list
            removedTrianglesList.Add(removedTriangle);
        }

        return removedTrianglesList.ToArray();
    }

    //  ----------------------------------------------------
    //  |   Removes the half edge loop starting with the
    //  |   index "index"
    //  ----------------------------------------------------

    public Triangle removeTriangle(int index) {

        // Create a new triangle to store our removed edges
        Triangle removedTriangle = new Triangle(this.edges[index], this.edges[index + 1], this.edges[index + 2]);

        // foreach half edge
        for (int i = 0; i < 3; ++i) {
            // Get the starting vertex
            Vertex startVertex = this.edges[index + 2 - i].startVertex;
            // Remove the half edge from the edges list
            this.edges.RemoveAt(index + 2 - i);

            // Set the startVertex's half edge to null
            startVertex.halfEdge = null;

            // Try to find another edge that starts with this vertex and set it
            // as the startVertex's half edge
            HalfEdge[] connectedEdges = this.getEdgesStartingWith(startVertex);
            if (connectedEdges.Length > 0) {
                startVertex.halfEdge = connectedEdges[0];
            }
        }

        return removedTriangle;
    }


    //  ----------------------------------------------------
    //  |   Creates a new vertex on an existing edge and
    //  |   deletes the old triangles and creates new ones,
    //  |   that connect to the new vertex
    //  ----------------------------------------------------

    public TriangleReplacement? splitEdge(Vertex v0, Vertex v1, GameObject verticesGameObject) {

        // Retrieve the two half edges of our edge inbetween the vertices v0 and v1
        HalfEdge h0 = this.getEdgeBetweenVertices(v0, v1);
        HalfEdge h0o = this.getEdgeBetweenVertices(v1, v0);

        // If no half edges exist, the two vertives share no edge
        if (h0 == null && h0o == null) {
            return null;
        }

        // Create the new vertex in the center of the edge (aligned with our grid of course)
        Vertex middleVertex = this.createVertexInCenter(new Vertex[] { v0, v1 }, verticesGameObject);

        // The two start vertices of our two opposing half edges
        Vertex v2 = null;
        Vertex v2o = null;

        // The old triangles that need to be deleted and the new triangles that need to be added
        List<Triangle> oldTriangles = new List<Triangle>();
        List<Triangle> newTriangles = new List<Triangle>();

        // If a half edge was found between v0 and v1
        if (h0 != null) {

            // Find the first index and create the triangle that starts with this index
            int h0FirstIndex = this.firstIndex(h0);
            oldTriangles.Add(new Triangle(this.edges[h0FirstIndex], this.edges[h0FirstIndex + 1], this.edges[h0FirstIndex + 2]));

            // Get the vertex that is not selected but was part of the initial triangle
            v2 = this.prev(h0).startVertex;
            // Create two new triangles that both connect to our new center vertex
            Triangle t0 = this.createTriangle(v0, middleVertex, v2);
            Triangle t1 = this.createTriangle(v1, middleVertex, v2);

            // Add these triangles to our new triangles list so that they can be added to our section
            newTriangles.Add(t0);
            newTriangles.Add(t1);
        }

        // If a half edge was found between v1 and v0
        if (h0o != null) {
            // Find the first index and create the triangle that starts with this index
            int h0oFirstIndex = this.firstIndex(h0o);
            oldTriangles.Add(new Triangle(this.edges[h0oFirstIndex], this.edges[h0oFirstIndex + 1], this.edges[h0oFirstIndex + 2]));

            // Get the vertex that is not selected but was part of the initial triangle
            v2o = this.prev(h0o).startVertex;
            // Create two new triangles that both connect to our new center vertex
            Triangle t0 = this.createTriangle(v0, middleVertex, v2o);
            Triangle t1 = this.createTriangle(v1, middleVertex, v2o);

            // Add these triangles to our new triangles list so that they can be added to our section
            newTriangles.Add(t0);
            newTriangles.Add(t1);
        }


        if (h0 != null) {
            // Remove the original three half edges from our edges list
            int firstIndexOfTriangle = this.firstIndex(h0);
            this.edges.RemoveAt(firstIndexOfTriangle);
            this.edges.RemoveAt(firstIndexOfTriangle);
            this.edges.RemoveAt(firstIndexOfTriangle);
        }

        if (h0o != null) {
            // Remove the original three half edges from our edges list
            int firstIndexOfTriangle = this.firstIndex(h0o);
            this.edges.RemoveAt(firstIndexOfTriangle);
            this.edges.RemoveAt(firstIndexOfTriangle);
            this.edges.RemoveAt(firstIndexOfTriangle);
        }

        return new TriangleReplacement(oldTriangles.ToArray(), newTriangles.ToArray());
    }

    #endregion

    #region Topology Helper functions


    //  ----------------------------------------------------
    //  |   Creates a vertex in the middle of all of the
    //  |   vertices given in the "vertices" array. The new
    //  |   vertex's position is of course already aligned
    //  |   with our grid
    //  ----------------------------------------------------

    private Vertex createVertexInCenter(Vertex[] vertices, GameObject verticesGameObject) {

        // Get the actual center pos by adding all of our vertices positions together and
        // dividing it by their amount
        Vector3 centerPos = new Vector3();
        foreach (Vertex v in vertices) {
            centerPos += v.position;
        }
        centerPos /= vertices.Length;

        // Get the cell in our grid that is closest to our center pos
        centerPos = Grid.getSelectedCell(centerPos, false);

        // if a vertex already exists in this cell, we just use this one
        Vertex vertex = this.checkForVertex(centerPos);
        if (vertex == null) {
            // Otherwise we create a new one at this position
            vertex = this.createVertex(centerPos, verticesGameObject);
        }

        return vertex;
    }

    //  ----------------------------------------------------
    //  |   Get the one edge that starts at the vertex
    //  |   "start" and ends at the vertex "end", if one
    //  |   exists
    //  ----------------------------------------------------

    public HalfEdge getEdgeBetweenVertices(Vertex start, Vertex end) {

        // Retrieve all edges that start with the given vertex "start"
        HalfEdge[] foundEgdes = this.getEdgesStartingWith(start);
        HalfEdge foundEdge = null;

        // Foreach edge check if the startVertex of the next edge is our
        // "end" vertex
        foreach (HalfEdge edge in foundEgdes) {
            if (end.Equals(this.next(edge).startVertex)) {
                foundEdge = edge;
                break;
            }
        }

        return foundEdge;
    }

    //  ----------------------------------------------------
    //  |   Retrieve all HalfEdges that start at the given
    //  |   vertex "startVertex"
    //  ----------------------------------------------------

    public HalfEdge[] getEdgesStartingWith(Vertex startVertex) {

        List<HalfEdge> foundEdges = new List<HalfEdge>();

        // Iterate over the edges array and collect every edge where
        // the start vertex is our given vertex "startVertex"
        foreach (HalfEdge e in this.edges) {
            if (e.startVertex.Equals(startVertex)) {
                foundEdges.Add(e);
            }
        }

        return foundEdges.ToArray();
    }

    //  ----------------------------------------------------
    //  |   Retrive all three half edges that make up the
    //  |   that the given half edge "edge" associates with
    //  ----------------------------------------------------

    public Vertex[] getTriangleVertices(HalfEdge edge) {
        HalfEdge nextEdge = this.next(edge);
        HalfEdge prevEdge = this.prev(edge);

        return new Vertex[3] { prevEdge.startVertex, edge.startVertex, nextEdge.startVertex };
    }

    //  ----------------------------------------------------
    //  |   Check if a vertex exists at the given position
    //  ----------------------------------------------------
    public Vertex checkForVertex(Vector3 position) {
        foreach (GameObject vertex in this.vertices) {
            if (vertex.transform.position.Equals(position)) {
                return vertex.GetComponent<VertexScript>().vertex;
            }
        }
        return null;
    }

    //  ----------------------------------------------------
    //  |   Check if a vertex exists at the given position
    //  ----------------------------------------------------
    public GameObject checkForVertexObject(Vector3 position) {
        foreach (GameObject vertex in this.vertices) {
            if (vertex.transform.position.Equals(position)) {
                return vertex;
            }
        }
        return null;
    }

    public void cleanVertices() {
        this.vertices.RemoveAll(e => e == null);
    }

    //  ----------------------------------------------------
    //  |   Check if the three given vertices already make
    //  |   up a triangle
    //  ----------------------------------------------------
    public bool doesTriangleExist(Vertex v0, Vertex v1, Vertex v2) {

        Vertex[] vertices = this.orientateVertices(v0, v1, v2);

        for (int i = 0; i < 3; ++i) {
            if (this.getEdgeBetweenVertices(vertices[i], vertices[(i + 1) % 3]) == null) {
                return false;
            }
        }
        return true;
    }

    #endregion

    #region File Handling

    public string[] serializeVertices() {

        List<string> verticesData = new List<string>();
        foreach(GameObject vertexObj in this.vertices) {
            Vertex vertex = vertexObj.GetComponent<VertexScript>().vertex;

            string x = vertex.position.x.ToString().Replace(',', '.');
            string y = vertex.position.y.ToString().Replace(',', '.');
            string z = vertex.position.z.ToString().Replace(',', '.');

            verticesData.Add(x + ";" + y + ";" + z + ";" + this.edges.IndexOf(vertex.halfEdge) + ";");
        }
        return verticesData.ToArray();
    }

    public string[] serializeEdges() {
        List<string> edgesData = new List<string>();
        foreach(HalfEdge edge in this.edges) {
            int indexOfVertex = this.getIndexOfVertexAtPosition(edge.startVertex.position);
            if (indexOfVertex == -1) {
                throw new Exception("DirectedEdgeDataStructure.serializeEdges: the index of a vertex was -1. Aborting serialization");
            }
            edgesData.Add(indexOfVertex + ";" + this.edges.IndexOf(edge.opposite) + ";");
        }

        return edgesData.ToArray();
    }

    private int getIndexOfVertexAtPosition(Vector3 position) {
        for(int i = 0; i < this.vertices.Count; ++i) {
            GameObject vertexObj = this.vertices[i];
            if (vertexObj.transform.position.Equals(position)) {
                return i;
            }
        }

        return -1;
    }

    #endregion
}

/*
 * A simple struct designed to easily pass around a group of half edges
 */
public struct Triangle
{
    public HalfEdge e0;
    public HalfEdge e1;
    public HalfEdge e2;

    public Triangle(HalfEdge e0, HalfEdge e1, HalfEdge e2) {
        this.e0 = e0;
        this.e1 = e1;
        this.e2 = e2;
    }

    public Triangle(List<HalfEdge> edges) {
        this.e0 = edges[0];
        this.e1 = edges[1];
        this.e2 = edges[2];
    }
}

/*
 *  Half edges store their start vertex and their opposite half edge, so that
 *  connectivity information can be retrieved
 */
public class HalfEdge
{

    public Vertex startVertex { get; set; }
    public HalfEdge opposite { get; set; }

    public HalfEdge(Vertex startVertex) {
        this.startVertex = startVertex;
    }

    public HalfEdge(Vertex startVertex, HalfEdge opposite) {
        this.startVertex = startVertex;
        this.opposite = opposite;
    }

    public override String ToString() {
        string secondHalf = this.opposite != null ? this.opposite.startVertex.ToString() : "opp not set";

        return "HalfEdge (" + this.startVertex + "; " + secondHalf + ")";
    }
}

/**
 * Vertices only store one of the half edges that start with them
 */
public class Vertex {

    // A half edge at the start point of this vertex
    public HalfEdge halfEdge { get; set; }

    // The position of the vertex
    public Vector3 position { get; set; }

    public Vertex(Vector3 position) {
        this.halfEdge = null;
        this.position = position;
    }

    public Vertex(Vector3 position, HalfEdge halfEdge) {
        this.halfEdge = halfEdge;
        this.position = position;
    }

    public override String ToString() {
        return "Vertex @(" + this.position.x + "; " + this.position.y + "; " + this.position.z + ")";
    }

    public override bool Equals(object obj) {

        if (obj == null) {
            return false;
        }

        return this.position.Equals(((Vertex) obj).position);
    }

    public override int GetHashCode() {
        return base.GetHashCode();
    }
}

/*
 * A simple struct to pass around information about removed and replacing triangles
 */
public struct TriangleReplacement
{
    public Triangle[] oldTriangles;
    public Triangle[] newTriangles;
    public TriangleReplacement(Triangle[] oldTriangles, Triangle[] newTriangles) {
        this.oldTriangles = oldTriangles;
        this.newTriangles = newTriangles;
    }
}