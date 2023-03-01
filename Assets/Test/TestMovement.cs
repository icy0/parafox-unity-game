using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMovement : MonoBehaviour {

    private bool isJumping = false;
    private int jumpCounter;

    void Start() {
    }

    void FixedUpdate() {

        float horizontal = Input.GetAxis("Horizontal");
        this.transform.position = new Vector3(this.transform.position.x + (horizontal * 0.2F), this.transform.position.y, this.transform.position.z);

        if (Input.GetButtonDown("Jump") && !this.isJumping) {
            this.isJumping = true;
            this.jumpCounter = 20;
        }

        if (this.jumpCounter > 0) {
            this.jumpCounter--;
            this.transform.position = new Vector3(this.transform.position.x, this.transform.position.y + 0.1F, this.transform.position.z);
        }

        Debug.DrawRay(this.transform.position, Vector2.down * 1.1F, Color.red);
        if (this.jumpCounter == 0 && Physics2D.Raycast(this.transform.position, Vector2.down, 1.1F, 1 << 8)) {
            this.isJumping = false;
        }
    }
}
