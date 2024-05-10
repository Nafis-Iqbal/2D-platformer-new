using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

//This script handles moving the character on the X axis, both on the ground and in the air.

public class PlayerMovement : MonoBehaviour
{
    #region OBJECTS & SCRIPTS
    [Header("Drag & Drops")]
    public GameObject playerCharacterBody;
    public GameObject colliderObject;
    public PlayerCombatSystem playerCombatSystemScript;
    private PlayerJump playerJump;
    private PlayerColumn playerColumn;
    private PlayerDodge playerDodge;
    private PlayerRoll playerRoll;
    private PlayerGrapplingGun playerGrapplingGun;
    private Rigidbody2D playerRB2D;
    private Rigidbody2D pushPullObjectRB2D;
    private Animator playerSpineAnimator;
    public GameObject checkSlopeObject;
    #endregion

    //[HideInInspector]
    public InteractionObject interactionObject;

    [Header("Movement Stats")]
    [SerializeField, Range(0f, 20f)][Tooltip("Maximum movement speed")] public float maxSpeed = 10f;
    [SerializeField, Range(0f, 100f)] public float maxAcceleration = 52f;
    [SerializeField, Range(0f, 100f)] public float maxDeceleration = 52f;
    [SerializeField, Range(0f, 100f)][Tooltip("How fast to stop when changing direction")] public float maxTurnSpeed = 80f;
    [SerializeField, Range(0f, 100f)][Tooltip("How fast to reach max speed when in mid-air")] public float maxAirAcceleration;
    [SerializeField, Range(0f, 100f)][Tooltip("How fast to stop in mid-air when no direction is used")] public float maxAirDeceleration;
    [SerializeField, Range(0f, 100f)][Tooltip("How fast to stop when changing direction when in mid-air")] public float maxAirTurnSpeed = 80f;
    [SerializeField, Range(0f, 5f)][Tooltip("How long strafe movement is possible after jump")] public float maxAirStrafeTime = 2.5f;
    [SerializeField, Range(0f, 1f)] public float walkMultiplier = 0.1f;
    [SerializeField, Range(0f, 2f)] public float joggMultiplier = 1.0f;
    [SerializeField, Range(0f, 2f)] public float sprintMultiplier = 0.1f;
    public float movementSpeedMultiplier = 1f;
    [SerializeField] private float speedMultiplier;
    public float slidingDownSpeed, slideJumpForce;
    [SerializeField][Tooltip("Friction to apply against movement on stick")] private float friction;
    public float baseMomentumForce;
    public float objectPushForce, objectPullForce;
    public float pushPullStateEnterTime;
    Vector2 pushPullVector;

    [Header("Options")]
    [Tooltip("When false, the charcter will skip acceleration and deceleration and instantly move and stop")][SerializeField] private bool useAcceleration;

    [Header("Calculations")]
    [SerializeField] public Vector2 tempVelocity;
    [SerializeField] private float directionX;
    private Vector2 desiredVelocity;
    private float maxSpeedChange;
    private float acceleration;
    private float deceleration;
    private float turnSpeed;
    public float tempSpeedTurn = 1.0f;
    public float tempSpeedAccel = 1.0f;
    public float tempSpeedDecel = 1.0f;
    public float tempSpeedWithoutAccel = 1.0f;
    public Rigidbody2D movingPlatformRB2D;
    Vector2 platformAssistedVelocity;
    Quaternion initialPlayerRotation;

    #region Slope Calculations
    public float slopeAngleThreshold;
    public float slideDownAngleThreshold;
    public PhysicsMaterial2D fullFriction, zeroFriction;
    [SerializeField]
    private float slopeDownAngle;
    private Vector2 slopeNormalPerp;
    [HideInInspector]
    public Vector2 onSlopeJumpVector;
    #endregion

    [Header("Current State")]
    public bool playerFacingRight = true;
    public bool onGround;
    public bool isOnSlope;
    public bool isSliding;
    public bool isSlideJumping;
    public bool isSprinting;
    public bool pressingKey;
    public float movementValue;
    public bool objectPushPullState;
    public bool isPushing, isPulling;

