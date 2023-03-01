using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PressureSwitchEffector : Effector {

    public Sprite inactiveSprite;
    public Sprite activeSprite;

    private int objectsInsideTrigger = 0;

    public override bool evaluate() {
        return this.isTriggered;
    }

    void Update() {
        SpriteRenderer sr = this.GetComponent<SpriteRenderer>();
        if (this.objectsInsideTrigger > 0) {
            sr.sprite = this.activeSprite;
        } else {
            sr.sprite = this.inactiveSprite;
        }
    }

    void OnTriggerEnter2D(Collider2D collider) {
        if (collider.gameObject.GetComponent<TelekinetableRockLevelObject>() != null || collider.gameObject.tag.Equals("Player")) {
            this.objectsInsideTrigger++;
            this.isTriggered = true;
        }
    }

    void OnTriggerExit2D(Collider2D collider) {
        if (collider.gameObject.GetComponent<TelekinetableRockLevelObject>() != null || collider.gameObject.tag.Equals("Player")) {
            this.objectsInsideTrigger--;
            
            if (this.objectsInsideTrigger <= 0) {
                this.objectsInsideTrigger = 0;
                this.isTriggered = false;
            }
        }
    }
}
