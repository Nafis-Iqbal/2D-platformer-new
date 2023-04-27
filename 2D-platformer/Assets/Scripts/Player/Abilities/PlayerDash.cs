using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerDash : MonoBehaviour {
    private Rigidbody2D body;
    [Header("Dash")]
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private Animator playerSpineAnimator;
    [SerializeField] public bool isExecuting;
    [SerializeField] public float dashingPower = 24f;
    [SerializeField] public float dashingTime = 0.2f;
    [SerializeField] public float dashingCooldown = 1f;
    [SerializeField] private bool canDash = true;

    private PlayerMovement playerMovement;

    private void Awake() {
        body = GetComponent<Rigidbody2D>();
        playerMovement = GetComponent<PlayerMovement>();
        playerSpineAnimator = GameManager.Instance.playerSpineAnimator;
    }

    /// <summary>
    /// removes gravity, adds velocity to the player and then reset gravity after some times.
    /// </summary>
    /// <returns></returns>
    private IEnumerator Dash() {
        canDash = false;
        isExecuting = true;
        float originalGravity = body.gravityScale;
        body.gravityScale = 0f;
        body.velocity = new Vector2(transform.localScale.x * dashingPower, 0f);
        yield return new WaitForSeconds(dashingTime);
        body.gravityScale = originalGravity;
        isExecuting = false;
        yield return new WaitForSeconds(dashingCooldown);
        canDash = true;
    }

    /// <summary>
    /// On dash key pressed play dash animation and dash with coroutine.
    /// </summary>
    /// <param name="context"></param>
    public void OnDash(InputAction.CallbackContext context) {
        if (canDash) {
            playerAnimator.SetTrigger("Dash");
            playerSpineAnimator.SetTrigger("Dash");
            StartCoroutine(Dash());
        }
    }
}