    [Tooltip("Used in Animator to select jogg, sprint or walk animations")]
    public int playerMovementInd;

    private void OnEnable()
    {
        Physics2D.IgnoreCollision(transform.GetComponent<Collider2D>(), colliderObject.GetComponent<Collider2D>());

        playerRB2D = GetComponent<Rigidbody2D>();
        playerJump = GetComponent<PlayerJump>();
        playerDodge = GetComponent<PlayerDodge>();
        playerRoll = GetComponent<PlayerRoll>();
        playerGrapplingGun = GetComponent<PlayerGrapplingGun>();
        playerCombatSystemScript = GetComponent<PlayerCombatSystem>();
        playerSpineAnimator = GameManager.Instance.playerSpineAnimator;
        playerFacingRight = true;
        isOnSlope = false;
        isSliding = isSlideJumping = false;
        objectPushPullState = false;
        isPushing = isPulling = false;
        pushPullStateEnterTime = Time.time;
        pushPullVector = platformAssistedVelocity = Vector2.zero;
    }

    private void Update()
    {
        #region HORIZONTAL MOVEMENT
        if (isSprinting == true)
        {
            if (playerCombatSystemScript.combatMode == true)
            {
                speedMultiplier = joggMultiplier;
            }
            else
            {
                speedMultiplier = sprintMultiplier;
            }

            playerMovementInd = 2;
        }
        else
        {
            if (playerCombatSystemScript.combatMode == true)
            {
                speedMultiplier = walkMultiplier;
            }
            else
            {
                speedMultiplier = joggMultiplier;
            }

            playerMovementInd = 1;
        }

        directionX = movementValue * speedMultiplier;
        #endregion

        if (playerCombatSystemScript.combatMode == true)
        {
            colliderObject.SetActive(true);
            Physics2D.IgnoreCollision(transform.GetComponent<Collider2D>(), colliderObject.GetComponent<Collider2D>());
        }
        else
        {
            colliderObject.SetActive(false);
        }

        if (playerDodge.isExecuting || playerRoll.isExecuting || playerGrapplingGun.grapplingRope.isGrappling)
        {
            return;
        }

        //LIMIT PLAYER MOVEMENT BASED ON STATES
        if (!MovementLimiter.instance.playerCanMove)
        {
            directionX = 0;
            playerMovementInd = 0;
        }

        if (directionX < .01f && directionX > -.01f)//directionX 0 and player stand still
        {
            pressingKey = false;
            playerMovementInd = 0;

            if (isOnSlope)
            {
                if (isSliding == false)
                {
                    if (playerRB2D.sharedMaterial != fullFriction)
                    {
                        playerRB2D.sharedMaterial = fullFriction;
                    }
                }
                else
                {
                    if (playerRB2D.sharedMaterial != zeroFriction)
                    {
                        playerRB2D.sharedMaterial = zeroFriction;
                    }
                    //Switch To Sliding Animation
                }
            }
            else
            {
                if (playerRB2D.sharedMaterial != zeroFriction)
                {
                    playerRB2D.sharedMaterial = zeroFriction;
                }
            }
        }
        //FLIP PLAYER FACING DIRECTION
        else if (directionX != 0)//Player moving
        {
            //Debug.Log("bichi");
            if (directionX > 0 && playerFacingRight == false)
            {
                if (objectPushPullState == false)
                {
                    rotateRight();
                }
            }
            else if (directionX < 0 && playerFacingRight == true)
            {
                if (objectPushPullState == false)
                {
                    rotateLeft();
                }
            }

            pressingKey = true;
            playerRB2D.sharedMaterial = zeroFriction;
        }

        if (objectPushPullState == false)
        {
            playerSpineAnimator.SetInteger("MoveSpeed", playerMovementInd);
            desiredVelocity = new Vector2(directionX, 0f) * Mathf.Max(maxSpeed - friction, 0f);
        }
        else
        {
            if (directionX > 0)
            {
                if (playerFacingRight)
                {
                    isPushing = true;
                    //desiredVelocity = new Vector2(objectPushVelocity, 0.0f);
                    playerSpineAnimator.SetBool("PushObject", true);
                    playerSpineAnimator.SetBool("PullObject", false);
                }
                else
                {
                    isPulling = true;
                    //desiredVelocity = new Vector2(objectPullVelocity, 0.0f);
                    playerSpineAnimator.SetBool("PullObject", true);
                    playerSpineAnimator.SetBool("PushObject", false);
                }
            }
            else if (directionX < 0)
            {
                if (playerFacingRight)
                {
                    isPulling = true;
                    //desiredVelocity = new Vector2(objectPullVelocity, 0.0f);
                    playerSpineAnimator.SetBool("PullObject", true);
                    playerSpineAnimator.SetBool("PushObject", false);
                }
                else
                {
                    isPushing = true;
                    //desiredVelocity = new Vector2(objectPushVelocity, 0.0f);
                    playerSpineAnimator.SetBool("PushObject", true);
                    playerSpineAnimator.SetBool("PullObject", false);
                }
            }
            else
            {
                //desiredVelocity = Vector2.zero;
                isPushing = false;
                isPulling = false;
                playerSpineAnimator.SetBool("PullObject", false);
                playerSpineAnimator.SetBool("PushObject", false);
            }

        }
    }

