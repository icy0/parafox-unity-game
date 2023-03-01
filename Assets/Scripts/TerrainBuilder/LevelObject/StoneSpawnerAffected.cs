using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoneSpawnerAffected : Affected {

    public static float SPAWNER_TIMEOUT = 0.5F;

    public Object stoneAsset;

    private float timeSinceLastSpawn = 0.0F;

    void Start() {
        if (this.GetComponent<SpriteRenderer>() != null) {
            this.GetComponent<SpriteRenderer>().enabled = false;
        }
    }

    public override bool evaluate() {

        this.timeSinceLastSpawn += Time.deltaTime;

        foreach (Node n in this.predecessors) {
            if (!n.output()) {
                return false;
            }
        }

        if (this.stoneAsset != null && this.timeSinceLastSpawn >= StoneSpawnerAffected.SPAWNER_TIMEOUT) {
            this.timeSinceLastSpawn = 0.0F;
            GameObject.Instantiate(this.stoneAsset, this.gameObject.transform.position, this.gameObject.transform.rotation, null);
        }
        return true;
    }
}