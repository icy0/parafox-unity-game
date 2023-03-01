using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    Arm Arm;

    private const float maxSteepSlope = 45.0f;

    [SerializeField]
    private BoxCollider2D playerBoxCollid;

    [SerializeField]
    private float speed = 3f;

    [SerializeField]
    private float jumpForce = 10f;

    private Rigidbody2D player;

    [SerializeField]
    private Transform groundPoint;

    [SerializeField]
    private LayerMask whatIsGround;

    [SerializeField]
    private float fallMultiplier = 2.5f;    

    public GameObject leftCorner;

    public GameObject rightCorner;

    public GameObject middle; 

    private int groundLayer = 1 << 8;

    private int additionalLayer = 1 << 11;

    private int platformgroundLayer = 1 << 10;

    private bool facingRight = true;

    private float canJump = 0f;

    private bool spaceBar = false;

    private float move;

    private Vector3 nullVelocity = Vector3.zero;    

    Vector3 rotatedVector = new Vector3(1, 0, 1);

    private bool isSteepSlope;

    private bool isJumping; // true solange in der Luft

    private Animator Animator;

    private Vector2 currentMousePos;



    // Start is called before the first frame update
    void Start()
    {
        Animator = transform.Find("fox").GetComponent<Animator>();
        player = GetComponent<Rigidbody2D>();
        Arm = GetComponent<Arm>();
        currentMousePos = new Vector2();
    }

    private void Update()
    {         
        HandleMovement();        
        if(IsGrounded() || IsPlatformGrounded())
        {
            Animator.SetBool("isGrounded", true);
            Animator.ResetTrigger("takeOf");
            isJumping = false;
            Animator.SetBool("isLanding", false);            
        }
        else
        {
            isJumping = true;
            Animator.SetBool("isGrounded", false);
        }

        if(IsGrounded())
        {
            GetComponent<PlatformSpawn>().resetCounter();
        }

        isSteepSlope = IsSteepSlope();
    }

    public Vector2 GetHandPosition()
    {
        return Arm.GetHandPosition();
    }

    //Handles the case jumping, falling, left and right moving and slope climbing.
    private void HandleMovement()
    {
        //For flipping the fox to prevent a broken shoulder        
        HandleArmPosition();
 
        Vector2 playerScreenPoint = Camera.main.WorldToScreenPoint(player.transform.position);
        //Debug.Log("Mouse pos " + currentMousePos.x + " player pos " + playerScreenPoint.x);
        float horizontal = (Input.GetAxis("Horizontal"));
        if (((Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)) && IsGrounded()))
        {            
            if (horizontal > 0 && !facingRight)
            {
                Animator.SetBool("isBackwardRunning", true);
                Animator.SetBool("isRunning", false);
            }
            else if (horizontal > 0 && facingRight)
            {
                Animator.SetBool("isBackwardRunning", false);
                Animator.SetBool("isRunning", true);
            }
            else if (horizontal < 0 && facingRight)
            {
                Animator.SetBool("isBackwardRunning", true);
                Animator.SetBool("isRunning", false);
            }
            else if (horizontal < 0 && !facingRight)
            {
                Animator.SetBool("isBackwardRunning", false);
                Animator.SetBool("isRunning", true);
            }
        }       
        else
        {
            Animator.SetBool("isRunning", false);
            Animator.SetBool("isBackwardRunning", false);
        }

        Vector3 vector3 = new Vector3(Input.GetAxis("Horizontal"), 0, 0f);
        move = vector3.x;
        Flip(Input.GetAxis("Horizontal"), playerScreenPoint.x);
        //float maxSpeed = 200f;
        //player.AddForce(vector3 * speed * Time.deltaTime) ;

        //If the slope have a angle under 45 degree and greater 0 degree, then slope is possible to climb.
        if (!IsSteepSlope())
        {
            Vector3 targetVelocity = new Vector2(move * speed, player.velocity.y - 0.15f);
            player.velocity = Vector3.SmoothDamp(player.velocity, targetVelocity, ref nullVelocity, 0.05f);
            //player.transform.position += vector3 * Time.deltaTime * speed;
            if (Math.Abs(move) <= 0.1f)
            {
                player.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
            }
            else
            {
                player.constraints = RigidbodyConstraints2D.FreezeRotation;
            }
            
        }
        else
        {
            player.constraints = RigidbodyConstraints2D.FreezeRotation;
        }


        if (Input.GetButtonDown("Jump") && !isSteepSlope) 
        {            
           
            Jump();
        }
        ApplyFallMultiplier();        
    }

    private void HandleArmPosition()
    {
        currentMousePos.x = Input.mousePosition.x;
        currentMousePos.y= Input.mousePosition.y;
    }

    //If the player is falling, a fall multiplier will be added.
    private void ApplyFallMultiplier()
    {
        if (player.velocity.y < 0) //Player is falling
        {
            Animator.SetBool("isLanding", true);
            Animator.SetTrigger("isLandingTrigger");
            Animator.SetBool("isJumping", false);            
            player.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }
       
    }

    //Flips the player horizontal left or right.
    private void Flip(float horizontal, float playerScreenPoint)
    {

        //Facing right and mouse is on the left side of the character
        if (currentMousePos.x < playerScreenPoint && facingRight)
        {
            Debug.Log("FAcing right and mouse is left SIde");
            facingRight = !facingRight;
            Vector3 scaleValue = player.transform.localScale;
            scaleValue.x *= -1;
            player.transform.localScale = scaleValue;
            //Animator.SetBool("isBackwardRunning", true);

        }
        //Facing left and mouse is on the right side of the character
        else if (currentMousePos.x > playerScreenPoint && !facingRight)
        {
            Debug.Log("FAcing left and mouse is right SIde");
            facingRight = !facingRight;
            Vector3 scaleValue = player.transform.localScale;
            scaleValue.x *= -1;
            player.transform.localScale = scaleValue;
            //Animator.SetBool("isBackwardRunning", false);
        }


    }
    private bool IsSteepSlope()
    {

        //rotatedVector - The Vector in which direction the new force will be added.
        //groundNormal - The normal of the current ground where the player is standing on.
        //input - The current horizontally force.
        //angle - The angle in degrees of the groundNormal and Verical up Vector.         
        Vector2 groundNormal;
        Vector2 vectorDownL = new Vector2(leftCorner.transform.position.x, leftCorner.transform.position.y + 0.5f);
        Vector2 vectorDownR = new Vector2(rightCorner.transform.position.x, rightCorner.transform.position.y + 0.5f);
        Vector2 groundNormalLeft = Physics2D.Raycast(vectorDownL, Vector2.down, 1f, groundLayer).normal;
        Vector2 groundNormalRight = Physics2D.Raycast(vectorDownR, Vector2.down, 1f, groundLayer).normal;
        Vector2 groundNormalInterpolated = (groundNormalLeft + groundNormalRight) / 2;

        
        float input = Input.GetAxis("Horizontal");
        float angleLeft = Vector2.Angle(groundNormalLeft, Vector2.up);
        float angleRight = Vector2.Angle(groundNormalRight, Vector2.up);
        float angle = Vector2.Angle(groundNormalInterpolated, Vector2.up);

        if ((IsGrounded() || IsPlatformGrounded()) && facingRight)
        {
            //When the player climbing the slope and facing right
            rotatedVector = Vector2.Perpendicular(-groundNormalInterpolated);
        }
        else if ((IsGrounded() || IsPlatformGrounded()) && !facingRight)
        {
            //When the player is climbing the slope and facing left
            rotatedVector = Vector2.Perpendicular(groundNormalInterpolated);
        }

        Debug.DrawRay(transform.position, groundNormalInterpolated * 1, Color.cyan);
        // Debug.Log("Winkel:" + angle);
        Debug.DrawLine(transform.position, transform.position + rotatedVector, Color.red);

        return angle > maxSteepSlope;

    }


    //This method will be called if the player presses the spacebar
    private void Jump()
    {
        if((IsGrounded() || IsPlatformGrounded()) && !IsSteepSlope() )
        {
            Animator.SetTrigger("takeOf");
            isJumping = true;
            Animator.SetBool("isJumping", true);

            Vector3 velocity = player.velocity;
            velocity.y = jumpForce;            
            player.velocity = velocity;
            player.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);

        }       
    }

    public bool IsPlatformGrounded()
    {
        Vector2 diagonalVectorDownL = leftCorner.transform.position;
        Vector2 vectorDownL = new Vector2(diagonalVectorDownL.x, diagonalVectorDownL.y + 0.5f);

        Vector2 diagonalVectorDownR = rightCorner.transform.position;
        Vector2 vectorDownR = new Vector2(diagonalVectorDownR.x, diagonalVectorDownR.y + 0.5f);
        Vector2 vertikalVectorDownM = middle.transform.position;

        //hit - If the Ray is colliding with the ground layer, hit will be true.
        //distance - The distance from the rays from the left and right corner and the middlepoint.
        //scaleValue - The sclaeValue for the left and right corner rays, that goes vertical down.
        bool hit = false;
        float distance = Vector2.Distance(leftCorner.transform.position, middle.transform.position) * 2;
        float scaleValue = 0.05f;
        //Debug.Log("Distance is: " + distance);

        //For debug purposes
        //TODO distance is only half as long then the real size, so I added the magic value 2 until I found what the cause of this is.
        Debug.DrawRay(diagonalVectorDownL, (vertikalVectorDownM - diagonalVectorDownL) * distance, Color.cyan); //Left Ray diagonal
        Debug.DrawRay(diagonalVectorDownR, (vertikalVectorDownM - diagonalVectorDownR) * distance, Color.cyan); //Right Ray diagonal
        Debug.DrawRay(vectorDownL, Vector2.down * (scaleValue + 0.5f), Color.magenta);
        Debug.DrawRay(vectorDownR, Vector2.down * (scaleValue + 0.5f), Color.magenta);

        //This Ray starts from the LEFTCORNER of the player and goes TO the MIDDLE of the player, diagonal
        if (Physics2D.Raycast(diagonalVectorDownL, vertikalVectorDownM - diagonalVectorDownL, distance, platformgroundLayer))
        {
            hit = true;
        }
        //This Ray starts from the LEFTCORNER of the player and goes with the distance of the scaleValue VERTICAL DOWN
        if (Physics2D.Raycast(vectorDownL, Vector2.down, scaleValue + 0.5f, platformgroundLayer))
        {
            hit = true;
        }
        //This Ray starts from the RIGHTCORNER of the player and goes TO the MIDDLE of the player, diagonal
        if (Physics2D.Raycast(diagonalVectorDownR, vertikalVectorDownM - diagonalVectorDownR, distance, platformgroundLayer))
        {
            hit = true;
        }
        //This Ray starts from the RIGHTCORNER of the player and goes with the distance of the scaleValue VERTICAL DOWN
        if (Physics2D.Raycast(vectorDownR, Vector2.down, scaleValue + 0.5f, platformgroundLayer))
        {
            hit = true;
        }

        return hit;
    }
  

    //Checks with Raytraces if the player is on the ground
    public bool IsGrounded()
    {
        //Initialize all Vector with the Gameobjects Transforms
        Vector2 diagonalVectorDownL = leftCorner.transform.position;
        Vector2 vectorDownL = new Vector2(diagonalVectorDownL.x, diagonalVectorDownL.y + 0.5f);
        
        Vector2 diagonalVectorDownR = rightCorner.transform.position;
        Vector2 vectorDownR = new Vector2(diagonalVectorDownR.x, diagonalVectorDownR.y + 0.5f);
        Vector2 vertikalVectorDownM = middle.transform.position;
        
        //hit - If the Ray is colliding with the ground layer, hit will be true.
        //distance - The distance from the rays from the left and right corner and the middlepoint.
        //scaleValue - The sclaeValue for the left and right corner rays, that goes vertical down.
        bool hit = false;
        float distance = Vector2.Distance(leftCorner.transform.position, middle.transform.position) * 3f;
        float scaleValue = 0.05f;
        //Debug.Log("Distance is: " + distance);

        //For debug purposes
        //TODO distance is only half as long then the real size, so I added the magic value 2.6 until I found what the cause of this is.
        Debug.DrawRay(diagonalVectorDownL, (vertikalVectorDownM - diagonalVectorDownL) * distance, Color.cyan); //Left Ray diagonal
        Debug.DrawRay(diagonalVectorDownR, (vertikalVectorDownM - diagonalVectorDownR) * distance, Color.cyan); //Right Ray diagonal
        Debug.DrawRay(vectorDownL, Vector2.down * (scaleValue + 0.5f), Color.magenta);
        Debug.DrawRay(vectorDownR, Vector2.down * (scaleValue + 0.5f), Color.magenta);

        //This Ray starts from the LEFTCORNER of the player and goes TO the MIDDLE of the player, diagonal
        if (Physics2D.Raycast(diagonalVectorDownL, vertikalVectorDownM - diagonalVectorDownL, distance, groundLayer | additionalLayer))
        {            
            hit = true;
        }
        //This Ray starts from the LEFTCORNER of the player and goes with the distance of the scaleValue VERTICAL DOWN
        if (Physics2D.Raycast(vectorDownL, Vector2.down , scaleValue + 0.5f, groundLayer | additionalLayer))
        {
            hit = true;
        }
        //This Ray starts from the RIGHTCORNER of the player and goes TO the MIDDLE of the player, diagonal
        if (Physics2D.Raycast(diagonalVectorDownR, vertikalVectorDownM - diagonalVectorDownR, distance, groundLayer | additionalLayer))
        {            
            hit = true;
        }
        //This Ray starts from the RIGHTCORNER of the player and goes with the distance of the scaleValue VERTICAL DOWN
        if (Physics2D.Raycast(vectorDownR, Vector2.down , scaleValue + 0.5f, groundLayer | additionalLayer))
        {
            hit = true;
        }

        return hit;        
    }

    
}