    private void FixedUpdate()
    {
        if (playerDodge.isExecuting ||
            playerRoll.isExecuting ||
            playerGrapplingGun.grapplingRope.isGrappling || playerGrapplingGun.playerIsOnAir
        )
        {
            return;
        }

        if (playerCombatSystemScript.isKnockedOffGround)
        {
            if (playerFacingRight)
            {
                playerRB2D.velocity = Vector2.left * playerCombatSystemScript.knockedOffVelocity;
            }
            else
            {
                playerRB2D.velocity = Vector2.right * playerCombatSystemScript.knockedOffVelocity;
            }
            return;
        }

        //checking slope angle of ground
        checkBaseSlope();

        if (isSliding && isSlideJumping == false)
        {
            if (playerFacingRight)
            {
                playerRB2D.velocity = -slopeNormalPerp * slidingDownSpeed * Mathf.Sin(Mathf.Deg2Rad * slopeDownAngle);
            }
            else
            {
                playerRB2D.velocity = slopeNormalPerp * slidingDownSpeed * Mathf.Sin(Mathf.Deg2Rad * slopeDownAngle);
            }
            return;
        }

        if (isSlideJumping)
        {
            return;
        }

        if (playerCombatSystemScript.isBlocking)
        {
            playerRB2D.velocity = Vector2.zero;
            return;
        }

        onGround = playerJump.onGround;
        tempVelocity = playerRB2D.velocity;

        if (playerCombatSystemScript.heavyAttackExecuting == false && playerCombatSystemScript.lightAttackExecuting == false)
        {
            if (objectPushPullState)
            {
                pushPullVector.x = directionX;
                moveDuringObjectPushPullState();
            }
            else if (useAcceleration)
            {
                runWithAcceleration();
            }
            else
            {
                runWithoutAcceleration();
            }
        }
    }

