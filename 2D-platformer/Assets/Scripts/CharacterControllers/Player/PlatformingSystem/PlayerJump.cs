using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

//This script handles moving the character on the Y axis, for jumping and gravity

public class PlayerJump : MonoBehaviour
{
    [Header("Components")]
    public Animator spineAnimator;
    [HideInInspector] public Rigidbody2D body;
    [HideInInspector] public Vector2 velocity;
    private PlayerAppearanceScript playerAppearanceScript;
    private PlayerMovement playerMovement;
    private PlayerDodge playerDodge;
    private PlayerColumn playerColumn;
    private PlayerGrapplingGun playerGrapplingGun;
    private PlayerCombatSystem playerCombatSystemScript;
    private CapsuleCollider2D playerGroundCheckCollider;

    [Header("Jumping Stats")]
    public float jumpMovementMultiplier = 1f;
    [SerializeField, Range(0f, 2f)] public float jumpMultiplierXSprint = 1f;
    [SerializeField, Range(0f, 2f)] public float jumpMultiplierXJogg = 1f;
    [HideInInspector]
    public float finalJumpMovementMultiplier;
    [SerializeField, Range(0f, 20f)][Tooltip("Maximum jump height")] public float maxJumpHeight = 10.0f;
    [SerializeField, Range(0f, 20f)][Tooltip("How long it takes to reach that height before coming back down")] public float timeToJumpApex;
    [SerializeField, Range(0f, 20f)][Tooltip("Gravity multiplier to apply when going up")] public float upwardMovementMultiplier = 1f;
    [SerializeField, Range(0f, 20f)][Tooltip("Gravity multiplier to apply when coming down")] public float downwardMovementMultiplier = 6.17f;
    [SerializeField, Range(0, 1)][Tooltip("How many times can you jump in the air?")] public int maxAirJumps = 0;
    public float autoLookDownFallHeight;
    List<Collider2D> groundCollidersInTouch = new List<Collider2D>();

    [Header("Ground Check Objects")]
    public GameObject checkHeightObject1;
    public GameObject checkHeightObject2;
    public float minLandHeight;
    public float finalMinLandHeight;
    public float dropCorrectionMultiplierForSlope;
    public float jumpAttacksMinimumLandHeight;
    public float movementMinimumLandHeight;

    [Header("Options")]
    [Tooltip("Should the character drop when you let go of jump?")] public bool variablejumpHeight;
    [SerializeField, Range(1f, 10f)][Tooltip("Gravity multiplier when you let go of jump")] public float jumpCutOff;
    [SerializeField][Tooltip("The fastest speed the character can fall")] public float speedLimit;
    [SerializeField, Range(0f, 0.3f)][Tooltip("How long should coyote time last?")] public float coyoteTime = 0.15f;
    [SerializeField, Range(0f, 0.3f)][Tooltip("How far from ground should we cache your jump?")] public float jumpBuffer = 0.15f;
    [SerializeField, Range(0f, 2.0f)] public float minimumJumpTime;

    [Header("Calculations")]
    public float jumpSpeed;
    public float jumpYSpeed;
    public float defaultGravityScale;
    public float gravityScaleLimit;
    public float gravMultiplier;

    [Header("Current State")]
    public bool isJumpPressed;
    public bool jumpClimbInProgress;
    public bool canJumpAgain = false;
    [SerializeField] private bool desiredJump;
    [SerializeField] private bool currentlyJumping;
    public bool isCharging = false;
    public bool onGround;
    float heightFromGround;
    public bool onMovingPlatform;
    public bool combatMode;
    public bool jumpAnimInProgress, jumpInProgress, wallJumpAnimInProgress, wallJumpInProgress;
    [HideInInspector]
    public float jumpBufferCounter, lastClimbUpTime, lastJumpStartTime;
    private float coyoteTimeCounter = 0;
    int playerMoveInd;

    #region Slope Calculations
    public bool isOnSlope;
    public float slopeMinAngleThreshold, slopeMaxAngleThreshold;
    [SerializeField]
    private float slopeDownAngle;
    private Vector2 slopeNormalPerp;
    [HideInInspector]
    public Vector2 onSlopeJumpVector;
    #endregion

    #region Fall Damage Calculations
    public float maxFallDistance;
    public bool wasGrounded;
    public bool isFalling, wasFalling;
    public float startOfFall;
    #endregion

    void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        playerColumn = GetComponent<PlayerColumn>();

