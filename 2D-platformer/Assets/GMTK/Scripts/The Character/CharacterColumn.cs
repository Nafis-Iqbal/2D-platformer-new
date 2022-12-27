using System;
using System.Collections;
using System.Collections.Generic;
using GMTK.PlatformerToolkit;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterColumn : MonoBehaviour {

    private enum Direction {
        up,
        left,
        right,
    }

    private Direction jumpDirection;
    private CharacterMovement characterMovement;
    private CharacterGround characterGround;
    private CharacterJump characterJump;
    private Rigidbody2D body;
    private float originalGravity;
    private bool hasResetVelocity = false;
    [SerializeField] private float columnWalkSpeed = 2f;

    [Header("Column Grab")]
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private Vector3 columnColliderOffset;
    [SerializeField] private float columnLength = 2f;
    [SerializeField] public bool canGrabColumn = false;
    [SerializeField] public bool hasGrabbedColumn = false;
    [SerializeField] private LayerMask columnLayerMask;
    [SerializeField] private float columnMoveSpeed = 20f;
    [SerializeField] private float columnMoveDirection;
    [SerializeField] private float columnJumpForce = 3f;
    [SerializeField] private float holdColumnTime = 1f;
    [SerializeField] private float gravityDuringTired = 1f;
    private float timeElapsedSinceColumnGrab = 0f;
    private float originalColliderSize;
    private bool columnMechanicsSet = false;
    private bool fallingStarted = false;

    private void Awake() {
        body = GetComponent<Rigidbody2D>();
        originalGravity = body.gravityScale;
        characterJump = GetComponent<CharacterJump>();
        characterMovement = GetComponent<CharacterMovement>();
        characterGround = GetComponent<CharacterGround>();
        disableColumnMechanics();
    }

    private void resetVelocityGravity() {
        if (!hasResetVelocity) {
            body.velocity = Vector2.zero;
            hasResetVelocity = true;
            originalGravity = body.gravityScale;
            body.gravityScale = 0f;
        }
    }

    private void Update() {
        if (characterGround.isGrounded && (timeElapsedSinceColumnGrab > holdColumnTime)) {
            timeElapsedSinceColumnGrab = 0f;
            hasGrabbedColumn = false;
        }

        if (!canGrabColumn) {
            hasGrabbedColumn = false;
            hasResetVelocity = false;
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

    private void startFalling() {
        if (!fallingStarted) {
            body.gravityScale = gravityDuringTired;
            enableTiredMechanics();
            fallingStarted = true;
        }
    }

    private void enableTiredMechanics() {
        playerAnimator.Play("column grab tired");
        PlayerInputManager.Instance.playerInputActions.Player.ColumnMove.Disable();
    }

    private void disableColumnMechanics() {
        timeElapsedSinceColumnGrab = 0f;
        PlayerInputManager.Instance.playerInputActions.Player.Run.Enable();
        PlayerInputManager.Instance.playerInputActions.Player.Walk.Enable();
        PlayerInputManager.Instance.playerInputActions.Player.Jump.Enable();

        PlayerInputManager.Instance.playerInputActions.Player.ColumnJump.Disable();
        PlayerInputManager.Instance.playerInputActions.Player.ColumnMove.Disable();
        PlayerInputManager.Instance.playerInputActions.Player.ColumnJumpDirection.Disable();
    }

    private void enableColumnMechanics() {
        PlayerInputManager.Instance.playerInputActions.Player.Run.Disable();
        PlayerInputManager.Instance.playerInputActions.Player.Walk.Disable();
        PlayerInputManager.Instance.playerInputActions.Player.Jump.Disable();

        PlayerInputManager.Instance.playerInputActions.Player.ColumnJump.Enable();
        PlayerInputManager.Instance.playerInputActions.Player.ColumnMove.Enable();
        PlayerInputManager.Instance.playerInputActions.Player.ColumnJumpDirection.Enable();
        resetVelocityGravity();
    }

    public void OnColumnJump(InputAction.CallbackContext context) {
        if (hasGrabbedColumn) {
            playerAnimator.SetTrigger("Jump");
            body.gravityScale = originalGravity;
            if (jumpDirection == Direction.up) {

                body.AddForce(Vector2.up * columnJumpForce, ForceMode2D.Impulse);
            } else if (jumpDirection == Direction.left) {
                body.AddForce(new Vector2(-1, 1) * columnJumpForce, ForceMode2D.Impulse);
            } else {
                body.AddForce(new Vector2(1, 1) * columnJumpForce, ForceMode2D.Impulse);
            }
        }
    }

    public void OnColumnMove(InputAction.CallbackContext context) {
        columnMoveDirection = context.ReadValue<float>();
    }

    private void OnDrawGizmos() {
        if (canGrabColumn) {
            Gizmos.color = Color.green;
        } else {
            Gizmos.color = Color.red;
        }

        Gizmos.DrawLine(transform.position + columnColliderOffset * transform.localScale.x, transform.position + columnColliderOffset * transform.localScale.x + new Vector3(columnLength, 0, 0) * transform.localScale.x);

    }

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
