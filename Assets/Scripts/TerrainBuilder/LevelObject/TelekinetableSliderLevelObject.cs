using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TelekinetableSliderLevelObject : LevelObjectScript {

    public float railLength = 1.0F;

    public GameObject rail;
    public GameObject telekinetableTrunk;
    public GameObject block;

    void Update() {

        if (this.rail != null) {
            this.rail.transform.localScale = new Vector3(1.0F, this.railLength, 1.0F);
        }

        if (this.block != null) {
            this.block.transform.localPosition = new Vector3(0.0F, 1.4F + (this.railLength - 1.0F), 0.0F);
        }

        if (this.telekinetableTrunk != null) {

            this.telekinetableTrunk.transform.localPosition = new Vector3(0.0F, -this.railLength + 1.0F, 0.0F);

            RestrictedTelekinetable rt = this.telekinetableTrunk.GetComponent<RestrictedTelekinetable>();

            Vector2 path = new Vector2(0.0F, 2.0F * this.railLength);
            path = Quaternion.AngleAxis(this.transform.rotation.z, Vector3.forward) * path;

            rt.SetRestrictionPath(path);
        }
    }

    public override void configureLevelObject(string[] attributes, Level level) {
        base.configureLevelObject(attributes, level);
        this.railLength = float.Parse(attributes[13]);
    }

    public override string serialize(Level level) {
        return base.serialize(level) + this.railLength + ";";
    }
}
