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
    public bool atColumnTop;
    public bool atColumnBase;
    [SerializeField] private LayerMask columnLayerMask;
    [SerializeField] private float columnMoveUpSpeed = 2.5f;
    [SerializeField] private float columnMoveDownSpeed = 4f;
    [SerializeField] private float columnJumpForce = 3f;
    [SerializeField] private float holdColumnTime = 1f;
    [SerializeField] private float gravityDuringTired = 1f;
    public float columnGrabCooldown = 1.5f;
    public bool isHoldLooking;
    public int columnPositionInd;
    public bool insideColumnRange;
    public int inRangeWallLadderID;
    public float columnMoveDirection;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        originalGravity = body.gravityScale;
        playerJump = GetComponent<PlayerJump>();
        playerMovement = GetComponent<PlayerMovement>();
        disableColumnMechanics();
        canGrabColumn = true;

        atColumnTop = false;
        atColumnBase = false;
    }

    private void OnEnable()
    {
        canGrabColumn = true;
        isHoldLooking = false;

        atColumnTop = false;
        atColumnBase = false;
        insideColumnRange = false;

        columnPositionInd = 0;
        inRangeWallLadderID = -1;
        columnMoveDirection = 0;
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

            if (columnMoveDirection > 0.1f && columnPositionInd < 2)
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

        if (playerJump.onGround && (timeElapsedSinceColumnGrab > holdColumnTime))
        {
            timeElapsedSinceColumnGrab = 0f;
            hasGrabbedColumn = false;
        }

        if (!canGrabColumn)
        {
            hasGrabbedColumn = false;
        }

        if (hasGrabbedColumn)
        {
            timeElapsedSinceColumnGrab += Time.deltaTime;

            if (timeElapsedSinceColumnGrab > holdColumnTime)
            {
                startFalling();
            }

            if (!columnMechanicsSet)
            {
                enableColumnMechanics();
                columnMechanicsSet = true;
            }

            columnPositionInd = checkEndPositionsInColumn();

            if (columnMoveDirection > 0.1f && columnPositionInd < 2)
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
        LayerMask mask = LayerMask.GetMask("Ground");

        RaycastHit2D upObjectHit = Physics2D.Raycast(climbCheckObjectUp.transform.position, Vector3.right, 3.0f, mask);
        RaycastHit2D downObjectHit = Physics2D.Raycast(climbCheckObjectDown.transform.position, Vector3.right, 3.0f, mask);

        if (downObjectHit.collider != null)
        {
            if (downObjectHit.distance < 1.0f && downObjectHit.collider.CompareTag("Platforms"))
            {
                atColumnBase = true;
                atColumnTop = false;
                return -1;//Cannot go below
            }
            else if (downObjectHit.distance < 1.0f && downObjectHit.collider.CompareTag("Column"))
            {
                atColumnBase = false;
                atColumnTop = false;

                if (upObjectHit.collider != null)
                {
                    if (upObjectHit.distance < 1.0f && upObjectHit.collider.CompareTag("Platforms"))
                    {
                        atColumnTop = true;
                        return 2;//Cannot go top
                    }
                }
                return 1;//Center
            }
        }
        return 0;
    }

    public void OnWallGrab(int wallLadderID)
    {
        if (checkEndPositionsInColumn() == 1)
        {
            MovementLimiter.instance.playerCanParkour = false;
            hasGrabbedColumn = true;
            playerSpineAnimator.SetTrigger("WallLadderClimb");
        }
    }
    public void OnColumnJumpClimb(InputAction.CallbackContext context)
    {
        if (hasGrabbedColumn)
        {
            if (isHoldLooking == true)
            {
                playerSpineAnimator.SetTrigger("Jump");
                body.velocity = Vector2.zero;
                columnMoveDirection = 0f;
                body.gravityScale = originalGravity;
                if (jumpDirection == Direction.up)
                {
                    // body.AddForce(Vector2.up * columnJumpForce, ForceMode2D.Impulse);
                    body.velocity = Vector2.up * columnJumpForce;
                }
                else if (jumpDirection == Direction.left)
                {
                    // body.AddForce(new Vector2(-1, 1) * columnJumpForce, ForceMode2D.Impulse);
                    body.velocity = new Vector2(-1f, 1f) * columnJumpForce;

                }
                else
                {
                    // body.AddForce(new Vector2(1, 1) * columnJumpForce, ForceMode2D.Impulse);
                    body.velocity = new Vector2(1f, 1f) * columnJumpForce;
                }
            }
            else
            {
                if (atColumnTop)
                {
                    ledgeColliderScript.triggerLedgeClimbAnim();
                    playerSpineAnimator.SetTrigger("WallLadderUpExit");
                }
                else if (atColumnBase)
                {
                    playerSpineAnimator.SetTrigger("WallLadderDownExit");
                }
            }
        }
    }

    public void OnColumnMove(InputAction.CallbackContext context)
    {
        columnMoveDirection = context.ReadValue<float>();
    }

    public void OnColumnJumpDirection(InputAction.CallbackContext context)
    {
        playerSpineAnimator.SetTrigger("WallLadderHoldLook");

        if (context.ReadValue<float>() < 0)
        {
            jumpDirection = Direction.left;
            isHoldLooking = true;
        }
        else if (context.ReadValue<float>() > 0)
        {
            jumpDirection = Direction.right;
            isHoldLooking = true;
        }
        else
        {
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
        Gizmos.DrawLine(climbCheckObjectUp.transform.position, climbCheckObjectUp.transform.position + new Vector3(0.2f, 0.0f, 0.0f));

        //down check object
        Gizmos.DrawLine(climbCheckObjectDown.transform.position, climbCheckObjectDown.transform.position + new Vector3(0.2f, 0.0f, 0.0f));
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
        playerSpineAnimator.SetTrigger("ClimbHoldTired");
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
