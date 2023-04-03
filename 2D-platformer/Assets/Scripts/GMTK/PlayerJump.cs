using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

//This script handles moving the character on the Y axis, for jumping and gravity

public class PlayerJump : MonoBehaviour {
    public float jumpMovementMultiplier = 1f;
    public float jumpMovementMultiplierX = 1f;
    public float jumpMovementMultiplierY = 1f;
    [Header("Components")]
    [HideInInspector] public Rigidbody2D body;
    private PlayerGround playerGround;
    [HideInInspector] public Vector2 velocity;
    private PlayerEffect playerEffect;
    private PlayerMovement playerMovement;
    private PlayerDash playerDash;
    private PlayerColumn playerColumn;
    private PlayerGrapplingGun playerGrapplingGun;


    [Header("Jumping Stats")]
    [SerializeField, Range(0f, 20f)][Tooltip("Maximum jump height")] public float jumpHeight = 7.3f;
    [SerializeField, Range(0f, 20f)][Tooltip("How long it takes to reach that height before coming back down")] public float timeToJumpApex;
    [SerializeField, Range(0f, 20f)][Tooltip("Gravity multiplier to apply when going up")] public float upwardMovementMultiplier = 1f;
    [SerializeField, Range(0f, 20f)][Tooltip("Gravity multiplier to apply when coming down")] public float downwardMovementMultiplier = 6.17f;
    [SerializeField, Range(0, 1)][Tooltip("How many times can you jump in the air?")] public int maxAirJumps = 0;

    [Header("Options")]
    [Tooltip("Should the character drop when you let go of jump?")] public bool variablejumpHeight;
    [SerializeField, Range(1f, 10f)][Tooltip("Gravity multiplier when you let go of jump")] public float jumpCutOff;
    [SerializeField][Tooltip("The fastest speed the character can fall")] public float speedLimit;
    [SerializeField, Range(0f, 0.3f)][Tooltip("How long should coyote time last?")] public float coyoteTime = 0.15f;
    [SerializeField, Range(0f, 0.3f)][Tooltip("How far from ground should we cache your jump?")] public float jumpBuffer = 0.15f;

    [Header("Calculations")]
    public float jumpSpeed;
    public float defaultGravityScale;
    public float gravMultiplier;

    [Header("Current State")]
    public bool canJumpAgain = false;
    [SerializeField] private bool desiredJump;
    [SerializeField] private bool pressingJump;
    [SerializeField] private bool currentlyJumping;
    public bool onGround;
    private float jumpBufferCounter;
    private float coyoteTimeCounter = 0;
    private Coroutine jumpCoroutine;
    public float beforeJumpDelay = 0.5f;

    void Awake() {
        playerMovement = GetComponent<PlayerMovement>();
        playerColumn = GetComponent<PlayerColumn>();

        //Find the character's Rigidbody and ground detection and juice scripts
        body = GetComponent<Rigidbody2D>();
        playerGround = GetComponent<PlayerGround>();
        playerDash = GetComponent<PlayerDash>();
        playerEffect = GetComponentInChildren<PlayerEffect>();
        playerGrapplingGun = GetComponent<PlayerGrapplingGun>();
        defaultGravityScale = 1f;
    }

    private void StartJump() {
        desiredJump = true;
        pressingJump = true;
    }

    private void StopJump() {
        pressingJump = false;
    }

    public void Jump(float jumpButtonHoldTime) {
        StartJump();
        StartCoroutine(StopJumpCoroutine(jumpButtonHoldTime));
    }

    IEnumerator StopJumpCoroutine(float waitingTime) {
        yield return new WaitForSeconds(waitingTime);
        StopJump();
    }

    IEnumerator StartJumpRoutine() {
        Debug.Log("in start routine");
        playerEffect.jumpEffects();
        yield return new WaitForSeconds(beforeJumpDelay);
        StartJump();
    }

    IEnumerator StopJumpRoutine() {
        Debug.Log("in stop routine");
        // if (jumpCoroutine != null) {
        //     Debug.Log("stopping jump...");
        //     StopCoroutine(jumpCoroutine);
        // }
        jumpCoroutine = null;
        yield return new WaitForSeconds(beforeJumpDelay);
        StopJump();
    }

    public void OnJump(InputAction.CallbackContext context) {
        //This function is called when one of the jump buttons (like space or the A button) is pressed.

        if (MovementLimiter.instance.playerCanMove) {
            //When we press the jump button, tell the script that we desire a jump.
            //Also, use the started and canceled contexts to know if we're currently holding the button
            if (context.started) {
                Debug.Log("context started");
                if (jumpCoroutine == null) {
                    jumpCoroutine = StartCoroutine(StartJumpRoutine());
                }
                // StartJump();
            }

            if (context.canceled) {
                Debug.Log("context end");
                // if (jumpCoroutine != null) {
                StartCoroutine(StopJumpRoutine());
                // }
            }
        }
    }

