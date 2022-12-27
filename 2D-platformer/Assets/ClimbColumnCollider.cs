using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClimbColumnCollider : MonoBehaviour {
    [SerializeField] private Animator playerAnimator;
    private Transform playerTransform;
    [SerializeField] private Rigidbody2D playerRigidbody;
    private float originalGravityScale;

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player")) {
            playerTransform = other.transform;
            if ((playerTransform.position.y < transform.position.y) && (playerRigidbody.velocity.y < 0.01f)) {
                StartCoroutine(movePlayerCoroutine());
            }
        }
    }

    IEnumerator movePlayerCoroutine() {
        PlayerInputManager.Instance.playerInputActions.Player.Disable();
        originalGravityScale = playerRigidbody.gravityScale;
        playerRigidbody.gravityScale = 0f;
        playerAnimator.Play("column crawl");
        yield return new WaitForSeconds(0.20f);
        playerTransform.position = transform.position;
        playerRigidbody.gravityScale = originalGravityScale;
        PlayerInputManager.Instance.playerInputActions.Player.Enable();
    }
}
