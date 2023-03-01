using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapAffected : Affected {

    private static float TIME_TILL_DESTRUCTION = 0.25F;
    private static int LOGS_TO_SPAWSN = 3;
    private static int LEAFS_TO_SPAWSN = 5;

    public Object log;
    public Object leafs;

    private float timeOnTrap = 0.0F;

    public override bool evaluate() {
        
        foreach (Node n in this.predecessors) {
            if(!n.output()) {
                this.timeOnTrap = 0.0F;
                return false;
            }
        }

        if (this.timeOnTrap >= TrapAffected.TIME_TILL_DESTRUCTION) {
            this.GetComponent<SpriteRenderer>().enabled = false;
            this.GetComponent<BoxCollider2D>().enabled = false;

            if (this.log != null) {
                for (int i = 0; i < TrapAffected.LOGS_TO_SPAWSN; ++i) {

                    float xOffset = Random.Range(-0.5F, 0.5F);
                    Vector3 spawnPosition = new Vector3(this.transform.position.x + xOffset, this.transform.position.y, this.transform.position.z);

                    GameObject.Instantiate(this.log, this.transform.position, this.transform.rotation, this.transform);
                }
            }

            if (this.log != null) {
                for (int i = 0; i < TrapAffected.LEAFS_TO_SPAWSN; ++i) {

                    float xOffset = Random.Range(-0.5F, 0.5F);
                    Vector3 spawnPosition = new Vector3(this.transform.position.x + xOffset, this.transform.position.y, this.transform.position.z);

                    GameObject.Instantiate(this.leafs, this.transform.position, this.transform.rotation, this.transform);
                }
            }

            return true;
        }

        this.timeOnTrap += Time.deltaTime;
        return false;
    }
}