    void Update() {
        //Check if we're on ground, using Kit's Ground script
        onGround = playerGround.isGrounded;

        if (playerDash.isDashing || playerColumn.hasGrabbedColumn || playerGrapplingGun.grapplingRope.isGrappling) {
            return;
        }
        setPhysics();


        //Jump buffer allows us to queue up a jump, which will play when we next hit the ground
        if (jumpBuffer > 0) {
            //Instead of immediately turning off "desireJump", start counting up...
            //All the while, the DoAJump function will repeatedly be fired off
            if (desiredJump) {
                jumpBufferCounter += Time.deltaTime;

                if (jumpBufferCounter > jumpBuffer) {
                    //If time exceeds the jump buffer, turn off "desireJump"
                    desiredJump = false;
                    jumpBufferCounter = 0;
                }
            }
        }

        //If we're not on the ground and we're not currently jumping, that means we've stepped off the edge of a platform.
        //So, start the coyote time counter...
        if (!currentlyJumping && !onGround) {
            coyoteTimeCounter += Time.deltaTime;
        } else {
            //Reset it when we touch the ground, or jump
            coyoteTimeCounter = 0;
        }
    }

    private void setPhysics() {
        //Determine the character's gravity scale, using the stats provided. Multiply it by a gravMultiplier, used later
        Vector2 newGravity = new Vector2(0, (-2 * jumpHeight) / (timeToJumpApex * timeToJumpApex));
        body.gravityScale = (newGravity.y / Physics2D.gravity.y) * gravMultiplier;
    }

    private void FixedUpdate() {
        if (playerColumn.hasGrabbedColumn || playerGrapplingGun.grapplingRope.isGrappling) {
            currentlyJumping = false;
        }
        if (playerDash.isDashing || playerColumn.hasGrabbedColumn || playerGrapplingGun.grapplingRope.isGrappling) {
            return;
        }
        //Get velocity from Kit's Rigidbody 
        velocity = body.velocity;

        //Keep trying to do a jump, for as long as desiredJump is true
        if (desiredJump) {
            DoAJump();
            body.velocity = velocity * jumpMovementMultiplier;

            //Skip gravity calculations this frame, so currentlyJumping doesn't turn off
            //This makes sure you can't do the coyote time double jump bug
            return;
        }

        calculateGravity();
    }

    private void calculateGravity() {
        //We change the character's gravity based on her Y direction

        //If Kit is going up...
        if (body.velocity.y > 0.01f) {
            if (onGround) {
                //Don't change it if Kit is stood on something (such as a moving platform)
                gravMultiplier = defaultGravityScale;
            } else {
                //If we're using variable jump height...)
                if (variablejumpHeight) {
                    //Apply upward multiplier if player is rising and holding jump
                    if (pressingJump && currentlyJumping) {
                        gravMultiplier = upwardMovementMultiplier;
                    }
                    //But apply a special downward multiplier if the player lets go of jump
                    else {
                        gravMultiplier = jumpCutOff;
                    }
                } else {
                    gravMultiplier = upwardMovementMultiplier;
                }
            }
        }

        //Else if going down...
        else if (body.velocity.y < -0.01f) {

            if (onGround)
            //Don't change it if Kit is stood on something (such as a moving platform)
            {
                gravMultiplier = defaultGravityScale;
            } else {
                //Otherwise, apply the downward gravity multiplier as Kit comes back to Earth
                gravMultiplier = downwardMovementMultiplier;
            }

        }
        //Else not moving vertically at all
        else {
            if (onGround || playerColumn.hasGrabbedColumn || playerGrapplingGun.grapplingRope.isGrappling) {
                currentlyJumping = false;
            }

            gravMultiplier = defaultGravityScale;
        }

        //Set the character's Rigidbody's velocity
        //But clamp the Y variable within the bounds of the speed limit, for the terminal velocity assist option
        // body.velocity = new Vector3(velocity.x * jumpMovementMultiplierX, Mathf.Clamp(velocity.y * jumpMovementMultiplierY, -speedLimit, 100));
    }

    private void DoAJump() {

        //Create the jump, provided we are on the ground, in coyote time, or have a double jump available
        if (!playerColumn.hasGrabbedColumn && (onGround || (coyoteTimeCounter > 0.03f && coyoteTimeCounter < coyoteTime) || canJumpAgain)) {
            desiredJump = false;
            jumpBufferCounter = 0;
            coyoteTimeCounter = 0;

            //If we have double jump on, allow us to jump again (but only once)
            canJumpAgain = (maxAirJumps == 1 && canJumpAgain == false);

            //Determine the power of the jump, based on our gravity and stats
            jumpSpeed = Mathf.Sqrt(-2f * Physics2D.gravity.y * body.gravityScale * jumpHeight);

            //If Kit is moving up or down when she jumps (such as when doing a double jump), change the jumpSpeed;
            //This will ensure the jump is the exact same strength, no matter your velocity.
            if (velocity.y > 0f) {
                jumpSpeed = Mathf.Max(jumpSpeed - velocity.y, 0f);
            } else if (velocity.y < 0f) {
                jumpSpeed += Mathf.Abs(body.velocity.y);
            }

            //Apply the new jumpSpeed to the velocity. It will be sent to the Rigidbody in FixedUpdate;
            velocity.y += jumpSpeed;
            currentlyJumping = true;

            // if (playerEffect != null) {
            //     playerEffect.jumpEffects();
            // }
        }

        if (jumpBuffer == 0) {
            //If we don't have a jump buffer, then turn off desiredJump immediately after hitting jumping
            desiredJump = false;
        }
    }
}
