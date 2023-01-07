using System.Collections;
using System.Collections.Generic;
using GMTK.PlatformerToolkit;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerRoll : MonoBehaviour {
    [Header("Roll")]
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private float rollForce = 25f;
    [SerializeField] private float rollDuration = 0.5f;
    public bool isRolling = false;
    private Rigidbody2D body;
    private PlayerGround playerGround;
    private bool onGround;

    private void Awake() {
        playerGround = GetComponent<PlayerGround>();
        body = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate() {
        onGround = playerGround.isGrounded;
    }

    IEnumerator deactivateRolling() {
        yield return new WaitForSeconds(rollDuration);
        isRolling = false;
    }

    public void OnRoll(InputAction.CallbackContext context) {
        if (onGround) {
            body.velocity = new Vector2(transform.localScale.x, 0) * rollForce;
            // Vector2 v = new Vector2(transform.localScale.x, 0) * rollForce;
            // body.AddForce(v, ForceMode2D.Impulse);
            playerAnimator.SetTrigger("Roll");
            isRolling = true;
            StartCoroutine(deactivateRolling());
        }
    }
}
