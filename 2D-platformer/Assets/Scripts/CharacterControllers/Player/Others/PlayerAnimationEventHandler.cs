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
    private PlayerRoll playerRoll;
    private PlayerDodge playerDodge;
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
        playerRoll = playerTransform.GetComponent<PlayerRoll>();
        playerDodge = playerTransform.GetComponent<PlayerDodge>();
        playerAppearanceScript = playerTransform.GetComponent<PlayerAppearanceScript>();
        lightAttackHitBox = GameManager.Instance.lightAttackHitBox;
    }

    #region Player Movement Limiter
    public void RestrictPlayerMovement()
    {
        MovementLimiter.instance.playerCanMove = false;
        playerMovement.stopPlayerCompletely();
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

    public void RestrictPlayerRigidbody()
    {
        Debug.Log("simu1");
        playerRB2D.simulated = false;
    }

    public void EnablePlayerRigidbody()
    {
        Debug.Log("simu2");
        playerRB2D.simulated = true;
    }
    #endregion

    #region Movement Physics Tweaks
    public void AddJumpForce()
    {
        playerJump.StartJump();
    }

    public void DisableCharging()
    {
        playerJump.DisableCharging();
    }

    public void ResetRollingVariables()
    {
        playerRoll.DeactivateRolling();
    }

    public void ResetDodgingVariables()
    {
        playerDodge.DeactivateDodging();
    }
    #endregion

    #region Combat Physics Tweaks
    public void ArmedWeaponsOnSwordSlash()
    {
        playerMovement.applyPlayerMomentum(1.0f);
    }

    public void ArmedWeaponsOnSwordSwing()
    {
        playerMovement.applyPlayerMomentum(1.0f);
    }

    public void ArmedWeaponsOnSwordThrust()
    {
        playerMovement.applyPlayerMomentum(1.0f);
    }

    public void ArmedWeaponsJumpAttackMomentum()
    {
        playerMovement.applyPlayerMomentum(12.0f);
    }

    public void ArmedWeaponsChargedAttackMomentum()
    {
        playerMovement.applyVariablePlayerMomentum("ChargeAttack", 30.0f);
    }
    #endregion

    #region Combat State Change Methods
    public void LightAttackStarted()
    {
        RestrictPlayerMovement();
        playerCombatSystemScript.lightAttackExecuting = true;
    }

    public void LightAttackEnded()
    {
        playerCombatSystemScript.lightAttackExecuting = false;
        EnablePlayerMovement();
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
    public void MovementOnJumpStart()
    {
        MovementLimiter.instance.playerCanMove = true;
        playerAppearanceScript.jumpEffects();
        playerJump.DisableCharging();
        playerJump.StartJump();
        playerJump.jumpAnimInProgress = true;

        PlayerInputManager.Instance.playerInputActions.Player.Jump.Disable();
        PlayerInputManager.Instance.playerInputActions.Player.RollDodge.Disable();
    }

    public void MovementOnJumpAnimEnd()
    {
        playerJump.jumpAnimInProgress = false;
    }

    public void ResetOnJumpDropEnd()
    {
        playerJump.DisableCharging();
        MovementLimiter.instance.playerCanParkour = true;
        MovementLimiter.instance.playerCanMove = true;

        PlayerInputManager.Instance.playerInputActions.Player.RollDodge.Enable();
        PlayerInputManager.Instance.playerInputActions.Player.Jump.Enable();
    }

    public void TriggerSystemsPlayerOnAir()
    {
        PlayerInputManager.Instance.playerInputActions.Player.GrapplingGun.Enable();
    }

    public void TriggerSystemsPlayerLanding()
    {
        PlayerInputManager.Instance.playerInputActions.Player.GrapplingGun.Disable();
        playerJump.jumpAnimInProgress = false;
    }

    public void MovementEnableGrappleBoost()
    {
        PlayerInputManager.Instance.playerInputActions.Player.GrappleBoost.Enable();
    }

    public void DisableGrappleBoost()
    {
        PlayerInputManager.Instance.playerInputActions.Player.GrappleBoost.Disable();
    }
    #endregion
}
