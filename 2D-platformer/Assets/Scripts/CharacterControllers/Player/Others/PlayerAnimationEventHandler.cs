using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationEventHandler : MonoBehaviour
{
    private PlayerCombatSystem playerCombatSystemScript;
    private GameObject lightAttackHitBox;
    private Transform playerTransform;
    private Rigidbody2D playerRB2D;
    private PlayerMovement playerMovement;
    private PlayerJump playerJump;
    private PlayerColumn playerColumn;
    private PlayerAppearanceScript playerAppearanceScript;

    private void Awake()
    {
        playerTransform = GameManager.Instance.playerTransform;
        playerRB2D = playerTransform.GetComponent<Rigidbody2D>();
        playerCombatSystemScript = playerTransform.GetComponent<PlayerCombatSystem>();

        playerMovement = playerTransform.GetComponent<PlayerMovement>();
        playerJump = playerTransform.GetComponent<PlayerJump>();
        playerColumn = playerTransform.GetComponent<PlayerColumn>();
        playerAppearanceScript = playerTransform.GetComponent<PlayerAppearanceScript>();
        lightAttackHitBox = GameManager.Instance.lightAttackHitBox;
    }

    #region Player Movement Limiter
    public void RestrictPlayerMovement()
    {
        MovementLimiter.instance.playerCanMove = false;
    }

    public void EnablePlayerMovement()
    {
        MovementLimiter.instance.playerCanMove = true;
    }

    public void RestrictPlayerCombatMoves()
    {
        MovementLimiter.instance.playerCanAttack = false;
    }

    public void EnablePlayerCombatMoves()
    {
        MovementLimiter.instance.playerCanAttack = true;
    }

    public void RestrictPlayerParkourMoves()
    {
        MovementLimiter.instance.playerCanParkour = false;
    }

    public void EnablePlayerParkourMoves()
    {
        MovementLimiter.instance.playerCanParkour = true;
    }

    public void DisableRigidbody()
    {
        Debug.Log("simu1");
        playerRB2D.simulated = false;
    }

    public void EnableRigidbody()
    {
        Debug.Log("simu2");
        playerRB2D.simulated = true;
    }
    #endregion

    #region Physics Tweaks
    public void AddJumpForce()
    {
        playerJump.StartJump();
    }

    public void DisableCharging()
    {
        playerJump.DisableCharging();
    }
    #endregion

    #region State Change Methods
    public void LightAttackStarted()
    {
        playerCombatSystemScript.lightAttackExecuting = true;
    }

    public void LightAttackEnded()
    {
        playerCombatSystemScript.lightAttackExecuting = false;
    }

    public void HeavyAttackStarted()
    {
        playerCombatSystemScript.heavyAttackExecuting = true;
    }

    public void HeavyAttackEnded()
    {
        playerCombatSystemScript.heavyAttackExecuting = false;
    }
    #endregion

    #region Player Movement Animation Events
    public void JumpStartOnMovement()
    {
        MovementLimiter.instance.playerCanMove = true;
        playerAppearanceScript.jumpEffects();
        playerJump.DisableCharging();
        playerJump.StartJump();
        PlayerInputManager.Instance.playerInputActions.Player.JumpRoll.Disable();
    }

    public void ResetSystemsOnJumpDropEnd()
    {
        playerJump.DisableCharging();
        MovementLimiter.instance.playerCanParkour = true;
        MovementLimiter.instance.playerCanMove = true;
        PlayerInputManager.Instance.playerInputActions.Player.JumpRoll.Enable();
    }
    #endregion
}
