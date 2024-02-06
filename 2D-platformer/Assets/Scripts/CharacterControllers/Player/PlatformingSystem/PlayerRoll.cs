using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerRoll : MonoBehaviour
{
    private Rigidbody2D body;
    private PlayerJump playerJump;
    private PlayerMovement playerMovement;

    [Header("Roll")]
    private Animator playerSpineAnimator;
    public float rollForce = 25f;
    public float rollCooldownDuration = 0.5f;
    private float timeElapsedSinceLastRoll;
    public bool canRoll = true;
    public bool isExecuting = false;
    public bool onGround;

    private void Awake()
    {
        playerJump = GetComponent<PlayerJump>();
        body = GetComponent<Rigidbody2D>();
        playerMovement = GetComponent<PlayerMovement>();
        playerSpineAnimator = GameManager.Instance.playerSpineAnimator;
    }

    private void Update()
    {
        if (!isExecuting)
        {
            timeElapsedSinceLastRoll += Time.deltaTime;
            if (timeElapsedSinceLastRoll > rollCooldownDuration)
            {
                canRoll = true;
            }
            else
            {
                canRoll = false;
            }
        }
    }

    private void FixedUpdate()
    {
        onGround = playerJump.onGround;
    }

    public void OnRoll(InputAction.CallbackContext context)
    {
        if (onGround && canRoll && !isExecuting)
        {
            Debug.Log($"val: {context.ReadValue<float>()}");
            isExecuting = true;

            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Enemy"), true);
            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("EnemyWeapon"), true);

            if (playerMovement.playerFacingRight)
            {
                body.velocity = Vector2.right * rollForce;
            }
            else
            {
                body.velocity = Vector2.left * rollForce;
            }

            playerSpineAnimator.SetTrigger("Roll");
        }
    }

    public void DeactivateRolling()
    {
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Enemy"), false);
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("EnemyWeapon"), false);
        isExecuting = false;
        timeElapsedSinceLastRoll = 0f;
    }
}
