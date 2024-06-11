using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerColumn : MonoBehaviour
{
    private enum Direction
    {
        up,
        left,
        right,
    }

    private Direction jumpDirection;
    private PlayerMovement playerMovement;
    private PlayerJump playerJump;
    private Rigidbody2D body;
    private float originalGravity;
    private bool hasResetVelocity = false;
    private float timeElapsedSinceColumnGrab = 0f;
    private bool columnMechanicsSet = false;
    private bool fallingStarted = false;
    public bool isExecuting;
    public float columnSearchDepth;

    [Header("Column Grab")]
    public ClimbColumnCollider ledgeColliderScript;
    [SerializeField] private GameObject climbCheckObjectUp;
    [SerializeField] private GameObject climbCheckObjectDown;
    [SerializeField] private Animator playerSpineAnimator;
    [SerializeField] private Vector3 columnColliderOffset;
    [SerializeField] private float columnWalkSpeed = 2f;
    [SerializeField] private float columnLength = 2f;
    public bool canGrabColumn;
    public bool hasGrabbedColumn = false;
    private float lastColumnReleaseTime;
    public bool atColumnTop;
    public bool atColumnBase;
    public bool isClimbingLedge;
    [SerializeField] private LayerMask columnLayerMask;
    [SerializeField] private float columnMoveUpSpeed = 2.5f;
    [SerializeField] private float columnMoveDownSpeed = 4f;
    [SerializeField] private float baseColumnJumpForce = 40f;
    public float columnJumpAngle = 50f;
    public bool jumpUpwards;
    public float horizontalJumpForceMultiplier;
    private Vector3 jumpAngleVector;
    [SerializeField] private float holdColumnTime = 1f;
    [SerializeField] private float gravityDuringTired = 1f;
    public float columnGrabCooldown = 1.5f;
    public bool isHoldLooking;
    public int columnPositionInd;
    public bool insideColumnRange;
    public int inRangeWallLadderID;
    public float columnMoveDirection;
    float finalColumnJumpForce;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        originalGravity = body.gravityScale;
        playerJump = GetComponent<PlayerJump>();
        playerMovement = GetComponent<PlayerMovement>();
        ledgeColliderScript = null;
        disableColumnMechanics();
        canGrabColumn = true;

        atColumnTop = false;
        atColumnBase = false;
    }

    private void OnEnable()
    {
        canGrabColumn = true;
        isHoldLooking = false;
        isClimbingLedge = false;
        jumpUpwards = false;

        atColumnTop = false;
        atColumnBase = false;
        insideColumnRange = false;

        columnPositionInd = 0;
        inRangeWallLadderID = -1;
        columnMoveDirection = 0;
        lastColumnReleaseTime = Time.time;
        playerSpineAnimator.ResetTrigger("Jump");
    }

    /// <summary>
    /// make player velocity = 0 and gravityScale = 0
    /// </summary>
    private void resetVelocityGravity()
    {
        if (!hasResetVelocity)
        {
            body.velocity = Vector2.zero;
            hasResetVelocity = true;
            originalGravity = body.gravityScale;
            body.gravityScale = 0f;
        }
    }

    private void FixedUpdate()
    {
        if (hasGrabbedColumn)
        {
            columnPositionInd = checkEndPositionsInColumn();

            if (columnMoveDirection > 0.1f && columnPositionInd < 2 && ledgeColliderScript.isSlipperyCollider == false)
            {
                var movePos = new Vector2(transform.position.x, transform.position.y + columnMoveDirection * columnMoveUpSpeed * Time.fixedDeltaTime);
                body.MovePosition(movePos);
            }
            else if (columnMoveDirection < -0.1f && columnPositionInd > -1)
            {
                var movePos = new Vector2(transform.position.x, transform.position.y + columnMoveDirection * columnMoveDownSpeed * Time.fixedDeltaTime);
                body.MovePosition(movePos);
            }
        }
    }

    private void Update()
    {
        if (insideColumnRange == true && hasGrabbedColumn == false)
        {
            OnWallGrab(inRangeWallLadderID);
        }
        else if (insideColumnRange == false)
        {
            atColumnBase = atColumnTop = false;
        }

        if (playerJump.onGround && (timeElapsedSinceColumnGrab > holdColumnTime))
        {
            timeElapsedSinceColumnGrab = 0f;
            hasGrabbedColumn = false;
            PlayerInputManager.Instance.lookDataRequired = false;

            atColumnBase = atColumnTop = false;
        }

        if (!canGrabColumn)
        {
            hasGrabbedColumn = false;
            PlayerInputManager.Instance.lookDataRequired = false;
            atColumnBase = atColumnTop = false;
        }

        if (hasGrabbedColumn)
        {
            timeElapsedSinceColumnGrab += Time.deltaTime;

            if (timeElapsedSinceColumnGrab > holdColumnTime && ledgeColliderScript != null && ledgeColliderScript.isSlipperyCollider == true)
            {
                startFalling();
            }

            if (!columnMechanicsSet)
            {
                enableColumnMechanics();
                columnMechanicsSet = true;
            }

            columnPositionInd = checkEndPositionsInColumn();

            if (columnMoveDirection > 0.1f && columnPositionInd < 2 && ledgeColliderScript.isSlipperyCollider == false)
            {
                playerSpineAnimator.SetInteger("WallLadderClimbDirection", 1);
            }
            else if (columnMoveDirection < -0.1f && columnPositionInd > -1)
            {
                playerSpineAnimator.SetInteger("WallLadderClimbDirection", -1);
            }
            else
            {
                playerSpineAnimator.SetInteger("WallLadderClimbDirection", 0);
            }
        }
        else
        {
            hasResetVelocity = false;
            fallingStarted = false;
            if (columnMechanicsSet)
            {
                disableColumnMechanics();
                columnMechanicsSet = false;
            }

            playerSpineAnimator.ResetTrigger("WallLadderHoldLook");
        }
    }

    private int checkEndPositionsInColumn()
    {
        if (climbCheckObjectDown.transform.position.y < ledgeColliderScript.downClimbLimitRef.transform.position.y)
        {
            atColumnBase = true;
            atColumnTop = false;
            return -1;//Cannot go below
        }
        else if (climbCheckObjectDown.transform.position.y > ledgeColliderScript.downClimbLimitRef.transform.position.y &&
        climbCheckObjectUp.transform.position.y < ledgeColliderScript.upClimbLimitRef.transform.position.y)
        {
            atColumnBase = false;
            atColumnTop = false;
            return 1;//Center
        }
        else if (climbCheckObjectUp.transform.position.y > ledgeColliderScript.upClimbLimitRef.transform.position.y)
        {
            atColumnBase = false;
            atColumnTop = true;

            return 2;//Cannot go top
        }

        return 0;
    }

    public void OnWallGrab(int wallLadderID)
    {
        if (checkEndPositionsInColumn() == 1 && Time.time - lastColumnReleaseTime > .25f)
        {
            MovementLimiter.instance.playerCanParkour = false;
            hasGrabbedColumn = true;
            PlayerInputManager.Instance.lookDataRequired = true;
            playerSpineAnimator.ResetTrigger("Jump");
            playerSpineAnimator.SetTrigger("WallLadderClimb");

            if (ledgeColliderScript.isRightCollider)
            {
                if (playerMovement.playerFacingRight)
                {
                    playerMovement.playerFacingRight = false;
                    playerMovement.rotateLeft();
                }
            }
            else
            {
                if (playerMovement.playerFacingRight == false)
                {
                    playerMovement.playerFacingRight = true;
                    playerMovement.rotateRight();
                }
            }
        }
    }
    public void OnColumnJumpClimb(InputAction.CallbackContext context)
    {
        if (hasGrabbedColumn)
        {
            lastColumnReleaseTime = Time.time;
            if (isHoldLooking == true)//JUMP FROM WALL
            {
                playerJump.wallJumpAnimInProgress = true;
                playerJump.wallJumpInProgress = true;
                playerSpineAnimator.SetTrigger("Jump");

                body.velocity = Vector2.zero;
                columnMoveDirection = 0f;
                body.gravityScale = originalGravity;
                hasGrabbedColumn = false;

                float aimAngle = Vector2.SignedAngle(PlayerInputManager.Instance.mousePosition.normalized, Vector2.right);
                aimAngle = -aimAngle;

                if (jumpDirection == Direction.left)
                {
                    if (aimAngle > 100.0f && aimAngle < 160.0f)
                    {
                        jumpUpwards = true;
                    }
                    else jumpUpwards = false;
                }
                else
                {
                    if (aimAngle > 20.0f && aimAngle < 70.0f)
                    {
                        jumpUpwards = true;
                    }
                    else jumpUpwards = false;
                }

                PlayerInputManager.Instance.lookDataRequired = false;

                if (jumpDirection == Direction.left)
                {
                    if (jumpUpwards)
                    {
                        columnJumpAngle = 50.0f;
                        finalColumnJumpForce = baseColumnJumpForce;
                        //jumpAngleVector = new Vector2(-1.0f, 1.0f);
                    }
                    else
                    {
                        columnJumpAngle = 10.0f;
                        finalColumnJumpForce = baseColumnJumpForce;
                        //jumpAngleVector = Vector2.left;
                    }

                    jumpAngleVector = Quaternion.AngleAxis(180.0f - columnJumpAngle, Vector3.forward) * new Vector3(1, 0, 0);
                    body.AddForce(jumpAngleVector * finalColumnJumpForce, ForceMode2D.Impulse);
                }
                else if (jumpDirection == Direction.right)
                {
                    if (jumpUpwards)
                    {
                        columnJumpAngle = 50.0f;
                        finalColumnJumpForce = baseColumnJumpForce;
                        //jumpAngleVector = new Vector2(1.0f, 1.0f);
                    }
                    else
                    {
                        columnJumpAngle = 10.0f;
                        finalColumnJumpForce = baseColumnJumpForce;
                        //jumpAngleVector = Vector2.right;
                    }

                    jumpAngleVector = Quaternion.AngleAxis(columnJumpAngle, Vector3.forward) * new Vector3(1, 0, 0);
                    body.AddForce(jumpAngleVector * finalColumnJumpForce, ForceMode2D.Impulse);
                }
            }
            else//CLIMB UP LEDGE
            {
                if (atColumnTop && ledgeColliderScript.hasClimbableLedge)
                {
                    ledgeColliderScript.triggerLedgeClimbAnim();
                    playerSpineAnimator.SetTrigger("WallLadderUpExit");
                    hasGrabbedColumn = false;
                    isClimbingLedge = true;
                    PlayerInputManager.Instance.lookDataRequired = false;
                }
                else if (atColumnBase)
                {
                    playerSpineAnimator.SetTrigger("WallLadderDownExit");
                    hasGrabbedColumn = false;
                    isClimbingLedge = true;
                    PlayerInputManager.Instance.lookDataRequired = false;
                }
            }

            PlayerInputManager.Instance.playerInputActions.Player.ColumnMove.Enable();
        }
    }

    public void OnColumnMove(InputAction.CallbackContext context)
    {
        columnMoveDirection = context.ReadValue<float>();
    }

    public void OnColumnJumpDirection(InputAction.CallbackContext context)
    {
        float contextValue = context.ReadValue<float>();

        if (playerMovement.playerFacingRight == true && contextValue < 0)
        {
            Debug.Log("baal val right: " + context.ReadValue<float>());

            jumpDirection = Direction.left;
            playerSpineAnimator.SetTrigger("WallLadderHoldLook");
            isHoldLooking = true;
        }
        else if (playerMovement.playerFacingRight == false && contextValue > 0)
        {
            Debug.Log("baal val left: " + context.ReadValue<float>());

            jumpDirection = Direction.right;
            playerSpineAnimator.SetTrigger("WallLadderHoldLook");
            isHoldLooking = true;
        }
        else if (isHoldLooking == true && contextValue < .01f && contextValue > -.01f)
        {
            playerSpineAnimator.SetTrigger("WallLadderHoldLook");
            isHoldLooking = false;
        }
    }

    public void OnColumnUpAndDownExit()
    {
        if (atColumnBase)
        {
            playerSpineAnimator.SetTrigger("WallLadderDownExit");
        }
        else if (atColumnTop)
        {
            playerSpineAnimator.SetTrigger("WallLadderUpExit");
        }
    }

    private void OnDrawGizmos()
    {
        if (canGrabColumn)
        {
            Gizmos.color = Color.green;
        }
        else
        {
            Gizmos.color = Color.red;
        }

        Gizmos.DrawLine(transform.position + columnColliderOffset * transform.localScale.x, transform.position + columnColliderOffset * transform.localScale.x + new Vector3(columnLength, 0, 0) * transform.localScale.x);

        //up check object
        Gizmos.DrawLine(climbCheckObjectUp.transform.position, climbCheckObjectUp.transform.position + new Vector3(columnSearchDepth, 0.0f, 0.0f));

        //down check object
        Gizmos.DrawLine(climbCheckObjectDown.transform.position, climbCheckObjectDown.transform.position + new Vector3(columnSearchDepth, 0.0f, 0.0f));
    }

    private void startFalling()
    {
        if (!fallingStarted)
        {
            body.gravityScale = gravityDuringTired;
            enableTiredMechanics();
            fallingStarted = true;
        }
    }

    private void enableTiredMechanics()
    {
        PlayerInputManager.Instance.playerInputActions.Player.ColumnMove.Disable();
    }

    private void disableColumnMechanics()
    {
        isExecuting = false;
        timeElapsedSinceColumnGrab = 0f;
    }

    private void enableColumnMechanics()
    {
        isExecuting = true;
        resetVelocityGravity();
    }
}
