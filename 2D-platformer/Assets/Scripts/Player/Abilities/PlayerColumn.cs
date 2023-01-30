using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerColumn : MonoBehaviour {
    private enum Direction {
        up,
        left,
        right,
    }

    private Direction jumpDirection;
    private PlayerMovement playerMovement;
    private PlayerGround playerGround;
    private PlayerJump playerJump;
    private Rigidbody2D body;
    private float originalGravity;
    private bool hasResetVelocity = false;
    private float timeElapsedSinceColumnGrab = 0f;
    private float originalColliderSize;
    private bool columnMechanicsSet = false;
    private bool fallingStarted = false;

    [Header("Column Grab")]
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private Vector3 columnColliderOffset;
    [SerializeField] private float columnWalkSpeed = 2f;
    [SerializeField] private float columnLength = 2f;
    [SerializeField] public bool canGrabColumn = false;
    [SerializeField] public bool hasGrabbedColumn = false;
    [SerializeField] private LayerMask columnLayerMask;
    [SerializeField] private float columnMoveSpeed = 20f;
    [SerializeField] private float columnJumpForce = 3f;
    [SerializeField] private float holdColumnTime = 1f;
    [SerializeField] private float gravityDuringTired = 1f;
    public float columnMoveDirection;

    private void Awake() {
        body = GetComponent<Rigidbody2D>();
        originalGravity = body.gravityScale;
        playerJump = GetComponent<PlayerJump>();
        playerMovement = GetComponent<PlayerMovement>();
        playerGround = GetComponent<PlayerGround>();
        disableColumnMechanics();
    }

    /// <summary>
    /// make player velocity = 0 and gravityScale = 0
    /// </summary>
    private void resetVelocityGravity() {
        if (!hasResetVelocity) {
            body.velocity = Vector2.zero;
            hasResetVelocity = true;
            originalGravity = body.gravityScale;
            body.gravityScale = 0f;
        }
    }

    private void Update() {
        if (playerGround.isGrounded && (timeElapsedSinceColumnGrab > holdColumnTime)) {
            timeElapsedSinceColumnGrab = 0f;
            hasGrabbedColumn = false;
        }

        if (!canGrabColumn) {
            hasGrabbedColumn = false;
        }

        if (hasGrabbedColumn) {
            timeElapsedSinceColumnGrab += Time.deltaTime;

            if (timeElapsedSinceColumnGrab > holdColumnTime) {
                startFalling();
            }

            if (!columnMechanicsSet) {
                enableColumnMechanics();
                columnMechanicsSet = true;
            }

            if (columnMoveDirection > 0.1f || columnMoveDirection < -0.1f) {
                var movePos = new Vector2(transform.position.x, transform.position.y + columnMoveDirection * columnMoveSpeed * Time.deltaTime);
                body.MovePosition(movePos);
                playerAnimator.SetFloat("columnWalkSpeed", columnWalkSpeed);
            } else {
                playerAnimator.SetFloat("columnWalkSpeed", 0);
            }
        } else {
            hasResetVelocity = false;
            fallingStarted = false;
            if (columnMechanicsSet) {
                disableColumnMechanics();
                columnMechanicsSet = false;
            }
        }

        RaycastHit2D raycastHit2D = Physics2D.Raycast(transform.position + columnColliderOffset * transform.localScale.x, Vector3.right * transform.localScale.x, columnLength, columnLayerMask);

        if (raycastHit2D.collider != null) {
            canGrabColumn = true;
        } else {
            canGrabColumn = false;
        }
    }

    /// <summary>
    /// Start falling from column
    /// </summary>
    private void startFalling() {
        if (!fallingStarted) {
            body.gravityScale = gravityDuringTired;
            enableTiredMechanics();
            fallingStarted = true;
        }
    }

    /// <summary>
    /// Enables tired animations
    /// </summary>
    private void enableTiredMechanics() {
        playerAnimator.Play("column grab tired");
        PlayerInputManager.Instance.playerInputActions.Player.ColumnMove.Disable();
    }

    /// <summary>
    /// Disables column mechanics and enables normal mechanics
    /// </summary>
    private void disableColumnMechanics() {
        timeElapsedSinceColumnGrab = 0f;
        PlayerInputManager.Instance.playerInputActions.Player.Run.Enable();
        PlayerInputManager.Instance.playerInputActions.Player.Walk.Enable();
        PlayerInputManager.Instance.playerInputActions.Player.Jump.Enable();

        PlayerInputManager.Instance.playerInputActions.Player.ColumnJump.Disable();
        PlayerInputManager.Instance.playerInputActions.Player.ColumnMove.Disable();
        PlayerInputManager.Instance.playerInputActions.Player.ColumnJumpDirection.Disable();
    }

    /// <summary>
    /// Disables normal mechanics and enables column mechanics
    /// </summary>
    private void enableColumnMechanics() {
        PlayerInputManager.Instance.playerInputActions.Player.Run.Disable();
        PlayerInputManager.Instance.playerInputActions.Player.Walk.Disable();
        PlayerInputManager.Instance.playerInputActions.Player.Jump.Disable();

        PlayerInputManager.Instance.playerInputActions.Player.ColumnJump.Enable();
        PlayerInputManager.Instance.playerInputActions.Player.ColumnMove.Enable();
        PlayerInputManager.Instance.playerInputActions.Player.ColumnJumpDirection.Enable();
        resetVelocityGravity();
    }

    /// <summary>
    /// On column jump add force to the player
    /// </summary>
    /// <param name="context">InputAction context</param>
    public void OnColumnJump(InputAction.CallbackContext context) {
        if (hasGrabbedColumn) {
            playerAnimator.SetTrigger("Jump");
            body.velocity = Vector2.zero;
            columnMoveDirection = 0f;
            body.gravityScale = originalGravity;
            if (jumpDirection == Direction.up) {
                // body.AddForce(Vector2.up * columnJumpForce, ForceMode2D.Impulse);
                body.velocity = Vector2.up * columnJumpForce;
            } else if (jumpDirection == Direction.left) {
                // body.AddForce(new Vector2(-1, 1) * columnJumpForce, ForceMode2D.Impulse);
                body.velocity = new Vector2(-1f, 1f) * columnJumpForce;

            } else {
                // body.AddForce(new Vector2(1, 1) * columnJumpForce, ForceMode2D.Impulse);
                body.velocity = new Vector2(1f, 1f) * columnJumpForce;
            }
            hasGrabbedColumn = false;
        }
    }

    /// <summary>
    /// On column move store the axis-value in move direction.
    /// </summary>
    /// <param name="context">Input action context</param>
    public void OnColumnMove(InputAction.CallbackContext context) {
        columnMoveDirection = context.ReadValue<float>();
    }

    /// <summary>
    /// Draws column hint gizmos.
    /// </summary>
    private void OnDrawGizmos() {
        if (canGrabColumn) {
            Gizmos.color = Color.green;
        } else {
            Gizmos.color = Color.red;
        }

        Gizmos.DrawLine(transform.position + columnColliderOffset * transform.localScale.x, transform.position + columnColliderOffset * transform.localScale.x + new Vector3(columnLength, 0, 0) * transform.localScale.x);

    }

    /// <summary>
    /// On column grab play column grab animation and set hasGrabbedColumn to true.
    /// </summary>
    /// <param name="context"></param>
    public void OnColumnGrab(InputAction.CallbackContext context) {
        if (context.phase == InputActionPhase.Performed) {
            if (hasGrabbedColumn) {
                hasGrabbedColumn = false;
            } else if (canGrabColumn) {
                hasGrabbedColumn = true;
                playerAnimator.Play("Column grab");
            }
        }
    }

    /// <summary>
    /// On column jump direction set, set jump direction.
    /// </summary>
    /// <param name="context">Input action context</param>
    public void OnColumnJumpDirection(InputAction.CallbackContext context) {
        if (context.ReadValue<float>() < 0) {
            jumpDirection = Direction.left;
        } else if (context.ReadValue<float>() > 0) {
            jumpDirection = Direction.right;
        } else {
            jumpDirection = Direction.up;
        }
    }
}
