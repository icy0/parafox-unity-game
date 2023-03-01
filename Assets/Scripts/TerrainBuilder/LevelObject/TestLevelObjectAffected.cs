using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestLevelObjectAffected : Affected {

    public override bool evaluate() {

        foreach (Node n in this.predecessors) {
            if (!n.output()) {
                return false;
            }
        }

        Debug.Log("POMMES!");
        return true;
    }

}
