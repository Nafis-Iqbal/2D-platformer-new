using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

//This script handles moving the character on the X axis, both on the ground and in the air.

public class PlayerMovement : MonoBehaviour
{
    public float tempSpeedTurn = 1.0f;
    public float tempSpeedAccel = 1.0f;
    public float tempSpeedDecel = 1.0f;
    public float tempSpeedWithoutAccel = 1.0f;

    public float movementSpeedMultiplier = 1f;
    public PlayerCombatSystem playerCombatSystemScript;
    private PlayerJump playerJump;
    private PlayerColumn playerColumn;
    private PlayerDodge playerDodge;
    private PlayerRoll playerRoll;
    private PlayerGrapplingGun playerGrapplingGun;
    private Rigidbody2D body;
    private Animator playerSpineAnimator;
    private Vector2 desiredVelocity;
    private float maxSpeedChange;
    private float acceleration;
    private float deceleration;
    private float turnSpeed;
    public bool isSprinting;


    private float delta;

    [Header("Movement Stats")]
    [SerializeField, Range(0f, 20f)][Tooltip("Maximum movement speed")] public float maxSpeed = 10f;
    [SerializeField, Range(0f, 100f)][Tooltip("How fast to reach max speed")] public float maxAcceleration = 52f;
    [SerializeField, Range(0f, 100f)][Tooltip("How fast to stop after letting go")] public float maxDeceleration = 52f;
    [SerializeField, Range(0f, 100f)][Tooltip("How fast to stop when changing direction")] public float maxTurnSpeed = 80f;
    [SerializeField, Range(0f, 100f)][Tooltip("How fast to reach max speed when in mid-air")] public float maxAirAcceleration;
    [SerializeField, Range(0f, 100f)][Tooltip("How fast to stop in mid-air when no direction is used")] public float maxAirDeceleration;
    [SerializeField, Range(0f, 100f)][Tooltip("How fast to stop when changing direction when in mid-air")] public float maxAirTurnSpeed = 80f;
    [SerializeField, Range(0f, 1f)][Tooltip("walk speed = walkMultiplier * joggingSpeed")] public float walkMultiplier = 0.1f;
    [SerializeField, Range(0f, 2f)][Tooltip("sprint speed = sprintMultiplier * joggingSpeed")] public float sprintMultiplier = 0.1f;
    [SerializeField][Tooltip("Friction to apply against movement on stick")] private float friction;

    [Header("Options")]
    [Tooltip("When false, the charcter will skip acceleration and deceleration and instantly move and stop")][SerializeField] private bool useAcceleration;

    [Header("Calculations")]
    [SerializeField] public Vector2 velocity;
    [SerializeField] private float directionX;

    [Header("Current State")]
    public bool playerFacingRight = true;
    public bool onGround;
    public bool pressingKey;
    public float movementValue;
    [Tooltip("Used in Animator to select jogg, sprint or walk animations")]
    public int playerMovementInd;
    [SerializeField] private float speedMultiplier;

    private void OnEnable()
    {

        //Find the character's Rigidbody and ground detection script
        body = GetComponent<Rigidbody2D>();
        playerJump = GetComponent<PlayerJump>();
        playerDodge = GetComponent<PlayerDodge>();
        playerRoll = GetComponent<PlayerRoll>();
        playerGrapplingGun = GetComponent<PlayerGrapplingGun>();
        playerCombatSystemScript = GetComponent<PlayerCombatSystem>();
        playerSpineAnimator = GameManager.Instance.playerSpineAnimator;
        playerFacingRight = true;
    }

    public void rotateLeft()
    {
        Vector3 tempScale = transform.localScale;
        tempScale.x *= -1.0f;
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

    public void OnMovement(InputAction.CallbackContext context)
    {
        //This is called when you input a direction on a valid input type, such as arrow keys or analogue stick
        //The value will read -1 when pressing left, 0 when idle, and 1 when pressing right.

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
            body.AddForce(Vector2.right * context.ReadValue<float>() * playerGrapplingGun.hangSwingForce);
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

    public void OnWalk(InputAction.CallbackContext context)
    {
        Debug.Log("walk context: " + context);
        if (context.phase == InputActionPhase.Started)
        {
            // isWalking = true;
            // Debug.Log("walk true");
        }
        else if (context.phase == InputActionPhase.Canceled)
        {
            // isWalking = false;
            // Debug.Log("walk false");
        }
    }

    private void Update()
    {

        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            isSprinting = true;
            speedMultiplier = sprintMultiplier;
            playerMovementInd = 2;
        }
        else
        {
            isSprinting = false;
            speedMultiplier = 1f;
            playerMovementInd = 1;
        }
        directionX = movementValue * speedMultiplier;

        if (playerDodge.isExecuting || playerRoll.isExecuting || playerGrapplingGun.grapplingRope.isGrappling)
        {
            return;
        }

        //Used to stop movement when the character is playing her death animation
        if (!MovementLimiter.instance.playerCanMove)
        {
            directionX = 0;
            playerMovementInd = 0;
        }

        //Used to flip the character's sprite when she changes direction
        //Also tells us that we are currently pressing a direction button
        if (directionX != 0)
        {
            //transform.localScale = new Vector3(directionX > 0 ? 1 : -1, 1, 1);
            if (directionX > 0 && playerFacingRight == false)
            {
                rotateRight();
            }
            else if (directionX < 0 && playerFacingRight == true)
            {
                rotateLeft();
            }

            pressingKey = true;
        }
        else
        {
            pressingKey = false;
            playerMovementInd = 0;
        }

        playerSpineAnimator.SetInteger("MoveSpeed", playerMovementInd);

        //Calculate's the character's desired velocity - which is the direction you are facing, multiplied by the character's maximum speed
        //Friction is not used in this game
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
        if (playerCombatSystemScript.heavyAttackExecuting || playerCombatSystemScript.lightAttackExecuting
            || playerCombatSystemScript.shurikenAttackExecuting || playerCombatSystemScript.projectileAttackExecuting
            || playerCombatSystemScript.isBlocking)
        {
            body.velocity = Vector2.zero;
            return;
        }

        //Fixed update runs in sync with Unity's physics engine

        //Get Kit's current ground status from her ground script
        onGround = playerJump.onGround;

        //Get the Rigidbody's current velocity
        velocity = body.velocity;

        //Calculate movement, depending on whether "Instant Movement" has been checked
        if (useAcceleration)
        {
            runWithAcceleration();
        }
        else
        {
            if (onGround)
            {
                runWithoutAcceleration();
            }
            else
            {
                runWithAcceleration();
            }
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
            if (Mathf.Sign(directionX) != Mathf.Sign(velocity.x))
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
        velocity.x = Mathf.MoveTowards(velocity.x, desiredVelocity.x, maxSpeedChange);

        //Update the Rigidbody with this new velocity
        body.velocity = velocity * movementSpeedMultiplier;

    }

    private void runWithoutAcceleration()
    {
        //If we're not using acceleration and deceleration, just send our desired velocity (direction * max speed) to the Rigidbody
        velocity.x = desiredVelocity.x;

        body.velocity = velocity * movementSpeedMultiplier * tempSpeedWithoutAccel;
    }

    public void teleportAndSynchronizePlayer(Vector3 targetTeleportPosition)
    {
        transform.position = targetTeleportPosition;
    }
}
