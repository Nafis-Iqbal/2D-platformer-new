using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerDash : MonoBehaviour
{
    private Rigidbody2D body;
    private PlayerMovement playerMovement;

    [Header("Dash")]
    private Animator playerSpineAnimator;
    public float dashingPower = 24f;
    public float dashingTime = 0.2f;
    public float dashingCooldown = 1f;
    public bool canDash = true;
    public bool isExecuting;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        playerMovement = GetComponent<PlayerMovement>();
        playerSpineAnimator = GameManager.Instance.playerSpineAnimator;
    }

    private IEnumerator Dash()
    {
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

    public void OnDash(InputAction.CallbackContext context)
    {
        if (canDash)
        {
            playerSpineAnimator.SetTrigger("Dash");
            StartCoroutine(Dash());
        }
    }
}