    #region Movement
    public void OnMovement(InputAction.CallbackContext context)
    {
        if (playerGrapplingGun.isExecuting)
        {
            if (context.ReadValue<float>() > 0f)
            {
                rotateRight();
            }
            else if (context.ReadValue<float>() < 0f)
            {
                rotateLeft();
            }
            playerRB2D.AddForce(Vector2.right * context.ReadValue<float>() * playerGrapplingGun.hangSwingForce);
        }

        if (context.phase == InputActionPhase.Started)
        {
            if (MovementLimiter.instance.playerCanMove)
            {
                movementValue = context.ReadValue<float>();
                directionX = movementValue * speedMultiplier;
            }
            //PLAYER ORIENTATION DURING COMBAT
            else
            {
                if (playerCombatSystemScript.lightAttackExecuting && playerCombatSystemScript.canChangeDirectionDuringAttack)
                {
                    Debug.Log("Behen");
                    if (context.ReadValue<float>() > 0f)
                    {
                        Debug.Log("mami");
                        rotateRight();
                    }
                    else if (context.ReadValue<float>() < 0f)
                    {
                        Debug.Log("boudi");
                        rotateLeft();
                    }
                }
            }
        }
        else if (context.phase == InputActionPhase.Canceled || context.phase == InputActionPhase.Performed)
        {
            movementValue = context.ReadValue<float>();
            directionX = movementValue * speedMultiplier;
        }
    }

    public void OnRun(InputAction.CallbackContext context)
    {
        //Check not required, as checks are done in update methods
        //if (playerGrapplingGun.isExecuting || playerColumn.isExecuting) return;

        if (context.phase == InputActionPhase.Performed)
        {
            isSprinting = true;
        }
        else if (context.phase == InputActionPhase.Canceled)
        {
            isSprinting = false;
        }
    }

    private void runWithoutAcceleration()
    {
        //FOR UNBIASED WALL JUMPS
        if (playerJump.wallJumpInProgress) return;

        //If we're not using acceleration and deceleration, just send our desired velocity (direction * max speed) to the Rigidbody
        tempVelocity.x = desiredVelocity.x;

        //MOVING PLATFORM CALCULATIONS
        if (playerJump.onMovingPlatform && movingPlatformRB2D != null)
        {
            platformAssistedVelocity.x = movingPlatformRB2D.velocity.x;
        }

        if (playerJump.jumpInProgress)//Use jump velocity along horizontal aksis
        {
            if (Time.time - playerJump.lastJumpStartTime > maxAirStrafeTime) tempVelocity.x = 0.0f;
            else tempVelocity.x = tempVelocity.x * playerJump.finalJumpMovementMultiplier;

            if (playerJump.onMovingPlatform) playerRB2D.velocity = (tempVelocity * movementSpeedMultiplier * tempSpeedWithoutAccel) + platformAssistedVelocity;
            else playerRB2D.velocity = tempVelocity * movementSpeedMultiplier * tempSpeedWithoutAccel;
        }
        else if (isOnSlope == false)//use normal velocity along eks aksis
        {
            if (playerJump.onMovingPlatform) playerRB2D.velocity = (tempVelocity * movementSpeedMultiplier * tempSpeedWithoutAccel) + platformAssistedVelocity;
            else playerRB2D.velocity = tempVelocity * movementSpeedMultiplier * tempSpeedWithoutAccel;
        }
        else
        {
            if (playerJump.onMovingPlatform) playerRB2D.velocity = (new Vector2(-tempVelocity.x * slopeNormalPerp.x, -tempVelocity.x * slopeNormalPerp.y) * movementSpeedMultiplier * tempSpeedWithoutAccel) + platformAssistedVelocity;
            else playerRB2D.velocity = new Vector2(-tempVelocity.x * slopeNormalPerp.x, -tempVelocity.x * slopeNormalPerp.y) * movementSpeedMultiplier * tempSpeedWithoutAccel;
        }
    }
    private void runWithAcceleration()
    {
        //Set our acceleration, deceleration, and turn speed stats, based on whether we're on the ground on in the air

        acceleration = onGround ? maxAcceleration : maxAirAcceleration;
        deceleration = onGround ? maxDeceleration : maxAirDeceleration;
        turnSpeed = onGround ? maxTurnSpeed : maxAirTurnSpeed;

        if (pressingKey)
        {
            //If the sign (i.e. positive or negative) of our input direction doesn't match our movement, it means we're turning around and so should use the turn speed stat.
            if (Mathf.Sign(directionX) != Mathf.Sign(tempVelocity.x))
            {
                maxSpeedChange = turnSpeed * Time.fixedDeltaTime * tempSpeedTurn;
            }
            else
            {
                //If they match, it means we're simply running along and so should use the acceleration stat
                maxSpeedChange = acceleration * Time.fixedDeltaTime * tempSpeedAccel;
            }
        }
        else
        {
            //And if we're not pressing a direction at all, use the deceleration stat
            maxSpeedChange = deceleration * Time.fixedDeltaTime * tempSpeedDecel;
        }

        //Move our velocity towards the desired velocity, at the rate of the number calculated above
        tempVelocity.x = Mathf.MoveTowards(tempVelocity.x, desiredVelocity.x, maxSpeedChange);

        //Update the Rigidbody with this new velocity
        playerRB2D.velocity = tempVelocity * movementSpeedMultiplier;
    }

