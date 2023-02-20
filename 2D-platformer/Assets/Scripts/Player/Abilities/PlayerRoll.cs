using System.Collections;
using System.Collections.Generic;
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
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Enemy"), false);
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("EnemyWeapon"), false);
        isRolling = false;
    }

    public void OnRoll(InputAction.CallbackContext context) {
        if (onGround) {
            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Enemy"), true);
            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("EnemyWeapon"), true);
            body.velocity = new Vector2(transform.localScale.x, 0) * rollForce;
            playerAnimator.SetTrigger("Roll");
            isRolling = true;
            StartCoroutine(deactivateRolling());
        }
    }
}
