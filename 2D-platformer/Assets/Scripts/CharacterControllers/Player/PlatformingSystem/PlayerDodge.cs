using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerDodge : MonoBehaviour
{
    private Rigidbody2D body;
    private PlayerMovement playerMovement;

    [Header("Dash")]
    private Animator playerSpineAnimator;
    public float dodgingPower = 24f;
    public float dodgingTime = 0.2f;
    public float dodgingCooldown = 1f;
    public bool canDodge = true;
    public bool isExecuting;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        playerMovement = GetComponent<PlayerMovement>();
        playerSpineAnimator = GameManager.Instance.playerSpineAnimator;
    }

    private IEnumerator Dodge()
    {
        canDodge = false;
        isExecuting = true;
        float originalGravity = body.gravityScale;
        body.gravityScale = 0f;
        body.velocity = new Vector2(transform.localScale.x * dodgingPower, 0f);
        yield return new WaitForSeconds(dodgingTime);
        body.gravityScale = originalGravity;
        isExecuting = false;
        yield return new WaitForSeconds(dodgingCooldown);
        canDodge = true;
    }

    public void OnDodge(InputAction.CallbackContext context)
    {
        if (canDodge)
        {
            playerSpineAnimator.SetTrigger("Dodge");
            StartCoroutine(Dodge());
        }
    }
}

