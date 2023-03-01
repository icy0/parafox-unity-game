using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightBrideLevelObject : LevelObjectScript {

    public GameObject nippleLeft;
    public GameObject nippleRight;
    public GameObject lightGameObject;

    public float laserLength = 1.0F;

    void Update() {
        
        if (this.nippleLeft != null) {
            this.nippleLeft.transform.localPosition = new Vector3(-this.laserLength / 2.0F, 0.0F, 0.0F);
        }

        if (this.nippleRight != null) {
            this.nippleRight.transform.localPosition = new Vector3(this.laserLength / 2.0F, 0.0F, 0.0F);
        }

        if (this.lightGameObject != null) {
            this.lightGameObject.transform.localScale = new Vector3(this.laserLength, 1.0F, 1.0F);
        }

        if (this.GetComponent<BoxCollider2D>() != null) {
            this.GetComponent<BoxCollider2D>().size = new Vector2(this.laserLength, 0.1F);
        }
    }

    public override void configureLevelObject(string[] attributes, Level level) {
        base.configureLevelObject(attributes, level);
        this.laserLength = float.Parse(attributes[13]);
    }

    public override string serialize(Level level) {
        return base.serialize(level) + this.laserLength + ";";
    }


}
