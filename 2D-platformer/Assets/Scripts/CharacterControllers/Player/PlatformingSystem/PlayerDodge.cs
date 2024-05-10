using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerDodge : MonoBehaviour
{
    private Rigidbody2D body;
    private PlayerJump playerJump;
    private PlayerMovement playerMovement;
    private PlayerCombatSystem playerCombatScript;

    [Header("Roll")]
    private Animator playerSpineAnimator;
    public float dodgeForce = 5f;
    public float dodgeCooldownDuration = 0.5f;
    private float timeElapsedSinceLastDodge;
    public bool canDodge = true;
    public bool isExecuting = false;
    public bool onGround;

    private void Awake()
    {
        playerJump = GetComponent<PlayerJump>();
        body = GetComponent<Rigidbody2D>();
        playerMovement = GetComponent<PlayerMovement>();
        playerSpineAnimator = GameManager.Instance.playerSpineAnimator;
        playerCombatScript = GetComponent<PlayerCombatSystem>();
    }

    void OnEnable()
    {
        isExecuting = false;
        canDodge = true;
        DeactivateDodging();
    }

    private void Update()
    {
        if (!isExecuting)
        {
            timeElapsedSinceLastDodge += Time.deltaTime;
            if (timeElapsedSinceLastDodge > dodgeCooldownDuration)
            {
                canDodge = true;
            }
            else
            {
                canDodge = false;
            }

            if (playerMovement.isSprinting == true) canDodge = false;
        }
    }

    private void FixedUpdate()
    {
        onGround = playerJump.onGround;
    }

    public void OnDodge(InputAction.CallbackContext context)
    {
        if (onGround && canDodge && !isExecuting && !playerMovement.isSprinting && 
        playerCombatScript.combatMode == true && !playerCombatScript.isHurt && !playerCombatScript.isKnockedOffGround && !playerCombatScript.inKnockedOffAnim && 
        !playerCombatScript.isHeavyAttackKeyPressed &&!playerCombatScript.lightAttackExecuting)
        {
            Debug.Log($"val: {context.ReadValue<float>()}");
            isExecuting = true;

            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Enemy"), true);
            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("EnemyWeapon"), true);

            if (playerMovement.playerFacingRight)
            {
                body.velocity = Vector2.left * dodgeForce;
            }
            else
            {
                body.velocity = Vector2.right * dodgeForce;
            }

            playerSpineAnimator.SetTrigger("Dodge");
        }
    }

    public void DeactivateDodging()
    {
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Enemy"), false);
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("EnemyWeapon"), false);
        isExecuting = false;
        timeElapsedSinceLastDodge = 0f;
    }
}
