using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GateAffected : Affected {

    private static float TIME_TO_OPEN = 5.0F;
    private static float TIME_TO_CLOSE = 1.0F;

    public GameObject gate;

    private Vector3 openPos;
    private Vector3 closedPos;

    public Vector3 travelEnd = Vector3.zero;
    public bool shouldOpen = false;

    void Start() {
        this.openPos = new Vector3(0.0F, 4.0F, 0.0F);
        this.closedPos = Vector3.zero;
    }

    public override bool evaluate() {

        bool isValid = true;

        foreach (Node n in this.predecessors) {
            if (!n.output()) {
                isValid = false;
                break;
            }
        }

        if (isValid && !this.shouldOpen) {
            this.travelEnd = this.openPos;
            this.shouldOpen = true;
        } else if (!isValid && this.shouldOpen) {
            this.travelEnd = this.closedPos;
            this.shouldOpen = false;
        } else {

        }

        return isValid;
    }

    void Update() {

        Vector3 currentPos = this.gate.transform.localPosition;
        Vector3 travelPerSecond;

        if (this.shouldOpen) {
            travelPerSecond = (this.openPos - this.closedPos) / GateAffected.TIME_TO_OPEN;
            this.gate.transform.localPosition = this.min(currentPos + travelPerSecond * Time.deltaTime, this.openPos);
        } else {
            travelPerSecond = (this.closedPos - this.openPos) / GateAffected.TIME_TO_CLOSE;
            this.gate.transform.localPosition = this.max(currentPos + travelPerSecond * Time.deltaTime, this.closedPos);
        }
    }

    private Vector3 max(Vector3 v0, Vector3 v1) {
        return new Vector3(Mathf.Max(v0.x, v1.x), Mathf.Max(v0.y, v1.y), v0.z);
    }

    private Vector3 min(Vector3 v0, Vector3 v1) {
        return new Vector3(Mathf.Min(v0.x, v1.x), Mathf.Min(v0.y, v1.y), v0.z);
    }
}