        //Find the character's Rigidbody and ground detection and juice scripts
        body = GetComponent<Rigidbody2D>();
        playerDodge = GetComponent<PlayerDodge>();
        playerCombatSystemScript = GetComponent<PlayerCombatSystem>();
        playerAppearanceScript = GetComponentInChildren<PlayerAppearanceScript>();
        playerGrapplingGun = GetComponent<PlayerGrapplingGun>();
        defaultGravityScale = 1f;
        combatMode = false;
        startOfFall = transform.position.y;
    }

    private void OnEnable()
    {
        ResetJumpVariables();
    }

    void Update()
    {

        if (playerDodge.isExecuting || playerColumn.hasGrabbedColumn || playerColumn.isClimbingLedge || playerGrapplingGun.grapplingRope.isGrappling)
        {
            jumpInProgress = false;
            spineAnimator.ResetTrigger("OnAir");
            spineAnimator.ResetTrigger("Landed");
            return;
        }

        if (wallJumpInProgress) isJumpPressed = false;

        //Check if we're on ground, using Kit's Ground script
        if (playerCombatSystemScript.heavyAttackExecuting || playerCombatSystemScript.cutOffHeavyAttackCharge > 0.0f) minLandHeight = jumpAttacksMinimumLandHeight;
        else minLandHeight = movementMinimumLandHeight;

        //onGround = checkIfOnGroundHeight();
        if (playerMovement.objectPushPullState)
        {
            onGround = checkIfOnGroundHeightPushPullState();
        }

        if (playerGrapplingGun.isExecuting == false && jumpAnimInProgress == false &&
        wallJumpAnimInProgress == false && Time.time - lastClimbUpTime > 1.0f &&
        playerCombatSystemScript.heavyAttackExecuting == false && playerCombatSystemScript.cutOffHeavyAttackCharge <= 0.0f)
        {
            if (onGround == false)
            {
                spineAnimator.SetTrigger("OnAir");

                if (playerMovement.objectPushPullState)
                {
                    playerMovement.objectPushPullState = false;
                    playerMovement.interactionObject.interactionBaseScript.ForceCloseInteraction();
                    Debug.Log("BC");
                }
            }
            else spineAnimator.SetTrigger("Landed");
        }

        if (onGround == false && body.velocity.y < 0.0f) isFalling = true;

        if (!wasFalling && isFalling) startOfFall = transform.position.y;
        if (!wasGrounded == onGround && onGround == true) takeFallDamage();

        wasFalling = isFalling;
        wasGrounded = onGround;

        if (heightFromGround > autoLookDownFallHeight)
        {
            CameraController.Instance.OnLookDownDuringFall();
        }
        else
        {
            CameraController.Instance.OnLookDownDuringFallEnd();
        }

        //Exclude separate Physics calculations when in object push pull state
        if (playerMovement.objectPushPullState == false) setPhysics();

        //Jump buffer allows us to queue up a jump, which will play when we next hit the ground
        if (jumpBuffer > 0)
        {
            //Instead of immediately turning off "desireJump", start counting up...
            //All the while, the DoAJump function will repeatedly be fired off
            if (desiredJump)
            {
                jumpBufferCounter += Time.deltaTime;

                if (jumpBufferCounter > jumpBuffer)
                {
                    //If time exceeds the jump buffer, turn off "desireJump"
                    desiredJump = false;
                    jumpBufferCounter = 0;
                }
            }
        }

        //If we're not on the ground and we're not currently jumping, that means we've stepped off the edge of a platform.
        //So, start the coyote time counter...
        if (!currentlyJumping && !onGround)
        {
            coyoteTimeCounter += Time.deltaTime;
        }
        else
        {
            //Reset it when we touch the ground, or jump
            coyoteTimeCounter = 0;
        }
    }

    private void FixedUpdate()
    {
        if (playerColumn.hasGrabbedColumn || playerGrapplingGun.grapplingRope.isGrappling)
        {
            currentlyJumping = false;
        }
        if (playerDodge.isExecuting || playerColumn.hasGrabbedColumn || playerGrapplingGun.grapplingRope.isGrappling)
        {
            return;
        }

        //Get velocity from Kit's Rigidbody 
        velocity = body.velocity;

        //Keep trying to do a jump, for as long as desiredJump is true
        if (desiredJump)
        {
            DoAJump();

            //Below conditions help to determine horizontal movement speed during jumps
            if (playerMoveInd == 1) finalJumpMovementMultiplier = jumpMultiplierXJogg;
            else if (playerMoveInd == 2) finalJumpMovementMultiplier = jumpMultiplierXSprint;
            else finalJumpMovementMultiplier = jumpMovementMultiplier;

            body.velocity = velocity;

            //Skip gravity calculations this frame, so currentlyJumping doesn't turn off
            //This makes sure you can't do the coyote time double jump bug

            return;
        }

        if (velocity.y < 0.0f) jumpClimbInProgress = false;

        if (jumpClimbInProgress == true && isJumpPressed == false && Time.time - lastJumpStartTime > minimumJumpTime && wallJumpInProgress == false)
        {
            jumpClimbInProgress = false;

            velocity.y = 0.0f;
            body.velocity = velocity;
        }

        //Exclude separate Gravity calculations when in object push pull state
        if (playerMovement.objectPushPullState == false) calculateGravity();
        else body.gravityScale = playerMovement.objectPushPullGravityScale;
    }

    private void DoAJump()
    {
        //Create the jump, provided we are on the ground, in coyote time, or have a double jump available
        if (!playerColumn.hasGrabbedColumn && (onGround || (coyoteTimeCounter > 0.03f && coyoteTimeCounter < coyoteTime) || canJumpAgain))
        {
            desiredJump = false;
            jumpBufferCounter = 0;
            coyoteTimeCounter = 0;
            jumpClimbInProgress = true;
            lastJumpStartTime = Time.time;

            //If we have double jump on, allow us to jump again (but only once)
            canJumpAgain = (maxAirJumps == 1 && canJumpAgain == false);

            //Determine the power of the jump, based on our gravity and stats
            jumpSpeed = Mathf.Sqrt(-2f * Physics2D.gravity.y * body.gravityScale * maxJumpHeight);

            //If Kit is moving up or down when she jumps (such as when doing a double jump), change the jumpSpeed;
            //This will ensure the jump is the exact same strength, no matter your velocity.
            if (velocity.y > 0f)
            {
                jumpSpeed = Mathf.Max(jumpSpeed - velocity.y, 0f);
            }
            else if (velocity.y < 0f)
            {
                jumpSpeed += Mathf.Abs(body.velocity.y);
            }

            //Apply the new jumpSpeed to the velocity. It will be sent to the Rigidbody in FixedUpdate;
            velocity.y += jumpSpeed;
            currentlyJumping = true;
        }

        if (jumpBuffer == 0)
        {
            //If we don't have a jump buffer, then turn off desiredJump immediately after hitting jumping
            desiredJump = false;
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        //Debug.Log("kire baal: " + context.phase + " Time: " + Time.time + "Value: " + context.ReadValue<float>());
        if (playerMovement.isSliding || playerMovement.isSlideJumping || jumpInProgress || onGround == false) return;

        playerMoveInd = playerMovement.playerMovementInd;

        if (context.ReadValue<float>() >= 1)
        {
            isJumpPressed = true;

            if (jumpInProgress == false)
            {
                //Debug.Log("ggg juump");
                if (MovementLimiter.instance.playerCanMove && MovementLimiter.instance.playerCanParkour)
                {
                    if (!playerGrapplingGun.grapplingRope.isGrappling && onGround) StartJumpAnimation();
                }
            }
        }
        else if (context.ReadValue<float>() < 1)
        {
            isJumpPressed = false;
        }
    }

    private void StartJumpAnimation()
    {
        isCharging = true;
        spineAnimator.ResetTrigger("Landed");
        spineAnimator.SetTrigger("Jump");
    }

    private void calculateGravity()
    {
        //If Kit is going up...
        if (body.velocity.y > 0.01f)
        {
            if (onGround)
            {
                //Don't change it if Kit is stood on something (such as a moving platform)
                gravMultiplier = defaultGravityScale;
            }
            else
            {
                if (variablejumpHeight)//If we're using variable jump height...)
                {
                    if (isJumpPressed)
                    {
                        //gravMultiplier = Mathf.Max(gravityScaleLimit, gravMultiplier - (Time.deltaTime * jumpYSpeed));
                        gravMultiplier = .7f;
                    }
                    else gravMultiplier = 1.0f;
                }
                else
                {
                    gravMultiplier = upwardMovementMultiplier;
                }
            }
        }
        //Else if going down...
        else if (body.velocity.y < -0.01f)
        {
            if (onGround)
            //Don't change it if Kit is stood on something (such as a moving platform)
            {
                gravMultiplier = defaultGravityScale;
            }
            else
            {
                //Otherwise, apply the downward gravity multiplier as Kit comes back to Earth
                gravMultiplier = downwardMovementMultiplier;
            }

        }
        //Else not moving vertically at all
        else
        {
            if (onGround || playerColumn.hasGrabbedColumn || playerGrapplingGun.grapplingRope.isGrappling)
            {
                currentlyJumping = false;
            }

            gravMultiplier = defaultGravityScale;
        }
    }

    private void setPhysics()
    {
        //Determine the character's gravity scale, using the stats provided. Multiply it by a gravMultiplier, used later
        Vector2 newGravity = new Vector2(0, (-2 * maxJumpHeight) / (timeToJumpApex * timeToJumpApex));
        body.gravityScale = (newGravity.y / Physics2D.gravity.y) * gravMultiplier;
    }

    bool checkIfBaseOnSlope()//Returns true when base on slope
    {
        LayerMask mask = LayerMask.GetMask("Ground");

        //check if standing on slope
        RaycastHit2D Hit3 = Physics2D.Raycast(playerMovement.checkSlopeObject.transform.position, Vector3.down, 10.0f, mask);
        if (Hit3.collider != null)
        {
            onSlopeJumpVector = Hit3.normal;
            slopeNormalPerp = Vector2.Perpendicular(Hit3.normal).normalized;
            slopeDownAngle = Vector2.Angle(Hit3.normal, Vector2.up);

            if (slopeDownAngle > slopeMinAngleThreshold && slopeDownAngle < slopeMaxAngleThreshold)
            {
                isOnSlope = true;
                return true;
            }
            else return false;
        }
        else
        {
            return false;
        }
    }

    public void StartJump()
    {
        desiredJump = true;
    }

    public void DisableCharging()
    {
        isCharging = false;
    }

    private void OnDrawGizmos()
    {
        if (onGround)
        {
            Gizmos.color = Color.green;
        }
        else
        {
            Gizmos.color = Color.red;
        }

        //up check object
        Gizmos.DrawLine(checkHeightObject1.transform.position, checkHeightObject1.transform.position + new Vector3(0.0f, -finalMinLandHeight, 0.0f));

        //down check object
        Gizmos.DrawLine(checkHeightObject2.transform.position, checkHeightObject2.transform.position + new Vector3(0.0f, -finalMinLandHeight, 0.0f));
    }

    void takeFallDamage()
    {
        float playerFullHealth = 200.0f;
        float fallDistance = Mathf.Abs(startOfFall - transform.position.y);

        float fallLimitDistanceRatio = fallDistance / maxFallDistance;
        //Debug.Log("Fall ratio: " + fallLimitDistanceRatio + " fall distance: " + fallDistance + " maxFall: " + maxFallDistance);
        //Calculating damage to apply
        //Less ratio means less damage.
        if (fallLimitDistanceRatio < .4f) return;
        else if (fallLimitDistanceRatio < .65f)
        {
            playerCombatSystemScript.TakeEnvironmentDamage(playerFullHealth * .35f);
            //apply 35% damage
        }
        else if (fallLimitDistanceRatio < .98f)
        {
            //apply 75% damage
            playerCombatSystemScript.TakeEnvironmentDamage(playerFullHealth * .75f);
        }
        else if (fallLimitDistanceRatio >= .98f)
        {
            //death
            playerCombatSystemScript.TakeEnvironmentDamage(playerFullHealth);
        }
    }

    public void ResetJumpVariables()
    {
        if (playerMovement.slideJumpQueued == false)
        {
            lastJumpStartTime = Time.time;

            isJumpPressed = jumpClimbInProgress = false;
            jumpAnimInProgress = jumpInProgress = wallJumpAnimInProgress = wallJumpInProgress = false;

            minLandHeight = movementMinimumLandHeight;

            spineAnimator.ResetTrigger("Jump");
            spineAnimator.SetBool("SlideJump", false);
            playerMovement.isSlideJumping = false;
        }

        isFalling = wasFalling = false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Platforms") && other.enabled == true)
        {
            if (!groundCollidersInTouch.Contains(other)) groundCollidersInTouch.Add(other);
            if (groundCollidersInTouch.Count > 0) onGround = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Platforms") && Time.time - playerMovement.pushPullStateEnterTime > 1.0f)
        {
            if (groundCollidersInTouch.Contains(other))
            {
                Debug.Log("exit object name " + other.gameObject);
                groundCollidersInTouch.Remove(other);
                if (groundCollidersInTouch.Count <= 0) onGround = false;
            }
            //Forgot cause of logic will reintroduce when find bug 
            //if (checkIfOnGroundHeight()) onGround = true;
        }
    }

    // void OnTriggerExit2D(Collider2D other)
    // {
    //     if (other.gameObject.CompareTag("Platforms") && Time.time - playerMovement.pushPullStateEnterTime > 1.0f && groundCount < 2)
    //     {
    //         onGround = false;
    //     }
    // }

    #region UnUsed
    //Returns true if player i close enough to the ground for landing animation
    private bool checkIfOnGroundHeightPushPullState()
    {
        LayerMask mask = LayerMask.GetMask("Ground");

        RaycastHit2D Hit1 = Physics2D.Raycast(checkHeightObject1.transform.position, Vector2.down, 100.0f, mask);
        RaycastHit2D Hit2 = Physics2D.Raycast(checkHeightObject2.transform.position, Vector2.down, 100.0f, mask);

        finalMinLandHeight = minLandHeight;
        heightFromGround = Mathf.Min(Hit1.distance, Hit2.distance);
        if (playerMovement.objectPushPullState) finalMinLandHeight = finalMinLandHeight * playerMovement.objectPushPullStateHeightCheckMultiplier;

        //Debug.Log("H1: " + Hit1.collider.gameObject + " dist: " + Hit1.distance + " H2: " + Hit2.collider.gameObject + " dist: " + Hit2.distance);
        if (Hit1.collider == null && Hit2.collider == null)
        {
            return false;
        }
        else if (Hit1.collider == null && Hit2.collider.CompareTag("Platforms") == true)
        {
            if (Hit2.distance > finalMinLandHeight)
            {
                return false;
            }
            else return true;
        }
        else if (Hit2.collider == null && Hit1.collider.CompareTag("Platforms") == true)
        {
            if (Hit1.distance > finalMinLandHeight)
            {
                return false;
            }
            else return true;
        }
        else if (Hit1.collider.CompareTag("Platforms") == true && Hit2.collider.CompareTag("Platforms") == true)
        {
            if (Hit1.distance > finalMinLandHeight && Hit2.distance > finalMinLandHeight)//if high on ground
            {
                //Debug.Log("On Air: " + Hit1.distance + " " + Hit2.distance + "lH: " + finalMinLandHeight);
                return false;
            }
            else
            {
                return true;//On Ground
            }
        }
        else
        {
            //Debug.Log("On bichi Ground: " + Hit1.distance + " gb " + Hit1.collider.gameObject);
            return false;//On Air
        }
    }

    private bool checkIfOnGroundHeight()
    {
        LayerMask mask = LayerMask.GetMask("Ground");

        finalMinLandHeight = minLandHeight;
        if (playerMovement.objectPushPullState) finalMinLandHeight = finalMinLandHeight * playerMovement.objectPushPullStateHeightCheckMultiplier;

        RaycastHit2D Hit1 = Physics2D.Raycast(checkHeightObject1.transform.position, Vector2.down, finalMinLandHeight, mask);
        RaycastHit2D Hit2 = Physics2D.Raycast(checkHeightObject2.transform.position, Vector2.down, finalMinLandHeight, mask);

        heightFromGround = Mathf.Min(Hit1.distance, Hit2.distance);

        //Debug.Log("H1: " + Hit1.collider.gameObject + " dist: " + Hit1.distance + " H2: " + Hit2.collider.gameObject + " dist: " + Hit2.distance);
        if (Hit1.collider == null && Hit2.collider == null)
        {
            return false;
        }
        else if (Hit1.collider == null && Hit2.collider.CompareTag("Platforms") == true)
        {
            if (Hit2.distance > finalMinLandHeight)
            {
                return false;
            }
            else return true;
        }
        else if (Hit2.collider == null && Hit1.collider.CompareTag("Platforms") == true)
        {
            if (Hit1.distance > finalMinLandHeight)
            {
                return false;
            }
            else return true;
        }
        else if (Hit1.collider.CompareTag("Platforms") == true && Hit2.collider.CompareTag("Platforms") == true)
        {
            if (Hit1.distance > finalMinLandHeight && Hit2.distance > finalMinLandHeight)//if high on ground
            {
                //Debug.Log("On Air: " + Hit1.distance + " " + Hit2.distance + "lH: " + finalMinLandHeight);
                return false;
            }
            else
            {
                return true;//On Ground
            }
        }
        else
        {
            //Debug.Log("On bichi Ground: " + Hit1.distance + " gb " + Hit1.collider.gameObject);
            return false;//On Air
        }
    }
    #endregion
}