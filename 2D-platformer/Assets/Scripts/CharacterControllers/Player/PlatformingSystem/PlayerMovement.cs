using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

//This script handles moving the character on the X axis, both on the ground and in the air.

public class PlayerMovement : MonoBehaviour
{
    #region OBJECTS & SCRIPTS
    [Header("Drag & Drops")]
    public PlayerCombatSystem playerCombatSystemScript;
    private PlayerJump playerJump;
    private PlayerColumn playerColumn;
    private PlayerDodge playerDodge;
    private PlayerRoll playerRoll;
    private PlayerGrapplingGun playerGrapplingGun;
    private Rigidbody2D playerRB2D;
    private Animator playerSpineAnimator;
    public GameObject checkSlopeObject;
    #endregion

    [Header("Movement Stats")]
    [SerializeField, Range(0f, 20f)][Tooltip("Maximum movement speed")] public float maxSpeed = 10f;
    [SerializeField, Range(0f, 100f)] public float maxAcceleration = 52f;
    [SerializeField, Range(0f, 100f)] public float maxDeceleration = 52f;
    [SerializeField, Range(0f, 100f)][Tooltip("How fast to stop when changing direction")] public float maxTurnSpeed = 80f;
    [SerializeField, Range(0f, 100f)][Tooltip("How fast to reach max speed when in mid-air")] public float maxAirAcceleration;
    [SerializeField, Range(0f, 100f)][Tooltip("How fast to stop in mid-air when no direction is used")] public float maxAirDeceleration;
    [SerializeField, Range(0f, 100f)][Tooltip("How fast to stop when changing direction when in mid-air")] public float maxAirTurnSpeed = 80f;
    [SerializeField, Range(0f, 1f)] public float walkMultiplier = 0.1f;
    [SerializeField, Range(0f, 2f)] public float joggMultiplier = 1.0f;
    [SerializeField, Range(0f, 2f)] public float sprintMultiplier = 0.1f;
    public float movementSpeedMultiplier = 1f;
    [SerializeField] private float speedMultiplier;
    public float baseMomentumForce;
    [SerializeField][Tooltip("Friction to apply against movement on stick")] private float friction;

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

    #region Slope Calculations
    public bool isOnSlope;
    public float slopeAngleThreshold, slideDownAngleThreshold;
    public PhysicsMaterial2D fullFriction, zeroFriction;
    [SerializeField]
    private float slopeDownAngle;
    private Vector2 slopeNormalPerp;
    #endregion

    [Header("Current State")]
    public bool playerFacingRight = true;
    public bool onGround;
    public bool isSliding;
    public bool isSprinting;
    public bool pressingKey;
    public float movementValue;

    [Tooltip("Used in Animator to select jogg, sprint or walk animations")]
    public int playerMovementInd;

    private void OnEnable()
    {
        playerRB2D = GetComponent<Rigidbody2D>();
        playerJump = GetComponent<PlayerJump>();
        playerDodge = GetComponent<PlayerDodge>();
        playerRoll = GetComponent<PlayerRoll>();
        playerGrapplingGun = GetComponent<PlayerGrapplingGun>();
        playerCombatSystemScript = GetComponent<PlayerCombatSystem>();
        playerSpineAnimator = GameManager.Instance.playerSpineAnimator;
        playerFacingRight = true;
        isOnSlope = false;
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
                rotateRight();
            }
            else if (directionX < 0 && playerFacingRight == true)
            {
                rotateLeft();
            }

            pressingKey = true;
            playerRB2D.sharedMaterial = zeroFriction;
        }

        playerSpineAnimator.SetInteger("MoveSpeed", playerMovementInd);

        desiredVelocity = new Vector2(directionX, 0f) * Mathf.Max(maxSpeed - friction, 0f);
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
        if (playerCombatSystemScript.isBlocking)
        {
            playerRB2D.velocity = Vector2.zero;
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

        onGround = playerJump.onGround;
        tempVelocity = playerRB2D.velocity;

        if (playerCombatSystemScript.heavyAttackExecuting == false && playerCombatSystemScript.lightAttackExecuting == false)
        {
            if (useAcceleration)
            {
                runWithAcceleration();
            }
            else
            {
                runWithoutAcceleration();
            }
        }
    }

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
        }
        else if (context.phase == InputActionPhase.Canceled || context.phase == InputActionPhase.Performed)
        {
            movementValue = context.ReadValue<float>();
            directionX = movementValue * speedMultiplier;
        }

    }

    public void OnRun(InputAction.CallbackContext context)
    {
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
        //If we're not using acceleration and deceleration, just send our desired velocity (direction * max speed) to the Rigidbody
        tempVelocity.x = desiredVelocity.x;

        if (playerJump.jumpInProgress)//Use jump velocity along horizontal aksis
        {
            tempVelocity.x = tempVelocity.x * playerJump.finalJumpMovementMultiplier;
            playerRB2D.velocity = tempVelocity * movementSpeedMultiplier * tempSpeedWithoutAccel;
        }
        else if (isOnSlope == false)//use normal velocity along eks aksis
        {
            playerRB2D.velocity = tempVelocity * movementSpeedMultiplier * tempSpeedWithoutAccel;
        }
        else
        {
            playerRB2D.velocity = new Vector2(-tempVelocity.x * slopeNormalPerp.x, -tempVelocity.x * slopeNormalPerp.y) * movementSpeedMultiplier * tempSpeedWithoutAccel;
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
        RaycastHit2D Hit3 = Physics2D.Raycast(checkSlopeObject.transform.position, Vector3.down, 1.0f, mask);
        if (onGround == true && Hit3.collider != null)
        {
            slopeNormalPerp = Vector2.Perpendicular(Hit3.normal).normalized;
            slopeDownAngle = Vector2.Angle(Hit3.normal, Vector2.up);

            if (slopeDownAngle > slopeAngleThreshold) isOnSlope = true;
            else isOnSlope = false;

            if (slopeDownAngle > slideDownAngleThreshold)
            {
                isSliding = true;
            }
            else isSliding = false;

            Debug.DrawRay(Hit3.point, Hit3.normal, Color.green);
            Debug.DrawRay(Hit3.point, slopeNormalPerp, Color.red);
        }
        else isOnSlope = false;

    }

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
