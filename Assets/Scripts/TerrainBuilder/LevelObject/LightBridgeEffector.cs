using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightBridgeEffector : Effector {

    public GameObject lightGameObject;

    private int objectsInTrigger = 0;

    public override bool evaluate() {
        return this.isTriggered;
    }

    void Update() {

        if (this.lightGameObject != null) {
            SpriteRenderer sr = this.lightGameObject.GetComponent<SpriteRenderer>();
            sr.enabled = !this.isTriggered;
        }
    }

    void OnTriggerEnter2D(Collider2D collider) {
        if (collider.gameObject.tag == "Telekinetable") {
            return;
        }

        this.objectsInTrigger++;
        this.isTriggered = true;
    }

    void OnTriggerExit2D(Collider2D collider) {

        if (collider.gameObject.tag == "Telekinetable") {
            return;
        }

        this.objectsInTrigger--;
        if (this.objectsInTrigger <= 0) {
            this.objectsInTrigger = 0;
            this.isTriggered = false;
        }
    }
}
