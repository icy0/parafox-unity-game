using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Affected : Node {

    public override bool isOutput() {
        return false;
    }

    public override bool evaluate() {
        return true;
    }

    public override bool output() {
        return false;
    }
}