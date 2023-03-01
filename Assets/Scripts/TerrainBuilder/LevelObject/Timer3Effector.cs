using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer3Effector : Effector {

    private const float TARGET_TIME = 3.0F;
    private float timePassed;

    private void Start() {
        SpriteRenderer renderer = this.GetComponent<SpriteRenderer>();
        renderer.enabled = false;
        this.timePassed = 0;
    }

    public override bool evaluate() {

        this.timePassed += Time.deltaTime;

        if (this.timePassed >= TARGET_TIME) {
            this.timePassed = 0.0F;
            this.isTriggered = true;
        } else {
            this.isTriggered = false;
        }

        return this.isTriggered;
    }
}
