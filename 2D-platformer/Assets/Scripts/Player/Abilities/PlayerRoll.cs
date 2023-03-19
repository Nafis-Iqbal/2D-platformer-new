using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerRoll : MonoBehaviour {
    [Header("Roll")]
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private float rollForce = 25f;
    [SerializeField] private float rollDuration = 0.5f;
    [SerializeField] private float rollCooldownDuration = 0.5f;
    [SerializeField] private float timeElapsedSinceLastRoll;
    [SerializeField] private bool canRoll = true;
    public bool isRolling = false;
    private Rigidbody2D body;
    private PlayerGround playerGround;
    private PlayerMovement playerMovement;
    private bool onGround;

    private void Awake() {
        playerGround = GetComponent<PlayerGround>();
        playerMovement = GetComponent<PlayerMovement>();
        body = GetComponent<Rigidbody2D>();
    }

    private void Update() {
        if (!isRolling) {
            timeElapsedSinceLastRoll += Time.deltaTime;
            if (timeElapsedSinceLastRoll > rollCooldownDuration) {
                canRoll = true;
            } else {
                canRoll = false;
            }
        }
    }

    private void FixedUpdate() {
        onGround = playerGround.isGrounded;
    }

    IEnumerator DeactivateRolling() {
        yield return new WaitForSeconds(rollDuration);
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Enemy"), false);
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("EnemyWeapon"), false);
        isRolling = false;
        timeElapsedSinceLastRoll = 0f;
    }

    public void OnRoll(InputAction.CallbackContext context) {
        if (onGround && canRoll) {
            isRolling = true;
            if (context.ReadValue<float>() > 0f) {
                playerMovement.rotateRight();
            } else {
                playerMovement.rotateLeft();
            }
            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Enemy"), true);
            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("EnemyWeapon"), true);
            body.velocity = new Vector2(transform.localScale.x, 0) * rollForce;
            playerAnimator.SetTrigger("Roll");
            StartCoroutine(DeactivateRolling());
        }
    }
}
