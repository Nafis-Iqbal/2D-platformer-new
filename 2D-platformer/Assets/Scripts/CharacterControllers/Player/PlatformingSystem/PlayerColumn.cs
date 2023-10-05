using System;
using System.Collections;
using System.Collections.Generic;
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
    private float originalColliderSize;
    private bool columnMechanicsSet = false;
    private bool fallingStarted = false;
    public bool isExecuting;

    [Header("Column Grab")]
    [SerializeField] private Animator playerSpineAnimator;
    [SerializeField] private Vector3 columnColliderOffset;
    [SerializeField] private float columnWalkSpeed = 2f;
    [SerializeField] private float columnLength = 2f;
    [SerializeField] public bool canGrabColumn;
    [SerializeField] public bool hasGrabbedColumn = false;
    [SerializeField] private LayerMask columnLayerMask;
    [SerializeField] private float columnMoveSpeed = 20f;
    [SerializeField] private float columnJumpForce = 3f;
    [SerializeField] private float holdColumnTime = 1f;
    [SerializeField] private float gravityDuringTired = 1f;
    public float columnMoveDirection;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        originalGravity = body.gravityScale;
        playerJump = GetComponent<PlayerJump>();
        playerMovement = GetComponent<PlayerMovement>();
        disableColumnMechanics();
        canGrabColumn = true;
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
        if (columnMoveDirection > 0.1f || columnMoveDirection < -0.1f)
        {
            var movePos = new Vector2(transform.position.x, transform.position.y + columnMoveDirection * columnMoveSpeed * Time.fixedDeltaTime);
            body.MovePosition(movePos);
        }
    }

    private void Update()
    {
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

            if (columnMoveDirection > 0.1f || columnMoveDirection < -0.1f)
            {
                playerSpineAnimator.SetFloat("columnWalkSpeed", columnWalkSpeed);
            }
            else
            {
                playerSpineAnimator.SetFloat("columnWalkSpeed", 0);
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
        }
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
        playerSpineAnimator.Play("column grab tired");
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

    public void OnColumnJump(InputAction.CallbackContext context)
    {
        if (hasGrabbedColumn)
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
            hasGrabbedColumn = false;
        }
    }

    public void OnColumnMove(InputAction.CallbackContext context)
    {
        columnMoveDirection = context.ReadValue<float>();
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

    }

    public void OnWallGrab()
    {
        hasGrabbedColumn = true;
        playerSpineAnimator.SetTrigger("WallClimb");
    }

    public void OnColumnJumpDirection(InputAction.CallbackContext context)
    {
        if (context.ReadValue<float>() < 0)
        {
            jumpDirection = Direction.left;
        }
        else if (context.ReadValue<float>() > 0)
        {
            jumpDirection = Direction.right;
        }
        else
        {
            jumpDirection = Direction.up;
        }
    }
}
