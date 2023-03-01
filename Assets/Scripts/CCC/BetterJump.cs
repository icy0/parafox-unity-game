using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BetterJump : MonoBehaviour
{

    private Rigidbody2D player;
    [SerializeField]
    private float fallMultiplier = 2.0f;
    [SerializeField]
    private float lowJumpMultiplier = 2.5f;

    private Animator Animator;

    private void Awake()
    {
        player = GetComponent<Rigidbody2D>();
        Animator = transform.Find("fox").GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {


        if (player.velocity.y < 0) //Player is falling
        {
            Animator.SetBool("isLanding", true);
            Animator.SetBool("isJumping", false);
            player.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }
        else if (player.velocity.y > 0 && !Input.GetButtonDown("Jump"))
        {
            player.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
        }
    }
}
