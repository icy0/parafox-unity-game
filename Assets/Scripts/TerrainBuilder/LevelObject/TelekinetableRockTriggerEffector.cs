using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TelekinetableRockTriggerEffector : Effector {

    public override bool evaluate() {
        return this.isTriggered;
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.GetComponent<TelekinetableRockLevelObject>() != null) {
            this.isTriggered = true;
        }
    }

    void OnTriggerExit2D(Collider2D other) {
        if (other.gameObject.GetComponent<TelekinetableRockLevelObject>() != null) {
            this.isTriggered = false;
        }
    }
}