    private void moveDuringObjectPushPullState()
    {
        if (isOnSlope == false)//use normal velocity along eks aksis
        {
            //pushPullObjectRB2D.velocity = tempVelocity * movementSpeedMultiplier * tempSpeedWithoutAccel;
            if (isPushing) pushPullObjectRB2D.AddForce(objectPushForce * pushPullVector);
            else if (isPulling) pushPullObjectRB2D.AddForce(objectPullForce * pushPullVector);
        }
        else
        {
            //pushPullObjectRB2D.velocity = new Vector2(tempVelocity.x * slopeNormalPerp.x, tempVelocity.x * slopeNormalPerp.y) * movementSpeedMultiplier * tempSpeedWithoutAccel;
            if (isPushing) pushPullObjectRB2D.AddForce(objectPushForce * pushPullVector);
            else if (isPulling) pushPullObjectRB2D.AddForce(objectPullForce * pushPullVector);
        }

        playerRB2D.velocity = pushPullObjectRB2D.velocity;
    }

    #endregion

    #region Physics & Accessories
    public void rotateLeft()
    {
        Vector3 tempScale = transform.localScale;
        if (tempScale.x > 0.0f) tempScale.x *= -1.0f;
        transform.localScale = tempScale;

        playerFacingRight = false;
    }

    public void rotateRight()
    {
        Vector3 tempScale = transform.localScale;
        tempScale.x = Mathf.Abs(tempScale.x);
        transform.localScale = tempScale;

        playerFacingRight = true;
    }

    public void applyPlayerMomentum(float momentumLevel)//momentum 
    {
        if (playerFacingRight)
        {
            //Debug.Log("mom" + Vector2.right * momentumLevel * baseMomentumForce);
            playerRB2D.AddForce(Vector2.right * momentumLevel * baseMomentumForce, ForceMode2D.Impulse);
        }
        else
        {
            //Debug.Log("fVec: " + momentumLevel + " " + baseMomentumForce);
            playerRB2D.AddForce(Vector2.left * momentumLevel * baseMomentumForce, ForceMode2D.Impulse);
        }
    }

    public void applyVariablePlayerMomentum(string momentumType, float momentumLevel)//momentum 
    {

        if (momentumType == "ChargeAttack")
        {
            if (playerFacingRight)
            {
                float appliedMomentumFraction = playerCombatSystemScript.cutOffHeavyAttackCharge / playerCombatSystemScript.totalHeavyAttackChargeAmount;
                Vector2 Dir = new Vector2(.750f, 1.0f);
                playerRB2D.AddForce(Dir * momentumLevel * baseMomentumForce * appliedMomentumFraction, ForceMode2D.Impulse);
            }
            else
            {
                float appliedMomentumFraction = playerCombatSystemScript.cutOffHeavyAttackCharge / playerCombatSystemScript.totalHeavyAttackChargeAmount;
                Vector2 Dir = new Vector2(-.750f, 1.0f);
                Debug.Log("fMom: " + appliedMomentumFraction);
                Debug.Log("fVec: " + Dir + " " + momentumLevel + " " + baseMomentumForce + " " + appliedMomentumFraction);
                playerRB2D.AddForce(Dir * momentumLevel * baseMomentumForce * appliedMomentumFraction, ForceMode2D.Impulse);
            }

            playerCombatSystemScript.cutOffHeavyAttackCharge = 0.0f;
        }
    }

