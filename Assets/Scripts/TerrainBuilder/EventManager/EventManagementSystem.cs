using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
public abstract class Node : MonoBehaviour {

    public static Vector2 NODE_SIZE = new Vector2(100.0F, 30.0F);

    public static int ID = 0;

    [SerializeField]
    public int id = -1;

    [SerializeField]
    public string nodeName { get; set; }

    [SerializeField]
    public Vector2 position { get; set; }

    [SerializeField]
    public List<Node> predecessors;

    public void initialize(string nodeName, float nodeCoordX, float nodeCoordY) {
        this.id = ID++;
        this.nodeName = nodeName;
        this.predecessors = new List<Node>();
        this.position = new Vector2(nodeCoordX, nodeCoordY);
    }

    public virtual void configureNode(string[] nodeInformation) {

        this.id = int.Parse(nodeInformation[1]);
        this.nodeName = nodeInformation[2];
        float x = float.Parse(nodeInformation[3]);
        float y = float.Parse(nodeInformation[4]);

        this.position = new Vector2(x, y);

        this.predecessors = new List<Node>();
    }

    public abstract bool output();

    public abstract bool isOutput();
    public abstract bool evaluate();

    public virtual void addPredecessor(Node n) {
        if (!this.predecessors.Contains(n)) {
            this.predecessors.Add(n);
        }
    }

    public virtual void removePredecessor(Node n) {
        this.predecessors.Remove(n);
    }

    public int getPredecessorCount() {
        int predecessorCount = 0;
        for (int i = 0; i < this.predecessors.Count; ++i) {
            predecessorCount += this.predecessors[i].getPredecessorCount() + 1;
        }

        return predecessorCount;
    }

    public override bool Equals(System.Object obj) {
        if ((obj == null) || !this.GetType().Equals(obj.GetType())) {
            return false;
        } else {
            return this.id == ((Node) obj).id;
        }
    }

    public override int GetHashCode() {
        return base.GetHashCode();
    }

    public Vector2 getOutputPosition() {
        return this.position + Node.NODE_SIZE * new Vector2(1.0F, 0.5F);
    }

    public Vector2 getInputPosition() {
        return this.position + Node.NODE_SIZE * new Vector2(0.0F, 0.5F);
    }

    public Rect getRect() {
        return new Rect(this.position, Node.NODE_SIZE);
    }

    public virtual string serialize() {

        int levelObjectID = this.gameObject.GetComponent<LevelObjectScript>().id;

        string output = levelObjectID + ";" + this.id + ";" + this.nodeName + ";" + this.position.x + ";" + this.position.y + ";" + this.predecessors.Count + ";";

        foreach(Node n in this.predecessors) {
            output += n.id + ";";
        }

        return output;
    }
}

[Serializable]
public class EventGroup {

    [SerializeField]
    public string name { get; set; }

    [SerializeField]
    public List<Node> nodes { get; private set; }

    [SerializeField]
    private Comparison<Node> sorter = (n1, n2) => {
        return (n1.getPredecessorCount() <= n2.getPredecessorCount()) ? -1 : 1;
    };

    public EventGroup(string name) {
        this.name = name;
        this.nodes = new List<Node>();
    }

    public void update() {
        foreach (Node n in this.nodes) {
            n.evaluate();
        }
    }

    public void addNode(Node node, string name, float nodeCoordX, float nodeCoordY) {
        node.initialize(name, nodeCoordX, nodeCoordY);
        this.nodes.Add(node);
        this.sort();
    }

    public void setSuccessor(Node node, Node successor) {
        if (node != null) {
            node.predecessors.Add(successor);
        }
    }

    public void setSuccessors(Node node, List<Node> successors) {
        if (node != null && successors != null) {
            node.predecessors.AddRange(successors);
        }
    }

    public void removeNode(Node node) {

        foreach(Node n in this.nodes) {
            if (n.predecessors.Contains(node)) {
                n.predecessors.Remove(node);
            }
        }

        this.nodes.Remove(node);
        this.sort();
    }

    public void sort() {
        if (this.nodes.Count > 1) {
            this.nodes.Sort(this.sorter);
        }
    }

    public string serialize() {
        return this.name + ";";
    }
}

[Serializable]
public class EventManagementSystem {

    [SerializeField]
    public List<EventGroup> eventGroups { get; private set; }

    public EventManagementSystem() {
        this.eventGroups = new List<EventGroup>();
    }
}
