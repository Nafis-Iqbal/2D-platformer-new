using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class ClimbColumnCollider : MonoBehaviour
{
    public bool isSlipperyCollider;
    public bool isRightCollider;
    public bool hasClimbableLedge;
    private Transform playerTransform;
    private float originalGravityScale;

    private PlayerColumn playerColumn;
    private Rigidbody2D playerRigidbody;
    private Animator ledgeClimbAnimator;
    public GameObject leftLedgeTeleportObject;
    public GameObject rightLedgeTeleportObject;
    public GameObject upClimbLimitRef, downClimbLimitRef;
    public int wallLadderID;

    private void Awake()
    {
        playerColumn = GameManager.Instance.playerTransform.GetComponent<PlayerColumn>();
        playerRigidbody = GameManager.Instance.playerTransform.GetComponent<Rigidbody2D>();
        ledgeClimbAnimator = GetComponent<Animator>();
    }

    /// On trigger enter the ledge section, start animating player.
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Player")
        {
            playerTransform = other.transform;
            playerColumn.inRangeWallLadderID = wallLadderID;
            playerColumn.insideColumnRange = true;
            playerColumn.ledgeColliderScript = this;
            ledgeClimbAnimator.ResetTrigger("AssistLedgeClimb");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.tag == "Player")
        {
            playerTransform = other.transform;
            playerColumn.ledgeColliderScript = null;

            playerColumn.insideColumnRange = false;
            playerColumn.hasGrabbedColumn = false;

            playerColumn.columnPositionInd = 0;
            playerColumn.inRangeWallLadderID = -1;
            playerColumn.columnMoveDirection = 0;
        }
    }

    public void triggerLedgeClimbAnim()
    {
        if (isRightCollider)
        {
            playerTransform.transform.position = rightLedgeTeleportObject.transform.position;
            playerTransform.parent = rightLedgeTeleportObject.transform;
        }
        else
        {
            playerTransform.transform.position = leftLedgeTeleportObject.transform.position;
            playerTransform.parent = leftLedgeTeleportObject.transform;
        }

        ledgeClimbAnimator.SetTrigger("AssistLedgeClimb");
    }

    public void deparentPlayerObject()
    {
        playerTransform.parent = null;
    }

    /// remove gravity and reset gravity after some time and move the player.
    IEnumerator movePlayerCoroutine()
    {
        PlayerInputManager.Instance.playerInputActions.Player.Disable();
        originalGravityScale = playerRigidbody.gravityScale;
        playerRigidbody.gravityScale = 0f;
        GameManager.Instance.playerSpineAnimator.SetTrigger("WallLadderClimb");
        yield return new WaitForSeconds(0.3f);
        playerTransform.position = transform.position;
        playerRigidbody.gravityScale = originalGravityScale;
        PlayerInputManager.Instance.playerInputActions.Player.Enable();
    }
}
