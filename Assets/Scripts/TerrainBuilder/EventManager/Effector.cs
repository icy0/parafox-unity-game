using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public abstract class Effector : Node {

    public bool isTriggered { get; set; }

    void Start() {
        this.isTriggered = false;
    }

    public override bool isOutput() {
        return true;
    }

    public override void addPredecessor(Node n) {}

    public override bool output() {
        return this.isTriggered;
    }
}