    public void stopPlayerCompletely()
    {
        playerRB2D.velocity = Vector2.zero;
    }

    public void checkBaseSlope()
    {
        LayerMask mask = LayerMask.GetMask("Ground");

        //check if standing on slope
        RaycastHit2D Hit3 = Physics2D.Raycast(checkSlopeObject.transform.position, Vector3.down, 10.0f, mask);
        if (onGround == true && Hit3.collider != null)
        {
            onSlopeJumpVector = Hit3.normal;
            slopeNormalPerp = Vector2.Perpendicular(Hit3.normal).normalized;
            slopeDownAngle = Vector2.Angle(Hit3.normal, Vector2.up);

            if (slopeDownAngle > slopeAngleThreshold) isOnSlope = true;
            else isOnSlope = false;

            if (slopeDownAngle < slideDownAngleThreshold)
            {
                if (isSliding == true)
                {
                    isSliding = false;
                    playerSpineAnimator.SetBool("Sliding", false);
                }
            }
            else if (isOnSlope == true && objectPushPullState == false)
            {
                if (isSliding == false)
                {
                    isSliding = true;
                    playerSpineAnimator.SetBool("Sliding", true);
                }
            }

            //Debug.DrawRay(Hit3.point, Hit3.normal, Color.green);
            //Debug.DrawRay(Hit3.point, slopeNormalPerp, Color.red);
        }
        else
        {
            isOnSlope = false;
            isSliding = false;
            playerSpineAnimator.SetBool("Sliding", false);
        }

        Debug.DrawRay(Hit3.point, Hit3.normal, Color.green);
        Debug.DrawRay(Hit3.point, slopeNormalPerp, Color.red);
    }

    public void OnSlideJump(InputAction.CallbackContext context)
    {
        if (isSliding)
        {
            Debug.Log("seelaide");
            playerJump.wallJumpAnimInProgress = true;
            playerSpineAnimator.SetBool("SlideJump", true);
            playerSpineAnimator.SetBool("Sliding", false);

            isSliding = false;
            isSlideJumping = true;
        }
    }

    public void AddSlideJumpForce()
    {
        playerRB2D.velocity = Vector2.zero;
        playerRB2D.AddForce(onSlopeJumpVector * slideJumpForce, ForceMode2D.Impulse);
    }
    #endregion

    #region Interaction Movement
    public void OnInteract()
    {
        if (interactionObject != null)
        {
            interactionObject.interactionBaseScript.OnInteract();
        }
    }

    public void enterPushPullObjectState(Rigidbody2D pushPullBody, float pushForce, float pullForce)
    {
        objectPushPullState = true;
        pushPullObjectRB2D = pushPullBody;

        objectPushForce = pushForce;
        objectPullForce = pullForce;

        pushPullStateEnterTime = Time.time;
        initialPlayerRotation = transform.rotation;
    }

    public void exitPushPullObjectState()
    {
        Debug.Log("KCHI");
        objectPushPullState = false;
        pushPullObjectRB2D = null;

        transform.rotation = initialPlayerRotation;
    }

    public void setParent(Transform newParent)
    {
        transform.parent = newParent;
    }

    public void resetParent()
    {
        transform.parent = null;
    }
    #endregion
    #region UNUSED
    public void teleportAndSynchronizePlayer(Vector3 targetTeleportPosition)
    {
        transform.position = targetTeleportPosition;
    }

    public void OnWalk(InputAction.CallbackContext context)
    {
        Debug.Log("walk context: " + context);
        if (context.phase == InputActionPhase.Started)
        {

        }
        else if (context.phase == InputActionPhase.Canceled)
        {

        }
    }
    #endregion
}
