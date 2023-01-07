using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class ClimbColumnCollider : MonoBehaviour {
    private Transform playerTransform;
    private float originalGravityScale;

    private CharacterColumn characterColumn;
    private Rigidbody2D playerRigidbody;

    private void Awake() {
        characterColumn = GameManager.Instance.playerTransform.GetComponent<CharacterColumn>();
        playerRigidbody = GameManager.Instance.playerTransform.GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// On trigger enter the ledge section, start animating player.
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player")) {
            playerTransform = other.transform;
            if ((playerTransform.position.y < transform.position.y) && (playerRigidbody.velocity.y < 0.01f) && characterColumn.columnMoveDirection > 0.1f) {
                StartCoroutine(movePlayerCoroutine());
            }
        }
    }

    /// <summary>
    /// remove gravity and reset gravity after some time and move the player.
    /// </summary>
    /// <returns></returns>
    IEnumerator movePlayerCoroutine() {
        PlayerInputManager.Instance.playerInputActions.Player.Disable();
        originalGravityScale = playerRigidbody.gravityScale;
        playerRigidbody.gravityScale = 0f;
        GameManager.Instance.playerAnimator.Play("column crawl");
        yield return new WaitForSeconds(0.3f);
        playerTransform.position = transform.position;
        playerRigidbody.gravityScale = originalGravityScale;
        PlayerInputManager.Instance.playerInputActions.Player.Enable();
    }
}
